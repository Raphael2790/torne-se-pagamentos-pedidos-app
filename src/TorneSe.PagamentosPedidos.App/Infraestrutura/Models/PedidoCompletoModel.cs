using System.Text.Json.Serialization;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

public class PedidoCompletoModel
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; }

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; }

    [JsonPropertyName("numero")]
    public string Numero { get; set; }

    [JsonPropertyName("complemento")]
    public string Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; }

    [JsonPropertyName("cidade")]
    public string Cidade { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; }

    [JsonPropertyName("cep")]
    public string Cep { get; set; }

    [JsonPropertyName("itens")]
    public List<ItemPedidoModel> Itens { get; set; } = new();

    [JsonPropertyName("formasPagamento")]
    public List<FormaPagamentoModel> FormasPagamento { get; set; } = new();
}

public class ItemPedidoModel
{
    [JsonPropertyName("nomeProduto")]
    public string NomeProduto { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("quantidade")]
    public int Quantidade { get; set; }

    [JsonPropertyName("idSku")]
    public int IdSku { get; set; }
}

public class FormaPagamentoModel
{
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("parcelas")]
    public int? Parcelas { get; set; }

    [JsonPropertyName("tokenCartao")]
    public string TokenCartao { get; set; }

    [JsonPropertyName("bandeira")]
    public string Bandeira { get; set; }

    [JsonPropertyName("chavePix")]
    public string ChavePix { get; set; }

    [JsonPropertyName("tipoChavePix")]
    public string TipoChavePix { get; set; }

    [JsonPropertyName("comprovantePix")]
    public string ComprovantePix { get; set; }
}
