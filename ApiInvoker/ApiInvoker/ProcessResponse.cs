using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        // ProcessResponse inspects the HttpResponseMessage for success.
        // Currently, it uses .EnsureSuccessStatusCode() to provide simple error handling.
        // That throws a simple HttpRequestException if the request was not successful.
        //
        // If it was successful, we move on to parsing (except for void/Task calls)
        // These methods first check for the presence of _callerHttpContext to determine
        // if this is a debug-friendly call. Debug calls are handled explicitly
        // so exception details can be easily received by callers. 

        #region ProcessResponse, ProcessResponseAsync, ProcessResponseAsync<T>

        private object ProcessResponse(HttpResponseMessage response, MethodInfo method)
        {
            if (DebugMode && _callerHttpContext != null)
                return HandleDebugRequest(response, method);

            response.EnsureSuccessStatusCode();
            return ParseSuccessfulResponse(response, method);
        }

        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, MethodInfo method)
        {
            if (DebugMode && _callerHttpContext != null)
                return await HandleDebugRequestAsync<T>(response, method);

            response.EnsureSuccessStatusCode();
            return await ParseSuccessfulResponseAsync<T>(response, method);
        }

        private void ProcessResponseAsync(HttpResponseMessage response, MethodInfo method)
        {
            //This is named Async to fit the pattern, but there's nothing async to do here.
            //Void needs no awaiting. All we are doing is checking if there's a need to throw.
            if (DebugMode && _callerHttpContext != null)
                HandleDebugRequestAsync(response);

            response.EnsureSuccessStatusCode();
            return;
        }

        #endregion

        // More robust exception handling is necessary when debugging, to ensure that
        // error messages are received back to the caller. While not strictly necessary,
        // it would certainly slow down development to have to rely solely on logging.

        #region HandleDebugRequest, HandleDebugRequestAsync, HandleDebugRequestAsync<T>

        private object HandleDebugRequest(HttpResponseMessage response, MethodInfo method)
        {
            if (response.IsSuccessStatusCode)
                return ParseSuccessfulResponse(response, method);

            // Request Failure!
            // Check the response for any content (such as an HTML debug exception page)
            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Non-500 errors will usually have no content
            if (string.IsNullOrEmpty(content))
            {
                //Dispose and throw an error with as much description as we have
                response.Content.Dispose();

                throw new ApiInvokerDebugException(response);
            }
            else
            {
                // If we do have content, write it to the caller's response stream.                
                _callerHttpContext.Response.ContentType = response.Content.Headers.ContentType.ToString();
                _callerHttpContext.Response.StatusCode = (int)response.StatusCode;
                _callerHttpContext.Response.WriteAsync(content);

                // Abort the invocation.
                throw new ApiInvokerDebugException();
            }
        }

        private async Task<T> HandleDebugRequestAsync<T>(HttpResponseMessage response, MethodInfo method)
        {
            if (response.IsSuccessStatusCode)
            {
                return await ParseSuccessfulResponseAsync<T>(response, method);
            }

            // Request Failure!
            // Check the response for any content (such as an HTML debug exception page)
            string content = await response.Content.ReadAsStringAsync();

            // Non-500 errors will usually have no content
            if (string.IsNullOrEmpty(content))
            {
                //Dispose and throw an error with as much description as we have
                response.Content.Dispose();

                throw new ApiInvokerDebugException(response);
            }
            else
            {
                // If we do have content, write it to the caller's response stream.                
                _callerHttpContext.Response.ContentType = response.Content.Headers.ContentType.ToString();
                _callerHttpContext.Response.StatusCode = (int)response.StatusCode;
                await _callerHttpContext.Response.WriteAsync(content);

                // Abort the invocation.
                throw new ApiInvokerDebugException();
            }
        }

        private void HandleDebugRequestAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            // Request Failure!
            // Check the response for any content (such as an HTML debug exception page)
            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Non-500 errors will usually have no content
            if (string.IsNullOrEmpty(content))
            {
                //Dispose and throw an error with as much description as we have
                response.Content.Dispose();

                throw new ApiInvokerDebugException(response);
            }
            else
            {
                // If we do have content, write it to the caller's response stream.                
                _callerHttpContext.Response.ContentType = response.Content.Headers.ContentType.ToString();
                _callerHttpContext.Response.StatusCode = (int)response.StatusCode;
                _callerHttpContext.Response.WriteAsync(content);

                // Abort the invocation.
                throw new ApiInvokerDebugException();
            }
        }

        #endregion

        // ParseSuccessfulResponse reads out the response content to a string
        // and deserializes it back to object (if necessary to do so)

        #region ParseSuccessfulResponse, ParseSuccessfulResponseAsync<T>

        private object ParseSuccessfulResponse(HttpResponseMessage response, MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
                return null;

            string raw = response.Content.ReadAsStringAsync()
                .GetAwaiter().GetResult();

            object result = null;
            result = JsonConvert.DeserializeObject(raw, method.ReturnType, ApiSerializerSettings.Configure());

            return result;
        }

        private async Task<T> ParseSuccessfulResponseAsync<T>(HttpResponseMessage response, MethodInfo method)
        {
            string raw = await response.Content.ReadAsStringAsync();

            //T result = default(T); //any reason to do this?

            var result = JsonConvert.DeserializeObject<T>(raw, ApiSerializerSettings.Configure());

            return result;
        }

        #endregion
    }
}
