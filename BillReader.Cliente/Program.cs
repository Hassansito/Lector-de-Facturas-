using BillReader.Cliente;
using BillReader.Cliente.Services;
using BillReader.Cliente.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5221/")
    };
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
    return client;
});
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
await builder.Build().RunAsync();
