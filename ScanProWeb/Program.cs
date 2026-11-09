using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ScanProWeb;
using ScanProWeb.Services.OrdenCompra;
using BlazorBootstrap;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("API", c => c.BaseAddress = new Uri("http://localhost:5220/"));
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<ApiProductService>();
builder.Services.AddScoped<ApiConfigService>();
builder.Services.AddScoped<ApiUserService>();
builder.Services.AddScoped<ApiEnterpriseService>();
builder.Services.AddScoped<ApiLabelService>();
builder.Services.AddScoped<IOrdenCompraService, OrdenCompraService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
