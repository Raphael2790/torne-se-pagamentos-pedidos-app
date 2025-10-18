namespace TorneSe.PagamentosPedidos.App.Domain.Messages;

/// <summary>
/// Mensagem para notificar a confirmação de um pagamento
/// </summary>
public class PagamentoConfirmadoMessage : Message
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
    /// Status do pagamento
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Valor do pagamento
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Moeda do pagamento
    /// </summary>
    public string Moeda { get; set; }

    /// <summary>
    /// Data e hora da confirmação
    /// </summary>
    public DateTime DataConfirmacao { get; set; }

    /// <summary>
    /// Data de criação do pedido
    /// </summary>
    public DateTime DataPedido { get; set; }
}
