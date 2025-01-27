using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    public int ID;
    public string Name;
    public float waitTime;

    //list of structs, structs of structs

    public struct waveData
    {
        private bool isCluster;
        private int ID;
        private float SpawnTime;
        private int PopulationDensity;
    }
}