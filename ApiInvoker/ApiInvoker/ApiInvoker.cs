using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiInvoker
{
    // Everything in the ApiInvoker is a single partial class, just split up for readability's sake.
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        private readonly HttpClient _httpClient;

        //used for exception-handling in lower environments
        private readonly HttpContext _callerHttpContext;

        public ApiInvoker(HttpClient httpClient, IHttpContextAccessor httpContextAccessor = null)
        {
            //HttpClient must have BaseAddress already set.
            _httpClient = httpClient;

            _callerHttpContext = httpContextAccessor?.HttpContext;

            // Headers are defaulted to JSON.
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // When enabled, ProcessResponse will try to return detailed debug information on exception.
        // Otherwise, only status codes will be returned.
        public bool DebugMode { get; set; }

        public virtual void AddRequestHeader(string name, object data)
        {
            _httpClient.DefaultRequestHeaders.Remove(name);

            var json = JsonConvert.SerializeObject(data);

            _httpClient.DefaultRequestHeaders.Add(name, json);
        }


        #region Dynamic Proxy Generation

        //returns this instance as a TContract. When methods on this are called, we intercept them.
        public TService Client => (dynamic)this;

        //performs the implicit conversions to the proxy
        static readonly ProxyGenerator _generator = new ProxyGenerator();
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(TService))
            {
                result = _generator.CreateInterfaceProxyWithoutTarget(typeof(TService), this);
                return true;
            }
            else
                return base.TryConvert(binder, out result);
        }

        #endregion

        #region Intercept & Execute / ExecuteAsync / ExecuteAsyncWithResult

        public void Intercept(IInvocation invocation)
        {
            var methodType = GetMethodType(invocation);

            switch (methodType)
            {
                case MethodType.Synchronous:
                    Execute(invocation);
                    break;

                case MethodType.AsyncAction: //async Task
                    ExecuteAsync(invocation);
                    break;

                case MethodType.AsyncFunction: //async Task<T>
                    ExecuteAsyncWithResult(invocation);
                    break;
            }
        }

        private void Execute(IInvocation invocation)
        {
            invocation.ReturnValue = InvokeCall(invocation);
        }

        private void ExecuteAsync(IInvocation invocation)
        {
            invocation.ReturnValue = InvokeCallAsync(invocation);
        }

        private void ExecuteAsyncWithResult(IInvocation invocation)
        {
            //in order to return a Task<T>, we have to call InvokeCallAsync<T>.
            //So we use reflection to dynamically produce that method and invoke it.

            //get T out of Task<T>
            var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];

            //locate InvokeCallAsync<T> (and not the non-generic Task one)
            //and produce the method InvokeCallAsync<resultType>
            var method = typeof(ApiInvoker<TService>)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "InvokeCallAsync" && m.IsGenericMethod)
                .MakeGenericMethod(resultType);

            //call it as a member of this instance of ApiInvoker and pass invocation as an argument
            invocation.ReturnValue = method.Invoke(this, new[] { invocation });
        }

        #endregion

        #region Determine Method Type (sync, async Task, or async Task<T>)

        private MethodType GetMethodType(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            if (returnType == typeof(Task))
                return MethodType.AsyncAction;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return MethodType.AsyncFunction;

            return MethodType.Synchronous;
        }

        private enum MethodType
        {
            Synchronous,
            AsyncAction,
            AsyncFunction
        }

        #endregion  
    }
}
