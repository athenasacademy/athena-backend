﻿using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using AthenasAcademy.Services.Core.Services.Interfaces;
using AthenasAcademy.Services.Core.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AthenasAcademy.Services.Core.Repositories.Interfaces;
using AthenasAcademy.Services.Core.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AthenasAcademy.Services.Core.Configurations.Mappers;

namespace AthenasAcademy.Services.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAthenasServicesDI(this IServiceCollection services)
    {
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<ICertificadoService, CertificadoService>();
        services.AddScoped<ICursoService, CursoService>();
        services.AddScoped<IInscricaoService, InscricaoService>();
        services.AddScoped<IMatriculaService, MatriculaService>();
        services.AddScoped<IPagamentoService, PagamentoService>();
        services.AddScoped<IAutorizaUsuarioService, AutorizaUsuarioService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddAthenasRepositoriesDI(this IServiceCollection services)
    {
        services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IAlunoRepository, AlunoRepository>();
        services.AddSingleton<ICertificadoRepository, CertificadoRepository>();
        services.AddSingleton<ICursoRepository, CursoRepository>();
        services.AddSingleton<IInscricaoRepository, InscricaoRepository>();
        services.AddSingleton<IMatriculaRepository, MatriculaRepository>();
        services.AddSingleton<IPagamentoRepository, PagamentoRepository>();

        return services;
    }

    public static IServiceCollection AddApiVersionamento(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                            new HeaderApiVersionReader("x-api-version"),
                                                            new MediaTypeApiVersionReader("x-api-version"));

        });

        return services;
    }

    public static IServiceCollection AddSwaggerGenDoc(this IServiceCollection services, string apiTitulo, string apiVersao)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = apiTitulo,
                Version = apiVersao,
                Description = "API para gerenciamento dos microservicos da Athenas Academy",
                Contact = new OpenApiContact
                {
                    Name = "Time de Desenvolvimento",
                    Email = "athenasacademy.flow@gmail.com",
                    Url = new Uri("https://www.athena-academy.tech")
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }

    public static IServiceCollection AddApiVersionamentoExplorer(this IServiceCollection services)
    {
        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:key"]);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerAutenticacaoJwtBearer(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            //options.SwaggerDoc(apiVersion, new OpenApiInfo { Title = apiTitle, Version = apiVersion });

            // Define o esquema de segurança Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                //Name = "Authorization",
                //Type = SecuritySchemeType.ApiKey,
                //Scheme = "Bearer",
                //BearerFormat = "JWT",
                //In = ParameterLocation.Header,

                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Header de autenticação JWT - Schema Bearer.\r\n\r\nInforme 'Bearer <token>'.\r\n\r\n"
            });

            // Define a exigência de segurança com o esquema Bearer
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddConfigureLowerCaseRoutes(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        return services;
    }

    public static IServiceCollection AddPoliciesCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("api-cors", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }
}