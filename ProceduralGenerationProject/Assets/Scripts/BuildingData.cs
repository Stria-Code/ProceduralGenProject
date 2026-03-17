using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/Building", order = 1)]
public class BuildingData : ScriptableObject
{
    [SerializeField] public Sprite image;
    [SerializeField] public string name;
    [SerializeField] public GameObject prefab;
    [SerializeField] public bool unlocked;
    [SerializeField] public int cost; //not used in this version


    //Can be expended per added tiles
    [SerializeField] public bool canBuildOnWater;
    [SerializeField] public bool canBuildOnLand;
    [SerializeField] public bool canBuildOnResources;
}
