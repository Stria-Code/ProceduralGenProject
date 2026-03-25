using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    private MapConfig currentMap;
    private TerrainPerlinGen perlinGen;

    [SerializeField] RawImage miniMap;

    public Tile[,] noiseGrid { get; private set; }

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        perlinGen = GetComponent<TerrainPerlinGen>();
    }

    public bool CheckIfInBoundaries(int x, int y)
    {
        if (x < 0 || y < 0 || x >= currentMap.mapData.width || y >= currentMap.mapData.height) return false;

        return true;
    }

    public void SetTileAtPos(int x, int y, TileData replacementTile)
    {

       Tile t =  noiseGrid[x, y].GetComponent<Tile>();

        if (t == null) return;
        
        t.spriteRenderer.color = replacementTile.colour;
       // SpriteRenderer sr = noiseGrid[x, y].
      //  Debug.Log(sr.gameObject.name);
      //  sr.color = Color.black;
      //  noiseGrid[x, y] = replacementTile;
        //noiseGrid[x, y].spriteRenderer = sr;
        // Add other transferable tile bits you need
     //   sr.color = noiseGrid[x, y].colour;
    }

    public void CreateNoiseGrid()// is 2nd pass 
    {
        noiseGrid = new Tile[currentMap.mapData.width, currentMap.mapData.height];

        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                noiseGrid[x, y] = new Tile();
                noiseGrid[x, y].tileData = new TileData();
               // SpriteRenderer sr = noiseGrid[x, y].spriteRenderer;
                noiseGrid[x, y].tileData = perlinGen.GetTerrainTile(x, y); //
                
            }
        }

        //miniMap.texture = Map.CreateTexture(noiseGrid);

    }
}
