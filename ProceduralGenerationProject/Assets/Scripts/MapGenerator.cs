using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.EventSystems.EventTrigger;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] RawImage miniMap;

    //Dictonary for groups of tiles
    Dictionary <int, GameObject> tileGroups;

    [SerializeField] Tile[] tiles;

    Texture2D texture;

    //Tiles
    [SerializeField] GameObject defaultTile;
    [SerializeField] Transform player;


    //Adjustables in the inspector
    [SerializeField] int seed; 
    [SerializeField] private MapSize mapSize;
    [SerializeField] private int smallWidth, smallHeight;
    [SerializeField] private int mediumWidth, mediumHeight;
    [SerializeField] private int largeWidth, largeHeight;
    [SerializeField] private int smallMapResMinAmount, smallMapResMaxAmount;
    [SerializeField] private int medMapResMinAmount, medMapResMaxAmount;
    [SerializeField] private int largeMapResMinAmount, largeMapResMaxAmount;
    [SerializeField] private int candidatesToCheckForResourcePlacement;
    [SerializeField] float magnification;

    //Map Sizes
    enum MapSize
    {
        Small,
        Medium,
        Large
    }

    //Tile Names
    enum TileType
    {
        Grass,
        DeepWater,
        Water,
        Shroud,
        Siberiums,
        Ironium
    }

    //Grid Size
    public int width { get; private set; }
    public int height { get; private set; }

    public Tile[,] noiseGrid { get; private set; }

    int xOffset = 0; //Reduce = move terrain left / Increase = move terrain right
    int yOffset = 0; //Reduce = move terrain down / Increase = move terrain up

    int resMinAmount, resMaxAmount;

    //Helper directions array
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private System.Random random = new System.Random();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (seed != 0)
        {
            random = new System.Random(seed);
        }
        else
        {
            random = new System.Random();
        }

        noiseGrid = MapSizeCreator();
        xOffset = random.Next(-10000, 10000);
        yOffset = random.Next(-10000, 10000);
        Debug.Log("xOffset: " + xOffset + " , " + "yOffset " + yOffset);

       // CreateGroups();
        CreateNoiseGrid();
        ApplyResources();

        ConfigureClusters(tiles[(int)TileType.Siberiums], 15, 300, tiles[(int)TileType.Grass]);
        ConfigureClusters(tiles[(int)TileType.Ironium], 15, 300, tiles[(int)TileType.Grass]);
        ConfigureClusters(tiles[(int)TileType.Water], 10, 5000, tiles[(int)TileType.Grass]);


        //BackUP cluster creation for tiles if none available on the map
        //***
        CreateClusters(tiles[(int)TileType.Water], tiles[(int)TileType.Grass]);
        CreateClusters(tiles[(int)TileType.Siberiums], tiles[(int)TileType.Grass]);
        CreateClusters(tiles[(int)TileType.Ironium], tiles[(int)TileType.Grass]);

        //***

        FindAndUpdateEdges();

        BuildMapFromGrid();

        miniMap.texture = Map.CreateTexture(noiseGrid);

        SpawnPlayer();
    }

    Tile[,] MapSizeCreator()
    {
        switch (mapSize)
        {
            case MapSize.Small:

                width = smallWidth;
                height = smallHeight;
                resMinAmount = smallMapResMinAmount;
                resMaxAmount = smallMapResMaxAmount;
                break;

            case MapSize.Medium:

                width = mediumWidth;
                height = mediumHeight;
                resMinAmount = medMapResMinAmount;
                resMaxAmount= medMapResMaxAmount;
                break;

            case MapSize.Large:

                width = largeWidth;
                height = largeHeight;
                resMinAmount= largeMapResMinAmount;
                resMaxAmount= largeMapResMaxAmount;
                break;

            default:

                width = 100;
                height = 100;
                resMinAmount = smallMapResMinAmount;
                resMaxAmount = smallMapResMaxAmount;
                break;
        
        }

        return new Tile[width, height];
    }

    public bool CheckIfInBoundaries(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return false;

        return true;
    }

    /*void CreateGroups()
    {
        //Create a new group in the inspector for each of the tile types
        tileGroups = new Dictionary<int, GameObject>();

        foreach (KeyValuePair<int, GameObject> key in tileSet)
        {
            //Create an empty parent object for this tile type
            GameObject tileGroup = new GameObject(key.Value.name);

            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector3(0, 0, 0);

            //Store it by tile ID
            tileGroups.Add(key.Key, tileGroup);
        }
    }*/

    void CreateTile(Tile tile, int x, int y)
    {
        //GameObject tileGroup = tileGroups[tile.ID];

        //Creates a tile using the passed in tile ID to make out which type of tile it should create
        //Groups it in the inspector with similar tiles
        GameObject tileObj = Instantiate(defaultTile);
        tileObj.GetComponent<SpriteRenderer>().color = tile.colour;
        tileObj.AddComponent<TileController>().tile = tile;

        //Inspector naming format for ease of access to the tile - coordinates mentioned in the name
        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tileObj.name= tile.name;
        //Gives its position
        tileObj.transform.localPosition = new Vector3(x, y, 0);
    }

    Tile CreateMainTerrain(int x, int y)
    {
        //Generate perlin noise value from coordinates input
        float terrainNoise = Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);

        //Thresholds for tile spawns
        if (terrainNoise < 0.10f) return tiles[(int)TileType.DeepWater]; //deepwater
        else if (terrainNoise < 0.20f) return tiles[(int)TileType.Water]; //water
        return tiles[(int)TileType.Grass]; //land
    }

    void CreateNoiseGrid()
    {
        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseGrid[x, y] = CreateMainTerrain(x, y);
            }
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


    void ApplyResources()
    {
        int resAmount = random.Next(resMinAmount, resMaxAmount);

        //Siberium deposits
        List<Vector2Int> siberiumNodes = GenerateBestCandidatePoints(resAmount, 50, tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in siberiumNodes)
        {
            int radius = random.Next(tiles[(int)TileType.Siberiums].minClusterRadius, tiles[(int)TileType.Siberiums].maxClusterRadius);
            GrowResourceCluster(pos, tiles[(int)TileType.Siberiums], tiles[(int)TileType.Grass], radius);
        }

        //Ironium deposits
        List<Vector2Int> ironiumNodes = GenerateBestCandidatePoints(resAmount, 50, tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in ironiumNodes)
        {
            int radius = random.Next(tiles[(int)TileType.Ironium].minClusterRadius, tiles[(int)TileType.Ironium].maxClusterRadius);
            GrowResourceCluster(pos, tiles[(int)TileType.Ironium], tiles[(int)TileType.Grass], radius);
        }
    }

    void GrowResourceCluster(Vector2Int center, Tile resource, Tile biomeToCheck, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = center.x + x;
                int ny = center.y + y;

                if (!CheckIfInBoundaries(nx, ny)) continue;

                if (noiseGrid[nx, ny].ID == biomeToCheck.ID)
                {
                    //Calculate the Euclidean distance - diagonal distance
                    float distance = Mathf.Sqrt(x * x + y * y);

                    //Only populate the space if the value is within the circle 
                    if (distance <= radius && Random.value > 0.3f) //added a chance to not spawn tiles so they are distributed unevenly
                    {
                        noiseGrid[nx, ny] = resource;
                    }
                }
            }
        }
    }

    //Mitchells best candidate algorithm
    List<Vector2Int> GenerateBestCandidatePoints(int count, int candidates, Tile biomeToCheck)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        //Starting point
        points.Add(new Vector2Int(Random.Range(0, width), Random.Range(0, height)));

        for (int i = 1; i < count; i++)
        {
            Vector2Int bestCandidate = Vector2Int.zero;
            //-1f so the first candidate is always valid
            float bestDistance = -1f;

            for (int c = 0; c < candidates; c++)
            {
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);

                if (noiseGrid[x, y].ID != biomeToCheck.ID) continue;

                //maxValue so the first candidate is always valid
                float minDist = float.MaxValue;

                foreach (Vector2Int p in points)
                {
                    float distance = Vector2Int.Distance(new Vector2Int(x, y), p);
                    if (distance < minDist) minDist = distance; //Now next check is gonna be distance < previous distance
                }


                if (minDist > bestDistance)
                {
                    bestDistance = minDist;
                    bestCandidate = new Vector2Int(x, y);
                }
            }

            points.Add(bestCandidate);
        }

        return points;
    }

    Vector2Int FindPlayerSpawnArea()
    {
        for (int i = 0; i < 100; i++)
        {
            //Random Position
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            //Check if the tile is land
            if (noiseGrid[x, y].ID == tiles[(int)TileType.Grass].ID) return new Vector2Int(x, y);
        }

        //Center of map - backup positioning
        return new Vector2Int(width/2, height/2);
    }

    void BuildMapFromGrid()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                CreateTile(noiseGrid[x, y], x, y);
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

            //Check each neighbour
            foreach(Vector2Int direction in directions)
            {
                int newX = currentTile.x + direction.x;
                int newY = currentTile.y + direction.y;

                //If its outside map bounds, skip it
                if (!CheckIfInBoundaries(newX, newY)) continue;

                //If this tile has not been checked and is of the same type
                if(!visited[newX, newY] && noiseGrid[newX, newY].ID == tileID)
                {
                    //Mark it as visited and add it to the queue to look at its neighbours later
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }

        return cluster;
    }

    void ConfigureClusters(Tile clusterTile, int minSize, int maxSize, Tile replacementTile)
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Check to see if a tile has not been visited and is of the required type
                if (!visited[x,y] && noiseGrid[x, y].ID == clusterTile.ID)
                {
                    //Use floodfill algorithm to get the full cluster
                    List<Vector2Int> cluster = FloodFill(x, y, clusterTile.ID, visited);

                    //Check if its within the required sizes
                    if (cluster.Count < minSize || cluster.Count > maxSize)
                    {
                        //If its not, replace the cluster with another specified type of tiles
                        foreach(Vector2Int tile in cluster)
                        {
                            noiseGrid[tile.x, tile.y] = replacementTile;
                        }
                    }
                }
            }

        }
    }

    //Function to find if a certain tile exists anywhere on the map
    bool FindTile(Tile tile)
    {
        bool doesExist = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (noiseGrid[x, y].ID == tile.ID)
                {
                    doesExist = true;
                }
            }
        }

        return doesExist;
    }

    void CreateClusters(Tile replacementTile, Tile tileToReplace)
    {
        if (!FindTile(replacementTile))
        {
            bool hasCreated = false;

            while (!hasCreated)
            { 
                //Random Position
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);

                //Check if the tile is land
                if (!hasCreated && noiseGrid[x, y].ID == tileToReplace.ID)
                {
                    noiseGrid[x, y] = replacementTile;

                    foreach (Vector2Int direction in directions)
                    {
                        int newX = x + direction.x;
                        int newY = y + direction.y;

                        if (newX < 0 || newY < 0 || newX >= width || newY >= height) continue;

                        if (noiseGrid[newX, newY].ID == tileToReplace.ID)
                        {
                            noiseGrid[newX, newY] = replacementTile;
                        }

                        foreach (Vector2Int secondDirection in directions)
                        {
                            int newX2 = newX + secondDirection.x;
                            int newY2 = newY + secondDirection.y;

                            if (newX2 < 0 || newY2 < 0 || newX2 >= width || newY2 >= height) continue;

                            if (noiseGrid[newX2, newY2].ID == tileToReplace.ID)
                            {
                                noiseGrid[newX2, newY2] = replacementTile;
                            }
                        }
                    }
                  
                    hasCreated = true;
                    
                }
            }
        }
    }

    void FindAndUpdateEdges()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(x < 1 || y < 1 || x >= width - 1 || y >= height - 1)
                {
                    noiseGrid[x, y] = tiles[(int)TileType.Shroud];
                }
            }
        }
    }

    void SpawnPlayer()
    {
        Vector2Int spawn = FindPlayerSpawnArea();

        //Moves the player
        player.position = new Vector3(spawn.x, spawn.y, 0);
    }
}
