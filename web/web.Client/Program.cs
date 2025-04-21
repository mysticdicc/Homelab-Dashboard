using ApexCharts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "RadzenBlazorApp1Theme";
    options.Duration = TimeSpan.FromDays(365);
});

builder.Services.AddScoped(sp =>
{
    NavigationManager navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});

builder.Services.AddTransient<danklibrary.DankAPI.Dash>();
builder.Services.AddTransient<danklibrary.DankAPI.Subnets>();
builder.Services.AddTransient<danklibrary.DankAPI.Monitoring>();

builder.Services.AddApexCharts();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
