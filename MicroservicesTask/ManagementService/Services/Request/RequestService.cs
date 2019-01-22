using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ManagementService.Services.Request
{
    public class RequestService : IRequestService
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public RequestService()
        {
            // Сериалайзер
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                NullValueHandling = NullValueHandling.Ignore
            };
            _serializerSettings.Converters.Add(new StringEnumConverter());
        }

        public async Task<TResult> GetAsync<TResult>(string uri)
        {
            // Создаём http-клиент
            var httpClient = CreateHttpClient();
            // Получаем ответ
            var response = await httpClient.GetAsync(uri);
            // Обрабатываем ответ
            await HandleResponse(response);
            // Сериализуем ответ в строку
            var serialized = await response.Content.ReadAsStringAsync();
            // Получаем результат путём десериализации json
            var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

            return result;
        }

        public async Task<TResult> PostAsync<TResult>(string uri)
        {
            var httpClient = CreateHttpClient();
            var response = await httpClient.PostAsync(uri, null);
            await HandleResponse(response);
            var responseData = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));
        }

        public Task<TResult> PostAsync<TResult>(string uri, TResult data)
        {
            return PostAsync<TResult, TResult>(uri, data);
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data)
        {
            var httpClient = CreateHttpClient();
            var serialized = await Task.Run(() => JsonConvert.SerializeObject(data, _serializerSettings));
            var response = await httpClient.PostAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));
            await HandleResponse(response);
            var responseData = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));
        }

        public async Task DeleteAsync(string uri)
        {
            var httpClient = CreateHttpClient();
            var response = await httpClient.DeleteAsync(uri);
            await HandleResponse(response);
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        /// <summary>
        /// Обработка ответа
        /// </summary>
        private static async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception(content);
                }

                throw new HttpRequestException(content);
            } // if
        } // HandleResponse
    }
}
