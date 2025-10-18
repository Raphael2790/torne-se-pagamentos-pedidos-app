using MediatR;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;

public class CancelarPagamentoRequest : IRequest<Result<CancelarPagamentoResponse>>
{
    public string IdPedido { get; set; }
    public string PaymentIntentId { get; set; }
    public string MotivoCancelamento { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; }
    public DateTime DataPedido { get; set; }
}
