using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { })
            ;

        var connectionString = Environment.GetEnvironmentVariable("ACADEMIA_DO_ZE_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = @"Server=DESKTOP-U63LS9P\SQLEXPRESS;Initial Catalog=db_academia_do_ze;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";

        builder.Services.AddScoped<ILogradouroRepository>(
            _ => new LogradouroRepository(connectionString, DatabaseType.SqlServer));
        builder.Services.AddScoped<IAlunoRepository>(
            _ => new AlunoRepository(connectionString, DatabaseType.SqlServer));
        builder.Services.AddScoped<IColaboradorRepository>(
            _ => new ColaboradorRepository(connectionString, DatabaseType.SqlServer));
        builder.Services.AddScoped<IMatriculaRepository>(
            _ => new MatriculaRepository(connectionString, DatabaseType.SqlServer));

        builder.Services.AddApplicationServices();

        // Register ViewModels and Views
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<DashboardListViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();

        return builder.Build();
    }
}
