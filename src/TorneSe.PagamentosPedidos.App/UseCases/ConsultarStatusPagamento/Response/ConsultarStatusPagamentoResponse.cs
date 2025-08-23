namespace TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Response;

public class ConsultarStatusPagamentoResponse
{
    public string PaymentIntentId { get; set; }
    public string Status { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public string IdPedido { get; set; }
    public string NomeCliente { get; set; }
}
