using Amazon.DynamoDBv2.DataModel;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

[DynamoDBTable("Pedidos")]
public class PedidoDynamoModel
{
    [DynamoDBRangeKey("Id")]
    public string Id { get; set; }

    [DynamoDBHashKey("DataPedido")]
    public string DataPedido { get; set; }

    [DynamoDBProperty("PedidoCompleto")]
    public string PedidoCompleto { get; set; }

    [DynamoDBProperty("ValorTotal")]
    public decimal ValorTotal { get; set; }

    [DynamoDBProperty("Status")]
    public string Status { get; set; }
}
