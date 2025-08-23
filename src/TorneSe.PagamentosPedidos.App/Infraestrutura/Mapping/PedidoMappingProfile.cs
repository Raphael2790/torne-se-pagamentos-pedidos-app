using AutoMapper;
using System.Text.Json;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Mapping;

public class PedidoMappingProfile : Profile
{
    public PedidoMappingProfile()
    {
        CreateMap<PedidoDynamoModel, CriarPagamentoDto>()
            .ForMember(dest => dest.IdPedido, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.ValorTotal))
            .ForMember(dest => dest.Moeda, opt => opt.MapFrom(src => "brl"))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => $"Pagamento do pedido {src.Id}"))
            .ForMember(dest => dest.EmailCliente, opt => opt.MapFrom(src => ObterEmailCliente(src.PedidoCompleto)))
            .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src => ObterNomeCliente(src.PedidoCompleto)))
            .ForMember(dest => dest.Metadados, opt => opt.MapFrom(src => ObterMetadados(src)));
    }

    private static string ObterEmailCliente(string pedidoCompletoJson)
    {
        try
        {
            var pedidoCompleto = JsonSerializer.Deserialize<PedidoCompletoModel>(pedidoCompletoJson);
            return pedidoCompleto?.Email ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ObterNomeCliente(string pedidoCompletoJson)
    {
        try
        {
            var pedidoCompleto = JsonSerializer.Deserialize<PedidoCompletoModel>(pedidoCompletoJson);
            return pedidoCompleto?.Nome ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static Dictionary<string, string> ObterMetadados(PedidoDynamoModel pedido)
    {
        var metadados = new Dictionary<string, string>
        {
            { "id_pedido", pedido.Id },
            { "data_pedido", pedido.DataPedido },
            { "status_pedido", pedido.Status },
            { "valor_total", pedido.ValorTotal.ToString("F2") }
        };

        try
        {
            var pedidoCompleto = JsonSerializer.Deserialize<PedidoCompletoModel>(pedido.PedidoCompleto);
            
            if (pedidoCompleto != null)
            {
                metadados.Add("nome_cliente", pedidoCompleto.Nome);
                metadados.Add("email_cliente", pedidoCompleto.Email);
                metadados.Add("telefone_cliente", pedidoCompleto.Telefone);
                metadados.Add("cidade_cliente", pedidoCompleto.Cidade);
                metadados.Add("estado_cliente", pedidoCompleto.Estado);
                metadados.Add("quantidade_itens", pedidoCompleto.Itens?.Count.ToString() ?? "0");
                metadados.Add("quantidade_formas_pagamento", pedidoCompleto.FormasPagamento?.Count.ToString() ?? "0");
            }
        }
        catch
        {
            // Se não conseguir deserializar, mantém apenas os metadados básicos
        }

        return metadados;
    }
}
