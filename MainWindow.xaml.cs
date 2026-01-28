using Microsoft.UI.Xaml;
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
    }
}
