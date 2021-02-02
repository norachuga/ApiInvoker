using System;
using System.Net.Http;
using System.Text;

namespace ApiInvoker
{
    public class ApiInvokerDebugException : Exception
    {
        // Just for consistency in the logs. Manually thrown when a 500 error is encountered in Debug Mode.
        // When you see this in the logs, you'll know to look for the Service exception.
        public ApiInvokerDebugException()
            : base("An exception occurred during service invocation. See the response or logs for details.")
        { }

        // This exception is used when a non-500 error is encountered in Debug Mode.
        // It stuffs all the information we have about the request and response into the message
        // to assist in debugging.
        public ApiInvokerDebugException(HttpResponseMessage response)
            : base(GenerateMessage(response))
        { }

        private static string GenerateMessage(HttpResponseMessage response)
        {
            var sb = new StringBuilder();

            int statusCode = -1;
            string statusMsg = "Unknown Code";
            string reason = "Unknown Reason";
            string request = "Unknown Request";

            if (response != null)
            {
                statusCode = (int)response.StatusCode;
                statusMsg = response.StatusCode.ToString();

                reason = response.ReasonPhrase;

                request = response.RequestMessage.ToString();
            }

            //Show all the status and message (if any) that we received as response
            sb.Append($"{statusCode} {statusMsg}: {reason}. ");

            //Dump the request that resulted in the error (method, URI, headers)
            sb.Append($" \n Raw request: \n {request}");

            return sb.ToString();
        }


    }
}
