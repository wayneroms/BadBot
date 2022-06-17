using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FtxApi.Models;
using FtxApi.Models.LeveragedTokens;
using FtxApi.Models.Markets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FtxApi
{
    public class FtxRestApi
    {
        private readonly ILogger _logger;
        private readonly FtxConfig _config;
        private readonly HMACSHA256 _hashMaker;
        private readonly HttpClient _httpClient;
        private long _nonce;
        
        public string Account
        {
            get => account;
            set => account = string.IsNullOrEmpty(value) ? null : value;
        }
        private string account = null;
        public bool HasAccount => Account != null;

        public FtxRestApi(IOptions<FtxConfig> config, ILogger<FtxRestApi> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config.Value;
            _logger = logger;
            _hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes(_config.Secret));
            _httpClient = httpClientFactory.CreateClient("FTX");
        }

        #region Coins

        public Task<FtxResult<List<Coin>>> GetCoinsAsync()
        {
            var path = $"/api/coins";
            return CallAsync<List<Coin>>(HttpMethod.Get, path);
        }

        #endregion

        #region Futures

        public Task<FtxResult<List<Future>>> GetFuturesAsync()
        {
            var path = $"/api/futures";
            return CallAsync<List<Future>>(HttpMethod.Get, path);
        }

        public Task<FtxResult<Future>> GetFutureAsync(string future)
        {
            var path = $"/api/futures/{future}";
            return CallAsync<Future>(HttpMethod.Get, path);
        }

        public Task<FtxResult<FutureStats>> GetFutureStatsAsync(string future)
        {
            var path = $"/api/futures/{future}/stats";
            return CallAsync<FutureStats>(HttpMethod.Get, path);
        }

        public async Task<List<FundingRate>> GetFundingRatesAsync(DateTime start, DateTime end)
        {
            List<FundingRate> allResults = new List<FundingRate>();
            int resultLength;

            do
            {
                var path = $"/api/funding_rates?start_time={FtxUtil.GetSecondsFromEpochStart(start)}&end_time={FtxUtil.GetSecondsFromEpochStart(end)}";
                var result = await CallAsync<List<FundingRate>>(HttpMethod.Get, path);
                var rates = result.Result;
                resultLength = rates.Count();

                if (resultLength != 0)
                {
                    allResults.AddRange(rates);
                    end = rates.Last().Time.UtcDateTime.AddMinutes(-1); //Set the end time to the earliest retrieved to get more
                }
            }
            while (resultLength == 500);

            return allResults;
        }

        public Task<FtxResult<List<Candle>>> GetHistoricalPricesAsync(string futureName, int resolution, int limit, DateTime start, DateTime end)
        {
            var path = $"/api/futures/{futureName}/mark_candles?resolution={resolution}&limit={limit}&start_time={FtxUtil.GetSecondsFromEpochStart(start)}&end_time={FtxUtil.GetSecondsFromEpochStart(end)}";
            return CallAsync<List<Candle>>(HttpMethod.Get, path);
        }

        #endregion

        #region Markets

        public Task<FtxResult<List<Market>>> GetMarketsAsync()
        {
            var path = $"/api/markets";
            return CallAsync<List<Market>>(HttpMethod.Get, path);
        }

        public Task<FtxResult<Market>> GetSingleMarketsAsync(string marketName)
        {
            var path = $"/api/markets/{marketName}";
            return CallAsync<Market>(HttpMethod.Get, path);
        }

        public Task<FtxResult<Orderbook>> GetMarketOrderBookAsync(string marketName, int depth = 20)
        {
            var path = $"/api/markets/{marketName}/orderbook?depth={depth}";
            return CallAsync<Orderbook>(HttpMethod.Get, path);
        }

        public Task<FtxResult<List<Trade>>> GetMarketTradesAsync(string marketName, int limit, DateTime start, DateTime end)
        {
            var path = $"/api/markets/{marketName}/trades?limit={limit}&start_time={FtxUtil.GetSecondsFromEpochStart(start)}&end_time={FtxUtil.GetSecondsFromEpochStart(end)}";
            return CallAsync<List<Trade>>(HttpMethod.Get, path);
        }

        #endregion

        #region Account

        public Task<FtxResult<AccountInfo>> GetAccountInfoAsync()
        {
            var path = $"/api/account";
            return CallSignAsync<AccountInfo>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<List<Position>>> GetPositionsAsync()
        {
            var path = $"/api/positions";
            return CallSignAsync<List<Position>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<AccountLeverage>> ChangeAccountLeverageAsync(int leverage)
        {
            var body = $"{{ \"leverage\": {leverage} }}";
            var path = $"/api/account/leverage";
            return CallSignAsync<AccountLeverage>(HttpMethod.Post, path, body);
        }

        #endregion

        #region Wallet

        public async Task<FtxResult<List<Coin>>> GetCoinAsync()
        {
            var path = $"/api/wallet/coins";
            var result = await CallAsyncSign(HttpMethod.Get, path, null);
            return Deserialize<FtxResult<List<Coin>>>(result);
        }

        public async Task<FtxResult<List<Balance>>> GetBalancesAsync()
        {
            var path = $"/api/wallet/balances";
            var result = await CallAsyncSign(HttpMethod.Get, path, null);
            return Deserialize<FtxResult<List<Balance>>>(result);
        }

        public async Task<FtxResult<DepositAddress>> GetDepositAddressAsync(string coin)
        {
            var path = $"/api/wallet/deposit_address/{coin}";
            var result = await CallAsyncSign(HttpMethod.Get, path, null);
            return Deserialize<FtxResult<DepositAddress>>(result);
        }

        public async Task<FtxResult<List<DepositHistory>>> GetDepositHistoryAsync()
        {
            var path = $"/api/wallet/deposits";
            var result = await CallAsyncSign(HttpMethod.Get, path, null);
            return Deserialize<FtxResult<List<DepositHistory>>>(result);
        }

        public async Task<FtxResult<List<WithdrawalHistory>>> GetWithdrawalHistoryAsync()
        {
            var path = $"/api/wallet/withdrawals";
            var result = await CallAsyncSign(HttpMethod.Get, path, null);
            return Deserialize<FtxResult<List<WithdrawalHistory>>>(result);
        }

        public async Task<FtxResult<WithdrawalHistory>> RequestWithdrawalAsync(string coin, decimal size, string addr, string tag, string pass, string code)
        {
            var body = "{"+
                $"\"coin\": \"{coin}\"," +
                $"\"size\": {size},"+
                $"\"address\": \"{addr}\","+
                $"\"tag\": {tag},"+
                $"\"password\": \"{pass}\","+
                $"\"code\": {code}" +
                "}";

            var path = $"/api/wallet/withdrawals";
            var result = await CallAsyncSign(HttpMethod.Post, path, body);
            return Deserialize<FtxResult<WithdrawalHistory>>(result);
        }

        #endregion

        #region Orders

        public Task<FtxResult<Order>> PlaceLimitOrderAsync(string market, string side, decimal? price, decimal size, bool reduceOnly = false, bool ioc = false, bool postOnly = false, string clientId = null)
            => PlaceOrderAsync(market, side, price, OrderType.Limit, size, reduceOnly, ioc, postOnly, clientId);

        public Task<FtxResult<Order>> PlaceMarketOrderAsync(string market, string side, decimal size, bool reduceOnly = false, string clientId = null)
            => PlaceOrderAsync(market, side, null, OrderType.Market, size, reduceOnly, false, false, clientId);

        public Task<FtxResult<Order>> PlaceOrderAsync(string market, string side, decimal? price, string orderType, decimal size, bool reduceOnly = false, bool ioc = false, bool postOnly = false, string clientId = null)
        {
            var body = "{" +
                $" \"market\": \"{market}\"," +
                $" \"side\": \"{side}\"," +
                $" \"price\": {price?.ToString() ?? "null"}," +
                $" \"type\": \"{orderType}\"," +
                $" \"size\": {size}," +
                (!string.IsNullOrEmpty(clientId) ? $" \"clientId\": \"{clientId}\"," : string.Empty) +
                $" \"reduceOnly\": {reduceOnly.ToString().ToLower()}," +
                $" \"ioc\": {ioc.ToString().ToLower()}," +
                $" \"postOnly\": {postOnly.ToString().ToLower()}" +
                " }";

            var path = $"/api/orders";
            return CallSignAsync<Order>(HttpMethod.Post, path, body);
        }

        public Task<FtxResult<TriggerOrder>> PlaceTakeProfitLimitOrderAsync(string market, string side, decimal price, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
            => PlaceTakeProfitOrderAsync(market, side, price, price, size, reduceOnly, retryUntilFilled);

        public Task<FtxResult<TriggerOrder>> PlaceTakeProfitMarketOrderAsync(string market, string side, decimal price, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
            => PlaceTakeProfitOrderAsync(market, side, price, null, size, reduceOnly, retryUntilFilled);

        public Task<FtxResult<TriggerOrder>> PlaceTakeProfitOrderAsync(string market, string side, decimal triggerPrice, decimal? orderPrice, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
        {
            var body = "{" +
                $" \"market\": \"{market}\"," +
                $" \"side\": \"{side}\"," +
                $" \"triggerPrice\": {triggerPrice}," +
                (orderPrice.HasValue ? $" \"price\": {orderPrice}," : string.Empty) +
                $" \"size\": {size}," +
                $" \"type\": \"takeProfit\"," +
                $" \"reduceOnly\": {reduceOnly.ToString().ToLower()}," +
                $" \"retryUntilFilled\": {retryUntilFilled.ToString().ToLower()}" +
                " }";

            var path = $"/api/conditional_orders";
            return CallSignAsync<TriggerOrder>(HttpMethod.Post, path, body);
        }

        public Task<FtxResult<TriggerOrder>> PlaceStopLimitOrderAsync(string market, string side, decimal price, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
            => PlaceStopOrderAsync(market, side, price, null, size, reduceOnly, retryUntilFilled);

        public Task<FtxResult<TriggerOrder>> PlaceStopMarketOrderAsync(string market, string side, decimal price, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
            => PlaceStopOrderAsync(market, side, price, null, size, reduceOnly, retryUntilFilled);

        public Task<FtxResult<TriggerOrder>> PlaceStopOrderAsync(string market, string side, decimal triggerPrice, decimal? orderPrice, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
        {
            
            var body = "{" +
                $" \"market\": \"{market}\"," +
                $" \"side\": \"{side}\"," +
                $" \"triggerPrice\": {triggerPrice}," +
                (orderPrice.HasValue ? $" \"price\": {orderPrice}," : string.Empty) +
                $" \"size\": {size}," +
                $" \"type\": \"stop\"," +
                $" \"reduceOnly\": {reduceOnly.ToString().ToLower()}," +
                $" \"retryUntilFilled\": {retryUntilFilled.ToString().ToLower()}" +
                " }";

            var path = $"/api/conditional_orders";
            return CallSignAsync<TriggerOrder>(HttpMethod.Post, path, body);
        }

        public Task<FtxResult<TriggerOrder>> PlaceTrailingStopOrderAsync(string market, string side, decimal trailValue, decimal size, bool reduceOnly = false, bool retryUntilFilled = true)
        {
            var body = "{" +
                $" \"market\": \"{market}\"," +
                $" \"side\": \"{side}\"," +
                $" \"trailValue\": {trailValue}," +
                $" \"size\": {size}," +
                $" \"type\": \"trailingStop\"," +
                $" \"reduceOnly\": {reduceOnly.ToString().ToLower()}," +
                $" \"retryUntilFilled\": {retryUntilFilled.ToString().ToLower()}" +
                " }";

            var path = $"/api/conditional_orders";
            return CallSignAsync<TriggerOrder>(HttpMethod.Post, path, body);
        }

        public Task<FtxResult<List<Order>>> GetOpenOrdersAsync(string market)
        {
            var marketQuery = string.IsNullOrEmpty(market) ? string.Empty : "?market=" + market;
            var path = $"/api/orders{marketQuery}";
            return CallSignAsync<List<Order>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<List<TriggerOrder>>> GetOpenTriggersAsync(string market = null, string type = null)
        {
            var marketQuery = string.IsNullOrEmpty(market) ? string.Empty : "market=" + market;
            var typeQuery = string.IsNullOrEmpty(type) ? string.Empty : "type=" + type;
            var path = $"/api/conditional_orders?{marketQuery}&{typeQuery}";
            return CallSignAsync<List<TriggerOrder>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<List<Order>>> GetOrderHistoryAsync(string market, string side = null, string orderType = null)
        {
            var marketQuery = string.IsNullOrEmpty(market) ? string.Empty : "market=" + market;
            var sideQuery = string.IsNullOrEmpty(side) ? string.Empty : "side=" + side;
            var orderQuery = string.IsNullOrEmpty(orderType) ? string.Empty : "orderType=" + orderType;
            var path = $"/api/orders/history?{marketQuery}&{sideQuery}&{orderQuery}";
            return CallSignAsync<List<Order>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<List<TriggerOrder>>> GetTriggerHistoryAsync(string market = null, string side = null, string orderType = null, string type = null)
        {
            var marketQuery = string.IsNullOrEmpty(market) ? string.Empty : "market=" + market;
            var sideQuery = string.IsNullOrEmpty(side) ? string.Empty : "side=" + side;
            var typeQuery = string.IsNullOrEmpty(type) ? string.Empty : "type=" + type;
            var orderQuery = string.IsNullOrEmpty(orderType) ? string.Empty : "orderType=" + orderType;
            var path = $"/api/conditional_orders/history?{marketQuery}&{sideQuery}&{typeQuery}&{orderQuery}";
            return CallSignAsync<List<TriggerOrder>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<Order>> GetOrderAsync(long id)
        {
            var path = $"/api/orders/{id}";
            return CallSignAsync<Order>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<Order>> GetOrderByClientIdAsync(string clientOrderId)
        {
            var path = $"/api/orders/by_client_id/{clientOrderId}";
            return CallSignAsync<Order>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<string>> CancelOrderAsync(long id)
        {
            var path = $"/api/orders/{id}";
            return CallSignAsync<string>(HttpMethod.Delete, path, null);
        }

        public Task<FtxResult<string>> CancelOrderByClientIdAsync(string clientOrderId)
        {
            var path = $"/api/orders/by_client_id/{clientOrderId}";
            return CallSignAsync<string>(HttpMethod.Delete, path, null);
        }

        public Task<FtxResult<string>> CancelTriggerOrderAsync(long id)
        {
            var path = $"/api/conditional_orders/{id}";
            return CallSignAsync<string>(HttpMethod.Delete, path, null);
        }

        public Task<FtxResult<string>> CancelOrdersAsync(string market, string side = null, bool conditionalOrdersOnly = false, bool limitOrdersOnly = false)
        {
            var body = "{" +
                (string.IsNullOrEmpty(market) ? string.Empty : $" \"market\": \"{market}\",") +
                (string.IsNullOrEmpty(side) ? string.Empty : $" \"side\": \"{side}\",") +
                $" \"conditionalOrdersOnly\": {conditionalOrdersOnly.ToString().ToLower()}," +
                $" \"limitOrdersOnly\": {limitOrdersOnly.ToString().ToLower()}" +
                " }";

            var path = $"/api/orders";
            return CallSignAsync<string>(HttpMethod.Delete, path, body);
        }

        #endregion

        #region Fills

        public Task<FtxResult<List<Fill>>> GetFillsAsync(string market, int limit, DateTime start, DateTime end)
        {
            var path = $"/api/fills?market={market}&limit={limit}&start_time={FtxUtil.GetSecondsFromEpochStart(start)}&end_time={FtxUtil.GetSecondsFromEpochStart(end)}";
            return CallSignAsync<List<Fill>>(HttpMethod.Get, path, null);
        }

        #endregion

        #region Funding

        public Task<FtxResult<List<FundingPayment>>> GetFundingPaymentAsync(DateTime start, DateTime end)
        {
            var path = $"/api/funding_payments?start_time={FtxUtil.GetSecondsFromEpochStart(start)}&end_time={FtxUtil.GetSecondsFromEpochStart(end)}";
            return CallSignAsync<List<FundingPayment>>(HttpMethod.Get, path, null);
        }

        #endregion

        #region Leveraged Tokens

        public Task<FtxResult<List<LeveragedToken>>> GetLeveragedTokensListAsync()
        {
            var path = $"/api/lt/tokens";
            return CallAsync<List<LeveragedToken>>(HttpMethod.Get, path);
        }

        public Task<FtxResult<LeveragedToken>> GetTokenInfoAsync(string tokenName)
        {
            var path = $"/api/lt/{tokenName}";
            return CallAsync<LeveragedToken>(HttpMethod.Get, path);
        }

        public Task<FtxResult<List<LeveragedTokenBalance>>> GetLeveragedTokenBalancesAsync()
        {
            var path = $"/api/lt/balances";
            return CallSignAsync<List<LeveragedTokenBalance>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<List<LeveragedTokenCreation>>> GetLeveragedTokenCreationListAsync()
        {
            var path = $"/api/lt/creations";
            return CallSignAsync<List<LeveragedTokenCreation>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<LeveragedTokenCreationRequest>> RequestLeveragedTokenCreationAsync(string tokenName, decimal size)
        {
            var body = $"{{ \"size\": {size} }}";
            var path = $"/api/lt/{tokenName}/create";
            return CallSignAsync<LeveragedTokenCreationRequest>(HttpMethod.Post, path, body);
        }

        public Task<FtxResult<List<LeveragedTokenRedemptionRequest>>> GetLeveragedTokenRedemptionListAsync()
        {
            var path = $"/api/lt/redemptions";
            return CallSignAsync<List<LeveragedTokenRedemptionRequest>>(HttpMethod.Get, path, null);
        }

        public Task<FtxResult<LeveragedTokenRedemption>> RequestLeveragedTokenRedemptionAsync(string tokenName, decimal size)
        {
            var body = $"{{ \"size\": {size} }}";
            var path = $"/api/lt/{tokenName}/redeem";
            return CallSignAsync<LeveragedTokenRedemption>(HttpMethod.Post, path, body);
        }

        #endregion

        #region Util

        private async Task<FtxResult<T>> CallAsync<T>(HttpMethod method, string endpoint, string body = null)
        {
            var result = await CallAsync(method, endpoint, body);
            return Deserialize<FtxResult<T>>(result);
        }

        private async Task<string> CallAsync(HttpMethod method, string endpoint, string body = null)
        {
            _logger?.LogInformation($"{method} {_httpClient.BaseAddress}{endpoint}");
            if (body != null)
                _logger?.LogInformation(body);
            
            var request = new HttpRequestMessage(method, endpoint);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        private async Task<FtxResult<T>> CallSignAsync<T>(HttpMethod method, string endpoint, string body = null)
        {
            var ressponse = await CallAsyncSign(method, endpoint, body);
            var result = DeserializeFtxResult<T>(ressponse);
            return result;
        }

        private async Task<string> CallAsyncSign(HttpMethod method, string endpoint, string body = null, string account = null)
        {
            _logger?.LogInformation($"{method} {_httpClient.BaseAddress}{endpoint}");
            if (body != null)
                _logger?.LogInformation(body);

            var sign = GenerateSignature(method, endpoint, body ?? string.Empty);

            var request = new HttpRequestMessage(method, endpoint);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            request.Headers.Add("FTX-KEY", _config.ApiKey);
            request.Headers.Add("FTX-SIGN", sign);
            request.Headers.Add("FTX-TS", _nonce.ToString());

            if (!string.IsNullOrEmpty(account))
                request.Headers.Add("FTX-SUBACCOUNT", account);
            else if (HasAccount)
                request.Headers.Add("FTX-SUBACCOUNT", Account);
            else if (_config.HasAccount)
                request.Headers.Add("FTX-SUBACCOUNT", _config.Account);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        private FtxResult<T> DeserializeFtxResult<T>(string message)
        {
            var result = Deserialize<FtxResult<T>>(message);
            if (!string.IsNullOrEmpty(result.Error))
            {
                if (result.Success)
                    _logger.LogInformation(result.Error);
                else
                    _logger.LogCritical(result.Error);
            }
            if (result.Result is string)
            {
                _logger.LogInformation(result.Result.ToString());
            }
            return result;
        }

        private T Deserialize<T>(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, ex.GetBaseException().Message);
                _logger?.LogCritical(message);
                throw;
            }
        }

        private string GenerateSignature(HttpMethod method, string url, string requestBody)
        {
            _nonce = GetNonce();
            var signature = $"{_nonce}{method.ToString().ToUpper()}{url}{requestBody}";
            var hash = _hashMaker.ComputeHash(Encoding.UTF8.GetBytes(signature));
            var hashStringBase64 = BitConverter.ToString(hash).Replace("-", string.Empty);
            return hashStringBase64.ToLower();
        }

        private long GetNonce()
        {
            return FtxUtil.GetMillisecondsFromEpochStart();
        }

        #endregion
    }
}