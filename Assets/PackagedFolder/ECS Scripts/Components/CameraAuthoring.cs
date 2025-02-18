using Unity.Entities;
using UnityEngine;

public class CameraAuthoring : MonoBehaviour
{
    public float moveSpeed;

    private class Baker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new CameraComponent
            {
                entity = e,
                moveSpeed = authoring.moveSpeed
            });
        }
    }


}
public struct CameraComponent : IComponentData
{
    public Entity entity;
    public float moveSpeed;
}
