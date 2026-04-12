using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABCD.Application {
    public interface IRecaptchaService {
        Task<bool> VerifyTokenAsync(string token);
    }

    public class RecaptchaService : IRecaptchaService {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;
        private readonly string _apiKey;
        private readonly float _scoreThreshold;

        public RecaptchaService(HttpClient httpClient, string projectId, string apiKey, float scoreThreshold = 0.5f) {
            _httpClient = httpClient;
            _projectId = projectId;
            _apiKey = apiKey;
            _scoreThreshold = scoreThreshold;

            if (string.IsNullOrEmpty(projectId)) {
                throw new ArgumentException("reCAPTCHA Enterprise Project ID is required", nameof(projectId));
            }

            if (string.IsNullOrEmpty(apiKey)) {
                throw new ArgumentException("reCAPTCHA Enterprise API Key is required", nameof(apiKey));
            }
        }

        public async Task<bool> VerifyTokenAsync(string token) {
            if (string.IsNullOrWhiteSpace(token)) {
                return false;
            }

            try {
                var assessmentUrl = $"https://recaptchaenterprise.googleapis.com/v1/projects/{_projectId}/assessments?key={_apiKey}";
                var requestBody = new {
                    @event = new {
                        token,
                        expectedAction = "login"
                    }
                };

                using var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(assessmentUrl, content);
                if (!response.IsSuccessStatusCode) {
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<EnterpriseAssessmentResponse>();
                if (result?.RiskAnalysis == null) {
                    return false;
                }

                return result.RiskAnalysis.Score >= _scoreThreshold;
            } catch {
                return false;
            }
        }

        private sealed class EnterpriseAssessmentResponse {
            [JsonPropertyName("riskAnalysis")]
            public RiskAnalysis? RiskAnalysis { get; set; }
        }

        private sealed class RiskAnalysis {
            [JsonPropertyName("score")]
            public float Score { get; set; }
        }
    }
}
