using web.Client.Pages;
using web.Components;
using web.Controllers;
using dankweb.API;

using ApexCharts;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped(sp =>
{
    NavigationManager navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});

//builder.Services.AddTransient(sp => new HttpClient());

builder.Services.AddTransient<danklibrary.DankAPI.Dash>();
builder.Services.AddTransient<danklibrary.DankAPI.Subnets>();
builder.Services.AddTransient<danklibrary.DankAPI.Monitoring>();

builder.Services.AddFluentUIComponents();
builder.Services.AddApexCharts();

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<danknetContext>(options =>
    options.UseSqlite("Data Source=./data/danknetlocal.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials()
);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(web.Client._Imports).Assembly);

app.MapControllers();

app.Run();
