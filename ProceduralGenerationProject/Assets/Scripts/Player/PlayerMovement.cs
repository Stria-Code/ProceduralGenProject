using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int playerSpeed;
    private PlayerCollision collisionController;
    private Vector2 movement;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collisionController = GetComponent<PlayerCollision>();
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
        Vector2 velocity = movement.normalized * playerSpeed;

        Vector3 predictedPos = transform.position + (Vector3)(velocity * Time.fixedDeltaTime);

        if (collisionController.IsWalkable(predictedPos))
        {
            rb.linearVelocity = velocity;
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

}

