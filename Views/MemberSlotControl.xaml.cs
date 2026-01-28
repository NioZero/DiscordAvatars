using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace DiscordAvatars.Views
{
    public sealed partial class MemberSlotControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MemberSlotControl), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public MemberSlotControl()
        {
            this.InitializeComponent();
        }
    }
}
