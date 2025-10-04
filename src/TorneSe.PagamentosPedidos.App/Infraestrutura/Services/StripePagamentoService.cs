using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class StripePagamentoService : IPagamentoService
{
    private readonly ILogger<StripePagamentoService> _logger;
    private readonly IConfiguration _configuration;

    public StripePagamentoService(ILogger<StripePagamentoService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Configurar a chave da API do Stripe
        var stripeApiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeApiKey))
        {
            throw new InvalidOperationException("Chave da API do Stripe não configurada");
        }

        StripeConfiguration.ApiKey = stripeApiKey;
    }

    public async Task<Result<string>> CriarPagamentoAsync(CriarPagamentoDto pagamentoDto)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(pagamentoDto.Valor * 100), // Stripe trabalha em centavos
                Currency = pagamentoDto.Moeda,
                Description = pagamentoDto.Descricao,
                ReceiptEmail = pagamentoDto.EmailCliente,
                // Configurar para captura manual - permite mais controle sobre o fluxo
                CaptureMethod = "manual",
                // Método de confirmação manual - aguarda confirmação via API
                ConfirmationMethod = "manual",
                // Métodos de pagamento permitidos
                PaymentMethodTypes = ["card", "boleto"],
                Metadata = new Dictionary<string, string>
                {
                    { "id_pedido", pagamentoDto.IdPedido },
                    { "nome_cliente", pagamentoDto.NomeCliente }
                }
            };

            // Adicionar metadados customizados se fornecidos
            foreach (var metadata in pagamentoDto.Metadados)
            {
                options.Metadata[metadata.Key] = metadata.Value;
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("PaymentIntent criado com sucesso: {PaymentIntentId} para pedido: {IdPedido}",
                paymentIntent.Id, pagamentoDto.IdPedido);

            return Result<string>.Success(paymentIntent.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento no Stripe para o pedido: {IdPedido}", pagamentoDto.IdPedido);
            return Result<string>.Error($"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar pagamento no Stripe para o pedido: {IdPedido}", pagamentoDto.IdPedido);
            return Result<string>.Error($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<PagamentoStatusDto>> ObterStatusPagamentoAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            var statusDto = new PagamentoStatusDto
            {
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Valor = paymentIntent.Amount / 100m, // Converter de centavos para reais
                Moeda = paymentIntent.Currency,
                DataCriacao = DateTime.UtcNow, // Usar data atual como fallback
                DataConfirmacao = null // Será preenchido quando o pagamento for confirmado via webhook
            };

            return Result<PagamentoStatusDto>.Success(statusDto);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao obter status do pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<PagamentoStatusDto>.Error($"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao obter status do pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<PagamentoStatusDto>.Error($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelarPagamentoAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var cancelOptions = new PaymentIntentCancelOptions
            {
                CancellationReason = "requested_by_customer"
            };

            var paymentIntent = await service.CancelAsync(paymentIntentId, cancelOptions);

            return Result<bool>.Success(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao cancelar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cancelar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ConfirmarPagamentoAsync(string paymentIntentId, string paymentMethodId)
    {
        try
        {
            var service = new PaymentIntentService();
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId,
                ReturnUrl = "https://seu-dominio.com/retorno-pagamento" // Configurar URL de retorno
            };

            var paymentIntent = await service.ConfirmAsync(paymentIntentId, confirmOptions);

            _logger.LogInformation("Pagamento confirmado: {PaymentIntentId}, Status: {Status}",
                paymentIntentId, paymentIntent.Status);

            return Result<bool>.Success(paymentIntent.Status == "succeeded" || paymentIntent.Status == "requires_capture");
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao confirmar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao confirmar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CapturarPagamentoAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var captureOptions = new PaymentIntentCaptureOptions();

            var paymentIntent = await service.CaptureAsync(paymentIntentId, captureOptions);

            _logger.LogInformation("Pagamento capturado: {PaymentIntentId}, Status: {Status}",
                paymentIntentId, paymentIntent.Status);

            return Result<bool>.Success(paymentIntent.Status == "succeeded");
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erro ao capturar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro do Stripe: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao capturar pagamento: {PaymentIntentId}", paymentIntentId);
            return Result<bool>.Error($"Erro interno: {ex.Message}");
        }
    }
}
