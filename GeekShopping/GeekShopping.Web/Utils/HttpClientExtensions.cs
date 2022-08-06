using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.Web.Utils
{
    public static class HttpClientExtensions
    {

        private static MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("application/json");

        public static async Task<T> ReadContentAs<T>(this HttpResponseMessage response) {
            if (!response.IsSuccessStatusCode) throw new ApplicationException("Something went wrong." + $" {response.ReasonPhrase}");
            var dateAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<T>(
                dateAsString,
                new JsonSerializerOptions{PropertyNameCaseInsensitive = true}
                );
        }

        public static Task<HttpResponseMessage> PostAsJson<T>(this HttpClient httpClient, string url, T data)
        {
            var dateAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dateAsString);

            content.Headers.ContentType = contentType;

            return httpClient.PostAsync(url, content);
        }

        public static Task<HttpResponseMessage> PutAsJson<T>(this HttpClient httpClient, string url, T data)
        {
            var dateAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dateAsString);

            content.Headers.ContentType = contentType;

            return httpClient.PutAsync(url, content);
        }

    }
}
