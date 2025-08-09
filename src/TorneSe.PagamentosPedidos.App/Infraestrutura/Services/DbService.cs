using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class DbService : IDbService
{
    public Task<bool> SaveAsync<T>(T entity)
    {
        throw new NotImplementedException();
    }
}