using Unity.Entities;
using Unity.Burst;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(PendingKillSystem))]
public partial struct HealthSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<HealthComponent>();
		state.RequireForUpdate<DamageBuffer>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		DamageFromBufferJob damageJob = new DamageFromBufferJob();
		damageJob.ScheduleParallel();
	}

	[BurstCompile]
	[WithDisabled(typeof(PendingKillComponent))]
	private partial struct DamageFromBufferJob : IJobEntity
	{
		[BurstCompile]
		public void Execute(ref HealthComponent healthComp, ref DynamicBuffer<DamageBuffer> damageBuffer, ref PendingKillComponent killComp, EnabledRefRW<PendingKillComponent> killEnable)
		{
			for (int i = 0; i < damageBuffer.Length; i++)
			{
				healthComp.CurrentHealth -= damageBuffer[i].Damage;
				if (healthComp.CurrentHealth <= 0)
				{
					killComp.Causer = damageBuffer[i].Causer;
					killEnable.ValueRW = true;
					break;
				}
				damageBuffer.Clear();
			}
		}
	}
}