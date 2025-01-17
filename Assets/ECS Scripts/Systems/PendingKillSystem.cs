using Unity.Entities;
using Unity.Burst;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(HealthSystem))]
public partial struct PendingKillSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PendingKillComponent>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		PendingKillJob killJob = new PendingKillJob
		{
			ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
							.CreateCommandBuffer(state.WorldUnmanaged)
							.AsParallelWriter()
		};
		killJob.ScheduleParallel();
	}

	[BurstCompile]
	private partial struct PendingKillJob : IJobEntity
	{
		public EntityCommandBuffer.ParallelWriter ecb;

		[BurstCompile]
		public void Execute([ChunkIndexInQuery] int index, Entity e, in PendingKillComponent killComp)
		{
			ecb.DestroyEntity(index, e);
		}
	}
}