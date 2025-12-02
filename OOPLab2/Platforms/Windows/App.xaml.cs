using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using Application = Microsoft.Maui.Controls.Application;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OOPLab2.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var window = Microsoft.Maui.Controls.Application.Current?.Windows[0].Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                var hwnd = WindowNative.GetWindowHandle(window);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                appWindow.Closing += async (s, e) =>
                {
                    e.Cancel = true;

                    var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
                    if (mainPage != null)
                    {
                        bool answer = await mainPage.DisplayAlert(
                            "Підтвердження виходу",
                            "Чи дійсно ви хочете завершити роботу з програмою?",
                            "Так",
                            "Ні");

                        if (answer)
                        {
                            Microsoft.Maui.Controls.Application.Current?.Quit();
                        }
                    }
                };
            }
        }
    }

}
