using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "Scriptable Objects/Cluster")]
public class Cluster : ScriptableObject
{
    public int ID;
    public string clusterName;
    public List<ClusterData> DataForClusters;
   
    [System.Serializable]
  public struct ClusterData
    {
        public int Type;
        public int AmountToSpawn;
    }

    public void AddData(int Type, int AmountToSpawn)
    {
        if (DataForClusters == null)
        {
            DataForClusters = new List<ClusterData>();
        }

        ClusterData clusterData = new ClusterData()
        {
            AmountToSpawn = AmountToSpawn,
            Type = Type,
        };
        
        DataForClusters.Add(clusterData);
    }
}
