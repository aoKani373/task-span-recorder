using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskSpanRecorder.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace TaskSpanRecorder.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IContentDialogService _contentDialogService;

        public ObservableCollection<TaskCategory> TaskCategories { get; } = new();
        public ObservableCollection<TaskSpan> TaskSpans { get; } = new();

        private readonly TaskCategory _idleCategory = new() { Id = 0, Name = "空き時間" };

        [ObservableProperty]
        private TaskCategory? _selectedTaskCategory;

        [ObservableProperty]
        private TaskSpan? _currentTaskSpan;

        [ObservableProperty]
        private string _currentStatusText = "待機中...";

        public MainViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;

            TaskCategories.Add(_idleCategory);
            TaskCategories.Add(new TaskCategory { Id = 1, Name = "開発" });
            TaskCategories.Add(new TaskCategory { Id = 2, Name = "会議" });

            SelectedTaskCategory = TaskCategories[1];
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
                Id = TaskSpans.Count + 1,
                TaskCategoryId = targetCategory.Id,
                TaskCategory = targetCategory,
                Date = currentDate,
                StartTime = currentTime,
                EndTime = null
            };

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
                int nextId = TaskCategories.Count > 0 ? TaskCategories.Max(c => c.Id) + 1 : 1;
                var newCategory = new TaskCategory { Id = nextId, Name = textBox.Text };

                TaskCategories.Add(newCategory);
                SelectedTaskCategory = newCategory;
            }
        }
    }
}
