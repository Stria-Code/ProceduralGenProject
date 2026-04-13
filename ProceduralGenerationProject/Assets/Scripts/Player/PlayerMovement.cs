using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] GameObject map;
    GridGenerator grid;
    private int playerSpeed;
    private PlayerCollision collisionController;
    private Vector2 movement;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collisionController = GetComponent<PlayerCollision>();
        grid = map.GetComponent<GridGenerator>();
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
        playerSpeed = grid.tileGrid[(int)transform.position.x, (int)transform.position.y].tileData.walkSpeed; 
        Vector2 velocity = movement.normalized * playerSpeed;

        Vector3 currentPos = transform.position;


        Vector3 predictedPos = transform.position + (Vector3)(velocity * Time.fixedDeltaTime);

        if (grid.tileGrid[Mathf.FloorToInt(predictedPos.x), Mathf.FloorToInt(predictedPos.y)].tileData.walkSpeed > 0)
        {
            rb.linearVelocity = velocity;
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    /*public bool IsWalkable(Vector3 predictedPos)
    {

        Vector2Int currentTile = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        Vector2 normalisedDirection = movement.normalized;
        Vector2Int targetTile = currentTile + new Vector2Int(Mathf.RoundToInt(normalisedDirection.x), Mathf.RoundToInt(normalisedDirection.y));

        return grid.tileGrid[targetTile.x, targetTile.y].tileData.walkable;
    }*/
}

