namespace GPMS.Web

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.Google
open GPMS.Infrastructure.Data
open GPMS.Infrastructure.Repositories
open GPMS.Application.Interfaces.Repositories

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        // Add services to the container.
        let connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        builder.Services.AddDbContext<GpmsDbContext>(fun options ->
            options.UseSqlServer(connectionString) |> ignore)

        // Register Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>() |> ignore
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>() |> ignore
        builder.Services.AddScoped<IProjectGroupRepository, ProjectGroupRepository>() |> ignore
        builder.Services.AddScoped<IReviewRoundRepository, ReviewRoundRepository>() |> ignore
        builder.Services.AddScoped<IReviewerAssignmentRepository, ReviewerAssignmentRepository>() |> ignore
        builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>() |> ignore
        builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>() |> ignore
        builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>() |> ignore
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>() |> ignore

        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()

        builder.Services.AddRazorPages()

        // add google authentication
        builder.Services.AddAuthentication(fun options ->
            options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddGoogle(fun options ->
                options.ClientId <- builder.Configuration["Authentication:Google:ClientId"]
                options.ClientSecret <- builder.Configuration["Authentication:Google:ClientSecret"]
                options.CallbackPath <- "/signin-google") |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthentication()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Auth}/{action=Login}/{id?}")


        app.MapRazorPages()

        app.Run()

        exitCode
