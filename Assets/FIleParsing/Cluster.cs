using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "Scriptable Objects/Cluster")]
public class Cluster : ScriptableObject
{
    public int ID;
    public string clusterName;
    //public List<ClusterData> DataForClusters;
    
    public TomBenScriptableImporter.Clusters.ClusterData[] clusterDatas;
   
    [System.Serializable]
  public struct ClusterData
    {
        public int Type;
        public int AmountToSpawn;
    }

    /* public void AddData(TomBenScriptableImporter.Clusters.ClusterData data, int length)
     {
         if (DataForClusters == null)
         {
             DataForClusters = new List<ClusterData>();
         }

        if (clusterDatas == null)
        {
            //clusterDatas = new ClusterData[length];
        }

        ClusterData clusterData = new ClusterData()
        {
            AmountToSpawn = data.AmountToSpawn,
            Type = data.Type,
        };
        
        DataForClusters.Add(clusterData);
    }*/
}
