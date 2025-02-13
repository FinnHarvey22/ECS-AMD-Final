using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Holder", menuName = "Scriptable Objects/Holder")]
public class Holder : ScriptableObject
{
    [SerializeField]
    public Enemy[] Types;
    [SerializeField]
    public Cluster[] Clusters;
    [SerializeField]
    public Wave[] Waves;


    public void AddTypes(List<Enemy> TypesSO)
    {
        Types = TypesSO.ToArray();
    }
    public void AddCluster(List<Cluster> ClusterSO)
    {
        Clusters = ClusterSO.ToArray();
    }
    public void AddWaves(List<Wave> WavesSO)
    {
        Waves = WavesSO.ToArray();
    }
}

