using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Wave", menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    public int ID;
    public string Name;
    
    private List<WaveData> _dataForWaves;

    [System.Serializable]
    public struct WaveData
    {
        public bool IsCluster;
        public int ID;
        public float SpawnTime;
        public int PopulationDensity;
    }

    //list of structs, structs of structs


    public void AddData(TomBenScriptableImporter.Waves.WaveData data)
    {

        if (_dataForWaves == null)
        {
            _dataForWaves = new List<WaveData>();
        }

        WaveData waveData = new WaveData()
        {
            ID = data.ID,
            IsCluster = data.IsCluster,
            SpawnTime = data.SpawnTime,
            PopulationDensity = data.PopulationDensity
        };
        

        _dataForWaves.Add(waveData);
    }
    
}
