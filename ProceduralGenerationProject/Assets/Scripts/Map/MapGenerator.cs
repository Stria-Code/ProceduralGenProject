using System.Collections;
using System.Collections.Generic;
using TileTypes;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private MapConfig currentMap;
    private ResourcesGen resourcesGen;
    private PlayerSpawnGen playerSpawn;
    private GridGenerator gridGenerator;

    [SerializeField] Transform player;

    //Dictonary for groups of tiles
    //Dictionary <int, GameObject> tileGroups;

    //Tiles
    [SerializeField] GameObject defaultTile;

    //Helper directions array
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        gridGenerator = GetComponent<GridGenerator>();
        resourcesGen = GetComponent<ResourcesGen>();
        playerSpawn = GetComponent<PlayerSpawnGen>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridGenerator.CreateNoiseGrid();
        StartCoroutine(nameof(LateStart));
 


    }
      IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1f);
        resourcesGen.ApplyResources();

        ConfigureClusters(currentMap.mapData.tiles[(int)TileType.Siberiums], 15, 300, currentMap.mapData.tiles[(int)TileType.Grass]);
        ConfigureClusters(currentMap.mapData.tiles[(int)TileType.Ironium], 15, 300, currentMap.mapData.tiles[(int)TileType.Grass]);
        ConfigureClusters(currentMap.mapData.tiles[(int)TileType.Water], 10, 5000, currentMap.mapData.tiles[(int)TileType.Grass]);


        //BackUP cluster creation for tiles if none available on the map
        //***
        CreateClusters(currentMap.mapData.tiles[(int)TileType.Water], currentMap.mapData.tiles[(int)TileType.Grass]);
        CreateClusters(currentMap.mapData.tiles[(int)TileType.Siberiums], currentMap.mapData.tiles[(int)TileType.Grass]);
        CreateClusters(currentMap.mapData.tiles[(int)TileType.Ironium], currentMap.mapData.tiles[(int)TileType.Grass]);
        //***

        FindAndUpdateEdges();

        BuildMapFromGrid();

        SpawnPlayer();
       
         yield return null;
    }
    void CreateTile(TileData tile, int x, int y)
    {
        //obj related 
        GameObject g =  Instantiate(defaultTile);
        g.name = string.Format("tile_x{0}_y{1}", x, y);
        g.transform.localPosition = new Vector3(x, y, 0);


       // link on a tile controller for tracking
        Tile t = g.AddComponent<Tile>();
        t.spriteRenderer = g.GetComponent<SpriteRenderer>();
        t.spriteRenderer.color = tile.colour;
        t.pos = new Vector2(x, y);   
      //  t.tile.colour = tile.colour;  // seb, decide if you want to only change appearance or data in the tile obj

        t.tileData = tile;

        gridGenerator.tileGrid[x,y] = t;

      
    }
 
   
    void SpawnPlayer()
    {
        Vector2Int spawn = playerSpawn.FindPlayerSpawnArea();

        //Moves the player
        player.position = new Vector3(spawn.x, spawn.y, 0);
    }

    void FindAndUpdateEdges()
    {
        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                if (x < 1 || y < 1 || x >= currentMap.mapData.width - 1 || y >= currentMap.mapData.height - 1)
                {
                    gridGenerator.tileGrid[x, y].tileData = currentMap.mapData.tiles[(int)TileType.Shroud];
                }
            }
        }
    }


    //FloodFill algorithm to check for all the connected tiles of the same type in a cluster
    List<Vector2Int> FloodFill(int x, int y, int tileID, bool[,] visited)
    {
        //List of tiles in a cluster
        List<Vector2Int> cluster = new List<Vector2Int>();

        //Waiting list for checking tiles
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        //Start at the first tile and mark it as visited to avoid infinite looping
        queue.Enqueue(new Vector2Int(x, y));
        visited[x, y] = true;

        //If there are tiles unchecked in the queue keep looping
        while (queue.Count > 0)
        {
            //Take a tile out of the queue and add it to the current cluster
            Vector2Int currentTile = queue.Dequeue();
            cluster.Add(currentTile);

            foreach (Vector2Int direction in directions)
            {
                int newX = currentTile.x + direction.x;
                int newY = currentTile.y + direction.y;

                if (!gridGenerator.CheckIfInBoundaries(newX, newY)) continue;

                //If this tile has not been checked and is of the same type
                if (!visited[newX, newY] && gridGenerator.tileGrid[newX, newY].tileData.ID == tileID)
                {
                    //Mark it as visited and add it to the queue to look at its neighbours later
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }

        return cluster;
    }

    void ConfigureClusters(TileData clusterTile, int minSize, int maxSize, TileData replacementTile)
    {
        bool[,] visited = new bool[currentMap.mapData.width, currentMap.mapData.height];

        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                //Check to see if a tile has not been visited and is of the required type
                if (!visited[x, y] && gridGenerator.tileGrid[x, y].tileData.ID == clusterTile.ID)
                {
                    //Use floodfill algorithm to get the full cluster
                    List<Vector2Int> cluster = FloodFill(x, y, clusterTile.ID, visited);

                    if (cluster.Count < minSize || cluster.Count > maxSize)
                    {
                        //If its not, replace the cluster with another specified type of tiles
                        foreach (Vector2Int tile in cluster)
                        {
                            gridGenerator.tileGrid[tile.x, tile.y].tileData = replacementTile;
                        }
                    }
                }
            }

        }
    }

    void BuildMapFromGrid()
    {
        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                CreateTile(gridGenerator.tileGrid[x, y].tileData, x, y);
            }
        }
    }


    //Backup cluster generation
    void CreateClusters(TileData replacementTile, TileData tileToReplace)
    {
        if (!FindTile(replacementTile))
        {
            bool hasCreated = false;

            while (!hasCreated)
            {
                int x = Random.Range(0, currentMap.mapData.width);
                int y = Random.Range(0, currentMap.mapData.height);

                if (!hasCreated && gridGenerator.tileGrid[x, y].tileData.ID == tileToReplace.ID)
                {
                    gridGenerator.tileGrid[x, y].tileData = replacementTile;

                    foreach (Vector2Int direction in directions)
                    {
                        int newX = x + direction.x;
                        int newY = y + direction.y;

                        if (newX < 0 || newY < 0 || newX >= currentMap.mapData.width || newY >= currentMap.mapData.height) continue;

                        if (gridGenerator.tileGrid[newX, newY].tileData.ID == tileToReplace.ID)
                        {
                            gridGenerator.tileGrid[newX, newY].tileData = replacementTile;
                        }

                        foreach (Vector2Int secondDirection in directions)
                        {
                            int newX2 = newX + secondDirection.x;
                            int newY2 = newY + secondDirection.y;

                            if (newX2 < 0 || newY2 < 0 || newX2 >= currentMap.mapData.width || newY2 >= currentMap.mapData.height) continue;

                            if (gridGenerator.tileGrid[newX2, newY2].tileData.ID == tileToReplace.ID)
                            {
                                gridGenerator.tileGrid[newX2, newY2].tileData = replacementTile;
                            }
                        }
                    }

                    hasCreated = true;

                }
            }
        }
    }
    bool FindTile(TileData tile)
    {
        bool doesExist = false;

        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                if (gridGenerator.tileGrid[x, y].tileData.ID == tile.ID)
                {
                    doesExist = true;
                }
            }
        }

        return doesExist;
    }

}

