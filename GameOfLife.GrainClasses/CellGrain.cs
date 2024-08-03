using GameOfLife.GrainInterfaces;
using Microsoft.Extensions.Logging;

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
                .Where(coord => coord is { x: >=0, x: < 10, y: >= 0, y: < 10 })
                .ToList();
        // Get references to neighbor grains
        foreach (var (nx, ny) in filteredNeighborPositions)
        {
            long neighborId = ((long)nx << 32) | (uint)ny;
            neighbors.Add(neighborId);
        }
        // _logger.LogInformation($"Cell coord: {x}, {y}");
        // filteredNeighborPositions.ForEach(coord => _logger.LogInformation($"Neighbor coord: {coord.x}, {coord.y}"));
        //
        // //_logger.LogInformation($"Cell ID: {this.GetPrimaryKeyLong()}");
        // logger.LogInformation($"neighbourcount: {neighbors.Count()}");
    }
    
    public Task SendStatus()
    {
        // _logger.LogInformation($"cell {x}, {y} sending alive status");
        Task.WhenAll(neighbors.Select(n => GrainFactory.GetGrain<ICellGrain>(n).ReceiveNeighborStatus(this._wasAlive)));
        return Task.CompletedTask;
    }

    public Task SetWasAlive(bool wasAlive)
    {
        _wasAlive = wasAlive;
        return Task.CompletedTask;
    }
    
    public Task ReceiveNeighborStatus(bool alive)
    {
        // _logger.LogInformation($"cell {x}, {y} receiving neighbour status");
        neighborUpdatesReceived += 1;
        if (alive)
        {
            neighborAliveSum += 1; 
        }

        if (neighborUpdatesReceived == neighbors.Count())
        {
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

            GrainFactory.GetGrain<IGridGrain>(0).CellUpdate(this.x, this.y, f(neighborAliveSum));
            neighborUpdatesReceived = 0;
            neighborAliveSum = 0;
        }

        return Task.CompletedTask;
    }
}