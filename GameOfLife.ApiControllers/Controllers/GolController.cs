using GameOfLife.GrainInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.ApiControllers.Controllers;

[ApiController]
[Route("[controller]")]
public class GolController : Controller
{
    private IGrainFactory _grainFactory; 
    public GolController(IGrainFactory grains)
    {
        _grainFactory = grains;
    }
    // GET
    [HttpGet]
    public async IAsyncEnumerable<List<List<bool>>> Index()
    {
        IGridGrain grid = _grainFactory.GetGrain<IGridGrain>(0);
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

        int count = 0;
        while (true)
        {
            (count, randomBools) = await grid.UpdateGrid(randomBools);
            List<List<bool>> listOfLists = new List<List<bool>>();

            for (int i = 0; i < randomBools.GetLength(0); i++)
            {
                List<bool> row = new List<bool>();
                for (int j = 0; j < randomBools.GetLength(1); j++)
                {
                    row.Add(randomBools[i, j]);
                }
                listOfLists.Add(row);
            }

            var result = listOfLists;
            yield return result;
        }
    }
}

