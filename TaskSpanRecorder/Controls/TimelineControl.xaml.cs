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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
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