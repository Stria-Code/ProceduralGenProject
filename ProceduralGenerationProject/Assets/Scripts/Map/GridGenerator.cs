using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    private MapConfig currentMap;
    private TerrainPerlinGen perlinGen;

    [SerializeField] RawImage miniMap;

    public Tile[,] tileGrid { get; private set; }

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        perlinGen = GetComponent<TerrainPerlinGen>();

        tileGrid = new Tile[currentMap.mapData.width, currentMap.mapData.height];
    }

    public bool CheckIfInBoundaries(int x, int y)
    {
        if (x < 0 || y < 0 || x >= currentMap.mapData.width || y >= currentMap.mapData.height) return false;

        return true;
    }

    public void SetTile(TileData replacementTile)
    { 

    }

    public void SetTileAtPos(int x, int y, TileData replacementTile)
    {
        if (!CheckIfInBoundaries(x, y)) return;

        Tile newTile = tileGrid[x, y].GetComponent<Tile>();

        if (newTile == null) return;
        
        newTile.tileData = replacementTile;
        newTile.spriteRenderer.color = replacementTile.colour;

    }

    public void CreateNoiseGrid()// is 2nd pass 
    {
        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                tileGrid[x, y] = new Tile();

               // SpriteRenderer sr = noiseGrid[x, y].spriteRenderer;
                tileGrid[x, y].tileData = perlinGen.GetTerrainTile(x, y); //
                
            }
        }

        miniMap.texture = Map.CreateTexture(tileGrid);

    }
}
