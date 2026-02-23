using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] MapGenerator map;

    void LateUpdate()
    {
        //Gets camera component
        Camera cam = Camera.main;

        //Gets the camera center and positions it to half of the camera view
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        //Start at the player's pos
        Vector3 pos = player.position;

        //Restrains the camera view from going over map edges
        float minX = camWidth - 0.5f;
        float maxX = (map.width - 0.5f) - camWidth;

        float minY = camHeight - 0.5f;
        float maxY = (map.height - 0.5f) - camHeight;

        //This clamps the camera to follow the player but also keep it constrained to the map bounds
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);


        //Move the camera
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}
