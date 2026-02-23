using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] MapGenerator map;
    [SerializeField] float speed = 5.0f;
    private Vector2 movement;

    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = Vector2.zero;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.aKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed) input.x = 1;
        if (Keyboard.current.sKey.isPressed) input.y = -1;
        if (Keyboard.current.wKey.isPressed) input.y = 1;

        if (input != Vector2.zero)
        {
            Vector2 targetPos = (Vector2)transform.position + input;

            int targetX = Mathf.RoundToInt(targetPos.x);
            int targetY = Mathf.RoundToInt(targetPos.y);

            if (map.noiseGrid[targetX, targetY].walkable)
            {
                movement = input;
            }
        }

        Vector2 currentPos = transform.position;

        if (input.x != 0)
        {
            int targetX = Mathf.RoundToInt(currentPos.x + input.x);
            int currentY = Mathf.RoundToInt(currentPos.y);

            if (map.noiseGrid[targetX, currentY].walkable)
                movement.x = input.x;
        }

        if (input.y != 0)
        {
            int currentX = Mathf.RoundToInt(currentPos.x);
            int targetY = Mathf.RoundToInt(currentPos.y + input.y);

            if (map.noiseGrid[currentX, targetY].walkable)
                movement.y = input.y;
        }

    }

    private void FixedUpdate()
    {
        //Movement.normalized to prevent diagonal speed increase
        rb.linearVelocity = movement.normalized * speed;
    }
}
