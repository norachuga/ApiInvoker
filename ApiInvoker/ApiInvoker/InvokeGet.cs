using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Dynamic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        // InvokeGet builds out the URI with route parameters and querystring,
        // then calls GetAsync and processes the HttpResponseMessage

        #region InvokeGet, InvokeGetAsync, InvokeGetAsync<T>

        protected virtual object InvokeGet(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildGetRequestUri(method, httpMethod, args);

            HttpResponseMessage response = _httpClient.GetAsync(uri)
                .GetAwaiter().GetResult(); //synchronous, blocking call.                

            return ProcessResponse(response, method);
        }

        protected virtual async Task InvokeGetAsync(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildGetRequestUri(method, httpMethod, args);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            ProcessResponseAsync(response, method);
        }

        protected virtual async Task<T> InvokeGetAsync<T>(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildGetRequestUri(method, httpMethod, args);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            return await ProcessResponseAsync<T>(response, method);
        }

        #endregion

        private string BuildGetRequestUri(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            var sb = new StringBuilder();

            sb.Append(method.Name);

            BindRouteParameters(sb, method, httpMethod, args);

            BindQuerystringParameters(sb, method, args);

            return sb.ToString();
        }
    }
}
