using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
	public float MoveSpeed;
	public float MaxHealth;
	public WeaponSO SelectedWeapon;
	private class Baker : Baker<PlayerAuthoring>
	{
		public override void Bake(PlayerAuthoring authoring)
		{
			WeaponSO[] weaponDatas = Resources.LoadAll<WeaponSO>("");
			int selectedIndex = Mathf.Min(0, Array.FindIndex<WeaponSO>(weaponDatas, 0, weaponDatas.Length, (WeaponSO test) => { return test == authoring.SelectedWeapon; }));

			Unity.Entities.Hash128 customHash = new Unity.Entities.Hash128((uint)weaponDatas.Length, (uint)weaponDatas[0].Damage, (uint)weaponDatas[^1].Damage, 0);

			if (!TryGetBlobAssetReference<WeaponDataPool>(customHash, out BlobAssetReference<WeaponDataPool> pool))
			{
				BlobBuilder builder = new BlobBuilder(Allocator.Temp);
				ref WeaponDataPool weaponPool = ref builder.ConstructRoot<WeaponDataPool>();

				BlobBuilderArray<WeaponData> arrayBuilder = builder.Allocate(ref weaponPool.pool, weaponDatas.Length);

				for (int i = 0; i < weaponDatas.Length; i++)
				{
					arrayBuilder[i] = new WeaponData
					{
						AttackDelay = weaponDatas[i].AttackDelay,
						AttackRange = weaponDatas[i].AttackRange,
						Damage = weaponDatas[i].Damage,
					};
				}

				pool = builder.CreateBlobAssetReference<WeaponDataPool>(Allocator.Persistent);
				builder.Dispose();

				AddBlobAssetWithCustomHash<WeaponDataPool>(ref pool, customHash);
			}

			Entity e = GetEntity(TransformUsageFlags.Dynamic);

			AddComponent(e, new PlayerTag());
			AddComponent(e, new InputMove { RequestedMove = new float3(), MoveSpeed = authoring.MoveSpeed });
			SetComponentEnabled<InputMove>(e, false);
			AddComponent(e, new InputAttack());
			SetComponentEnabled<InputAttack>(e, false);
			AddComponent(e, new Weapon
			{
				Timer = 0.0f,
				ColliderBlob = Unity.Physics.SphereCollider.Create(new Unity.Physics.SphereGeometry { Center = float3.zero, Radius = pool.Value.pool[selectedIndex].AttackRange }),
				WeaponPool = pool,
				SelectedWeapon = selectedIndex,
			});
			AddComponent(e, new HealthComponent { MaxHealth = authoring.MaxHealth, CurrentHealth = authoring.MaxHealth });
			AddBuffer<DamageBuffer>(e);
			AddComponent<PendingKillComponent>(e);
			SetComponentEnabled<PendingKillComponent>(e, false);
			AddComponent(e, new CameraFollowComp());
		}
	}
}

public struct PlayerTag : IComponentData { }

public struct InputMove : IComponentData, IEnableableComponent
{
	public float3 RequestedMove;
	public float MoveSpeed;
}



public struct InputAttack : IComponentData, IEnableableComponent { }

public struct Weapon : IComponentData, IEnableableComponent
{
	public float Timer;
	public BlobAssetReference<Unity.Physics.Collider> ColliderBlob;
	public BlobAssetReference<WeaponDataPool> WeaponPool;
	public int SelectedWeapon;
}

public struct WeaponDataPool
{
	public BlobArray<WeaponData> pool;
}

public struct WeaponData
{
	public float Damage;
	public float AttackDelay;
	public float AttackRange;
}
public struct CameraFollowComp : IComponentData
{
	
}