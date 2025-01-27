using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "Scriptable Objects/Cluster")]
public class Cluster : ScriptableObject
{
    public int ID;
    public string clusterName;
    public int Type;
    public int AmountToSpawn;
}
