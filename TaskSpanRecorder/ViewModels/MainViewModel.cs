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

        [ObservableProperty]
        private TaskCategory? _selectedTaskCategory;

        public MainViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
        }

        [RelayCommand]
        private void StartTask()
        {
            StopTask();

            
        }

        [RelayCommand]
        private void StopTask()
        {

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
