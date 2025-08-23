namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

public class CancelarPagamentoResponse
{
    public string PaymentIntentId { get; set; }
    public bool Cancelado { get; set; }
    public DateTime DataCancelamento { get; set; }
    public string MotivoCancelamento { get; set; }
}
