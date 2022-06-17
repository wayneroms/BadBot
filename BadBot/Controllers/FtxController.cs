using FtxApi;
using FtxApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BadBot.Controllers
{
    [Route("ftx")]
    [ApiController]
    public class FtxController : ControllerBase
    {
        public readonly FtxRestApi Client;
        private readonly ILogger Logger;
        private readonly string Token;

        public FtxController(FtxRestApi ftx, IConfiguration config, ILogger<FtxController> logger)
        {
            Client = ftx;
            Logger = logger;
            Token = config["WebhookToken"];
        }

        [HttpGet("futures")]
        public Task<FtxResult<List<Future>>> GetFuturesAsync()
            => Client.GetFuturesAsync();

        [HttpGet("futures/{market}")]
        public Task<FtxResult<Future>> GetFutureAsync(string market)
            => Client.GetFutureAsync(market);

#if DEBUG

        [HttpGet("account")]
        public Task<FtxResult<AccountInfo>> GetAccountAsync([FromQuery] string? account)
        {
            Client.Account = account;
            return Client.GetAccountInfoAsync();
        }

        [HttpGet("positions")]
        public Task<FtxResult<List<Position>>> GetPositionsAsync([FromQuery] string? account)
        {
            Client.Account = account;
            return Client.GetPositionsAsync();
        }

        [HttpGet("orders/{id}")]
        public Task<FtxResult<Order>> GetOrderAsync([FromQuery] string? account, long id)
        {
            Client.Account = account;
            return Client.GetOrderAsync(id);
        }

        [HttpGet("orders")]
        public Task<FtxResult<List<Order>>> GetOpenOrdersAsync([FromQuery] string? account, [FromQuery] string market)
        {
            Client.Account = account;
            return Client.GetOpenOrdersAsync(market);
        }

        [HttpGet("orders/history")]
        public Task<FtxResult<List<Order>>> GetOrderHistoryAsync([FromQuery] string? account, [FromQuery] string market, [FromQuery] string? side, [FromQuery] string? orderType)
        {
            Client.Account = account;
            return Client.GetOrderHistoryAsync(market, side, orderType);
        }

        [HttpGet("triggers")]
        public Task<FtxResult<List<TriggerOrder>>> GetOpenTriggersAsync([FromQuery] string? account, [FromQuery] string? market, [FromQuery] string? type)
        {
            Client.Account = account;
            return Client.GetOpenTriggersAsync(market, type);
        }

        [HttpGet("triggers/history")]
        public Task<FtxResult<List<TriggerOrder>>> GetTriggerHistoryAsync([FromQuery] string? account, [FromQuery] string? market, [FromQuery] string? side, [FromQuery] string? type, [FromQuery] string? orderType)

        {
            Client.Account = account;
            return Client.GetTriggerHistoryAsync(market, side, type, orderType);
        }

#endif

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheckAsync()
        {
            try
            {
                await Client.GetAccountInfoAsync();
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

            if (request.HasAccount)
                Client.Account = request.Account;

            return
                request.IsEntry
                ? Entry(request)
                : request.IsExit
                ? Exit(request)
                : Task.CompletedTask;
        }

        private Task<Order?> Exit(WebhookRequest request)
            => Exit(request.FtxMarket, request.FtxSide, request.Type, request.LimitPriceIncrementOffset);

        private async Task<Order?> Exit(string market, string side, string type, int offset, AccountInfo? account = null)
        {
            var otherSide = SideType.Reverse(side);
            try
            {
                Logger?.LogInformation($"Cancelling {market} trigger orders.");
                await Client.CancelOrdersAsync(market, conditionalOrdersOnly: true).GetResult();

                if (account == null)
                    account = await Client.GetAccountInfoAsync().GetResult();

                var existing = account.Positions.FirstOrDefault(x => x.Future == market);

                if (existing == null || existing.Side == side || existing.Size == 0)
                {
                    Logger?.LogInformation($"Exit not required.");
                }
                else
                {
                    Logger?.LogInformation($"Closing {existing.Future} {existing.Side.ToUpper()} position.");
                    return type == OrderType.Limit
                        ? await PlaceLimitExitAsync(market, side, existing.Size, offset)
                        : await PlaceMarketExitAsync(market, side, existing.Size);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogCritical(ex, ex.GetBaseException().Message);
            }
            return null;
        }

        private async Task Entry(WebhookRequest request)
        {
            try
            {
                var market = request.FtxMarket ?? string.Empty;
                var side = request.FtxSide;
                var otherSide = SideType.Reverse(side);

                var account = await Client.GetAccountInfoAsync().GetResult();

                if (request.Swing)
                {
                    await Exit(market, otherSide, request.Type, request.LimitPriceIncrementOffset, account);
                }
                else
                {
                    var existing = account.Positions.FirstOrDefault(x => x.Future == market);
                    if (existing != null && existing.RealizedPnl != 0 && existing.Size == 0)
                    {
                        Logger?.LogInformation($"Cancelling {market} trigger orders.");
                        await Client.CancelOrdersAsync(market, conditionalOrdersOnly: true).GetResult();
                    }
                }

                var value = CalculateValue(account.Collateral, account.Leverage, request.Position, request.Value);

                var summary =
                    request.IsFloat
                    ? await PlaceFloatEntryAsync(request, value)
                    : request.IsLimit
                    ? await PlaceLimitEntryAsync(market, side, value, request.LimitPriceIncrementOffset)
                    : await PlaceMarketEntryAsync(market, side, value);

                if (summary.Size == 0)
                {
                    Logger?.LogWarning("Failed to fill order within timeout.");
                    return;
                }

                summary.StopPrice = CalculateStopPrice(side, summary.EntryPrice, request.StopPercent, request.StopPrice);
                summary.ProfitPrice = CalculateProfitPrice(side, summary.EntryPrice, request.ProfitPercent, request.ProfitPrice);

                var sl = await Client.PlaceStopMarketOrderAsync(market, otherSide, summary.StopPrice, summary.Size, request.Swing).GetResult();
                var tp = await Client.PlaceTakeProfitLimitOrderAsync(market, otherSide, summary.ProfitPrice, summary.Size, request.Swing).GetResult();

                summary.ProfitId = tp.Id;
                summary.StopId = sl.Id;

                Logger?.LogInformation(JsonSerializer.Serialize(summary));
            }
            catch (Exception ex)
            {
                Logger?.LogCritical(ex, ex.GetBaseException().Message);
            }
        }

        private async Task<Order> PlaceLimitExitAsync(string market, string side, decimal size, int offset, bool wait = false)
        {
            var future = await Client.GetFutureAsync(market).GetResult();
            var price = CalculateEntryPrice(future, side, offset);
            var result = await Client.PlaceLimitOrderAsync(market, side, price, size, true).GetResult();
            return wait
                ? await WaitUntilFilledAsync(result, TimeSpan.FromMinutes(15))
                : result;
        }

        private async Task<Order> PlaceMarketExitAsync(string market, string side, decimal size, bool wait = false)
        {
            var result = await Client.PlaceMarketOrderAsync(market, side, size, true).GetResult();
            return wait
                ? await WaitUntilFilledAsync(result, TimeSpan.FromMinutes(15))
                : result;
        }

        private async Task<OrderSummary> PlaceLimitEntryAsync(string market, string side, decimal value, decimal offset = 0)
        {
            var future = await Client.GetFutureAsync(market).GetResult();
            var price = CalculateEntryPrice(future, side, offset);
            var size = CalculateSize(value, price, future.SizeIncrement);
            var result = await Client.PlaceLimitOrderAsync(market, side, price, size).GetResult();
            var order = await WaitUntilFilledAsync(result, TimeSpan.FromMinutes(15));
            return new OrderSummary
            {
                Account = Client.Account,
                Market = market,
                Side = side,
                Size = order.FilledSize,
                EntryId = order.Id,
                EntryPrice = price
            };
        }

        private async Task<OrderSummary> PlaceMarketEntryAsync(string market, string side, decimal value)
        {
            var future = await Client.GetFutureAsync(market).GetResult();
            var price = CalculateEntryPrice(future, side, 0);
            var size = CalculateSize(value, price, future.SizeIncrement);
            var result = await Client.PlaceMarketOrderAsync(market, side, size).GetResult();
            var order = await WaitUntilFilledAsync(result, TimeSpan.FromMinutes(15));
            return new OrderSummary
            {
                Account = Client.Account,
                Market = market,
                Side = side,
                Size = order.FilledSize,
                EntryId = order.Id,
                EntryPrice = order.AvgFillPrice ?? order.Price ?? price
            };
        }

        private async Task<OrderSummary> PlaceFloatEntryAsync(WebhookRequest item, decimal value, decimal additionalSize = 0)
        {
            var future = await Client.GetFutureAsync(item.FtxMarket).GetResult();
            var entryprice = CalculateEntryPrice(future, item.FtxSide, item.LimitPriceIncrementOffset);
            var price = entryprice;
            var size = CalculateSize(value, price, future.SizeIncrement) + additionalSize;
            decimal remainingSize = size;
            decimal filledSize = 0;
            var orders = new List<Order>();
            DateTime expiry = DateTime.UtcNow.AddMinutes(15);
            var summary = new OrderSummary
            {
                Account = item.Account,
                Market = item.FtxMarket,
                Side = item.Side,
                Size = size,
                EntryPrice = price
            };
            do
            {
                var order = await Client.PlaceLimitOrderAsync(item.FtxMarket, item.FtxSide, price, remainingSize, false, false, true).GetResult();
                order = await WaitUntilFilledAsync(order, TimeSpan.FromMinutes(5), item.FloatThreshold, item.LimitPriceIncrementOffset);
                orders.Add(order);
                if (!order.Price.HasValue)
                    order.Price = price;

                filledSize = orders.Sum(x => x.FilledSize);
                remainingSize = size - filledSize;

                if (remainingSize < future.SizeIncrement)
                    break;

                future = await Client.GetFutureAsync(order.Market).GetResult();
                price = CalculateEntryPrice(future, order.Side, item.LimitPriceIncrementOffset);
            }
            while (DateTime.UtcNow < expiry && future.SizeIncrement < remainingSize);

            summary.EntryId = orders.First().Id;
            summary.Size = filledSize;
            return summary;
        }

        private async Task<Order> WaitUntilFilledAsync(Order order, TimeSpan timeout, decimal floatThreshold = 0, decimal limitOffset = 0)
        {
            if (order.RemainingSize == 0)
            {
                Logger?.LogInformation("Order filled instantly.");
                return order;
            }
            decimal price = order.Price ?? 0;
            DateTime start = DateTime.UtcNow;
            DateTime expiry = start.Add(timeout);

            do
            {
                await Task.Delay(1000);
                var update = await Client.GetOrderAsync(order.Id).GetResult();

                if (update.IsClosed || update.RemainingSize == 0 || update.FilledSize == update.Size)
                {
                    Logger?.LogInformation($"Order filled in { DateTime.UtcNow - start }.");
                    return update;
                }
                else if (order.FilledSize != update.FilledSize)
                {
                    Logger?.LogInformation($"Order {order} filled {order.FilledSize} => {update.FilledSize}.");
                }
                else
                {
                    Logger?.LogInformation($"Order unchanged.");
                }

                order = update;

                if (floatThreshold > 0 && price > 0)
                {
                    var future = await Client.GetFutureAsync(order.Market).GetResult();
                    var newPrice = CalculateEntryPrice(future, order.Side, limitOffset);
                    var aboveThreshold = AboveThreshold(order.Side, newPrice, price, floatThreshold);
                    if (aboveThreshold)
                        break;
                }

            }
            while (DateTime.UtcNow < expiry && (!order.IsClosed || order.RemainingSize > 0));

            return await EnsureClosed(order);
        }

        private async Task<Order> EnsureClosed(Order order, int retries = 10)
        {
            if (order.IsClosed)
                return order;

            var result = await Client.CancelOrderAsync(order.Id);
            if (!result.Success)
                return order;

            for (int i = 1; i <= retries; i++)
            {
                await Task.Delay(1000);
                order = await Client.GetOrderAsync(order.Id).GetResult();
                if (order.IsClosed)
                    break;
            }
            return order;
        }

#if DEBUG
        private decimal CalculateEntryPrice(Future future, string side, decimal offset)
            => !future.Ask.HasValue || !future.Bid.HasValue
            ? throw new Exception()
            : side == SideType.Buy
            ? 0.9m * (future.Bid.Value - (future.PriceIncrement * offset))
            : 1.1m * (future.Ask.Value + (future.PriceIncrement * offset));
#else
        private decimal CalculateEntryPrice(Future future, string side, decimal offset)
            => !future.Ask.HasValue || !future.Bid.HasValue
            ? throw new Exception()
            : side == SideType.Buy
            ? future.Bid.Value - (future.PriceIncrement * offset)
            : future.Ask.Value + (future.PriceIncrement * offset);
#endif

        private decimal CalculateProfitPrice(string side, decimal entryPrice, decimal profitPercent, decimal profitPrice = 0)
            => profitPrice > 0
            ? profitPrice
            : side == SideType.Buy
            ? entryPrice * (1 + (profitPercent / 100))
            : entryPrice * (1 - (profitPercent / 100));

        private decimal CalculateStopPrice(string side, decimal entryPrice, decimal stopPercent, decimal stopPrice = 0)
            => stopPrice > 0
            ? stopPrice
            : side == SideType.Buy
            ? entryPrice * (1 - (stopPercent / 100))
            : entryPrice * (1 + (stopPercent / 100));

        private decimal CalculateValue(decimal collateral, decimal leverage, decimal sizePercent, decimal sizeValue)
            => sizeValue > 0 ? sizeValue : collateral * leverage * sizePercent / 100;

        private decimal CalculateSize(decimal sizeValue, decimal price, decimal sizeIncrement)
            => Math.Floor(sizeValue / price / sizeIncrement) * sizeIncrement;

        private decimal CalculatePriceChangePercent(decimal newPrice, decimal oldPrice)
            => (newPrice - oldPrice) * 100 / oldPrice;

        private bool AboveThreshold(string side, decimal newPrice, decimal oldPrice, decimal thresholdPercent = 0.05m)
            => ((side == SideType.Buy ? 1 : -1) * CalculatePriceChangePercent(newPrice, oldPrice)) > thresholdPercent;

    }
}
