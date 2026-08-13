using DAProjectChecker.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace DAProjectChecker.Browser;

public class ProjectWatcher : IAsyncDisposable
{
    private readonly WebsiteOptions _options;

    private IPlaywright? _playwright;
    private IBrowserContext? _context;
    private IPage? _page;

    public ProjectWatcher(
        IOptions<WebsiteOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(
        Func<int, Task> onProjectsFound)
    {

        _playwright =
            await Playwright.CreateAsync();

        var profileDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "browser-profile");

        _context =
            await _playwright.Chromium
                .LaunchPersistentContextAsync(
                    profileDirectory,
                    new BrowserTypeLaunchPersistentContextOptions
                    {
                        Headless = false
                    });

        _page = _context.Pages.FirstOrDefault()
            ?? await _context.NewPageAsync();

        Console.WriteLine(
            $"Opening {_options.Url}");

        await _page.GotoAsync(_options.Url);

        Console.WriteLine();
        Console.WriteLine(
            "Browser opened.");

        Console.WriteLine(
            "Log in if necessary.");

        Console.WriteLine(
            "Press ENTER after you are logged in.");

        Console.ReadLine();

        Console.WriteLine();
        Console.WriteLine(
            "Project watcher started.");

        //var lastRefresh = DateTime.UtcNow;
        //bypass appsettings.json refresh interval for now
        //var randomRefreshSeconds = Random.Shared.Next(180, 301);
        //var randomRefreshSeconds = Random.Shared.Next(6, 7);
        //var nextRefresh = DateTime.Now.AddSeconds(randomRefreshSeconds);

        while (true)
        {
            var randomRefreshSeconds = Random.Shared.Next(180, 301);
            var nextRefresh = DateTime.Now.AddSeconds(randomRefreshSeconds);


            try
            {
                var projectCount =
                    await CheckProjectsAsync();

                if (projectCount > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        $"Projects found: {projectCount}");

                    await onProjectsFound(projectCount);

                    Console.WriteLine(
                        "Notification sent.");

                    Console.WriteLine(
                        "Stopping watcher.");

                    break;
                }

                // Refresh periodically
                if (DateTime.Now  >= nextRefresh)
                {
                    Console.WriteLine(
                        "Refreshing page...");

                    await _page.ReloadAsync(
                        new PageReloadOptions
                        {
                            WaitUntil =
                                WaitUntilState.DOMContentLoaded
                        });

                    //lastRefresh =
                    //    DateTime.UtcNow;
                    nextRefresh =
                        DateTime.Now;
                }
                Console.WriteLine(
                    $"Next refresh in {nextRefresh} seconds. Current Time: {DateTime.Now:T}");

                Console.WriteLine(
                    $"[{DateTime.Now:T}] " +
                    $"No projects. " +
                    $"Checking again in " +
                    //$"{_options.CheckIntervalSeconds}s.");
                    $"{randomRefreshSeconds}s.");

                //await Task.Delay(
                //    TimeSpan.FromSeconds(
                //        _options.CheckIntervalSeconds));
                await Task.Delay(
                    TimeSpan.FromSeconds(
                        randomRefreshSeconds));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error: {ex.Message}");

                await Task.Delay(
                    TimeSpan.FromSeconds(10));
            }
        }
    }

    private async Task<int> CheckProjectsAsync()
    {
        if (_page is null)
        {
            throw new InvalidOperationException(
                "Browser page is not initialized.");
        }

        Console.WriteLine(
            $"[{DateTime.Now:T}] Checking Projects...");

        // Find the Projects tab.
        //
        // We're deliberately not relying on
        // ":r0:-tab-projects" because that looks
        // like a generated React ID.
        var projectsTab = _page.Locator(
            "[role='tab'][id='_r_0_-tab-projects']",
            new PageLocatorOptions
            {
                HasTextString = "Projects"
                //HasTextString = "Qualifications"
            });

        if (await projectsTab.CountAsync() == 0)
        {
            Console.WriteLine(
                "Projects tab not found.");

            return 0;
        }

        await projectsTab.First.ClickAsync();

        // Give the page time to render
        // the Projects panel.
        await _page.WaitForTimeoutAsync(1000);

        var projectsPanel = _page.Locator(
            "[role='tabpanel'][id*='tabpanel-projects']");
        //var projectsPanel = _page.Locator(
        //    "[role='tabpanel'][id*='tabpanel-qualifications']");

        if (await projectsPanel.CountAsync() == 0)
        {
            Console.WriteLine(
                "Projects panel not found.");

            return 0;
        }

        var table = projectsPanel.Locator(
            "table[data-total-columns]");

        if (await table.CountAsync() == 0)
        {
            return 0;
        }

        var rows = table.Locator(
            "tbody tr");

        return await rows.CountAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
        }

        _playwright?.Dispose();
    }
}