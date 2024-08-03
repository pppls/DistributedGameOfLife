using Orleans;

namespace GameOfLife.GrainInterfaces;

public interface IGridGrain : IGrainWithIntegerKey
{
    Task<bool> Done();
    
    Task UpdateGrid();
    Task ResetGrid();

    public Task CellUpdate(long x, long y, bool isAlive);
}