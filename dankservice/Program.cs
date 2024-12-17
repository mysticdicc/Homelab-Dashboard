using dankservice;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7124") });
builder.Services.AddTransient<danklibrary.DankAPI.Subnets>();

var host = builder.Build();
host.Run();
