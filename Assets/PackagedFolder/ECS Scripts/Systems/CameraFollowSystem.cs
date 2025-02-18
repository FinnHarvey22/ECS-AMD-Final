using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]

public partial struct CameraFollowSystem : ISystem
{
    private ComponentLookup<LocalToWorld> _lookupL2W;
    private Entity _entityToFollow;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _lookupL2W = state.GetComponentLookup<LocalToWorld>();
        
        state.RequireForUpdate<CameraFollowComp>();
        state.RequireForUpdate<CameraComponent>();
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (_entityToFollow == Entity.Null)
        {
            _entityToFollow = SystemAPI.GetSingletonEntity<CameraFollowComp>();
            
        }
        _lookupL2W.Update(ref state);

        CameraMoveJob job = new CameraMoveJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            EntityToFollow = _entityToFollow,
            lookupL2W = _lookupL2W

        };
        job.ScheduleParallel();

    }
    
    [BurstCompile]
    private partial struct CameraMoveJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Entity EntityToFollow;
        [ReadOnly] public ComponentLookup<LocalToWorld> lookupL2W;

        public void Execute(in CameraComponent camComp, ref LocalTransform camLT, in LocalToWorld camL2W)
        {
            float3 followPos = lookupL2W[EntityToFollow].Position;
            float3 targetVec = math.normalizesafe(followPos - camL2W.Position) * deltaTime * camComp.moveSpeed;
            
   
            camLT.Position.x += targetVec.x;
            camLT.Position.z += targetVec.z;
            
        }
    }
}
