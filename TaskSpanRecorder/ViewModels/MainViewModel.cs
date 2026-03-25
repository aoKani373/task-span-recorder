using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        private TaskCategory _idleCategory;

        [ObservableProperty]
        private TaskCategory? _selectedTaskCategory;

        [ObservableProperty]
        private TaskSpan? _currentTaskSpan;

        [ObservableProperty]
        private string _currentStatusText = "待機中...";

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

            var spans = _dbContext.TaskSpans.Include(ts => ts.TaskCategory).ToList();
            foreach (var s in spans)
            {
                TaskSpans.Add(s);
            }

            CurrentTaskSpan = TaskSpans.LastOrDefault(ts => ts.EndTime == null);
            if (CurrentTaskSpan != null)
            {
                CurrentStatusText = $"実行中: {CurrentTaskSpan.TaskCategory?.Name} (開始: {CurrentTaskSpan.StartTime:HH:mm})";
            }
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
    }
}
