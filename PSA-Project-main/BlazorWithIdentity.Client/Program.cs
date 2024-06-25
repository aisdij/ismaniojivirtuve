using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Project.Frontend.Services;
using Blazored.SessionStorage;
using Project.Frontend.Website;
using Project.Frontend.Website.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();

// Custom Services go here
builder.Services.AddBlazoredSessionStorageAsSingleton();
builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IWebsiteHttpClient, WebsiteHttpClient>();
// Custom Services End Here 

builder.Services.AddSingleton<HttpClient>(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();