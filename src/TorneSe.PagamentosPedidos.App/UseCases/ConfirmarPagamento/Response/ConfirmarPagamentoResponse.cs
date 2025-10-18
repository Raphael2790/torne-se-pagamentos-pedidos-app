namespace TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Response;

/// <summary>
/// Response da confirmação de pagamento
/// </summary>
public class ConfirmarPagamentoResponse
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
    /// Indica se o pagamento foi confirmado
    /// </summary>
    public bool Confirmado { get; set; }

    /// <summary>
    /// Data e hora da confirmação
    /// </summary>
    public DateTime DataConfirmacao { get; set; }

    /// <summary>
    /// Indica se o status foi atualizado no DynamoDB com sucesso
    /// </summary>
    public bool StatusAtualizado { get; set; }

    /// <summary>
    /// Indica se a mensagem foi enviada com sucesso para a fila
    /// </summary>
    public bool MensagemEnviada { get; set; }
}
