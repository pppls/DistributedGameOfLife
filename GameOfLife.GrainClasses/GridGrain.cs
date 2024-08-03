using System.Diagnostics;
using System.Text;
using GameOfLife.GrainInterfaces;
using Microsoft.Extensions.Logging;

namespace GameOfLife.GrainClasses;

public class GridGrain : Grain, IGrainBase, IGridGrain
{
    private long[,] cells;

    private int width = 10;
    private int height = 10;
    private bool[,] alive;
    private ILogger<GridGrain> _logger;
    private int responseCount;
    
    public GridGrain(ILogger<GridGrain> logger)
    {
        _logger = logger;
        _logger.LogInformation("Activating ");
        cells = new long[width, height];
        alive = new bool[10,10];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                long id = ((long)x << 32) | (uint)y;
                _logger.LogInformation($"Cell id: {id}");
                cells[x, y] = id;
            }
        }
    }

    public async Task UpdateGrid()
    {
        var tasks = new List<Task>();
        for (long x = 0; x < width; x++)
        {
            for (long y = 0; y < height; y++)
            {
                tasks.Add(GrainFactory.GetGrain<ICellGrain>(cells[x, y]).SendStatus());
            }
        }
        Task.WhenAll(tasks);
    }
    
    public Task<bool> Done()
    {
        if (responseCount == height * width)
        {
            var tasks = new List<Task>();
            responseCount = 0;
            for (long x = 0; x < width; x++)
            {
                for (long y = 0; y < height; y++)
                {
                    tasks.Add(GrainFactory.GetGrain<ICellGrain>(cells[x, y]).SetWasAlive(alive[x,y]));
                }
            }

            Task.WhenAll(tasks);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task CellUpdate(long x, long y, bool isAlive)
    {
        var status = isAlive ? "alive" : "dead";
        // _logger.LogInformation($"cell {x}, {y} is {status}");
        alive[x, y] = isAlive;
        responseCount += 1;
        if (responseCount == height * width)
        {
            // _logger.LogInformation($"grid ready");
            _logger.LogInformation($"{GridToString(alive)}");
        }
        return Task.CompletedTask;
    }
    
    string GridToString(bool[,] grid)
    {
        var builder = new StringBuilder();
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                builder.Append(grid[x, y] ? "X " : ". ");
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
    
    public Task ResetGrid()
    {
        var tasks = new List<Task>();
        for (long x = 0; x < width; x++)
        {
            for (long y = 0; y < height; y++)
            {
                tasks.Add(GrainFactory.GetGrain<ICellGrain>(cells[x,y]).SetWasAlive(alive[x,y]));
            }
        }

        return Task.WhenAll(tasks);
    }
}