using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using System;

[BurstCompile]
public partial struct PlayerAttackSystem : ISystem
{
	private BufferLookup<DamageBuffer> lookupDamBuff;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		lookupDamBuff = state.GetBufferLookup<DamageBuffer>();
		state.RequireForUpdate<InputAttack>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		lookupDamBuff.Update(ref state);

		NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);

		foreach(var (weaponData, weaponOn, L2W, e) in SystemAPI.Query<RefRW<Weapon>, EnabledRefRW<Weapon>, RefRO<LocalToWorld>>().WithEntityAccess().WithAll<InputAttack>())
		{
			hits.Clear();
			unsafe
			{
				ColliderDistanceInput colliderDistanceInput = new ColliderDistanceInput
				{
					Collider = weaponData.ValueRO.ColliderBlob.AsPtr(),
					Transform = new RigidTransform(L2W.ValueRO.Rotation, L2W.ValueRO.Position),
					MaxDistance = weaponData.ValueRO.WeaponPool.Value.pool[weaponData.ValueRO.SelectedWeapon].AttackRange,
					Scale = 1f
				};

				new PlayerAttackJob
				{
					Input = colliderDistanceInput,
					DistanceHits = hits,
					world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
				}.Schedule().Complete();
			}

			foreach (DistanceHit hit in hits)
			{
				if (hit.Entity == e) { continue; }

				if (!lookupDamBuff.HasBuffer(hit.Entity)) { continue; }

				lookupDamBuff[hit.Entity].Add(new DamageBuffer { Causer = e, Damage = weaponData.ValueRO.WeaponPool.Value.pool[weaponData.ValueRO.SelectedWeapon].Damage });
			}
			weaponData.ValueRW.Timer = weaponData.ValueRO.WeaponPool.Value.pool[weaponData.ValueRO.SelectedWeapon].AttackDelay;
			weaponOn.ValueRW = false;

		}

		hits.Dispose();

		WeaponReloadJob reloadJob = new WeaponReloadJob
		{
			dt = SystemAPI.Time.DeltaTime,
		};

		reloadJob.ScheduleParallel();
	}

	[BurstCompile]
	private partial struct PlayerAttackJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public ColliderDistanceInput Input;
		public NativeList<DistanceHit> DistanceHits;
		[ReadOnly] public PhysicsWorld world;

		public void Execute()
		{
			world.CalculateDistance(Input, ref DistanceHits);
		}
	}

	[BurstCompile]
	[WithDisabled(typeof(Weapon))]
	private partial struct WeaponReloadJob : IJobEntity
	{
		[ReadOnly] public float dt;

		[BurstCompile]
		public void Execute(ref Weapon weaponData, EnabledRefRW<Weapon> weaponOn)
		{
			weaponData.Timer -= dt;

			if( weaponData.Timer > 0 ) { return; }

			weaponOn.ValueRW = true;
		}
	}
}