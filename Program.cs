using DAProjectChecker.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DAProjectChecker.Browser;
using DAProjectChecker.Notifications;

var builder = Host.CreateApplicationBuilder(args);

#region Configurations
builder.Services
    .AddOptions<WebsiteOptions>()
    .Bind(builder.Configuration.GetSection("Website"))
    .ValidateOnStart();

builder.Services
    .AddOptions<NtfyOptions>()
    .Bind(builder.Configuration.GetSection("Ntfy"))
    .ValidateOnStart();
#endregion

#region Services
builder.Services.AddSingleton<DAProjectChecker.Browser.ProjectWatcher>();
builder.Services.AddSingleton<DAProjectChecker.Notifications.NtfyNotifier>();
#endregion

using var host = builder.Build();

var watcher = host.Services
    .GetRequiredService<DAProjectChecker.Browser.ProjectWatcher>();

var notifier = host.Services
    .GetRequiredService<DAProjectChecker.Notifications.NtfyNotifier>();

await watcher.StartAsync(async projectCount =>
{
    await notifier.SendAsync(
        "Projects Available!",
        $"{projectCount} project(s) are now available.");
});