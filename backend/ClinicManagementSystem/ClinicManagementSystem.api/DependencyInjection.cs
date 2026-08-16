using ClinicManagementSystem.api.Authentication;
using ClinicManagementSystem.api.Persistence;
using ClinicManagementSystem.api.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ClinicManagementSystem.api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
        {

            services.AddControllers();
            services.AddAuthConfig(configuration);
            services.AddCorsConfig(configuration);

            services
                .AddSwaggerConfig()
                .AddMapsterConfig()
                .AddFluentValidationConfig()
                .AddDIsConfig()
                .AddDbConfig(configuration);

            return services;
        }

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            return app;
        }

        public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            return services;
        }
        public static IServiceCollection AddDbConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Clinic Management System API",
                    Version = "v1",
                    Description = "API for managing clinics, doctors, and appointments",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Clinic Management System",
                        Email = "support@clinicmanagement.com"
                    }
                });

                // Add JWT Authentication
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
        public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(mappingConfig));

            return services;
        }
        public static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
                .AddFluentValidationAutoValidation();

            return services;
        }
        public static IServiceCollection AddDIsConfig(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IClinicService, ClinicService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAssistantService, AssistantService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
        public static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IJwtProvider, JwtProvider>();
            
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            // Runtime validation
            if (string.IsNullOrEmpty(jwtSettings?.Key))
                throw new InvalidOperationException("JWT Key is not configured. Please set it in User Secrets or Environment Variables.");

            if (jwtSettings.Key.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters long.");

            if (string.IsNullOrEmpty(jwtSettings.Issuer))
                throw new InvalidOperationException("JWT Issuer is not configured.");

            if (string.IsNullOrEmpty(jwtSettings.Audience))
                throw new InvalidOperationException("JWT Audience is not configured.");

            if (jwtSettings.ExpirationInMinutes < 1 || jwtSettings.ExpirationInMinutes > 10080)
                throw new InvalidOperationException("JWT ExpirationInMinutes must be between 1 and 10080 (7 days).");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
