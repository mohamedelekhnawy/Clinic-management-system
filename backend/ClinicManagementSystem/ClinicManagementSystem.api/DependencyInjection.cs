namespace ClinicManagementSystem.api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddControllers();

            services
                .AddSwaggerConfig()
                .AddMapsterConfig()
                .AddFluentValidationConfig()
                .AddDIsConfig();

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

            return services;
        }
    }
}
