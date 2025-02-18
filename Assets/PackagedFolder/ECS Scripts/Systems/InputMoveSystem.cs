using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public partial struct InputMoveSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<InputMove>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		InputMoveJob moveJob = new InputMoveJob
		{
			dt = SystemAPI.Time.DeltaTime
		};

		moveJob.ScheduleParallel();
	}

	[BurstCompile]
	private partial struct InputMoveJob : IJobEntity
	{
		[ReadOnly] public float dt;

		[BurstCompile]
		public void Execute(ref LocalTransform LT, in InputMove moveComp)
		{
			float3 moveVec = LT.InverseTransformDirection(math.normalizesafe(moveComp.RequestedMove) * dt * moveComp.MoveSpeed);
			LT.Position += moveVec;
		}
	}
}