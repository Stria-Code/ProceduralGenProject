using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

static public class Map
{ 

    static public Texture2D CreateTexture(Tile[,] gridToDraw)
    {
        Texture2D texture = new Texture2D(gridToDraw.GetLength(0), gridToDraw.GetLength(1));

        for (int x = 0; x < gridToDraw.GetLength(0); x++)
        {
            for (int y = 0; y < gridToDraw.GetLength(1); y++)
            {
                texture.SetPixel(x, y, gridToDraw[x, y].colour);
            }
        }

        //texture.SetPixel((int)player.position.x, (int)player.position.y, Color.red);
        texture.filterMode = FilterMode.Point;
        texture.EncodeToJPG();

        texture.Apply();
        return texture;
    }
}
