using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
	public GameObject PrefabToSpawn;
	public float SpawnDelay;
	

	private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
	{
		public override void Bake(EnemySpawnerAuthoring authoring)
		{
			Entity e = GetEntity(TransformUsageFlags.None);

			AddComponent(e, new EnemySpawnerComponent
			{
				PrefabToSpawn = GetEntity(authoring.PrefabToSpawn, TransformUsageFlags.Dynamic),
				Timer = 0.0f,
				SpawnDelay = authoring.SpawnDelay,
				SpawnPosition = authoring.transform.position
			});
		}
	}
}
