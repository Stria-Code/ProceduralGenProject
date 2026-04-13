using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class PointsGen : MonoBehaviour
{
    private MapConfig currentMap;
    private GridGenerator gridGenerator;

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        gridGenerator = GetComponent<GridGenerator>();
    }


    //Mitchells best candidate algorithm
    public List<Vector2Int> GenerateBestCandidatePoints(int pointsCount, int candidates, TileData biomeToCheck)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        //Starting point
        points.Add(new Vector2Int(Random.Range(0, currentMap.mapData.width), Random.Range(0, currentMap.mapData.height)));

        for (int i = 1; i < pointsCount; i++)
        {
            Vector2Int bestCandidate = Vector2Int.zero;
            //-1f so the first candidate is always valid
            float bestDistance = -1f;

            for (int c = 0; c < candidates; c++)
            {
                int x = Random.Range(0, currentMap.mapData.width);
                int y = Random.Range(0, currentMap.mapData.height);

                if (gridGenerator.tileGrid[x, y].tileData.ID != biomeToCheck.ID) continue;

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
}
