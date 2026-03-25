using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public TimelineControl()
        {
            InitializeComponent();

            this.Loaded += (s, e) => AutoFitTimeline();

            this.DataContextChanged += TimelineControl_DataContextChanged;
        }


        private void TimelineControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainViewModel oldVm)
            {
                oldVm.TaskSpans.CollectionChanged -= TaskSpans_CollectionChanged;
            }
            if (e.NewValue is MainViewModel newVm)
            {
                newVm.TaskSpans.CollectionChanged += TaskSpans_CollectionChanged;
            }
        }

        private void TaskSpans_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AutoFitTimeline();
        }

        private void TimeLineScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoFitTimeline();
        }

        private void AutoFitTimeline()
        {
            if (DataContext is not MainViewModel vm || !vm.TaskSpans.Any() || TimelineScrollViewer.ViewportWidth == 0)
                return;

            double minSeconds = vm.TaskSpans.Min(t => t.StartSeconds);
            double maxSeconds = vm.TaskSpans.Max(t => t.EndTime.HasValue
                ? t.EndTime.Value.ToTimeSpan().TotalSeconds
                : DateTime.Now.TimeOfDay.TotalSeconds);

            double durationSeconds = maxSeconds - minSeconds;
            if (durationSeconds <= 0) durationSeconds = 60;

            double paddingFactor = 0.95;
            double newScale = (TimelineScrollViewer.ViewportWidth * paddingFactor) / durationSeconds;

            ZoomSlider.Value = newScale;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                double leftPaddingOffset = TimelineScrollViewer.ViewportWidth * ((1.0 - paddingFactor) / 2.0);
                double targetScrollPosition = (minSeconds * newScale) - leftPaddingOffset;

                TimelineScrollViewer.ScrollToHorizontalOffset(targetScrollPosition);

            }), DispatcherPriority.Loaded);
        }
    }
}
