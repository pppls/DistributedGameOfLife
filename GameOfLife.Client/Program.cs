using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GameOfLife.GrainInterfaces;
IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();
using IHost host = builder.Build();
await host.StartAsync();
IClusterClient client = host.Services.GetRequiredService<IClusterClient>();
IGridGrain grid = client.GetGrain<IGridGrain>(0);

Console.WriteLine("Starting program");
while (true)
{
    await grid.UpdateGrid();
    while (!(await grid.Done()))
    {
        await Task.Delay(100); // Wait for 100 milliseconds before checking again
    }
    // Optionally, add a delay here if you want to control how often the grid updates
    await Task.Delay(1000); // Wait for 1 second before the next grid update
}


