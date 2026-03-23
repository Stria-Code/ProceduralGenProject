using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraControl : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.scroll.down.magnitude > 0.1f && Camera.main.orthographicSize != 9) Camera.main.orthographicSize += 1;
        if (Mouse.current.scroll.up.magnitude > 0.1f && Camera.main.orthographicSize != 5) Camera.main.orthographicSize -= 1;
    }
}
