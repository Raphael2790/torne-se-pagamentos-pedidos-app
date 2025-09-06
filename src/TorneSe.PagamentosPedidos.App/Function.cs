using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TorneSe.PagamentosPedidos.App.Extensions;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Request;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TorneSe.PagamentosPedidos.App;

public class Function
{
    private readonly IMediator _mediator;
    private readonly ILogger<Function> _logger;

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        services.ConfigureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation("Processando mensagem SQS - MessageId: {MessageId}", message.MessageId);

            // Deserializar o corpo da mensagem
            var messageBody = JsonSerializer.Deserialize<Dictionary<string, object>>(message.Body);
            
            if (message.MessageAttributes.TryGetValue("evento", out var evento))
            {
                _logger.LogError("Mensagem inválida: atributo 'evento' não encontrado");
                return;
            }
            
            _logger.LogInformation("Evento identificado: {Evento}", evento);

            // Roteamento baseado no evento
            switch (evento.StringValue)
            {
                case "iniciar_pagamento":
                    await ProcessarIniciarPagamento(messageBody, context);
                    break;
                    
                case "consultar_pagamento":
                    await ProcessarConsultarPagamento(messageBody, context);
                    break;
                    
                case "cancelar_pagamento":
                    await ProcessarCancelarPagamento(messageBody, context);
                    break;
                    
                default:
                    _logger.LogWarning("Evento não reconhecido: {Evento}", evento);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem SQS - MessageId: {MessageId}", message.MessageId);
        }
    }

    private async Task ProcessarIniciarPagamento(Dictionary<string, object> messageBody, ILambdaContext context)
    {
        try
        {
            var request = new CriarPagamentoRequest
            {
                IdPedido = messageBody.GetValueOrDefault("idPedido")?.ToString(),
                Status = messageBody.GetValueOrDefault("status")?.ToString(),
                DataPedido = DateTime.TryParse(messageBody.GetValueOrDefault("dataPedido")?.ToString(), out var dataPedido) 
                    ? dataPedido : DateTime.UtcNow,
                DataHoraEvento = DateTime.TryParse(messageBody.GetValueOrDefault("dataHoraEvento")?.ToString(), out var dataHoraEvento) 
                    ? dataHoraEvento : DateTime.UtcNow
            };

            var resultado = await _mediator.Send(request);
            
            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Pagamento iniciado com sucesso - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}", 
                    request.IdPedido, resultado.Data.PaymentIntentId);
                return;
            }

            _logger.LogError("Falha ao iniciar pagamento - IdPedido: {IdPedido}, Erro: {Erro}", 
                request.IdPedido, resultado.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar iniciar pagamento");
        }
    }

    private async Task ProcessarConsultarPagamento(Dictionary<string, object> messageBody, ILambdaContext context)
    {
        try
        {
            var request = new ConsultarStatusPagamentoRequest();

            var resultado = await _mediator.Send(request);
            
            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Status consultado com sucesso - PagamentoStatus: {PagamentoStatus}, Status: {Status}", 
                    request.PagamentoStatus, resultado.Data.Status);
                return;
            }
            
            _logger.LogError("Falha ao consultar status - PagamentoStatus: {PagamentoStatus}, Erro: {Erro}", 
                request.PagamentoStatus, resultado.Message);    
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar consultar pagamento");
        }
    }

    private async Task ProcessarCancelarPagamento(Dictionary<string, object> messageBody, ILambdaContext context)
    {
        try
        {
            var request = new CancelarPagamentoRequest
            {
                PaymentIntentId = messageBody.GetValueOrDefault("paymentIntentId")?.ToString(),
                MotivoCancelamento = messageBody.GetValueOrDefault("motivoCancelamento")?.ToString() ?? "Solicitação do cliente"
            };

            var resultado = await _mediator.Send(request);
            
            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Pagamento cancelado com sucesso - PaymentIntentId: {PaymentIntentId}", 
                    request.PaymentIntentId);
                return;
            }

            _logger.LogError("Falha ao cancelar pagamento - PaymentIntentId: {PaymentIntentId}, Erro: {Erro}", 
                request.PaymentIntentId, resultado.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cancelar pagamento");
        }
    }
}