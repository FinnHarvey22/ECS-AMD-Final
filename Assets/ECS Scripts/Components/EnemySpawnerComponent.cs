using Unity.Entities;
using Unity.Mathematics;

public struct EnemySpawnerComponent : IComponentData
{
	public Entity PrefabToSpawn;

	public float Timer;

	public float SpawnDelay;

	public float3 SpawnPosition;
}
