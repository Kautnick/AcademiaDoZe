using Microsoft.Extensions.DependencyInjection;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Application.Services;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ILogradouroService, LogradouroService>();
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IMatriculaService, MatriculaService>();
        return services;
    }
}
