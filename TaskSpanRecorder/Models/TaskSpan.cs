using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSpanRecorder.Models
{
    public partial class TaskSpan : ObservableObject
    {
        public int Id { get; set; }
        public int TaskCategoryId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }

        [ObservableProperty]
        private TimeOnly? _endTime = null;

        public TaskCategory? TaskCategory { get; set; } = null;

        public double StartSeconds => StartTime.ToTimeSpan().TotalSeconds;

        public double DurationSeconds => ((EndTime ?? TimeOnly.FromDateTime(DateTime.Now)) - StartTime).TotalSeconds;

        partial void OnEndTimeChanged(TimeOnly? value)
        {
            OnPropertyChanged(nameof(DurationSeconds));
        }
    }
}
