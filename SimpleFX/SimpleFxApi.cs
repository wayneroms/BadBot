using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleFX.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SimpleFX
{
    public class SimpleFxApi
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly SimpleFxConfig _config;

        public string? Reality
        {
            get => reality ?? _config.Reality;
            set => reality = string.IsNullOrEmpty(value) ? null : value;
        }
        private string? reality = null;

        public int Account
        {
            get => account ?? _config.AccountNo;
            set => account = value > 0 ? value : null;
        }
        private int? account = null;

        internal static readonly List<AccountCache> AccountCache = new();

        public static DateTime BearerTimestamp { get; private set; } = DateTime.MinValue;
        public static string? BearerToken { get; private set; }

        public SimpleFxApi(IOptions<SimpleFxConfig> config, ILogger<SimpleFxApi> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config.Value;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("SimpleFX");
        }

        /// <summary>
        /// GET https://simplefx.com/utils/instruments.json
        /// </summary>
        public async Task<List<Instrument>> GetInstumentsAsync()
        {
            var response = await CallAsync<Dictionary<string, Instrument>>(HttpMethod.Get, "/utils/instruments.json", auth: false);
            return response.Values.ToList();
        }

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/accounts
        /// </summary>
        public Task<List<Mt4Account>> GetAccountsAsync()
            => CallResultAsync<List<Mt4Account>>(HttpMethod.Get, "/api/v3/accounts");

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/accounts/DEMO/460071
        /// </summary>
        public async Task<Mt4Account> GetAccountAsync(string? reality = null, int? account = null)
        {
            reality = reality ?? Reality;
            account = account ?? Account;
            var existing = AccountCache.FirstOrDefault(x => x.Reality == reality && x.Account == account);
            if (existing != null)
            {
                if (existing.Timestamp.AddMinutes(5) < DateTime.UtcNow)
                    AccountCache.Remove(existing);
                else
                    return existing.Mt4Account;
            }
            var result = await CallResultAsync<Mt4Account>(HttpMethod.Get, $"/api/v3/accounts/{reality}/{account}");
            AccountCache.Insert(0, new AccountCache { Account = account.Value, Reality = reality, Mt4Account = result });
            return result;
        }

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/trading/orders/active
        /// </summary>
        public Task<GetOrdersResponse> GetActiveOrdersAsync(string? reality = null, int? account = null)
        {
            var request = new GetActiveOrdersRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality
            };
            return CallAsync<GetOrdersResponse>(HttpMethod.Get, "/api/v3/trading/orders/active", request);
        }

        /// <summary>
        /// GET https://rest.simplefx.com/api/v3/trading/orders/history
        /// </summary>
        public Task<GetOrdersResponse> GetOrderHistoryAsync(DateTime from, DateTime? to = null, string? reality = null, int? account = null)
        {
            if (!to.HasValue)
                to = DateTime.Now;
            var request = new GetOrderHistoryRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                TimeFrom = (long)(from - DateTime.UnixEpoch).TotalMilliseconds,
                TimeTo = (long)(to.Value - DateTime.UnixEpoch).TotalMilliseconds,
            };
            return CallAsync<GetOrdersResponse>(HttpMethod.Get, "/api/v3/trading/orders/history", request);
        }

        /// <summary>
        /// POST https://rest.simplefx.com/api/v3/trading/orders/market
        /// </summary>
        public Task<MarketExecutionReport> PlaceMarketOrderAsync(string symbol, string side, decimal size, bool isFIFO, decimal tp, decimal sl, string? requestId = null, string? reality = null, int? account = null)
        {
            var request = new PlaceMarketOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Symbol = symbol,
                Side = side,
                Volume = size,
                IsFIFO = isFIFO,
                TakeProfit = tp,
                StopLoss = sl,
                RequestId = requestId
            };
            return CallResultAsync<MarketExecutionReport>(HttpMethod.Post, "/api/v3/trading/orders/market", request);
        }

        /// <summary>
        /// PUT https://rest.simplefx.com/api/v3/trading/orders/market
        /// </summary>
        public Task<ModifyMarketOrderResponse> ModifyMarketOrderAsync(long id, decimal tp, decimal sl, string? reality = null, int? account = null)
        {
            var request = new ModifyMarketOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Id = id,
                TakeProfit = tp,
                StopLoss = sl
            };
            return CallResultAsync<ModifyMarketOrderResponse>(HttpMethod.Post, "/api/v3/trading/orders/market", request);
        }

        /// <summary>
        /// DELETE https://rest.simplefx.com/api/v3/trading/orders/market
        /// </summary>
        public Task<ModifyMarketOrderResponse> CancelMarketOrderAsync(long id, decimal size, string? requestId = null, string? reality = null, int? account = null)
        {
            var request = new CloseMarketOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Id = id,
                Volume = size,
                RequestId = requestId
            };
            return CallResultAsync<ModifyMarketOrderResponse>(HttpMethod.Delete, "/api/v3/trading/orders/market", request);
        }

        /// <summary>
        /// DELETE https://rest.simplefx.com/api/v3/trading/orders/market/bysymbol
        /// </summary>
        public Task<ModifyMarketOrderResponse> CancelMarketOrderAsync(string symbol, string? requestId = null, string? reality = null, int? account = null)
        {
            var request = new ClosePositionRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Symbol = symbol,
                RequestId = requestId
            };
            return CallResultAsync<ModifyMarketOrderResponse>(HttpMethod.Delete, "/api/v3/trading/orders/market/bysymbol", request);
        }

        /// <summary>
        /// DELETE https://rest.simplefx.com/api/v3/trading/orders/market
        /// </summary>
        public Task<ModifyMarketOrderResponse> CancelMarketOrdersAsync(string symbol, IEnumerable<long> ids, string? requestId = null, string? reality = null, int? account = null)
        {
            var request = new CloseMarketOrdersRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Symbol = symbol,
                RequestId = requestId
            };
            request.OrderIds.AddRange(ids);
            return CallResultAsync<ModifyMarketOrderResponse>(HttpMethod.Delete, "/api/v3/trading/orders/market/closemany", request);
        }

        /// <summary>
        /// POST https://rest.simplefx.com/api/v3/trading/orders/pending
        /// </summary>
        public Task<PendingExecutionReport> PlacePendingOrderAsync(string symbol, string side, decimal size, decimal price, decimal tp, decimal sl, TimeSpan? timeout = null, string? requestId = null, string? reality = null, int? account = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromHours(1);
            var request = new PlacePendingOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Symbol = symbol,
                Side = side,
                Volume = size,
                ActivationPrice = price,
                TakeProfit = tp,
                StopLoss = sl,
                ExpiryTime = (long)(DateTime.Now.Add(timeout.Value) - DateTime.UnixEpoch).TotalMilliseconds,
                RequestId = requestId
            };
            return CallResultAsync<PendingExecutionReport>(HttpMethod.Post, "/api/v3/trading/orders/pending", request);
        }

        /// <summary>
        /// PUT https://rest.simplefx.com/api/v3/trading/orders/pending
        /// </summary>
        public Task<PendingExecutionReport> ModifyPendingOrderAsync(long id, decimal size, decimal price, decimal tp, decimal sl, TimeSpan? timeout = null, string? requestId = null, string? reality = null, int? account = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromHours(1);
            var request = new ModifyPendingOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Id = id,
                Volume = size,
                ActivationPrice = price,
                TakeProfit = tp,
                StopLoss = sl,
                ExpiryTime = (long)(DateTime.Now.Add(timeout.Value) - DateTime.UnixEpoch).TotalMilliseconds,
                RequestId = requestId
            };
            return CallResultAsync<PendingExecutionReport>(HttpMethod.Put, "/api/v3/trading/orders/pending", request);
        }

        /// <summary>
        /// PUT https://rest.simplefx.com/api/v3/trading/orders/pending
        /// </summary>
        public Task<PendingExecutionReport> CancelPendingOrderAsync(long id,  string? requestId = null, string? reality = null, int? account = null)
        {
            var request = new CancelPendingOrderRequest
            {
                Login = account ?? Account,
                Reality = reality ?? Reality,
                Id = id,
                RequestId = requestId
            };
            return CallResultAsync<PendingExecutionReport>(HttpMethod.Delete, "/api/v3/trading/orders/pending", request);
        }

        /// <summary>
        /// POST https://rest.simplefx.com/api/v3/auth/key
        /// </summary>
        public async Task Auth()
        {
            if (DateTime.UtcNow < BearerTimestamp.AddMinutes(55))
            {
                var request = new AuthRequest
                {
                    ClientId = _config.Id,
                    ClientSecret = _config.Secret
                };
                var response = await CallResultAsync<AuthResponse>(HttpMethod.Post, "/api/v3/auth/key", request);
                BearerTimestamp = DateTime.UtcNow;
                BearerToken = response.Token;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        }

        private async Task<T> CallResultAsync<T>(HttpMethod method, string endpoint, object? body = null, bool auth = true)
        {
            var result = await CallAsync(method, endpoint, JsonSerializer.Serialize(body), auth);
            return DeserializeResult<T>(result);
        }

        private async Task<T> CallAsync<T>(HttpMethod method, string endpoint, object? body = null, bool auth = true)
        {
            var result = await CallAsync(method, endpoint, JsonSerializer.Serialize(body), auth);
            return Deserialize<T>(result);
        }

        private async Task<string> CallAsync(HttpMethod method, string endpoint, string? body = null, bool auth = true)
        {
            if (auth)
                await Auth();

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

        private T DeserializeResult<T>(string message)
        {
            var result = Deserialize<Result<T>>(message);
            if (!string.IsNullOrEmpty(result.Message))
            {
                if (result.Code >= 400)
                    _logger.LogInformation(result.Message);
                else
                    _logger.LogCritical(result.Message);
            }
            if (result.Data is string)
            {
                _logger.LogInformation(result.Data.ToString());
            }
            return result.Data;
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

    }
}
