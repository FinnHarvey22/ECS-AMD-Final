using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Wave", menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    public int ID;
    public string Name;
    public float waitTime;
    public List<waveData> dataForWaves;

    //list of structs, structs of structs

    public struct waveData
    {
        public bool isCluster;
        public int ID;
        public float SpawnTime;
        public int PopulationDensity;
    }
}
