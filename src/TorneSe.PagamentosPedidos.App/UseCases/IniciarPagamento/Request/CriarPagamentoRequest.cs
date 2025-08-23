using MediatR;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Request;

public class CriarPagamentoRequest : IRequest<Result<CriarPagamentoResponse>>
{
    public string IdPedido { get; set; }
    public string Status { get; set; }
    public DateTime DataPedido { get; set; }
    public DateTime DataHoraEvento { get; set; }
}