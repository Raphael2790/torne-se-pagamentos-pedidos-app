namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IDbService
{
    Task<bool> SaveAsync<T>(T entity);
}