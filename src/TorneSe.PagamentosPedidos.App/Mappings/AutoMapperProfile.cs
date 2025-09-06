using AutoMapper;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PedidoDynamoModel, CriarPagamentoDto>();
    }
}