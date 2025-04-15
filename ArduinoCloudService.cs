using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace IoT.Services
{
    public class ArduinoCloudService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;

        private readonly string _clientId = "0pnRNDz6wMx7DB7FbeGyWClgun0MlyW9";
        private readonly string _clientSecret = "YL5AEhBy2lTVg8J9BLqMAOCVi0tV62giIYHZC5TS3lvGp84G3OsY5LkJZfVAzvrN";
        private readonly string _thingId = "3284f568-841e-4717-9604-cfcdb38f1e39";

        public ArduinoCloudService()
        {
            _httpClient = new HttpClient();
        }

        private async Task AuthenticateAsync()
        {
            var values = new Dictionary<string, string>
    {
        { "grant_type", "client_credentials" },
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "audience", "https://api2.arduino.cc/iot" }
    };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://api2.arduino.cc/iot/v1/clients/token", content);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔐 Token request status: {response.StatusCode}");
            Console.WriteLine($"🧾 Token response body: {json}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Token request failed. Check client_id, client_secret, and audience.");
            }

            dynamic data = JsonConvert.DeserializeObject(json);
            _accessToken = data.access_token;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }


        public async Task<List<CloudProperty>> GetAllThingPropertiesAsync()
        {
            if (string.IsNullOrEmpty(_accessToken)) await AuthenticateAsync();

            var allProperties = new List<CloudProperty>();

            var url = $"https://api2.arduino.cc/iot/v2/things/{_thingId}/properties";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Thing ID: {_thingId}, Response: {json}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Không tìm thấy Thing với ID: {_thingId}");
                return allProperties;
            }

            try
            {
                var properties = JsonConvert.DeserializeObject<List<CloudProperty>>(json);
                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        Console.WriteLine($"📦 Property: {prop.name} = {prop.last_value}");
                    }

                    allProperties.AddRange(properties);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Lỗi khi parse JSON cho Thing ID: {_thingId} - {ex.Message}");
            }

            return allProperties;
        }
        public async Task UpdateThingPropertyAsync(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(_accessToken)) await AuthenticateAsync();

            var url = $"https://api2.arduino.cc/iot/v2/things/{_thingId}/properties";
            var propsResponse = await _httpClient.GetAsync(url);
            var propsJson = await propsResponse.Content.ReadAsStringAsync();
            var props = JsonConvert.DeserializeObject<List<CloudProperty>>(propsJson);

            var targetProp = props?.FirstOrDefault(p => p.name == propertyName);
            if (targetProp == null) return;

            var propertyId = targetProp.id;
            var updateUrl = $"https://api2.arduino.cc/iot/v2/things/{_thingId}/properties/{propertyId}/publish";

            var payload = new { value = value };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(updateUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }

    // ✅ Move this outside of the class
    public class CloudProperty
    {
        public string id { get; set; }
        public string name { get; set; }
        public object last_value { get; set; } // Use object to support any value type
        public DateTime updated_at { get; set; }
    }
}
