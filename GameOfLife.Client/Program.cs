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
bool success = false;
while (!success)
{
    try
    {
        await host.StartAsync();
        success = true; // If StartAsync succeeds, set success to true to exit the loop
    }
    catch (Exception)
    {
        await Task.Delay(100); // Wait for 1 second before retrying
    }
}
IClusterClient client = host.Services.GetRequiredService<IClusterClient>();
IGridGrain grid = client.GetGrain<IGridGrain>(0);

Console.WriteLine("Starting program");
var height = 100;
var width = 100;
bool[,] randomBools = new bool[height, height];
Random rand = new Random();

for (int i = 0; i < height; i++)
{
    for (int j = 0; j < height; j++)
    {
        randomBools[i, j] = rand.Next(2) == 1;
    }
}

var c = 0;

while (true)
{
    randomBools = await grid.UpdateGrid(randomBools);
    Console.WriteLine(++c);
    //Console.WriteLine(UpdateGrid(randomBools));// Example board, replace with your actual board

    for (int i = 0; i < randomBools.GetLength(0); i++)
    {
        for (int j = 0; j < randomBools.GetLength(1); j++)
        {
            Console.Write(randomBools[i, j] ? "1 " : ". ");
        }
        Console.WriteLine();
    }

}

async Task<bool[,]> UpdateGrid(bool[,] aliveInput)
{
    int height = aliveInput.GetLength(0); // Number of rows
    int width = aliveInput.GetLength(1);

        
    var tasks1 = new List<Task>();
    for (long x = 0; x < width; x++)
    {
        for (long y = 0; y < height; y++)
        {
            long localX = x;
            long localY = y;
            long id = ((long)x << 32) | (uint)y;
            tasks1.Add(client.GetGrain<ICellGrain>(id).SetWasAlive(aliveInput[localX,localY]));
        }
    }
    await Task.WhenAll(tasks1);
        
    var tasks = new List<Task>();
    var aliveOutput = new bool[height,width];
    for (long x = 0; x < width; x++)
    {
        for (long y = 0; y < height; y++)
        {
            long id = ((long)x << 32) | (uint)y;
            long localX = x;
            long localY = y;
            var f = async (Task<bool> t) =>
            {
                aliveOutput[localX, localY] = await t;
            };
            var t = f(client.GetGrain<ICellGrain>(id).SendStatus(height, width));
            tasks.Add(t);
        }
    }
        
    await Task.WhenAll(tasks);
        
    return aliveOutput;
}
