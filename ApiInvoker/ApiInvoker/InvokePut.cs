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
        // InvokePut builds out the URI with route parameters (no querystring),
        // then generates the HttpContent payload (if there is one).
        // then calls PutAsync and processes the HttpResponseMessage.
        // It is identical to InvokePost in every way except for calling PutAsync.

        #region InvokePut, InvokePutAsync, InvokePutAsync<T>

        protected virtual object InvokePut(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPutRequestUri(method, httpMethod, args);
            var content = GetPutRequestContent(method, args);

            HttpResponseMessage response = _httpClient.PutAsync(uri, content)
                .GetAwaiter().GetResult();

            return ProcessResponse(response, method);
        }

        protected virtual async Task InvokePutAsync(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPutRequestUri(method, httpMethod, args);
            var content = GetPutRequestContent(method, args);

            HttpResponseMessage response = await _httpClient.PutAsync(uri, content);

            ProcessResponseAsync(response, method);
        }

        protected virtual async Task<T> InvokePutAsync<T>(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPutRequestUri(method, httpMethod, args);
            var content = GetPutRequestContent(method, args);

            HttpResponseMessage response = await _httpClient.PutAsync(uri, content);

            return await ProcessResponseAsync<T>(response, method);
        }

        #endregion       

        private string GetPutRequestUri(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(method.Name);

            BindRouteParameters(sb, method, httpMethod, args);

            return sb.ToString();
        }

        private HttpContent GetPutRequestContent(MethodInfo method, object[] args)
        {
            return BindRequestContent(method, args);
        }
    }
}
