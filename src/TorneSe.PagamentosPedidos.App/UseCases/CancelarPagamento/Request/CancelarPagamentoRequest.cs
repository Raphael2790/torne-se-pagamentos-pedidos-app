using MediatR;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;

public class CancelarPagamentoRequest : IRequest<Result<CancelarPagamentoResponse>>
{
    public string PaymentIntentId { get; set; }
    public string MotivoCancelamento { get; set; }
}
