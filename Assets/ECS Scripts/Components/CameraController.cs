using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class CameraController : SystemBase
{
    public static CameraController instance;
    
    static Quaternion defaultRot;
    static float DefaultDistance;

    static Queue<AnchorTarget> focusQueue;
    static AnchorTarget preFocusAnchorTarget;

    private static bool ActiveFocus;

    protected override void OnStartRunning()
    {
        instance = this;
        focusQueue = new Queue<AnchorTarget>();
       CamControllerObject camController = SystemAPI.ManagedAPI.GetSingleton<CamControllerObject>();
       defaultRot = camController.camAnchor.rotation;
       DefaultDistance = -camController.camTransform.localPosition.z;
       
       ActiveFocus = false;
       

    }

    protected override void OnUpdate()
    {
        if (ActiveFocus)
        {
            CamControllerObject camController = SystemAPI.ManagedAPI.GetSingleton<CamControllerObject>();

            AnchorTarget target;

            if (focusQueue.Count > 0)
            {
                target = focusQueue.Peek();
            }
            else
            {
                target = preFocusAnchorTarget;
            }

            Vector3 position;
            if (target.position != null)
            {
                position = (Vector3)target.position;
            }
            else
            {
                position = SystemAPI.GetComponentRO<LocalTransform>(target.entity).ValueRO.Position;
            }

            float lerpSpeed = 4;
            camController.camAnchor.position =
                Vector3.Lerp(camController.camAnchor.position, position, SystemAPI.Time.DeltaTime * 4);
            camController.camAnchor.rotation = Quaternion.Slerp(camController.camAnchor.rotation, defaultRot,
                SystemAPI.Time.DeltaTime * lerpSpeed);
            camController.camTransform.localPosition = Vector3.Lerp(camController.camTransform.localPosition,
                -Vector3.forward * target.distance, SystemAPI.Time.DeltaTime * lerpSpeed);
        }
    }

    public static void PushFocus(Entity e, float dist = 0) { instance._PushFocus(e, dist);}

    void _PushFocus(Entity e, float dist)
    {
        if (focusQueue.Count == 0)
        {
            CamControllerObject camControllerObject = SystemAPI.ManagedAPI.GetSingleton<CamControllerObject>();
            preFocusAnchorTarget = new AnchorTarget(camControllerObject.camAnchor.position,
                -camControllerObject.camTransform.localPosition.z);
        }

        AnchorTarget target = new AnchorTarget(e, dist == 0 ? DefaultDistance : dist);
        
            focusQueue.Enqueue(target);
            ActiveFocus = true;
    }

    public static void PopFocus(Entity e) { instance._PopFocus(e); }

    void _PopFocus(Entity e)
    {
        while (focusQueue.Any(target => target.entity.Equals(e)))
        {
            focusQueue.Dequeue();
        }

        ActiveFocus = true;
        
    }
    
}

public class AnchorTarget
{
    public Entity entity;
    public float distance;
    public Vector3? position;

    public AnchorTarget(Entity e, float dist)
    {
        entity = e;
        distance = dist;
        position = null;
    }

    public AnchorTarget(Vector3 p, float dist)
    {
        position = p;
        distance = dist;
    }
}
