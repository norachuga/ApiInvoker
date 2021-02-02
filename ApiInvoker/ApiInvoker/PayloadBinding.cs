using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace ApiInvoker
{
    public partial class ApiInvoker<TService> : DynamicObject, IInterceptor where TService : class
    {

        // usable by POST and PUT

        private HttpContent BindRequestContent(MethodInfo method, object[] args)
        {
            var bodyParameters = GetAttributedParameters<FromBodyAttribute>(method);
            var formParameters = GetAttributedParameters<FromFormAttribute>(method);

            if (bodyParameters.Any())
            {
                if (bodyParameters.Count() > 1)
                    throw new InvalidOperationException("Too many [FromBody] parameters. There can only be a single parameter in a request body content binding.");

                if (formParameters.Any())
                    throw new InvalidOperationException("Request parameters have mixed content-types. Method cannot have both [FromBody] and [FromForm] parameters.");

                return BindBodyContent(method, bodyParameters.Single(), args);
            }

            if (formParameters.Any())
                return BindFormContent(method, formParameters, args);

            // Parameterless calls are not allowed. A call without parameters is most likely unintentional and they just forgot to put
            // any binding attributes on the method in the service contract. This prevents intentional usage as well, but it is very
            // unlikely that you would have a valid reason to use a parameterless POST (instead of using GET).
            throw new InvalidOperationException("No attributed parameters were found. A POST or PUT call must have parameters. The service contract is likely missing any [FromBody] or [FromForm] parameters on this method.");
        }

        private StringContent BindBodyContent(MethodInfo method, ParameterInfo parameter, object[] args)
        {
            var value = args[parameter.Position];

            string json = JsonConvert.SerializeObject(value, ApiSerializerSettings.Configure());

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private HttpContent BindFormContent(MethodInfo method, IEnumerable<ParameterInfo> parameters, object[] args)
        {
            var content = new MultipartFormDataContent();

            //The flow of this part can be confusing. 
            //The only things that get added to the Multipart content are files and strings.
            //So we evaluate the parameter to determine which it is.
            //  If it is a file, files, string, or value type, it will be added and processing concludes.            
            //  If it is a collection, it will be iterated over and recursed to ProcessModel to handle each item.
            //  If it is a null, it is simply ignored.
            //  If it is a complex object, it is sent to ProcessModelProperties.
            //ProcessModelProperties iterates over the properties of the object and sends them to ProcessModel.

            //The important part of this is to produce keys that will allow MVC's model binder to assemble the object.
            //Think of the HTML names produced by HtmlHelpers in a View, like @Html.TextBoxFor(m => m.Person.FirstName).
            //Child properties must be dot-notated, and collection items must be indexed:
            //      model.Person.Name
            //      model.Person.Age
            //      model.Children[0].Name
            //      model.Children[0].Age
            //      model.Children[1].Name
            //      model.Children[1].Age            
            //An exception to this is IFormFile collections. For some reason, they must be added without indices.
            //Every file in the collection will be added with the same key.

            foreach (var parameter in parameters)
            {
                var value = args[parameter.Position];
                ProcessModel(content, value, parameter.Name);
            }

            return content;
        }

        private void AddStringToMultipart(MultipartFormDataContent content, object value, string name)
        {
            string formatted = Convert.ToString(value);

            var data = new StringContent(formatted);

            content.Add(data, name);
        }

        private void AddFormFileToMultipart(MultipartFormDataContent content, IFormFile file, string name)
        {
            using (var ms = new MemoryStream())
            {
                //This could be done async, but would require a more refactoring to make the whole method chain async.
                //The benefit would likely be negligible, but I would still like to do it in the future.
                file.CopyTo(ms);

                var data = new ByteArrayContent(ms.ToArray());
                data.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                content.Add(data, name, file.FileName);
            }
        }

        private void ProcessModelProperties(MultipartFormDataContent content, object model, string rootName)
        {
            var properties = model.GetType().GetProperties();

            foreach (var property in properties)
            {
                //Produce the dot-notation name for child properties
                string keyName;
                if (rootName == null)
                    keyName = property.Name;
                else
                    keyName = string.Join(".", rootName, property.Name);

                var value = property.GetValue(model);

                ProcessModel(content, value, keyName);
            }
        }

        private void ProcessModel(MultipartFormDataContent content, object model, string keyName)
        {
            switch (model)
            {
                //IFormFile get added as ByteArrayContent
                case IFormFile file:
                    AddFormFileToMultipart(content, file, keyName);
                    break;

                //IFormFile collections all use the same key name
                case IEnumerable<IFormFile> files:
                    foreach (var file in files)
                        AddFormFileToMultipart(content, file, keyName);
                    break;

                //strings or values get added as StringContent
                case string str:
                case object obj when model.GetType().IsValueType:
                    AddStringToMultipart(content, model, keyName);
                    break;

                //Produce indexed name for collections: CollectionProperty[0].ChildProperty
                case IEnumerable collection:
                    int index = 0;
                    foreach (var item in collection)
                    {
                        string itemName = $"{keyName}[{index}]";
                        ProcessModel(content, item, itemName);
                        index++;
                    }
                    break;

                //Ignore nulls
                case null:
                    break;

                //Recurse on classes
                default:
                    ProcessModelProperties(content, model, keyName);
                    break;

            }
        }
    }
}
