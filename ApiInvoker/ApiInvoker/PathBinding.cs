using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        // These methods handle converting parameters into a string request path: name + route + querystring

        // Currently, it is left up to the individual calls to set the action name
        // and provide a StringBuilder to these methods. It is a minimal burden to do so,
        // but I haven't fully explored if there's other ways for REST services to name 
        // actions that we WANT to deal with. We're acting on the assumption that the routing
        // has been set to [controller]/[action] (because it was mandatory where this originated).
        // It's certainly possible to achieve compatibility with other routing schemes, with enough motivation.

        #region Route Parameters

        // usable by GET POST PUT DELETE
        protected StringBuilder BindRouteParameters(StringBuilder sb, MethodInfo method, HttpMethodAttribute httpMethod, object[] args)
        {
            if (!string.IsNullOrEmpty(httpMethod.Template))
            {
                string template = httpMethod.Template;
                //so we've got "{foo}fff{bar}"
                //and I want to produce "/1fff2"

                //mark route
                sb.Append("/");

                //find what parameters we're seeking. route parameters are required.                
                foreach (var parameter in method.GetParameters())
                {
                    string name = GetParameterName<FromRouteAttribute>(parameter);

                    string target = $"{{{name}}}";

                    //if parameter is tokenized in the template
                    if (template.Contains(target))
                    {
                        //route parameters are required, so I think there can't be a mismatch here?
                        var value = args[parameter.Position];

                        template = template.Replace(target, UriSafeArgument(value));
                    }
                }

                sb.Append(template);
            }

            return sb;
        }

        #endregion

        #region Querystring Parameters

        // exclusive to GET
        protected StringBuilder BindQuerystringParameters(StringBuilder sb, MethodInfo method, object[] args)
        {
            var queryParameters = GetAttributedParameters<FromQueryAttribute>(method);

            if (queryParameters.Any())
            {
                var values = new List<string>();
                foreach (var parameter in queryParameters)
                {
                    var value = args[parameter.Position];
                    if (value != null) //optional parameters will be null
                    {
                        string name = GetParameterName<FromQueryAttribute>(parameter);

                        string encoded = UriSafeArgument(value);

                        values.Add($"{name}={encoded}");
                    }
                }

                //join them all up ?a=1&b=2&c=3
                if (values.Any())
                {
                    sb.Append("?");
                    sb.Append(string.Join("&", values));
                }
            }

            return sb;
        }

        #endregion

        /// <summary>
        /// Converts a primitive into html-encoded string
        /// </summary>
        private string UriSafeArgument(object arg)
        {
            //Not sure if there's a better way to stringify than Format but it handles all primitives afaik           
            return HttpUtility.HtmlEncode(string.Format("{0}", arg));
        }
    }
}
