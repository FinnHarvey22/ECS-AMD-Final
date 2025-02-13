using Unity.Burst;
using Unity.Entities;
using UnityEngine;


[BurstCompile]
public partial struct NewEnemySpawnerSystem : ISystem
{
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnerComponent>();
        
    }
}
