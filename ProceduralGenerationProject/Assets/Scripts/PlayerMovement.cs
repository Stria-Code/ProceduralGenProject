using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{

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

        if (Keyboard.current.aKey.isPressed) movement.x = -1;
        if (Keyboard.current.dKey.isPressed) movement.x = 1;
        if (Keyboard.current.sKey.isPressed) movement.y = -1;
        if (Keyboard.current.wKey.isPressed) movement.y = 1;
    }

    private void FixedUpdate()
    {
        //movement.normalized to prevent diagonal speed increase
        rb.linearVelocity = movement.normalized * speed;
    }
}
