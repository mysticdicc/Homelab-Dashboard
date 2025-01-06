using ApexCharts;
using dankweb;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddFluentUIComponents();
builder.Services.AddApexCharts();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7124") });
builder.Services.AddTransient<danklibrary.DankAPI.Dash>();
builder.Services.AddTransient<danklibrary.DankAPI.Subnets>();
builder.Services.AddTransient<danklibrary.DankAPI.Monitoring>();



await builder.Build().RunAsync();
