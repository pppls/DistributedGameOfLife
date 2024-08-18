using System.Diagnostics;
using System.Text;
using GameOfLife.GrainInterfaces;
using Microsoft.Extensions.Logging;

namespace GameOfLife.GrainClasses;

public class GridGrain : Grain, IGrainBase, IGridGrain
{
    private ILogger<GridGrain> _logger;
    private int _count;
    public GridGrain(ILogger<GridGrain> logger)
    {
        _count = 0;
        _logger = logger;
        _logger.LogInformation("Activating ");

    }

    public async Task<(int, bool[,])> UpdateGrid(bool[,] aliveInput)
    {
        _count += 1;
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
                tasks1.Add(GrainFactory.GetGrain<ICellGrain>(id).SetWasAlive(aliveInput[localX,localY]));
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
                var t = f(GrainFactory.GetGrain<ICellGrain>(id).SendStatus(height, width));
                tasks.Add(t);
            }
        }
        
        await Task.WhenAll(tasks);
        
        return (_count, aliveOutput);
    }
}