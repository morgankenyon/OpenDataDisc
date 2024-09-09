using Microsoft.Extensions.DependencyInjection;
using OpenDataDisc.Services.Interfaces;

namespace OpenDataDisc.Services
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddOpenDataDiscServices(this IServiceCollection services)
        {
            services.AddTransient<IDataSchemaService, DataSchemaService>();
            services.AddTransient<ISensorService, SensorService>();
            return services;
        }
    }
}
