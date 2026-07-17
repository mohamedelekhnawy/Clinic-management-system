using ClinicManagementSystem.api.Persistence;
using Microsoft.Extensions.Configuration;

namespace ClinicManagementSystem.api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
        {

            services.AddControllers();


            services
                .AddSwaggerConfig()
                .AddMapsterConfig()
                .AddFluentValidationConfig()
                .AddDIsConfig()
                .AddDbConfig(configuration);

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
            services.AddSwaggerGen();

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
            services.AddScoped<IClinicService, ClinicService>();
            services.AddScoped<IDoctorService, DoctorService>();

            return services;
        }
    }
}
