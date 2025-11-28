using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Shop.Abstractions.Security;
using Shop.Authorization;
using Shop.Domain.Models;
using Shop.Infrastructure.Security;
using Shop.Infrastructure.Security.Tokens;
using System.Text;

namespace Shop.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddJwtBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };

            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    context.Request.Cookies.TryGetValue("access_token", out var token);
                    if (token is not null)
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddPolicies(this IServiceCollection services)
    {
        services.AddTransient<IAuthorizationHandler, ItemOwnerHandler>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy("ItemOwner", policy => policy.Requirements.Add(new ItemOwnerRequirement()));

        return services;
    }

    public static IServiceCollection AddCategoryStatistics(this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<CategoryStatistics>(configuration.GetSection("CategoryStatistics"));
 

    public static IServiceCollection AddMediatR(this IServiceCollection services) => 
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    public static IServiceCollection AddValidators(this IServiceCollection services) =>
        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
   

    public static IServiceCollection ConfigureJwtOptions(this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

    public static IServiceCollection AddRequiredServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();

        services.AddTransient<ITokenProvider, JwtTokenProvider>();

        return services;
    }
    
}
