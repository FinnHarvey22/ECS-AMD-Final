using Unity.Entities;

public struct HealthComponent : IComponentData
{
	public float MaxHealth;
	public float CurrentHealth;
}

public struct DamageBuffer : IBufferElementData
{
	public float Damage;
	public Entity Causer;
}

public struct PendingKillComponent : IComponentData, IEnableableComponent
{
	public Entity Causer;
}