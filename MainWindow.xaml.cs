using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using DiscordAvatars.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DiscordAvatars
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            _viewModel = new MainViewModel();
            Root.DataContext = _viewModel;
            Closed += OnClosed;
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            _viewModel.SaveState();
        }

        private async void OnSelectFolderClick(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            picker.FileTypeFilter.Add("*");

            StorageFolder folder = await PickSingleFolderAsync(picker);
            if (folder != null)
            {
                _viewModel.SetSelectedFolder(folder.Path);
            }
        }

        private static Task<StorageFolder> PickSingleFolderAsync(FolderPicker picker)
        {
            var tcs = new TaskCompletionSource<StorageFolder>();
            var operation = picker.PickSingleFolderAsync();
            operation.Completed = (op, status) =>
            {
                switch (status)
                {
                    case AsyncStatus.Completed:
                        tcs.TrySetResult(op.GetResults());
                        break;
                    case AsyncStatus.Error:
                        tcs.TrySetException(op.ErrorCode);
                        break;
                    default:
                        tcs.TrySetCanceled();
                        break;
                }
            };
            return tcs.Task;
        }
    }
}
