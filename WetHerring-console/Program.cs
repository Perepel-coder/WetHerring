using Application;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WetHerring_console;


HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<MainProcess>();
builder.Services.AddSingleton(new DeepMorphyService(withLemmatization: true));

builder.Services.AddApplication();

using IHost _host = builder.Build();

MainProcess? process = _host.Services.GetService<MainProcess>();

if (process is null)
{
    Console.WriteLine("Невозможно создать главный процесс");
    Environment.Exit(0);
}

process.Run();