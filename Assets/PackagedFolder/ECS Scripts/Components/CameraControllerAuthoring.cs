using Unity.Entities;
using UnityEngine;

public class CameraControllerAuthoring : MonoBehaviour
{
    public Transform anchor, cam;
    
  private class CameraControlBaker : Baker<CameraControllerAuthoring>
    {
        public override void Bake(CameraControllerAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponentObject(e, new CamControllerObject
            {
                camAnchor = authoring.anchor,
                camTransform = authoring.cam
            });
        }
    }
}

public class CamControllerObject : IComponentData
{
    public Transform camAnchor, camTransform;

    //public Entity camEntity;
}

