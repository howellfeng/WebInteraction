using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebInteraction
{
    public class WebApiClient<TData, TKey> : WebApiClient
    {
        private Func<TData, TKey> _idSelector = null;
        private Action<TData, TKey> _idWriter = null;
        public WebApiClient(string server, string prefix, string controller, Func<TData, TKey> idSelector, Action<TData, TKey> idWriter, int timeout = 15) : base(server, prefix, controller, timeout)
        {
            _idSelector = idSelector;
            _idWriter = idWriter;
        }
        public WebApiClient(string server, string controller, Func<TData, TKey> idSelector, Action<TData, TKey> idWriter, int timeout = 15) : this(server, "api/", controller, idSelector, idWriter, timeout)
        {

        }
        public TData QueryData(string url, object para)
        {
            return Query<TData>(url, para);
        }
        public TData[] QueryDataList(string url, object para)
        {
            return Query<TData[]>(url, para);
        }
        public virtual void Create(TData data)
        {
            TData result = process<TData>(_client.PostAsJsonAsync(ControllUrl, data));
            _idWriter(data, _idSelector(result));
        }
        public virtual TData[] LoadAll()
        {
            return QueryDataList(string.Empty, null);
        }
        public virtual TData Find(TKey id)
        {
            return QueryData($"/{formatId(id)}", null);
        }
        public virtual TData[] FindAll(TKey key)
        {
            return QueryDataList(string.Empty, new { key = key });
        }
        public virtual void Remove(TKey id)
        {
            string url = $"{ControllUrl}/{formatId(id)}";
            process(_client.DeleteAsync(url));
        }
        public virtual void Update(TData data)
        {
            process(_client.PutAsJsonAsync(ControllUrl, data));
        }

        private string formatId(TKey id)
        {
            string url = id.ToString();
            if (url.Contains(":"))
                return HttpUtility.UrlEncode(url);
            else
                return url;
        }
    }
    public class WebApiClient
    {
        protected HttpClient _client;
        public string ControllUrl { get; private set; }
        public WebApiClient(string server, string prefix, string controller, int timeout = 15)
        {
            ControllUrl = $"{prefix}{controller}";
            _client = new HttpClient();
            _client.BaseAddress = new Uri(server);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _client.Timeout = TimeSpan.FromSeconds(timeout);
        }
        public WebApiClient(string server, string controller, int timeout = 15) : this(server, "api/", controller, timeout)
        {
        }
        public TResult Query<TResult>(string url, object para)
        {
            return Task.Run(() => QueryAsync<TResult>(url, para)).WaitForResult();
        }
        public Task<TResult> QueryAsync<TResult>(string url, object para)
        {
            url = $"{ControllUrl}{url}{formatPara(para)}";
            return processAsync<TResult>(_client.GetAsync(url));
        }
        #region
        protected void process(Task<HttpResponseMessage> opertion)
        {
            Task.Run(() => processAsync(opertion)).Wait();
        }
        protected TResult process<TResult>(Task<HttpResponseMessage> operation)
        {
            return Task.Run(() => processAsync<TResult>(operation)).WaitForResult();
        }

        protected async Task<HttpResponseMessage> processAsync(Task<HttpResponseMessage> operation)
        {
            try
            {
                HttpResponseMessage rsp = await operation;
                HttpError.CheckResponse(rsp);
                return rsp;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException("任务执行超时");
            }
        }
        protected async Task<TResult> processAsync<TResult>(Task<HttpResponseMessage> operation)
        {
            HttpResponseMessage rsp = await processAsync(operation);
            return await rsp.Content.ReadAsAsync<TResult>();
        }
        protected async Task<String> processStringAsync(Task<HttpResponseMessage> operation)
        {
            HttpResponseMessage rsp = await processAsync(operation);
            return await rsp.Content.ReadAsStringAsync();
        }
        protected string formatPara(object para)
        {
            if (para == null)
                return string.Empty;
            else
            {
                RouteValueDictionary dic = new RouteValueDictionary(para);
                return $"?{string.Join("&", dic.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value?.ToString())}"))}";
            }
        }
        #endregion
    }
}
