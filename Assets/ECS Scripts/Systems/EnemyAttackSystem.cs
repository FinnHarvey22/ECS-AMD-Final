using Unity.Entities;
using Unity.Burst;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct EnemyAttackSystem : ISystem
{
	private ComponentLookup<EnemyComponent> lookupEnemy;
	private ComponentLookup<PlayerTag> lookupPlayer;
	private BufferLookup<DamageBuffer> lookupDamageBuffer;
	private ComponentLookup<PendingKillComponent> lookupPendingKill;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		lookupEnemy = state.GetComponentLookup<EnemyComponent>();
		lookupPlayer = state.GetComponentLookup<PlayerTag>();
		lookupDamageBuffer = state.GetBufferLookup<DamageBuffer>();
		lookupPendingKill = state.GetComponentLookup<PendingKillComponent>();
		state.RequireForUpdate<EnemyComponent>();
		state.RequireForUpdate<PlayerTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		lookupEnemy.Update(ref state);
		lookupPlayer.Update(ref state);
		//lookupDamageBuffer.Update(ref state);
		lookupPendingKill.Update(ref state);

		state.Dependency = new EnemyPlayerTriggerJob
		{
			lookupEnemy = lookupEnemy,
			lookupPlayer = lookupPlayer,
			ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
							.CreateCommandBuffer(state.WorldUnmanaged)
		}.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

	}

	[BurstCompile]
	private struct EnemyPlayerTriggerJob : ITriggerEventsJob
	{
		[ReadOnly] public ComponentLookup<EnemyComponent> lookupEnemy;
		[ReadOnly] public ComponentLookup<PlayerTag> lookupPlayer;
		public EntityCommandBuffer ecb;

		public void Execute(TriggerEvent triggerEvent)
		{
			Entity entityA = triggerEvent.EntityA;
			Entity entityB = triggerEvent.EntityB;

			bool isBodyAEnemy = lookupEnemy.HasComponent(entityA);
			bool isBodyBEnemy = lookupEnemy.HasComponent(entityB);

			//return if both bodies are enemies
			if (isBodyAEnemy && isBodyBEnemy) { return; }

			bool isBodyAPlayer = lookupPlayer.HasComponent(entityA);
			bool isBodyBPlayer = lookupPlayer.HasComponent(entityB);

			//return if enemy is overlapping a non-player
			if((isBodyAEnemy && !isBodyBPlayer) || (isBodyBEnemy && !isBodyAPlayer)) { return; }

			Entity EnemyEntity = isBodyAEnemy ? entityA : entityB;
			Entity PlayerEntity = isBodyAEnemy ? entityB : entityA;

			DamageBuffer damageEvent = new DamageBuffer { Damage = lookupEnemy[EnemyEntity].Damage, Causer = EnemyEntity };
			ecb.AppendToBuffer(PlayerEntity, damageEvent);
			ecb.SetComponentEnabled<PendingKillComponent>(EnemyEntity, true);
		}
	}
}