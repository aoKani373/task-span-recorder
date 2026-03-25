using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TaskSpanRecorder.Models;
using TaskSpanRecorder.ViewModels;

namespace TaskSpanRecorder.Controls
{
    /// <summary>
    /// TimelineControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineControl : UserControl
    {
        public ObservableCollection<TimeMark> TimeMarks { get; } = new();

        public TimelineControl()
        {
            InitializeComponent();

            GenerateFixedTimeMarks();

            this.Loaded += (s, e) => FocusCurrentTime();
        }

        private void GenerateFixedTimeMarks()
        {
            TimeMarks.Clear();
            for (int i = 0; i <= 86400; i += 900)
            {
                bool isHour = i % 3600 == 0;
                TimeMarks.Add(new TimeMark
                {
                    Seconds = i,
                    Label = TimeSpan.FromSeconds(i).ToString(@"hh\:mm"),
                    LineOpacity = isHour ? 0.6 : 0.2
                });
            }
        }

        private void FocusCurrentTime()
        {
            if (TimelineScrollViewer.ViewportWidth == 0) return;

            double targetDurationSeconds = 3600.0;
            double newScale = TimelineScrollViewer.ViewportWidth / targetDurationSeconds;

            TimelineScale.ScaleX = newScale;

            double nowSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
            double targetScrollSeconds = nowSeconds - 1800;

            if (targetScrollSeconds < 0) targetScrollSeconds = 0;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TimelineScrollViewer.ScrollToHorizontalOffset(targetScrollSeconds * newScale);
            }), DispatcherPriority.Loaded);
        }

        private void TimelineScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FocusCurrentTime();
        }

        private void TimelineScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                TimelineScrollViewer.LineLeft();
                TimelineScrollViewer.LineLeft();
            }
            else
            {
                TimelineScrollViewer.LineRight();
                TimelineScrollViewer.LineRight();
            }

            e.Handled = true;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb thumb ||
                thumb.DataContext is not Models.TaskSpan currentSpan ||
                this.DataContext is not MainViewModel vm) return;

            bool isLeftThumb = thumb.HorizontalAlignment == HorizontalAlignment.Left;

            var orderedSpans = vm.TaskSpans.Where(t => t.Date == currentSpan.Date).OrderBy(t => t.StartTime).ToList();
            int currentIndex = orderedSpans.IndexOf(currentSpan);

            TaskSpan? leftSpan = null;
            TaskSpan? rightSpan = null;

            if (isLeftThumb)
            {
                rightSpan = currentSpan;
                leftSpan = currentIndex > 0 ? orderedSpans[currentIndex - 1] : null;
            }
            else
            {
                leftSpan = currentSpan;
                rightSpan = currentIndex < orderedSpans.Count - 1 ? orderedSpans[currentIndex + 1] : null;
            }

            if (leftSpan == null && rightSpan == null) return;

            double currentBoundarySeconds = rightSpan != null ? rightSpan.StartSeconds : leftSpan.EndTime.Value.ToTimeSpan().TotalSeconds;

            double newBoundarySeconds = currentBoundarySeconds + e.HorizontalChange;

            double minSeconds = leftSpan != null ? leftSpan.StartSeconds + 60 : 0;
            double maxSeconds = 86400;
            if (rightSpan != null)
            {
                maxSeconds = rightSpan.EndTime.HasValue
                    ? rightSpan.EndTime.Value.ToTimeSpan().TotalSeconds - 60
                    : DateTime.Now.TimeOfDay.TotalSeconds - 60;
            }

            if (newBoundarySeconds < minSeconds) newBoundarySeconds = minSeconds;
            if (newBoundarySeconds > maxSeconds) newBoundarySeconds = maxSeconds;

            var newBoundaryTime = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(newBoundarySeconds));

            if (leftSpan != null) leftSpan.EndTime = newBoundaryTime;
            if (rightSpan != null) rightSpan.StartTime = newBoundaryTime;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.SaveChanges();
            }
        }
    }
    public class TimeMark
    {
        public double Seconds { get; set; }
        public string Label { get; set; } = string.Empty;
        public double LineOpacity { get; set; }
    }

    public class CategoryYConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is Models.TaskCategory category && values[1] is IEnumerable<Models.TaskCategory> categories)
            {
                int index = categories.ToList().IndexOf(category);
                if (index >= 0)
                {
                    return index * 60.0;
                }
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scale && scale > 0)
            {
                return 1.0 / scale;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}