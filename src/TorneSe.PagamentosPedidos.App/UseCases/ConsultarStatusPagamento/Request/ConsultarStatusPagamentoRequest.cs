using MediatR;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Request;

public class ConsultarStatusPagamentoRequest : IRequest<Result<ConsultarStatusPagamentoResponse>>
{
    public string PaymentIntentId { get; set; }
}
