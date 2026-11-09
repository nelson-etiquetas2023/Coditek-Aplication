using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ScanProMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Registrar servicios API con HttpClient
        builder.Services.AddHttpClient<IApiProductService, ApiProductService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7220/");
        });

        builder.Services.AddHttpClient<IApiConfigService, ApiConfigService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7220/");
        });

        builder.Services.AddHttpClient<IApiUserService, ApiUserService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7220/");
        });

        builder.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            // Colores personalizados de la paleta Pantone CODIX-LABELS
            {"PrimaryColor", Color.Parse("#195CCD")},
            {"PrimaryDarkColor", Color.Parse("#071D48")},
            {"PrimaryDarkerColor", Color.Parse("#031127")},
            {"SecondaryColor", Color.Parse("#B4D6EA")},
            {"BackgroundColor", Color.Parse("#FFFFFF")},
            {"SurfaceColor", Color.Parse("#F4F8FC")},
            {"TextPrimaryColor", Color.Parse("#031127")},
            {"TextSecondaryColor", Color.Parse("#4A5A7A")},
            {"BorderColor", Color.Parse("#B4D6EA")},
        });

        return builder.Build();
    }
}