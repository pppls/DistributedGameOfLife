using Orleans;

namespace GameOfLife.GrainInterfaces;

public interface IGridGrain : IGrainWithIntegerKey
{
    Task<(int, bool[,])> UpdateGrid(bool[,] aliveInput);
}