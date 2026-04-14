using System.Net.Http.Json;
using System.Text.Json.Serialization;

using ABCD.Lib;

using Microsoft.Extensions.Options;

namespace ABCD.Application {
    public interface IRecaptchaService {
        Task<bool> VerifyTokenAsync(string token);
    }

    public class RecaptchaService : IRecaptchaService {
        private readonly HttpClient _httpClient;
        private readonly RecaptchaSettings _settings;

        public RecaptchaService(HttpClient httpClient, IOptions<RecaptchaSettings> settings) {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> VerifyTokenAsync(string token) {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey.StartsWith("your", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token)) {
                return false;
            }

            try {
                var parameters = new FormUrlEncodedContent([
                    new KeyValuePair<string, string>("secret", _settings.ApiKey),
                    new KeyValuePair<string, string>("response", token)
                ]);

                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", parameters);
                if (!response.IsSuccessStatusCode) {
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<SiteVerifyResponse>();
                if (result == null || !result.Success) {
                    return false;
                }

                return result.Score >= _settings.ScoreThreshold;
            } catch {
                return false;
            }
        }

        private sealed class SiteVerifyResponse {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("action")]
            public string Action { get; set; } = string.Empty;
        }
    }
}
