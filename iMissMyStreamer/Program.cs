using iMissMyStreamer.Components;
using iMissMyStreamer.Services.Data;
using iMissMyStreamer.Services.Persist;
using iMissMyStreamer.Workers;
using Microsoft.Extensions.FileProviders;

namespace iMissMyStreamer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IPersistService, PersistService>();
        builder.Services.AddSingleton<IDataService, DataService>();

#if DEBUG
#else

        builder.Services.AddHostedService<TwitchAPIWorker>();
#endif


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();
        }

        app.UseStaticFiles();

#if DEBUG
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(@"D:\imlMnt"),
            RequestPath = "/data"
        });

#else
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(@"/data"),
            RequestPath = "/data"
        });
#endif
        //app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // A man is not dead while his name is still spoken
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Clacks-Overhead", "GNU Terry Pratchett");
                return Task.CompletedTask;
            });

            await next();
        });

        app.Run();
    }
}
