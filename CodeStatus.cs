
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Units;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace FoundryBlazor;

public class CodeStatus
{
    public string Version()
    {
        var version = GetType().Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        var ver = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        return ver;
    }   
}

public static class FoundryBlazorExtensions
{
    public static IServiceCollection AddFoundryBlazorServices(this IServiceCollection services, EnvConfig envConfig)
    {
        services.AddSingleton<IEnvConfig>(provider => envConfig);
        //Mentor Services
        services.AddScoped<ComponentBus>();
        services.AddScoped<NotificationService>();
        services.AddScoped<IToast, Toast>();

        services.AddScoped<DialogService>();
        services.AddScoped<IPopupDialog, PopupDialog>();

        services.AddScoped<IQRCodeService, QRCodeService>();
        services.AddScoped<ICommand, CommandService>();
        services.AddScoped<IPanZoomService, PanZoomService>();
        services.AddScoped<IDrawing, FoDrawing2D>();
        services.AddScoped<IArena, FoArena3D>();

        services.AddScoped<IPageManagement, PageManagementService>();
        services.AddScoped<IHitTestService, HitTestService>();
        services.AddScoped<ISelectionService, SelectionService>();
        services.AddScoped<IStageManagement, StageManagementService>();
        services.AddScoped<IWorkspace, FoWorkspace>();


        services.AddScoped<IFoundryService, FoundryService>();
        services.AddScoped<IWorldManager, WorldManager>();
        services.AddScoped<IUnitSystem, UnitSystem>();
        return services;
    }


}
