using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] MapGenerator map;
    [SerializeField] float speed = 5.0f;
    private Vector2 movement;

    private Rigidbody2D rb;
    private Collider2D col;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = Vector2.zero;

        if (Keyboard.current.aKey.isPressed) movement.x = -1;
        if (Keyboard.current.dKey.isPressed) movement.x = 1;
        if (Keyboard.current.sKey.isPressed) movement.y = -1;
        if (Keyboard.current.wKey.isPressed) movement.y = 1;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = movement.normalized * speed;
        Vector3 predictedPos = transform.position + (Vector3)(velocity * Time.fixedDeltaTime);

        if (IsWalkable(predictedPos))
        {
            rb.linearVelocity = velocity;
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    private bool IsWalkable(Vector3 predictedCenter)
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
                if (!map.CheckIfInBoundaries(x, y))
                {
                    return false;
                }

                if (!map.noiseGrid[x, y].walkable)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

