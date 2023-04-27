using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class RestClient
    {
        static readonly HttpClient Client = new HttpClient();
        public static bool DoNotCountNextRequest { get; set; }
        public static int PostLoadCount { get; private set; }
        public static int PatchLoadCount { get; private set; }
        public static int PutLoadCount { get; private set; }
        public static int DeleteLoadCount { get; private set; }
        public static int GetLoadCount { get; private set; }
        public static int LoadCount { get { return PostLoadCount + PatchLoadCount + PutLoadCount + DeleteLoadCount + GetLoadCount; } }
        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static string GetQueryString(Dictionary<string, object> queries)
        {
            StringBuilder queryStringBuilder = new StringBuilder();
            int i = 0;
            foreach (var query in queries)
            {
                if (string.IsNullOrEmpty(query.Key) || query.Value == null)
                    continue;
                if (i == 0)
                    queryStringBuilder.Append('?');
                else
                    queryStringBuilder.Append('&');
                if (query.Value.GetType().IsArray ||
                    query.Value.GetType().IsAssignableFrom(typeof(IList)))
                {
                    int j = 0;
                    foreach (object value in query.Value as IEnumerable)
                    {
                        if (j > 0)
                            queryStringBuilder.Append('&');
                        queryStringBuilder.Append(query.Key);
                        queryStringBuilder.Append("=");
                        queryStringBuilder.Append(value);
                        ++j;
                    }
                }
                else
                {
                    queryStringBuilder.Append(query.Key);
                    queryStringBuilder.Append('=');
                    queryStringBuilder.Append(query.Value);
                }
                ++i;
            }
            return queryStringBuilder.ToString();
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url)
        {
            return await Get<TResponse>(url, new Dictionary<string, object>(), string.Empty);
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url, Dictionary<string, object> queries)
        {
            return await Get<TResponse>(url, queries, string.Empty);
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url, string authorizationToken)
        {
            return await Get<TResponse>(url, new Dictionary<string, object>(), authorizationToken);
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url, Dictionary<string, object> queries, string authorizationToken)
        {
            Result result = await Get(url + GetQueryString(queries), authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Delete<TResponse>(string url)
        {
            return await Delete<TResponse>(url, new Dictionary<string, object>(), string.Empty);
        }

        public static async Task<Result<TResponse>> Delete<TResponse>(string url, Dictionary<string, object> queries)
        {
            return await Delete<TResponse>(url, queries, string.Empty);
        }

        public static async Task<Result<TResponse>> Delete<TResponse>(string url, Dictionary<string, object> queries, string authorizationToken)
        {
            Result result = await Delete(url + GetQueryString(queries), authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Post<TForm, TResponse>(string url, TForm data)
        {
            return await Post<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Post<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Post(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Post<TResponse>(string url, string authorizationToken)
        {
            Result result = await Post(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Post(string url, string authorizationToken)
        {
            return await Post(url, "{}", authorizationToken);
        }

        public static async Task<Result<TResponse>> Patch<TForm, TResponse>(string url, TForm data)
        {
            return await Patch<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Patch<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Patch(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Patch<TResponse>(string url, string authorizationToken)
        {
            Result result = await Patch(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Patch(string url, string authorizationToken)
        {
            return await Patch(url, "{}", authorizationToken);
        }

        public static async Task<Result<TResponse>> Put<TForm, TResponse>(string url, TForm data)
        {
            return await Put<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Put<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Put(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Put<TResponse>(string url, string authorizationToken)
        {
            Result result = await Put(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Put(string url, string authorizationToken)
        {
            return await Put(url, "{}", authorizationToken);
        }

        public static async Task<Result> Get(string url, string authorizationToken)
        {
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.WriteLine($"[Info] GET request {id} {url}");
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = false;
            bool isNetworkError = false;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                GetLoadCount++;
            using (var webRequest = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    Debug.WriteLine($"[Info] GET {id} with authorization token {authorizationToken}");
                    webRequest.Headers.Add("Authorization", "Bearer " + authorizationToken);
                }
                try
                {
                    using var resp = await Client.SendAsync(webRequest);
                    {
                        responseCode = (long)resp.StatusCode;
                        stringContent = await resp.Content.ReadAsStringAsync();
                    }
                    isHttpError = resp.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    isNetworkError = true;
                    error = ex.Message;
                    Debug.WriteLine($"[Error] Cannot GET: {url} {ex.Message} {id}");
                    errorLogged = true;
                    Console.WriteLine($"[Error] Cannot GET: {url} {ex.Message}");
                }
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.WriteLine($"[Error] GET Result {id} {stringContent}");
                else
                    Debug.WriteLine($"[Info] GET Result {id} {responseCode} {stringContent}");
            }
            if (!doNotCountNextRequest)
                GetLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Delete(string url, string authorizationToken)
        {
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.WriteLine($"[Info] DELETE request {id} {url}");
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = false;
            bool isNetworkError = false;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                DeleteLoadCount++;
            using (var webRequest = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    Debug.WriteLine($"[Info] DELETE {id} with authorization token {authorizationToken}");
                    webRequest.Headers.Add("Authorization", "Bearer " + authorizationToken);
                }
                try
                {
                    using var resp = await Client.SendAsync(webRequest);
                    {
                        responseCode = (long)resp.StatusCode;
                        stringContent = await resp.Content.ReadAsStringAsync();
                    }
                    isHttpError = resp.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    isNetworkError = true;
                    error = ex.Message;
                    Debug.WriteLine($"[Error] Cannot DELETE: {url} {ex.Message} {id}");
                    errorLogged = true;
                    Console.WriteLine($"[Error] Cannot DELETE: {url} {ex.Message}");
                }
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.WriteLine($"[Error] DELETE Result {id} {stringContent}");
                else
                    Debug.WriteLine($"[Info] DELETE Result {id} {responseCode} {stringContent}");
            }
            if (!doNotCountNextRequest)
                DeleteLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Post<TForm>(string url, TForm data)
        {
            return await Post(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), null);
        }

        public static async Task<Result> Post<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Post(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), authorizationToken);
        }

        public static async Task<Result> Post(string url, string data, string authorizationToken)
        {
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.WriteLine($"[Info] POST request {id} {url} {data}");
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = false;
            bool isNetworkError = false;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PostLoadCount++;
            using (var webRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    Debug.WriteLine($"[Info] POST {id} with authorization token {authorizationToken}");
                    webRequest.Headers.Add("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");
                try
                {
                    using var resp = await Client.SendAsync(webRequest);
                    {
                        responseCode = (long)resp.StatusCode;
                        stringContent = await resp.Content.ReadAsStringAsync();
                    }
                    isHttpError = resp.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    isNetworkError = true;
                    error = ex.Message;
                    Debug.WriteLine($"[Error] Cannot POST: {url} {ex.Message} {id}");
                    errorLogged = true;
                    Console.WriteLine($"[Error] Cannot POST: {url} {ex.Message}");
                }
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.WriteLine($"[Error] POST Result {id} {stringContent}");
                else
                    Debug.WriteLine($"[Info] POST Result {id} {responseCode} {stringContent}");
            }
            if (!doNotCountNextRequest)
                PostLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Patch<TForm>(string url, TForm data)
        {
            return await Patch(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), null);
        }

        public static async Task<Result> Patch<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Patch(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), authorizationToken);
        }

        public static async Task<Result> Patch(string url, string data, string authorizationToken)
        {
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.WriteLine($"[Info] PATCH request {id} {url} {data}");
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = false;
            bool isNetworkError = false;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PatchLoadCount++;
            using (var webRequest = new HttpRequestMessage(HttpMethod.Patch, url))
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    Debug.WriteLine($"[Info] PATCH {id} with authorization token {authorizationToken}");
                    webRequest.Headers.Add("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");
                try
                {
                    using var resp = await Client.SendAsync(webRequest);
                    {
                        responseCode = (long)resp.StatusCode;
                        stringContent = await resp.Content.ReadAsStringAsync();
                    }
                    isHttpError = resp.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    isNetworkError = true;
                    error = ex.Message;
                    Debug.WriteLine($"[Error] Cannot PATCH: {url} {ex.Message} {id}");
                    errorLogged = true;
                    Console.WriteLine($"[Error] Cannot PATCH: {url} {ex.Message}");
                }
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.WriteLine($"[Error] PATCH Result {id} {stringContent}");
                else
                    Debug.WriteLine($"[Info] PATCH Result {id} {responseCode} {stringContent}");
            }
            if (!doNotCountNextRequest)
                PatchLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Put<TForm>(string url, TForm data)
        {
            return await Put(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), null);
        }

        public static async Task<Result> Put<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Put(url, JsonConvert.SerializeObject(data, JsonSerializerSettings), authorizationToken);
        }

        public static async Task<Result> Put(string url, string data, string authorizationToken)
        {
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.WriteLine($"[Info] PUT request {id} {url} {data}");
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = false;
            bool isNetworkError = false;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PutLoadCount++;
            using (var webRequest = new HttpRequestMessage(HttpMethod.Put, url))
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    Debug.WriteLine($"[Info] PUT {id} with authorization token {authorizationToken}");
                    webRequest.Headers.Add("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");
                try
                {
                    using var resp = await Client.SendAsync(webRequest);
                    {
                        responseCode = (long)resp.StatusCode;
                        stringContent = await resp.Content.ReadAsStringAsync();
                    }
                    isHttpError = resp.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    isNetworkError = true;
                    error = ex.Message;
                    Debug.WriteLine($"[Error] Cannot PUT: {url} {ex.Message} {id}");
                    errorLogged = true;
                    Console.WriteLine($"[Error] Cannot PUT: {url} {ex.Message}");
                }
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.WriteLine($"[Error] PUT Result {id} {stringContent}");
                else
                    Debug.WriteLine($"[Info] PUT Result {id} {responseCode} {stringContent}");
            }
            if (!doNotCountNextRequest)
                PutLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static string GetQueryString(params KeyValuePair<string, string>[] parameters)
        {
            string queryString = string.Empty;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (string.IsNullOrEmpty(parameters[i].Key) ||
                    string.IsNullOrEmpty(parameters[i].Value))
                    continue;
                if (!string.IsNullOrEmpty(queryString))
                    queryString += "&";
                else
                    queryString += "?";
                queryString += $"{parameters[i].Key}={parameters[i].Value}";
            }
            return queryString;
        }

        public static string GetUrl(string apiUrl, string action)
        {
            if (apiUrl.EndsWith("/"))
                apiUrl = apiUrl.Substring(0, apiUrl.Length - 1);
            if (action.StartsWith("/"))
                action = action.Substring(1);
            return $"{apiUrl}/{action}";
        }

        public static string GetNetworkErrorMessage(long responseCode)
        {
            switch (responseCode)
            {
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 402:
                    return "Payment Required";
                case 403:
                    return "Forbidden";
                case 404:
                    return "Not Found";
                case 405:
                    return "Method Not Allowed";
                case 406:
                    return "Not Acceptable";
                case 407:
                    return "Proxy Authentication Required";
                case 408:
                    return "Request Timeout";
                case 409:
                    return "Conflict";
                case 410:
                    return "Gone";
                case 411:
                    return "Length Required";
                case 412:
                    return "Precondition Failed";
                case 413:
                    return "Request Entity Too Large";
                case 414:
                    return "Request-url Too Long";
                case 415:
                    return "Unsupported Media Type";
                case 416:
                    return "Requested Range Not Satisfiable";
                case 417:
                    return "Expectation Failed";
                case 500:
                    return "Internal Server Error";
                case 501:
                    return "Not Implemented";
                case 502:
                    return "Bad Gateway";
                case 503:
                    return "Service Unavailable";
                case 504:
                    return "Gateway Timeout";
                case 505:
                    return "HTTP Version Not Supported";
                default:
                    if (responseCode >= 400 && responseCode < 500)
                        return "Client Error";
                    if (responseCode >= 500 && responseCode < 600)
                        return "Server Error";
                    return "Unknow Error";
            }
        }

        public interface IResult
        {
            long ResponseCode { get; }
            bool IsHttpError { get; }
            bool IsNetworkError { get; }
            string StringContent { get; }
            string Error { get; }
        }

        public struct Result : IResult
        {
            public long ResponseCode { get; private set; }
            public bool IsHttpError { get; private set; }
            public bool IsNetworkError { get; private set; }
            public string StringContent { get; private set; }
            public string Error { get; private set; }

            public Result(long responseCode, bool isHttpError, bool isNetworkError, string stringContent, string error)
            {
                ResponseCode = responseCode;
                IsHttpError = isHttpError;
                IsNetworkError = isNetworkError;
                StringContent = stringContent;
                Error = error;
                if (IsHttpError && string.IsNullOrEmpty(Error))
                    Error = GetNetworkErrorMessage(responseCode);
            }
        }

        public struct Result<T> : IResult
        {
            public long ResponseCode { get; private set; }
            public bool IsHttpError { get; private set; }
            public bool IsNetworkError { get; private set; }
            public string StringContent { get; private set; }
            public string Error { get; private set; }
            public T Content { get; private set; }

            public Result(long responseCode, bool isHttpError, bool isNetworkError, string stringContent, string error)
            {
                ResponseCode = responseCode;
                StringContent = stringContent;
                IsHttpError = isHttpError;
                IsNetworkError = isNetworkError;
                Error = error;
                Content = default;
                if (!IsHttpError && !IsNetworkError)
                {
                    try
                    {
                        Content = JsonConvert.DeserializeObject<T>(stringContent, JsonSerializerSettings);
                    }
                    catch (Exception ex)
                    {
                        // It may not able to deserialize
                        Console.WriteLine($"[Error] Can't deserialize content: {stringContent}, {ex}");
                    }
                }
                if (IsHttpError && string.IsNullOrEmpty(Error))
                    Error = GetNetworkErrorMessage(responseCode);
            }
        }
    }

    public static class RestClientResultExtensions
    {
        public static bool IsError(this RestClient.IResult result)
        {
            return result.IsHttpError || result.IsNetworkError;
        }
    }
}
