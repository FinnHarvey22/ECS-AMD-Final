using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Wave", menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    public int ID;
    public string Name; 
    public List<WaveData> dataForWaves;

    //list of structs, structs of structs

    [System.Serializable]
    public struct WaveData
    {
        public bool isCluster;
        public int ID;
        public float SpawnTime;
        public int PopulationDensity;
    }

    public void AddData(bool isCluster, int ID, string SpawnTime, string populationDensity)
    {
        
        if (int.TryParse(populationDensity, out int PopDensityIntoutput))
        {
            
        }
        if (float.TryParse(SpawnTime, out float SpawnTimeFloatOutput))

        
        Debug.Log($"{isCluster} + {ID} + {SpawnTime} + {populationDensity}");
        dataForWaves = new List<WaveData>();
        WaveData test = new WaveData()
        {
            isCluster = isCluster,
            ID = ID,
            SpawnTime = SpawnTimeFloatOutput,
            PopulationDensity = PopDensityIntoutput
        };
        
        dataForWaves.Add(test);
    }
}
