using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace DiscordAvatars
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            if (!Bootstrap.TryInitialize(0x00010008, null, out var error))
            {
                ShowBootstrapError(error);
                return;
            }

            Application.Start(_ => new App());
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

        private static void ShowBootstrapError(int error)
        {
            var message = "No se pudo inicializar Windows App Runtime.\n\n" +
                          "Instala o repara el runtime y vuelve a intentar.\n\n" +
                          $"Codigo de error: 0x{error:X8}";
            MessageBoxW(IntPtr.Zero, message, "Discord Avatars", 0);
        }
    }
}
