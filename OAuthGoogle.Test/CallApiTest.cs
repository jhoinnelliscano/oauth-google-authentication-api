using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace OAuthGoogle.Test
{
    public class CallApiTest
    {
        private const string ClientId = "672184144596-fbeid4gsugk3ti2acl5f9g4eu79tp85j.apps.googleusercontent.com";
        private const string ClientSecret = "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";
        private const string TokenEndpoint = "https://accounts.google.com/o/oauth2/token";
        private const string ApiUrl = "https://localhost:7228/WeatherForecast"; // 
        private const string RedirectUri = "http://localhost:7228/callback";
        private const string AuthorizationCode = "<AUTHORIZATION_CODE_FROM_GOOGLE>";

        [Fact]
        public async Task ServicioA_DevuelveListaCiudades_Exitosamente()
        {
            var token = await GetAccessTokenAsync();
            Assert.False(string.IsNullOrEmpty(token), "El token no debe ser nulo o vacío.");

            var ciudades = await CallApiAsync(token);
            Assert.NotNull(ciudades);
            Assert.NotEmpty(ciudades);
            Assert.True(ciudades.Length == 15, "El servicio debe devolver exactamente 15 ciudades.");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            using var client = new HttpClient();

            var requestBody = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("code", AuthorizationCode), // Authorization Code
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            ]);

            var response = await client.PostAsync(TokenEndpoint, requestBody);
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"Error al obtener el token: {responseContent}");

            dynamic json = JsonConvert.DeserializeObject(responseContent);
            return json?.access_token;
        }

        private async Task<string[]> CallApiAsync(string token)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(ApiUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"Error al llamar al servicio: {responseContent}");

            return JsonConvert.DeserializeObject<string[]>(responseContent);
        }
    }
}