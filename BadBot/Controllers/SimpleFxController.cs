using Microsoft.AspNetCore.Mvc;
using SimpleFX;
using SimpleFX.Models;
using System.Text.Json;

namespace BadBot.Controllers
{
    [Route("sfx")]
    [ApiController]
    public class SimpleFxController : ControllerBase
    {
        public readonly SimpleFxApi Client;
        private readonly ILogger Logger;
        private readonly string Token;

        public SimpleFxController(SimpleFxApi client, IConfiguration config, ILogger<SimpleFxController> logger)
        {
            Client = client;
            Logger = logger;
            Token = config["WebhookToken"];
        }

#if DEBUG

        /// <summary>
        /// GET https://simplefx.com/utils/instruments.json
        /// </summary>
        [HttpGet("instruments")]
        public Task<List<Instrument>> GetInstumentsAsync()
            => Client.GetInstumentsAsync();

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/accounts
        /// </summary>
        [HttpGet("accounts")]
        public Task<List<Mt4Account>> GetAccountsAsync()
            => Client.GetAccountsAsync();

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/accounts/DEMO/460071
        /// </summary>
        [HttpGet("account")]
        public Task<Mt4Account> GetAccountAsync()
            => Client.GetAccountAsync(null, null);

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/accounts/DEMO/460071
        /// </summary>
        [HttpGet("account/{reality}/{account}")]
        public Task<Mt4Account> GetAccountAsync(string? reality, int? account)
            => Client.GetAccountAsync(reality, account);

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/trading/orders/active
        /// </summary>
        public Task<GetOrdersResponse> GetActiveOrdersAsync([FromQuery] string? reality, [FromQuery] int? account)
            => Client.GetActiveOrdersAsync(reality, account);

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/trading/orders/history
        /// </summary>
        public Task<GetOrdersResponse> GetOrderHistoryAsync([FromQuery] DateTime from, [FromQuery] DateTime? to, [FromQuery] string? reality, [FromQuery] int? account)
            => Client.GetOrderHistoryAsync(from, to, reality, account);

#endif

        [HttpGet("health")]
        public IActionResult HealthCheckAsync()
        {
            try
            {
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("webhook")]
        public IActionResult Webhook(WebhookRequest item)
        {
            try
            {
                return Ok();
            }
            finally
            {
                if (item == null)
                {
                    Logger.LogWarning("WebhookRequest is null.");
                }
                else if (!item.IsValid || item.Tkn != Token)
                {
                    Logger.LogWarning("WebhookRequest invalid.");
                }
                else if (item.Tkn != Token)
                {
                    Logger.LogWarning("Webhook token invalid.");
                }
                else
                {
                    Response.OnCompleted(async () => await HandleWebhook(item));
                }
            }
        }

        private Task HandleWebhook(WebhookRequest request)
        {
            Logger?.LogInformation(JsonSerializer.Serialize(request));

            return
                request.IsEntry
                ? Entry(request)
                : request.IsExit
                ? Exit(request)
                : Task.CompletedTask;
        }

        private Task Exit(WebhookRequest request)
            => Exit(request.Symbol, request.Side, request.Type, request.LimitPriceIncrementOffset);

        private Task Exit(string symbol, string side, string type, int offset)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Logger?.LogCritical(ex, ex.GetBaseException().Message);
            }
            return Task.CompletedTask;
        }

        private Task Entry(WebhookRequest request)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Logger?.LogCritical(ex, ex.GetBaseException().Message);
            }
            return Task.CompletedTask;
        }

    }
}
