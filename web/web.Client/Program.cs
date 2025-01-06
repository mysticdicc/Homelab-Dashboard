using ApexCharts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp =>
{
    NavigationManager navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});

builder.Services.AddTransient<danklibrary.DankAPI.Dash>();
builder.Services.AddTransient<danklibrary.DankAPI.Subnets>();
builder.Services.AddTransient<danklibrary.DankAPI.Monitoring>();

builder.Services.AddFluentUIComponents();
builder.Services.AddApexCharts();

await builder.Build().RunAsync();
