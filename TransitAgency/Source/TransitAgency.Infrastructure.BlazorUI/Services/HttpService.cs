using TransitAgency.Infrastructure.BlazorUI.Helpers;
using TransitAgency.Infrastructure.BlazorUI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TransitAgency.Infrastructure.BlazorUI.Services
{

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        UPLOAD
    }

    public interface IHttpService
    {
        Task<ApiResponse> GetAsync(string requestUri);
        Task<ApiResponse> PostAsync(string requestUri, object data);
        Task<ApiResponse> PutAsync(string requestUri, object data);
        Task<ApiResponse> UploadAsync(string requestUri, object data);
        Task<ApiResponse> DeleteAsync(string requestUri);

        Task<ApiResponse<T>> GetAsync<T>(string requestUri);
        Task<ApiResponse<T>> PostAsync<T>(string requestUri, object data);
        Task<ApiResponse<T>> PutAsync<T>(string requestUri, object data);
        Task<ApiResponse<T>> UploadAsync<T>(string requestUri, object data);
        Task<ApiResponse<T>> DeleteAsync<T>(string requestUri);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorageService;
        private readonly IConfiguration _configuration;
        private readonly string _userKey = "user";

        private JsonSerializerOptions defaultJsonSerializerOptions =>
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        public HttpService(HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            IConfiguration configuration)
        {


            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            _configuration = configuration;
        }

        public async Task<ApiResponse> GetAsync(string requestUri)
        {
            return await ExecuteAsync(requestUri, HttpMethod.GET);
        }

        public async Task<ApiResponse> PostAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, HttpMethod.POST, data);
        }

        public async Task<ApiResponse> PutAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, HttpMethod.PUT, data);
        }

        public async Task<ApiResponse> UploadAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, HttpMethod.UPLOAD, data);
        }

        public async Task<ApiResponse> DeleteAsync(string requestUri)
        {
            return await ExecuteAsync(requestUri, HttpMethod.DELETE);
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string requestUri)
        {
            return await ExecuteAsync<T>(requestUri, HttpMethod.GET);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string requestUri, object data)
        {
            return await ExecuteAsync<T>(requestUri, HttpMethod.POST, data);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string requestUri, object data)
        {
            return await ExecuteAsync<T>(requestUri, HttpMethod.PUT, data);
        }

        public async Task<ApiResponse<T>> UploadAsync<T>(string requestUri, object data)
        {
            return await ExecuteAsync<T>(requestUri, HttpMethod.UPLOAD, data);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string requestUri)
        {
            return await ExecuteAsync<T>(requestUri, HttpMethod.DELETE);
        }

        private async Task<ApiResponse> ExecuteAsync(string requestUri, HttpMethod httpMethod, object data = null)
        {
            try
            {
                var httpResponseMessage = await GetHttpResponseMessage(requestUri, httpMethod, data);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await Deserialize<ApiResponse>(httpResponseMessage, defaultJsonSerializerOptions);
                }
                else
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized) // token expired
                    {
                        await _localStorageService.RemoveItem(_userKey);
                    }

                    var response = new ApiResponse();

                    response.AddError(httpResponseMessage.ReasonPhrase, await httpResponseMessage.Content.ReadAsStringAsync());

                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                var response = new ApiResponse();

                response.AddError(ex);

                return response;
            }
        }

        private async Task<ApiResponse<T>> ExecuteAsync<T>(string requestUri, HttpMethod httpMethod, object data = null)
        {
            try
            {
                var httpResponseMessage = await GetHttpResponseMessage(requestUri, httpMethod, data);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await Deserialize<ApiResponse<T>>(httpResponseMessage, defaultJsonSerializerOptions);
                }
                else
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized) // token expired
                    {
                        await _localStorageService.RemoveItem(_userKey);
                    }

                    var response = new ApiResponse<T>();

                    response.AddError(httpResponseMessage.ReasonPhrase, await httpResponseMessage.Content.ReadAsStringAsync());

                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                var response = new ApiResponse<T>();

                response.AddError(ex);

                return response;
            }
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessage(string requestUri, HttpMethod httpMethod, object data = null)
        {
            string dataJson = null;
            if (data != null && httpMethod != HttpMethod.UPLOAD)
            {
                dataJson = JsonSerializer.Serialize(data);
            }

            HttpResponseMessage httpResponseMessage;

            if (await _localStorageService.ContainsKeyAsync(_userKey))
            {
                var user = await _localStorageService.GetItem<User>(_userKey);
                if (user != null)
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", user.Token);
            }

            switch (httpMethod)
            {
                case HttpMethod.GET:
                    httpResponseMessage = await _httpClient.GetAsync(requestUri);
                    break;
                case HttpMethod.POST:
                    httpResponseMessage = await _httpClient.PostAsync(requestUri, new StringContent(dataJson, Encoding.UTF8, "application/json"));
                    break;
                case HttpMethod.PUT:
                    httpResponseMessage = await _httpClient.PutAsync(requestUri, new StringContent(dataJson, Encoding.UTF8, "application/json"));
                    break;
                case HttpMethod.UPLOAD:
                    httpResponseMessage = await _httpClient.PostAsync(requestUri, (HttpContent)data);
                    break;
                case HttpMethod.DELETE:
                    httpResponseMessage = await _httpClient.DeleteAsync(requestUri);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Utility code not implemented for this Http method {0}", httpMethod));
            }

            return httpResponseMessage;
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage httpResponse, JsonSerializerOptions options)
        {
            var responseString = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                return JsonSerializer.Deserialize<T>(responseString, options);
            }

            return default;
        }
    }

    /*
     public interface IHttpService
     {
         Task<T> Get<T>(string uri);
         Task Post(string uri, object value);
         Task<T> Post<T>(string uri, object value);
         Task Put(string uri, object value);
         Task<T> Put<T>(string uri, object value);
         Task Delete(string uri);
         Task<T> Delete<T>(string uri);
     }

     public class HttpService : IHttpService
     {
         private HttpClient _httpClient;
         private NavigationManager _navigationManager;
         private ILocalStorageService _localStorageService;
         private IConfiguration _configuration;

         public HttpService(
             HttpClient httpClient,
             NavigationManager navigationManager,
             ILocalStorageService localStorageService,
             IConfiguration configuration
         ) {
             _httpClient = httpClient;
             _navigationManager = navigationManager;
             _localStorageService = localStorageService;
             _configuration = configuration;
         }

         public async Task<T> Get<T>(string uri)
         {
             var request = new HttpRequestMessage(HttpMethod.Get, uri);
             return await sendRequest<T>(request);
         }

         public async Task Post(string uri, object value)
         {
             var request = createRequest(HttpMethod.Post, uri, value);
             await sendRequest(request);
         }

         public async Task<T> Post<T>(string uri, object value)
         {
             var request = createRequest(HttpMethod.Post, uri, value);
             return await sendRequest<T>(request);
         }

         public async Task Put(string uri, object value)
         {
             var request = createRequest(HttpMethod.Put, uri, value);
             await sendRequest(request);
         }

         public async Task<T> Put<T>(string uri, object value)
         {
             var request = createRequest(HttpMethod.Put, uri, value);
             return await sendRequest<T>(request);
         }

         public async Task Delete(string uri)
         {
             var request = createRequest(HttpMethod.Delete, uri);
             await sendRequest(request);
         }

         public async Task<T> Delete<T>(string uri)
         {
             var request = createRequest(HttpMethod.Delete, uri);
             return await sendRequest<T>(request);
         }

         // helper methods

         private HttpRequestMessage createRequest(HttpMethod method, string uri, object value = null)
         {
             var newUri = $"{_httpClient.BaseAddress}{uri}";
             var request = new HttpRequestMessage(method, newUri);
             if (value != null)
                 request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
             return request;
         }

         private async Task sendRequest(HttpRequestMessage request)
         {
             await addJwtHeader(request);

             // send request
             using var response = await _httpClient.SendAsync(request);

             // auto logout on 401 response
             if (response.StatusCode == HttpStatusCode.Unauthorized)
             {
                 _navigationManager.NavigateTo("account/logout");
                 return;
             }

             await handleErrors(response);
         }

         private async Task<T> sendRequest<T>(HttpRequestMessage request)
         {
             await addJwtHeader(request);

             // send request
             using var response = await _httpClient.SendAsync(request);

             // auto logout on 401 response
             if (response.StatusCode == HttpStatusCode.Unauthorized)
             {
                 _navigationManager.NavigateTo("account/logout");
                 return default;
             }

             await handleErrors(response);

             var options = new JsonSerializerOptions();
             options.PropertyNameCaseInsensitive = true;
             options.Converters.Add(new StringConverter());
             return await response.Content.ReadFromJsonAsync<T>(options);
         }

         private async Task addJwtHeader(HttpRequestMessage request)
         {
             // add jwt auth header if user is logged in and request is to the api url
             var user = await _localStorageService.GetItem<User>("user");
             var isApiUrl = !request.RequestUri.IsAbsoluteUri;
             if (user != null && isApiUrl)
                 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
         }

         private async Task handleErrors(HttpResponseMessage response)
         {
             // throw exception on error response
             if (!response.IsSuccessStatusCode)
             {
                 var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                 throw new Exception(error["message"]);
             }
         }
     }
    */
}