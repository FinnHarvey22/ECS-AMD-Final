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

    public void AddData(TomBenScriptableImporter.Clusters.ClusterData data)
    {
        if (DataForClusters == null)
        {
            DataForClusters = new List<ClusterData>();
        }

        ClusterData clusterData = new ClusterData()
        {
            AmountToSpawn = data.AmountToSpawn,
            Type = data.Type,
        };
        
        DataForClusters.Add(clusterData);
    }
}
