using Orleans;
using Orleans.Runtime;

namespace GameOfLife.GrainInterfaces;

public interface ICellGrain : IGrainWithIntegerKey
{
    Task SendStatus();

    Task SetWasAlive(bool alive);
    
    Task ReceiveNeighborStatus(bool alive);
}