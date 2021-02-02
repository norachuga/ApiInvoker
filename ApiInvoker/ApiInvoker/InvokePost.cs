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
        // InvokePost builds out the URI with route parameters (no querystring),
        // then generates the HttpContent payload (if there is one).
        // then calls PostAsync and processes the HttpResponseMessage

        #region InvokePost, InvokePostAsync, InvokePostAsync<T>

        protected virtual object InvokePost(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPostRequestUri(method, httpMethod, args);
            var content = GetPostRequestContent(method, args);

            HttpResponseMessage response = _httpClient.PostAsync(uri, content)
                .GetAwaiter().GetResult();

            return ProcessResponse(response, method);
        }

        protected virtual async Task InvokePostAsync(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPostRequestUri(method, httpMethod, args);
            var content = GetPostRequestContent(method, args);

            HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

            ProcessResponseAsync(response, method);
        }

        protected virtual async Task<T> InvokePostAsync<T>(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            string uri = GetPostRequestUri(method, httpMethod, args);
            var content = GetPostRequestContent(method, args);

            HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

            return await ProcessResponseAsync<T>(response, method);
        }

        #endregion       

        private string GetPostRequestUri(MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(method.Name);

            BindRouteParameters(sb, method, httpMethod, args);

            return sb.ToString();
        }

        private HttpContent GetPostRequestContent(MethodInfo method, object[] args)
        {
            return BindRequestContent(method, args);
        }
    }
}
