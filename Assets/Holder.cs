using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Holder", menuName = "Scriptable Objects/Holder")]
public class Holder : ScriptableObject
{
    [SerializeField]
    public ScriptableObject[] Types;
    [SerializeField]
    public ScriptableObject[] Clusters;
    [SerializeField]
    public ScriptableObject[] Waves;


    public void AddTypes(List<ScriptableObject> TypesSO)
    {
        Types = TypesSO.ToArray();
    }
    public void AddCluster(List<ScriptableObject> ClusterSO)
    {
        Clusters = ClusterSO.ToArray();
    }
    public void AddWaves(List<ScriptableObject> WavesSO)
    {
        Waves = WavesSO.ToArray();
    }
}

