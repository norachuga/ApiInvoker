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
        // InvokeDelete builds out the URI with route parameters (does not permit querystrings),
        // then calls DeleteAsync and processes the HttpResponseMessage

        #region InvokeDelete, InvokeDeleteAsync, InvokeDeleteAsync<T>        

        protected virtual object InvokeDelete(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildDeleteRequestUri(method, httpMethod, args);

            HttpResponseMessage response = _httpClient.DeleteAsync(uri)
                .GetAwaiter().GetResult();

            return ProcessResponse(response, method);
        }

        protected virtual async Task InvokeDeleteAsync(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildDeleteRequestUri(method, httpMethod, args);

            HttpResponseMessage response = await _httpClient.DeleteAsync(uri);

            ProcessResponseAsync(response, method);
        }

        protected virtual async Task<T> InvokeDeleteAsync<T>(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = BuildDeleteRequestUri(method, httpMethod, args);

            HttpResponseMessage response = await _httpClient.DeleteAsync(uri);

            return await ProcessResponseAsync<T>(response, method);
        }

        #endregion

        private string BuildDeleteRequestUri(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            var sb = new StringBuilder();

            sb.Append(method.Name);

            BindRouteParameters(sb, method, httpMethod, args);

            return sb.ToString();
        }
    }
}
