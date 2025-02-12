using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using SelfCheckoutApp.Services;


namespace SelfCheckoutApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCore()
                .UseBarcodeReader();


            builder.Services.AddSingleton<UserSession>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
