using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
	public float MoveSpeed;
	public float Damage;
	public float MaxHealth;

	private class Baker : Baker<EnemyAuthoring>
	{
		public override void Bake(EnemyAuthoring authoring)
		{
			Entity e = GetEntity(TransformUsageFlags.Dynamic);

			AddComponent(e, new EnemyComponent
			{
				MoveSpeed = authoring.MoveSpeed,
				Damage = authoring.Damage
			});
			AddComponent(e, new HealthComponent { MaxHealth = authoring.MaxHealth, CurrentHealth = authoring.MaxHealth });
			AddBuffer<DamageBuffer>(e);
			AddComponent<PendingKillComponent>(e);
			SetComponentEnabled<PendingKillComponent>(e, false);
		}
	}
}

public struct EnemyComponent : IComponentData
{
	public float MoveSpeed;
	public float Damage;
}