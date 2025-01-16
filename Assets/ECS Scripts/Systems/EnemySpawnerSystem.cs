using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EnemySpawnerComponent>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach(RefRW<EnemySpawnerComponent> spawner in SystemAPI.Query<RefRW<EnemySpawnerComponent>>())
		{
			spawner.ValueRW.Timer += SystemAPI.Time.DeltaTime;
			
			if(spawner.ValueRO.Timer < spawner.ValueRO.SpawnDelay)
			{
				continue;
			}

			Entity newEnemy = state.EntityManager.Instantiate(spawner.ValueRO.PrefabToSpawn);

			LocalTransform newLT = LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition);

			state.EntityManager.SetComponentData(newEnemy, newLT);

			spawner.ValueRW.Timer -= spawner.ValueRO.SpawnDelay;
		}
	}
}
