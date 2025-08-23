using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class StripeWebhookService
{
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPagamentoService _pagamentoService;

    public StripeWebhookService(
        ILogger<StripeWebhookService> logger, 
        IConfiguration configuration,
        IPagamentoService pagamentoService)
    {
        _logger = logger;
        _configuration = configuration;
        _pagamentoService = pagamentoService;
    }

    public async Task<bool> ProcessarWebhookAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("Webhook secret não configurado");
                return false;
            }

            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            _logger.LogInformation("Webhook recebido: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await ProcessarPagamentoConfirmado(stripeEvent.Data.Object as PaymentIntent);
                    break;
                    
                case "payment_intent.payment_failed":
                    await ProcessarPagamentoFalhou(stripeEvent.Data.Object as PaymentIntent);
                    break;
                    
                case "payment_intent.canceled":
                    await ProcessarPagamentoCancelado(stripeEvent.Data.Object as PaymentIntent);
                    break;
                    
                default:
                    _logger.LogInformation("Evento não processado: {EventType}", stripeEvent.Type);
                    break;
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Stripe");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar webhook");
            return false;
        }
    }

    private Task ProcessarPagamentoConfirmado(PaymentIntent paymentIntent)
    {
        if (paymentIntent == null) return Task.CompletedTask;

        _logger.LogInformation("Pagamento confirmado: {PaymentIntentId}", paymentIntent.Id);
        
        // Aqui você pode implementar a lógica para atualizar o status do pedido
        // Por exemplo, enviar uma mensagem para uma fila SQS
        // await _messageService.SendAsync(new PagamentoConfirmadoMessage { PaymentIntentId = paymentIntent.Id }, queueUrl);
        
        return Task.CompletedTask;
    }

    private Task ProcessarPagamentoFalhou(PaymentIntent paymentIntent)
    {
        if (paymentIntent == null) return Task.CompletedTask;

        _logger.LogWarning("Pagamento falhou: {PaymentIntentId}", paymentIntent.Id);
        
        // Aqui você pode implementar a lógica para tratar falha de pagamento
        // Por exemplo, enviar uma mensagem para uma fila SQS
        // await _messageService.SendAsync(new PagamentoFalhouMessage { PaymentIntentId = paymentIntent.Id }, queueUrl);
        
        return Task.CompletedTask;
    }

    private Task ProcessarPagamentoCancelado(PaymentIntent paymentIntent)
    {
        if (paymentIntent == null) return Task.CompletedTask;

        _logger.LogInformation("Pagamento cancelado: {PaymentIntentId}", paymentIntent.Id);
        
        // Aqui você pode implementar a lógica para tratar cancelamento de pagamento
        // Por exemplo, enviar uma mensagem para uma fila SQS
        // await _messageService.SendAsync(new PagamentoCanceladoMessage { PaymentIntentId = paymentIntent.Id }, queueUrl);
        
        return Task.CompletedTask;
    }
}
