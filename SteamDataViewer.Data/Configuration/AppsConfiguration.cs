using Microsoft.Extensions.DependencyInjection;
using SteamDataViewer.Data.Apps;
using SteamDataViewer.Data.Apps.GetApps;

namespace SteamDataViewer.Data.Configuration;

public static class AppsConfiguration
{
    public static IServiceCollection AddAppsService(this IServiceCollection services)
    => services.AddTransient<ILocalAppsService, LocalAppsService>()
            .AddTransient<IOnlineAppsService, OnlineAppsService>()
            .AddTransient<IAppsService, AppsService>();
}