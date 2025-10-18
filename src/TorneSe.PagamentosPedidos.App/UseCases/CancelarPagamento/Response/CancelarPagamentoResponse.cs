namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

public class CancelarPagamentoResponse
{
    public string IdPedido { get; set; }
    public string PaymentIntentId { get; set; }
    public bool Cancelado { get; set; }
    public DateTime DataCancelamento { get; set; }
    public string MotivoCancelamento { get; set; }
    public bool StatusAtualizado { get; set; }
    public bool MensagemEnviada { get; set; }
}
