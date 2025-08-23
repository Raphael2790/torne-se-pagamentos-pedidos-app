using TorneSe.PagamentosPedidos.App.Common;

namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IPagamentoService
{
    Task<Result<string>> CriarPagamentoAsync(CriarPagamentoDto pagamentoDto);
    Task<Result<PagamentoStatusDto>> ObterStatusPagamentoAsync(string paymentIntentId);
    Task<Result<bool>> CancelarPagamentoAsync(string paymentIntentId);
}

public class CriarPagamentoDto
{
    public string IdPedido { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "brl";
    public string Descricao { get; set; }
    public string EmailCliente { get; set; }
    public string NomeCliente { get; set; }
    public Dictionary<string, string> Metadados { get; set; } = new();
}

public class PagamentoStatusDto
{
    public string PaymentIntentId { get; set; }
    public string Status { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConfirmacao { get; set; }
}
