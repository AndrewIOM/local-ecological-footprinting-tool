namespace Oxlel.LeftMarine.WebUI

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Ecoset.WebUI
open Hangfire
open System.Text.Json.Serialization

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddControllersWithViews()
            .AddNewtonsoftJson()
            .AddRazorRuntimeCompilation()
            .AddJsonOptions(fun opt ->
                opt.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter()) ) |> ignore
        services.AddRazorPages().AddNewtonsoftJson() |> ignore
        services.AddEcosetUI this.Configuration |> ignore
        services.AddEcosetDataPackageAPI this.Configuration |> ignore

        let sp = services.BuildServiceProvider()
        let staticFileOptions = sp.GetService<IOptions<StaticFileOptions>>()
        services.AddWebOptimizer(fun (pipeline:WebOptimizer.IAssetPipeline) ->
            pipeline.AddScssBundle("/styles/main.css", "/styles/main.scss")
                .UseFileProvider(staticFileOptions.Value.FileProvider) |> ignore
            pipeline.AddScssBundle("/styles/report.css", "/styles/report.scss")
                .UseFileProvider(staticFileOptions.Value.FileProvider) |> ignore
        ) |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =

        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseStatusCodePagesWithReExecute("/Home/Errors/{0}") |> ignore
            app.UseExceptionHandler("/Home/Errors/500") |> ignore
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts() |> ignore

        // app.UseHttpsRedirection() |> ignore
        app.UseWebOptimizer() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore
        app.UseEndpoints(fun endpoints ->
            endpoints.MapAreaControllerRoute(
                "Interpretation", "Interpretation", "{controller=Home}/{action=Index}/{id?}") |> ignore
            endpoints.MapControllerRoute(
                name = "areas",
                pattern = "{area:exists}/{controller=Home}/{action=Index}/{id?}") |> ignore
            endpoints.MapControllerRoute(
                name = "default",
                pattern = "{controller=Home}/{action=Index}/{id?}") |> ignore
            endpoints.MapRazorPages() |> ignore) |> ignore

        // Ecoset configuration
        app.UseSwagger() |> ignore
        app.UseSwaggerUI(fun c ->
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "LEFT API v1")) |> ignore
        app.UseHangfireDashboard() |> ignore
        app.UseHangfireServer() |> ignore
        app.UseEcosetMigrations(this.Configuration) |> ignore
        app.UseEcosetRoles(this.Configuration) |> ignore
        app.UseEcosetAdminUser(this.Configuration) |> ignore

    member val Configuration : IConfiguration = null with get, set
