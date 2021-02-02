using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {
        // I could take or leave any of these, but they do make the code a little more readable
        // while keeping the purpose clear.

        private IEnumerable<ParameterInfo> GetAttributedParameters<TAttribute>(MethodInfo method) where TAttribute : Attribute
        {
            return method.GetParameters()?.Where(p => p.GetCustomAttributes<TAttribute>().Any());
        }

        private bool HasAttribute<TAttribute>(ParameterInfo parameter) where TAttribute : Attribute
        {
            return parameter.GetCustomAttribute<TAttribute>() != null;
        }

        private string GetParameterName<TAttribute>(ParameterInfo parameter) where TAttribute : Attribute, IModelNameProvider
        {
            return parameter.GetCustomAttribute<TAttribute>()?.Name ?? parameter.Name;
        }
    }
}
