using TileTypes;
using UnityEngine;

public class TerrainPerlinGen : MonoBehaviour
{
    private MapConfig currentMap;

    [SerializeField] int seed;
    [SerializeField] int magnification;

    //Reduce = move terrain down / Increase = move terrain up
    //Reduce = move terrain left / Increase = move terrain right
    private int xOffset, yOffset;

    private System.Random random = new System.Random();
    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();

        if (seed != 0)
        {
            random = new System.Random(seed);
        }
        else
        {
            random = new System.Random();
        }

        xOffset = random.Next(-10000, 10000);
        yOffset = random.Next(-10000, 10000);
        Debug.Log("xOffset: " + xOffset + " , " + "yOffset " + yOffset);
    }

    public TileData GetTerrainTile(int x, int y)
    {
        float xCoord = (float)x / currentMap.mapData.width * magnification + xOffset;
        float yCoord = (float)y / currentMap.mapData.height * magnification + yOffset;

        //Generate perlin noise value from coordinates input
        float terrainNoise = Mathf.PerlinNoise(xCoord, yCoord);
        Debug.Log(x+" "+y+" "+terrainNoise);

        //Thresholds for tile spawns
        if (terrainNoise < currentMap.mapData.threshold[(int)TileType.DeepWater]) return currentMap.mapData.tiles[(int)TileType.DeepWater]; //deepwater
        else if (terrainNoise < currentMap.mapData.threshold[(int)TileType.Water]) return currentMap.mapData.tiles[(int)TileType.Water]; //water
        return currentMap.mapData.tiles[(int)TileType.Grass]; //land
    }
}

/* void ApplyResources()
{
 for (int x = 0; x < width; x++)
 {
     for (int y = 0; y < height; y++)
     {
         Tile baseTile = noiseGrid[x, y];

         if (baseTile.ID == tiles[(int)TileType.DeepWater].ID || baseTile.ID == tiles[(int)TileType.Water].ID) continue;

         float resourceNoise = Mathf.PerlinNoise((x + xOffset) / magnification,(y + yOffset) / magnification);

         if (baseTile.ID == tiles[(int)TileType.Grass].ID)
         {
             if (resourceNoise < 0.05f) noiseGrid[x, y] = tiles[(int)TileType.Siberiums];
             else if (resourceNoise < 0.15f) noiseGrid[x, y] = tiles[(int)TileType.Ironium];
         }
     }
 }
}*/

