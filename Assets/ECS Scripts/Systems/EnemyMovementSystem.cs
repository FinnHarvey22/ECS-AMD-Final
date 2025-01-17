using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
	private ComponentLookup<LocalToWorld> lookupL2W;
	private Entity playerEntity;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		lookupL2W = state.GetComponentLookup<LocalToWorld>();

		state.RequireForUpdate<EnemyComponent>();
		state.RequireForUpdate<PlayerTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		if (playerEntity == Entity.Null)
		{
			playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
		}

		lookupL2W.Update(ref state);

		EnemyMoveJob job = new EnemyMoveJob
		{
			deltaTime = SystemAPI.Time.DeltaTime,
			playerEntity = playerEntity,
			lookupL2W = lookupL2W
		};

		job.ScheduleParallel();
	}

	[BurstCompile]
	private partial struct EnemyMoveJob : IJobEntity
	{
		[ReadOnly] public float deltaTime;
		[ReadOnly] public Entity playerEntity;
		[ReadOnly] public ComponentLookup<LocalToWorld> lookupL2W;

		public void Execute(in EnemyComponent enemyComp, ref LocalTransform enemyLT, in LocalToWorld enemyL2W)
		{
			float3 playerPos = lookupL2W[playerEntity].Position;
			float3 targetVec = math.normalizesafe(playerPos - enemyL2W.Position) * deltaTime * enemyComp.MoveSpeed;

			enemyLT.Position += targetVec;
		}
	}
}
