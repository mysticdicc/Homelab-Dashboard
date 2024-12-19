using dankservice;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.Run();