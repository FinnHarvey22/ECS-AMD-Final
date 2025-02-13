using System;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class ScriptableObjectsToBlobs : MonoBehaviour
{
    public Holder m_holder;
    public int WaveNumber;

    private class ScriptableObjectsToBlobsBaker : Baker<ScriptableObjectsToBlobs>
    {
        public override void Bake(ScriptableObjectsToBlobs authoring)
        {
            Holder[] holders = Resources.LoadAll<Holder>("");
            int selectedIndex = Mathf.Min(0,
                Array.FindIndex<Holder>(holders, 0, holders.Length,
                    (Holder test) => { return test == authoring.m_holder; }));

            Debug.Log($"enemy length = {holders[selectedIndex].Types.Length}");
            Debug.Log($"cluster length = {holders[selectedIndex].Clusters.Length}");
            Debug.Log($"wave length = {holders[selectedIndex].Waves.Length}");

            Entity e = GetEntity(TransformUsageFlags.None);

            if (holders[selectedIndex].Types.Length != 0)
            {

                Unity.Entities.Hash128 enemyCustomHash = new Unity.Entities.Hash128(
                    (uint)holders[selectedIndex].Types.Length,
                    (uint)holders[selectedIndex].Types[0].damage, (uint)holders[selectedIndex].Types[^1].damage, 0);

                if (!TryGetBlobAssetReference<EnemyDataPool>(enemyCustomHash,
                        out BlobAssetReference<EnemyDataPool> enemyPool))
                {
                    BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                    ref EnemyDataPool enemyDataPool = ref builder.ConstructRoot<EnemyDataPool>();

                    BlobBuilderArray<Enemies> arrayBuilder = builder.Allocate(ref enemyDataPool.EnemyPool,
                        holders[selectedIndex].Types.Length);

                    for (int i = 0; i < holders[selectedIndex].Types.Length; i++)
                    {
                        arrayBuilder[i] = new Enemies
                        {
                            ID = holders[selectedIndex].Types[i].ID,
                            Damage = holders[selectedIndex].Types[i].damage,
                            Health = holders[selectedIndex].Types[i].health,
                            Speed = holders[selectedIndex].Types[i].speed
                        };
                    }

                    enemyPool = builder.CreateBlobAssetReference<EnemyDataPool>(Allocator.Persistent);
                    builder.Dispose();

                    AddBlobAssetWithCustomHash<EnemyDataPool>(ref enemyPool, enemyCustomHash);
                }

                AddComponent(e, new ManagerEnemyPool()
                {
                    EnemyPool = enemyPool
                });
            }

            if (holders[selectedIndex].Clusters.Length != 0)
            {

                Unity.Entities.Hash128 clusterCustomHash = new Unity.Entities.Hash128(
                    (uint)holders[selectedIndex].Clusters.Length,
                    (uint)holders[selectedIndex].Clusters[0].ID,
                    (uint)holders[selectedIndex].Clusters[0].clusterDatas.Length, 0);

                if (!TryGetBlobAssetReference<ClusterDataPool>(clusterCustomHash,
                        out BlobAssetReference<ClusterDataPool> clusterPool))
                {
                    BlobBuilder builder = new BlobBuilder(Allocator.Temp);

                    ref ClusterDataPool clusterDataPool = ref builder.ConstructRoot<ClusterDataPool>();

                    BlobBuilderArray<Clusters> arrayBuilder =
                        builder.Allocate(ref clusterDataPool.ClusterPool, holders[selectedIndex].Clusters.Length);

                    for (int i = 0; i < holders[selectedIndex].Clusters.Length; i++)
                    {
                        arrayBuilder[i] = new Clusters { ID = holders[selectedIndex].Clusters[i].ID };
                        BlobBuilderArray<Clusters.ClusterData> clusterArrayBuilder =
                            builder.Allocate(ref arrayBuilder[i].CDataArray,
                                holders[selectedIndex].Clusters[i].clusterDatas.Length);

                        for (int entity = 0; entity < clusterArrayBuilder.Length; entity++)
                        {
                            clusterArrayBuilder[entity].Type =
                                holders[selectedIndex].Clusters[i].clusterDatas[entity].Type;
                            clusterArrayBuilder[entity].AmountToSpawn = holders[selectedIndex].Clusters[i]
                                .clusterDatas[entity].AmountToSpawn;
                        }

                    }

                    clusterPool = builder.CreateBlobAssetReference<ClusterDataPool>(Allocator.Persistent);
                    builder.Dispose();

                    AddBlobAssetWithCustomHash<ClusterDataPool>(ref clusterPool, clusterCustomHash);
                }

                AddComponent(e, new ManagerClusterPool
                {
                    ClusterPool = clusterPool
                });
            }

            if (holders[selectedIndex].Waves.Length != 0)
            {
                Unity.Entities.Hash128 waveCustomHash = new Unity.Entities.Hash128(
                    (uint)holders[selectedIndex].Waves.Length, (uint)holders[selectedIndex].Waves[0].ID,
                    (uint)holders[selectedIndex].Waves[0].WaveDataArray.Length, 0);

                if (!TryGetBlobAssetReference<WaveDataPool>(waveCustomHash,
                        out BlobAssetReference<WaveDataPool> wavePool))
                {
                    BlobBuilder builder = new BlobBuilder(Allocator.Temp);

                    ref WaveDataPool wDataPool = ref builder.ConstructRoot<WaveDataPool>();

                    BlobBuilderArray<Waves> arrayBuilder =
                        builder.Allocate(ref wDataPool.WavePool, holders[selectedIndex].Waves.Length);

                    for (int i = 0; i < holders[selectedIndex].Waves.Length; i++)
                    {
                        arrayBuilder[i] = new Waves() { ID = holders[selectedIndex].Waves[i].ID };
                        BlobBuilderArray<Waves.WaveData> waveArrayBuilder =
                            builder.Allocate(ref arrayBuilder[i].WDataArray,
                                holders[selectedIndex].Waves[i].WaveDataArray.Length);

                        for (int entity = 0; entity < waveArrayBuilder.Length; entity++)
                        {
                            waveArrayBuilder[entity].ID = holders[selectedIndex].Waves[i].WaveDataArray[entity].ID;
                            waveArrayBuilder[entity].PopulationDensity = holders[selectedIndex].Waves[i]
                                .WaveDataArray[entity].PopulationDensity;
                            waveArrayBuilder[entity].SpawnTime =
                                holders[selectedIndex].Waves[i].WaveDataArray[entity].SpawnTime;
                            waveArrayBuilder[entity].IsCluster =
                                holders[selectedIndex].Waves[i].WaveDataArray[entity].IsCluster;
                        }

                    }

                    wavePool = builder.CreateBlobAssetReference<WaveDataPool>(Allocator.Persistent);
                    builder.Dispose();

                    AddBlobAssetWithCustomHash<WaveDataPool>(ref wavePool, waveCustomHash);

                    AddComponent(e, new ManagerWavePool
                    {
                        WavePool = wavePool
                    });
                }
            }


            AddComponent(e, new ManagerTag());
            
        }
    }

    public struct ManagerTag : IComponentData { }

    
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
        //public BlobString enemyName;
        public int ID;
        public float Health;
        public float Speed;
        public float Damage;
    };

    public struct Clusters
    {
        public int ID;
        //public BlobString clusterName;

        public BlobArray<ClusterData> CDataArray;

        public struct ClusterData
        {
            public int Type;
            public int AmountToSpawn;
        }
    };

    public struct Waves
    {
        public int ID;
        //public BlobString Name;

        //public List<WaveData> _dataForWaves;

        public BlobArray<WaveData> WDataArray;

        public struct WaveData
        {
            public bool IsCluster;
            public int ID;
            public float SpawnTime;
            public int PopulationDensity;
        }

    };
    public struct ManagerEnemyPool : IComponentData
    {
        public BlobAssetReference<EnemyDataPool> EnemyPool;
    }
    public struct ManagerClusterPool : IComponentData
    {
        public BlobAssetReference<ClusterDataPool> ClusterPool;
    }
    public struct ManagerWavePool : IComponentData
    {
        public BlobAssetReference<WaveDataPool> WavePool;
    }
}

