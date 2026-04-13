using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

using TileTypes;

public class ResourcesGen : MonoBehaviour
{
    private MapConfig currentMap;
    private GridGenerator gridGenerator;
    private PointsGen pointsGen;

    //Adjustable Cluster Sizes
    [SerializeField] int minClusterRadiusIronium, maxClusterRadiusIronium;
    [SerializeField] int minClusterRadiusSiberiums, maxClusterRadiusSiberiums;

    //Adjustables in the inspector
    [SerializeField] int candidatesToCheckForResourcePlacement;

    private System.Random random = new System.Random();

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        gridGenerator = GetComponent<GridGenerator>();
        pointsGen = GetComponent<PointsGen>();
    }

    public void ApplyResources()
    {
        int resAmount = random.Next(currentMap.mapData.minResAmount, currentMap.mapData.maxResAmount);

        //Siberium deposits
        List<Vector2Int> siberiumNodes = pointsGen.GenerateBestCandidatePoints(resAmount, candidatesToCheckForResourcePlacement, currentMap.mapData.tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in siberiumNodes)
        {
            int radius = random.Next(minClusterRadiusSiberiums, maxClusterRadiusSiberiums);
            GrowResourceCluster(pos, currentMap.mapData.tiles[(int)TileType.Siberiums], currentMap.mapData.tiles[(int)TileType.Grass], radius);
        }

        //Ironium deposits
        List<Vector2Int> ironiumNodes = pointsGen.GenerateBestCandidatePoints(resAmount, candidatesToCheckForResourcePlacement, currentMap.mapData.tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in ironiumNodes)
        {
            int radius = random.Next(minClusterRadiusIronium, maxClusterRadiusIronium);
            GrowResourceCluster(pos, currentMap.mapData.tiles[(int)TileType.Ironium], currentMap.mapData.tiles[(int)TileType.Grass], radius);
        }
    }


    void GrowResourceCluster(Vector2Int center, TileData resource, TileData biomeToCheck, int radius)
    { 
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = center.x + x;
                int ny = center.y + y;

                if (!gridGenerator.CheckIfInBoundaries(nx, ny)) continue;

                if (gridGenerator.tileGrid[nx, ny].tileData.ID == biomeToCheck.ID)
                {
                    //Calculate the diagonal distance
                    float distance = Mathf.Sqrt(x * x + y * y);

                    //Only populate the space if the value is within the circle 
                    if (distance <= radius && Random.value > 0.3f) //added a chance to not spawn tiles so they are distributed unevenly
                    {
                        gridGenerator.tileGrid[nx, ny].tileData = resource;
                    }
                }
            }
        }
    }
}
