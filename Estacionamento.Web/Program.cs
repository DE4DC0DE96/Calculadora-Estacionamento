using System.Diagnostics;

const string DefaultUrl = "http://localhost:5187";

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]) &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls(DefaultUrl);
}

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

OpenBrowserWhenReady(app, args);

app.Run();

static void OpenBrowserWhenReady(WebApplication app, string[] args)
{
    if (args.Any(arg => string.Equals(arg, "--no-browser", StringComparison.OrdinalIgnoreCase)))
    {
        return;
    }

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        string url = app.Urls.FirstOrDefault() ?? DefaultUrl;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // If the browser cannot be opened automatically, the console still shows the local URL.
        }
    });
}
