using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Services;
using TorneSe.PagamentosPedidos.App.Mappings;

namespace TorneSe.PagamentosPedidos.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPagamentoService, StripePagamentoService>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IPagamentoRepository, PagamentoRepository>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Function).Assembly));
        services.AddLogging();
        services.AddSingleton(configuration);

        return services;
    }
}