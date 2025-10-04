using Amazon.DynamoDBv2.DataModel;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

[DynamoDBTable("Pagamentos")]
public class PagamentoDynamoModel
{
    [DynamoDBHashKey("IdPedido")]
    public string IdPedido { get; set; }

    [DynamoDBRangeKey("PaymentIntentId")]
    public string PaymentIntentId { get; set; }

    [DynamoDBProperty("Status")]
    public string Status { get; set; }

    [DynamoDBProperty("Valor")]
    public decimal Valor { get; set; }

    [DynamoDBProperty("Moeda")]
    public string Moeda { get; set; }

    [DynamoDBProperty("DataCriacao")]
    public DateTime DataCriacao { get; set; }

    [DynamoDBProperty("DataAtualizacao")]
    public DateTime DataAtualizacao { get; set; }

    [DynamoDBProperty("UrlPagamento")]
    public string UrlPagamento { get; set; }

    [DynamoDBProperty("MetadosPagamento")]
    public string MetadosPagamento { get; set; }
}