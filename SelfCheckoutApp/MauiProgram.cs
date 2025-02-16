using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using SelfCheckoutApp.Services;


using SelfCheckoutApp.Constants;
using SelfCheckoutApp;
using System.Reflection.Metadata;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader();

        builder.Services.AddSingleton<UserSession>();
        builder.Services.AddSingleton(new ServerStatusService(ApiConstants.BaseUri));

        return builder.Build();
    }
}