using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;


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
                .UseBarcodeReader() // Ensures ZXing is registered
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(CameraBarcodeReaderView), typeof(CameraBarcodeReaderViewHandler));
                }) // Correctly registers the camera barcode reader
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
