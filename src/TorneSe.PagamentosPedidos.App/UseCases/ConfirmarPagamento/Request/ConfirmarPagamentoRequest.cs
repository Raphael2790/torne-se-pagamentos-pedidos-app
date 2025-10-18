using MediatR;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Request;

/// <summary>
/// Request para confirmar pagamento
/// </summary>
public class ConfirmarPagamentoRequest : IRequest<Result<ConfirmarPagamentoResponse>>
{
    /// <summary>
    /// Identificador do pedido
    /// </summary>
    public string IdPedido { get; set; }

    /// <summary>
    /// Identificador do PaymentIntent no Stripe
    /// </summary>
    public string PaymentIntentId { get; set; }

    /// <summary>
    /// Valor do pagamento
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Moeda do pagamento
    /// </summary>
    public string Moeda { get; set; }

    /// <summary>
    /// Data do pedido
    /// </summary>
    public DateTime DataPedido { get; set; }
}
