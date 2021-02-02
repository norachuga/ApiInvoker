using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        // InvokeCall inspects the HttpMethod attribute, and if it passes validation,
        // calls the individal invoke call for the respective verb

        #region InvokeCall, InvokeCallAsync, InvokeCallAsyncWithResult<T>

        protected virtual object InvokeCall(IInvocation invocation)
        {
            var httpMethod = GetHttpMethodAttribute(invocation.Method);

            string verb = httpMethod.HttpMethods.First();
            switch (verb)
            {
                case "GET":
                    return InvokeGet(invocation.Method, httpMethod, invocation.Arguments);
                case "POST":
                    return InvokePost(invocation.Method, httpMethod, invocation.Arguments);
                case "PUT":
                    return InvokePut(invocation.Method, httpMethod, invocation.Arguments);
                case "DELETE":
                    return InvokeDelete(invocation.Method, httpMethod, invocation.Arguments);

                default:
                    throw new NotSupportedException($"ApiInvoker does not support the verb: {verb}");
            }
        }

        protected virtual async Task InvokeCallAsync(IInvocation invocation)
        {
            var httpMethod = GetHttpMethodAttribute(invocation.Method);

            string verb = httpMethod.HttpMethods.First();
            switch (verb)
            {
                case "GET":
                    await InvokeGetAsync(invocation.Method, httpMethod, invocation.Arguments);
                    break;
                case "POST":
                    await InvokePostAsync(invocation.Method, httpMethod, invocation.Arguments);
                    break;
                case "PUT":
                    await InvokePutAsync(invocation.Method, httpMethod, invocation.Arguments);
                    break;
                case "DELETE":
                    await InvokeDeleteAsync(invocation.Method, httpMethod, invocation.Arguments);
                    break;

                default:
                    throw new NotSupportedException($"RestInvoker does not support the verb: {verb}");
            }
        }

        // IMPORTANT: This method name is referenced via magic string in ExecuteAsyncWithResult
        // If you rename it, be sure to update the string in that method.
        protected virtual async Task<T> InvokeCallAsync<T>(IInvocation invocation)
        {
            var httpMethod = GetHttpMethodAttribute(invocation.Method);

            string verb = httpMethod.HttpMethods.First();
            switch (verb)
            {
                case "GET":
                    return await InvokeGetAsync<T>(invocation.Method, httpMethod, invocation.Arguments);
                case "POST":
                    return await InvokePostAsync<T>(invocation.Method, httpMethod, invocation.Arguments);
                case "PUT":
                    return await InvokePutAsync<T>(invocation.Method, httpMethod, invocation.Arguments);
                case "DELETE":
                    return await InvokeDeleteAsync<T>(invocation.Method, httpMethod, invocation.Arguments);

                default:
                    throw new NotSupportedException($"RestInvoker does not support the verb: {verb}");
            }
        }

        #endregion

        private HttpMethodAttribute GetHttpMethodAttribute(MethodInfo method)
        {
            var attribs = method.GetCustomAttributes<HttpMethodAttribute>();

            if (attribs == null)
                throw new NotImplementedException("Method cannot be called without a HttpMethodAttribute.");

            if (attribs.Count() == 1)
                return attribs.Single();

            //Reject if there are multiple verbs on a single method. We can't know which verb they meant to execute
            //and it's possible (but unwise) for a method to respond with different behavior depending on the verb
            //ex: if (Request.Method == "DELETE") { ... } else { ... }
            var verbs = attribs.SelectMany(x => x.HttpMethods).Distinct();
            if (verbs.Count() > 1)
                throw new AmbiguousMatchException("RestInvoker does not support multiple HTTP verbs on a single method.");

            //If we have multiple attributes for the same verb (for route overloading), just pick the first one.
            //They will all bind the same. If the consumer is using route overloading, they probably shouldn't be using invoker.
            return attribs.First();
        }
    }
}
