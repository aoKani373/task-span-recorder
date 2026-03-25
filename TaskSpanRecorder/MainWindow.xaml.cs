using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using TaskSpanRecorder.ViewModels;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

namespace TaskSpanRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            SystemThemeWatcher.Watch(this);
            InitializeComponent();

            IContentDialogService contentDialogService = new ContentDialogService();
            contentDialogService.SetDialogHost(RootContentDialogPresenter);

            DataContext = new MainViewModel(contentDialogService);
        }
    }
}