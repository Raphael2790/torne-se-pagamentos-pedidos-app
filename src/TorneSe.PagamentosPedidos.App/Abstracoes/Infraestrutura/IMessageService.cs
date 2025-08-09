using TorneSe.PagamentosPedidos.App.Domain.Messages;

namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IMessageService
{
    Task<bool> SendAsync<T>(T message, string queueUrl) where T : Message;
}