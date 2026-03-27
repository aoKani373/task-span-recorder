using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskSpanRecorder.Data;
using TaskSpanRecorder.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace TaskSpanRecorder.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IContentDialogService _contentDialogService;
        private readonly AppDbContext _dbContext;

        public ObservableCollection<TaskCategory> TaskCategories { get; } = new();
        public ObservableCollection<TaskSpan> TaskSpans { get; } = new();

        public ObservableCollection<ISeries> CategoryPieSeries { get; } = new();

        private TaskCategory _idleCategory = null!;

        [ObservableProperty]
        private TaskCategory? _selectedTaskCategory;

        [ObservableProperty]
        private TaskSpan? _currentTaskSpan;

        [ObservableProperty]
        private string _currentStatusText = "待機中...";

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        partial void OnStartDateChanged(DateTime value) => UpdateAggregation();
        partial void OnEndDateChanged(DateTime value) => UpdateAggregation();

        public MainViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;

            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();

            LoadData();
        }

        [RelayCommand]
        private void StartSelectedTask()
        {
            if (SelectedTaskCategory == null) return;
            SwitchToCategory(SelectedTaskCategory);
        }

        [RelayCommand]
        private void SwitchToIdle()
        {
            SwitchToCategory(_idleCategory);
        }

        private void LoadData()
        {
            var categories = _dbContext.TaskCategories.ToList();
            foreach (var c in categories)
            {
                TaskCategories.Add(c);
            }

            _idleCategory = TaskCategories.First(c => c.Id == -1);
            SelectedTaskCategory = TaskCategories.FirstOrDefault(c => c.Id == 1);

            var spans = _dbContext.TaskSpans
                .Include(ts => ts.TaskCategory)
                .ThenInclude(tc => tc.TaskGroup)
                .ToList();

            foreach (var s in spans)
            {
                TaskSpans.Add(s);
            }

            CurrentTaskSpan = TaskSpans.LastOrDefault(ts => ts.EndTime == null);
            if (CurrentTaskSpan != null)
            {
                CurrentStatusText = $"実行中: {CurrentTaskSpan.TaskCategory?.Name} (開始: {CurrentTaskSpan.StartTime:HH:mm})";
            }

            UpdateAggregation();
        }

        private void SwitchToCategory(TaskCategory targetCategory)
        {
            if (CurrentTaskSpan?.TaskCategoryId == targetCategory.Id) return;
            
            var now = DateTime.Now;
            var currentDate = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);

            if (CurrentTaskSpan != null)
            {
                CurrentTaskSpan.EndTime = currentTime;
                CurrentStatusText = $"終了: {CurrentTaskSpan.TaskCategory?.Name}";
            }
            var newTaskSpan = new TaskSpan
            {
                TaskCategoryId = targetCategory.Id,
                Date = currentDate,
                StartTime = currentTime,
                EndTime = null
            };

            _dbContext.TaskSpans.Add(newTaskSpan);
            _dbContext.SaveChanges();

            TaskSpans.Add(newTaskSpan);
            CurrentTaskSpan = newTaskSpan;

            CurrentStatusText = $"実行中: {targetCategory.Name} (開始: {currentTime:HH:mm})";
        }

        [RelayCommand]
        private async Task AddTaskCategoryAsync()
        {
            var textBox = new Wpf.Ui.Controls.TextBox
            {
                PlaceholderText = "新しいカテゴリ名を入力"
            };

            var dialog = new ContentDialog
            {
                Title = "カテゴリの追加",
                Content = textBox,
                PrimaryButtonText = "追加",
                CloseButtonText = "キャンセル",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await _contentDialogService.ShowAsync(dialog, default);

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newCategory = new TaskCategory { Name = textBox.Text };

                _dbContext.TaskCategories.Add(newCategory);
                _dbContext.SaveChanges();

                TaskCategories.Add(newCategory);
                SelectedTaskCategory = newCategory;
            }
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        [RelayCommand]
        public void UpdateAggregation()
        {
            CategoryPieSeries.Clear();

            var start = DateOnly.FromDateTime(StartDate);
            var end = DateOnly.FromDateTime(EndDate);

            var targetSpans = TaskSpans.Where(ts => ts.Date >= start && ts.Date <= end && ts.TaskCategoryId != -1);

            var grouped = targetSpans.GroupBy(ts => ts.TaskCategory?.Name ?? "不明")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalHours = g.Sum(ts => ts.DurationSeconds) / 3600.0
                })
                .Where(g => g.TotalHours > 0)
                .ToList();

            var jpTypeface = SKTypeface.FromFamilyName("Yu Gothic UI");

            foreach (var item in grouped)
            {
                string cleanName = Regex.Replace(item.CategoryName, @"\p{Cs}|\p{So}", "").Trim();

                CategoryPieSeries.Add(new PieSeries<double>
                {
                    Name = cleanName,
                    Values = new[] { item.TotalHours },
                    DataLabelsFormatter = point => $"{cleanName} ({point.Model:F1} h)",
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White) { SKTypeface = jpTypeface },
                    ToolTipLabelFormatter = point => $"{point.Model:F2} 時間"
                });
            }
        }
    }
}
