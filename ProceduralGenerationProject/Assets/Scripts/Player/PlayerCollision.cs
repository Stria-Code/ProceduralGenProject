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

   /* public bool IsWalkable(Vector3 predictedPos)
    {
        if (col == null) return true;

        //Get collider bounds
        Bounds bounds = col.bounds;

        //Moves the collider to the next predicted tile's center
        Vector3 offset = predictedPos - transform.position;
        bounds.center += offset;

        //Converts bound corners to ints for grid readability
        int minX = (int)(bounds.min.x - 0.1f);
        int minY = (int)(bounds.min.y - 0.1f);
        int maxX = (int)(bounds.max.x + 0.1f);
        int maxY = (int)(bounds.max.y + 0.1f);

        //Loop through potentially overlapping tiles
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (!grid.CheckIfInBoundaries(x, y))
                {
                    return false;
                }

                if (!grid.tileGrid[x, y].tileData.walkable)
                {
                    return false;
                }
            }
        }

        return true;
    }*/
}
