using TileTypes;
using UnityEngine;

public class TerrainPerlinGen : MonoBehaviour
{
    private MapConfig currentMap;

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
    }

    public Tile CreateMainTerrain(int x, int y, int xOffset, int yOffset, int magnification)
    {
        //Generate perlin noise value from coordinates input
        float terrainNoise = Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);

        //Thresholds for tile spawns
        if (terrainNoise < 0.10f) return currentMap.mapData.tiles[(int)TileType.DeepWater]; //deepwater
        else if (terrainNoise < 0.20f) return currentMap.mapData.tiles[(int)TileType.Water]; //water
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

