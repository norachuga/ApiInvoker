# ApiInvoker
Dynamic proxy for accessing APIs with low effort in a package-driven environment.

ApiInvoker allows you to take an interface of an API controller and use it to interact
without needing to manually write HttpClient calls. The interface just needs to be
decorated with the same HttpMethod and binding attributes as the controller is.
In its current state, the controller's routing must be *[controller]/[action]* . 

ApiInvoker is meant for a package-driven environment, where an API provides a Nuget
package containing the interface(s) and entities for its controllers. The consumer then
only needs to provide the base URI for the controller, using the provided IServiceCollection
extension methods. 

IHttpClientBuilder is returned by the extension methods, to allow hooking additional delegates
to control headers, etc of the underlying HttpClient. The Invoke* methods are also all virtual,
so they can be directly overridden if there's need for very special handling for certain APIs.

## Included Example
An API and Client are included as examples. The API has a number of calls available,
and the client is a console app that calls each one and dumps the returned object(s)
to the console window. The API is in a separate solution, so they can be easily run 
side-by-side. 

To test it out, first run the TestAPI in IIS Express. Verify the address matches what is
currently in the TestClient's .AddApiClient call, and adjust if for some reason it hosts
differently on your machine. Then just run the console app and check the output. 

The console app only shows off synchronous calls, since it is synchronous. However,
under the hood, ApiInvoker is async-first, and synchronous calls are just async calls
executed synchronously with .GetAwaiter.GetResult().