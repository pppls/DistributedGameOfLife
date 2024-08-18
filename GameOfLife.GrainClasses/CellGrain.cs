using System.Diagnostics;
using GameOfLife.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;

namespace GameOfLife.GrainClasses;

public class CellGrain : Grain, IGrainBase, ICellGrain 
{
    private List<long> neighbors;
    private bool _wasAlive;
    private ILogger<CellGrain> _logger;
    private int neighborAliveSum;
    private int neighborUpdatesReceived;
    
    private long x;
    private long y;
    
    public CellGrain(ILogger<CellGrain> logger)
    {
        _logger = logger;
        _wasAlive = new Random().Next(2) == 1;
        neighbors = new List<long>();
        x = this.GetPrimaryKeyLong() >> 32;
        y = (int)(this.GetPrimaryKeyLong() & 0xFFFFFFFF);

        // Calculate neighbor positions
        var neighborPositions = new List<(long x, long y)>
        {
            (x - 1, y - 1), (x - 1, y), (x - 1, y + 1),
            (x, y - 1), (x, y + 1),
            (x + 1, y - 1), (x + 1, y), (x + 1, y + 1)
        };

        var filteredNeighborPositions =
            neighborPositions
                .Where(coord => coord is { x: >=0, x: < 100, y: >= 0, y: < 100 })
                .ToList();
        // Get references to neighbor grains
        foreach (var (nx, ny) in filteredNeighborPositions)
        {
            long neighborId = ((long)nx << 32) | (uint)ny;
            neighbors.Add(neighborId);
        }

    }
    
    public async Task<bool> SendStatus(int height, int width)
    {
        var tasks = await Task.WhenAll(neighbors.Select(n => GrainFactory.GetGrain<ICellGrain>(n).GetStatus()));
        Func<int, bool> f = (sumAlive) =>
        {
            switch (sumAlive)
            {
                case 0:
                case 1:
                    return false;
                case 2:
                    return _wasAlive;
                case 3:
                    return true;
                case > 4:
                    return false;
            }

            return false;
        };
        var isAlive = f(tasks.Count(x => x));
        return isAlive;
    }
    
    public Task<bool> GetStatus()
    {
        return Task.FromResult(_wasAlive);
    }
    
    public Task SetWasAlive(bool wasAlive)
    {
        _wasAlive = wasAlive;
        return Task.CompletedTask;
    }
}