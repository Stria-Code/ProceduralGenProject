using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] GameObject map;
    private GridGenerator grid;
    private Collider2D col;

    private void Awake()
    {
        grid = map.GetComponent<GridGenerator>();
        col = GetComponent<Collider2D>();
    }

    public bool IsWalkable(Vector3 predictedCenter)
    {
        if (col == null) return true;

        //Get collider bounds
        Bounds bounds = col.bounds;

        //Moves the collider to the next predicted tile's center
        Vector3 offset = predictedCenter - transform.position;
        bounds.center += offset;

        //Converts bound corners to ints for grid readability (+0.5f for allignment)
        int minX = Mathf.FloorToInt(bounds.min.x + 0.5f);
        int minY = Mathf.FloorToInt(bounds.min.y + 0.5f);
        int maxX = Mathf.FloorToInt(bounds.max.x + 0.5f);
        int maxY = Mathf.FloorToInt(bounds.max.y + 0.5f);

        //Loop through potentially overlapping tiles
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (!grid.CheckIfInBoundaries(x, y))
                {
                    return false;
                }

                if (!grid.noiseGrid[x, y].walkable)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
