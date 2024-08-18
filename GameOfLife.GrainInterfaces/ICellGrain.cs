using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace GameOfLife.GrainInterfaces;

public interface ICellGrain : IGrainWithIntegerKey
{
    [ReadOnly]
    Task<bool> SendStatus(int h, int w);

    Task SetWasAlive(bool alive);
    
    [ReadOnly]
    Task<bool> GetStatus();
}