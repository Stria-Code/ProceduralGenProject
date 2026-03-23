using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile", order = 1)]
public class Tile : ScriptableObject
{
    [SerializeField] public int ID;
    [SerializeField] public string name;
    [SerializeField] public bool walkable;
    [SerializeField] public int walkSpeed;
    [SerializeField] public int[] buildOnTop;
    [SerializeField] public int health;
    [SerializeField] public Color colour;
    [SerializeField] public Sprite image;
}
