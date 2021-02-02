using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace ApiInvoker
{
    public static class InvokerBindingExtensions
    {
        public static IHttpClientBuilder AddApiClient<TService>(this IServiceCollection services, Uri endpoint, bool debugMode)
            where TService : class
        {
            //Ensure trailing slash, because HttpClient doesn't?
            if (!endpoint.ToString().EndsWith("/"))
                endpoint = new Uri($"{endpoint}/");


            // Binds ApiInvoker<TService> to be injectable, with a pre-configured HttpClient injected into it
            var httpClientBuilder =
            services.AddHttpClient<ApiInvoker<TService>>(
                // 2.1 workaround - Under the hood, Typed Clients are all given a name using typeof().Name.
                // This causes a bug with generic clients because the name will be the same for all of them ("ApiInvoker`1")
                // Typed Clients can be explicitly given a name (for use as a Named Client), and this will avoid the issue.
                // Since we don't care about the name, just generate a GUID for it. This is a known bug that will be fixed in 2.2.
                // --------------
                // 2021 Update - It was fixed back in 2018 and this could be re-written to drop this workaround.
                // https://github.com/aspnet/HttpClientFactory/issues/151
                Guid.NewGuid().ToString(),
                c => { c.BaseAddress = endpoint; }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    // enables use of Windows Authentication with HttpClient
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true,
                    PreAuthenticate = true
                };
            });

            // Wherever TService is required, a dynamic proxy of ApiInvoker will be injected in its place.
            services.AddScoped<TService>(serviceProvider =>
            {
                var invoker = serviceProvider.GetService<ApiInvoker<TService>>();
                
                //invoker.AddRequestHeader("header-name", "data");

                invoker.DebugMode = debugMode;

                return invoker.Client;
            });

            // Returns IHttpClientBuilder so that callers can hook DelegateHandlers onto the injected HttpClient.
            return httpClientBuilder;
        }

        #region The many overloads of AddApiClient

        // string overload, since appSettings object graphs must be primitive.
        public static IHttpClientBuilder AddApiClient<TService>(this IServiceCollection services, string endpoint, bool debugMode)
            where TService : class
            => AddApiClient<TService>(services, new Uri(endpoint), debugMode);

        // Debugless overloads of both kinds
        public static IHttpClientBuilder AddApiClient<TService>(this IServiceCollection services, Uri endpoint)
            where TService : class
            => AddApiClient<TService>(services, endpoint, false);

        public static IHttpClientBuilder AddApiClient<TService>(this IServiceCollection services, string endpoint)
            where TService : class
            => AddApiClient<TService>(services, new Uri(endpoint), false);


        // Func Factory overloads, so endpoint and debugmode can be passed from appSettings using serviceProvider.GetService<IEndpoint> etc
        public static IHttpClientBuilder AddApiClient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, Uri> endpointFactory,
            Func<IServiceProvider, bool> debugModeFactory)
            where TService : class
        {
            var serviceProvider = services.BuildServiceProvider();

            Uri endpoint = endpointFactory.Invoke(serviceProvider);
            bool debugMode = debugModeFactory.Invoke(serviceProvider);

            return AddApiClient<TService>(services, endpoint, debugMode);
        }

        public static IHttpClientBuilder AddApiClient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, string> endpointFactory,
            Func<IServiceProvider, bool> debugModeFactory)
            where TService : class
        {
            var serviceProvider = services.BuildServiceProvider();

            string endpoint = endpointFactory.Invoke(serviceProvider);
            bool debugMode = debugModeFactory.Invoke(serviceProvider);

            return AddApiClient<TService>(services, endpoint, debugMode);
        }

        public static IHttpClientBuilder AddApiClient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, Uri> endpointFactory)
            where TService : class
            => AddApiClient<TService>(services, endpointFactory.Invoke(services.BuildServiceProvider()), false);

        public static IHttpClientBuilder AddApiClient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, string> endpointFactory)
            where TService : class
            => AddApiClient<TService>(services, endpointFactory.Invoke(services.BuildServiceProvider()), false);

        #endregion
    }
}
