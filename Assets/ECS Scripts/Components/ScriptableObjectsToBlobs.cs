using System;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class ScriptableObjectsToBlobs : MonoBehaviour
{
    public Holder m_holder;
    public Enemy[] m_Enemy;
    public Cluster[] m_Cluster;
    public Wave[] m_Wave;
    private class ScriptableObjectsToBlobsBaker : Baker<ScriptableObjectsToBlobs>
    {


        public override void Bake(ScriptableObjectsToBlobs authoring)
        {
            Holder[] holders = Resources.LoadAll<Holder>("");
            int selectedIndex = Mathf.Min(0, Array.FindIndex<Holder>(holders, 0, holders.Length, (Holder test) => { return test == authoring.m_holder; }));

            Enemy[] enemies = new Enemy[holders[selectedIndex].Types.Length];
            enemies.AddRange(holders[selectedIndex].Types);
            
            Cluster[] clusters = new Cluster[holders[selectedIndex].Clusters.Length];
            clusters.AddRange(holders[selectedIndex].Clusters);

            Wave[] waves = new Wave[holders[selectedIndex].Waves.Length];
            waves.AddRange(holders[selectedIndex].Waves);
            
            Unity.Entities.Hash128 enemyCustomHash = new Unity.Entities.Hash128((uint)enemies.Length, (uint)enemies[0].damage, (uint)enemies[^1].damage, 0);
            
            if (!TryGetBlobAssetReference<EnemyDataPool>(enemyCustomHash, out BlobAssetReference<EnemyDataPool> enemyPool))
            {
                BlobBuilder enemyBuilder = new BlobBuilder(Allocator.Temp);
                ref EnemyDataPool enemyDataPool = ref enemyBuilder.ConstructRoot<EnemyDataPool>();

                BlobBuilderArray<Enemies> arrayBuilder = enemyBuilder.Allocate(ref enemyDataPool.EnemyPool, enemies.Length);

                for (int i = 0; i < enemies.Length; i++)
                {
                    arrayBuilder[i] = new Enemies
                    {
                        ID = enemies[i].ID,
                        damage = enemies[i].damage,
                        health = enemies[i].health,
                        speed = enemies[i].speed
                    };
                }

                enemyPool = enemyBuilder.CreateBlobAssetReference<EnemyDataPool>(Allocator.Persistent);
                enemyBuilder.Dispose();

                AddBlobAssetWithCustomHash<EnemyDataPool>(ref enemyPool, enemyCustomHash);
            }
            
            Unity.Entities.Hash128 clusterCustomHash = new Unity.Entities.Hash128((uint)clusters.Length, (uint)clusters[0].ID, (uint)clusters[0].clusterDatas.Length, 0);
            
            if (!TryGetBlobAssetReference<ClusterDataPool>(clusterCustomHash, out BlobAssetReference<ClusterDataPool> clusterPool))
            {
                BlobBuilder clusterBuilder = new BlobBuilder(Allocator.Temp);
                ref ClusterDataPool clusterDataPool = ref clusterBuilder.ConstructRoot<ClusterDataPool>();

                BlobBuilderArray<Enemies> arrayBuilder = clusterBuilder.Allocate(ref enemyPool.enemyPool, enemies.Length);

                for (int i = 0; i < enemies.Length; i++)
                    
                {
                    arrayBuilder[i] = new Enemies
                    {
                        ID = enemies[i].ID,
                        damage = enemies[i].damage,
                        health = enemies[i].health,
                        speed = enemies[i].speed
                    };
                }

                pool = builder.CreateBlobAssetReference<EnemyDataPool>(Allocator.Persistent);
                builder.Dispose();

                AddBlobAssetWithCustomHash<EnemyDataPool>(ref pool, clusterCustomHash);
            }
        }
    }
}

public struct EnemyDataPool
{
    public BlobArray<Enemies> EnemyPool;
}
public struct ClusterDataPool
{
    public BlobArray<Clusters> ClusterPool;
}
public struct WaveDataPool
{
    public BlobArray<Waves> WavePool;
}



public struct Enemies
{
    public BlobString enemyName;
    public int ID;
    public float health;
    public float speed;
    public float damage;
};

public struct Clusters
{
    public int ID;
    public BlobString clusterName;
    
    public BlobArray<TomBenScriptableImporter.Clusters.ClusterData>[] ClusterDatas;
    
    public struct ClusterData
    {
        public int Type;
        public int AmountToSpawn;
    }
};
public struct Waves
{
    public int ID;
    public BlobString Name;
    
    //public List<WaveData> _dataForWaves;

    public BlobArray<TomBenScriptableImporter.Waves.WaveData>[] WaveDataArray;
};

