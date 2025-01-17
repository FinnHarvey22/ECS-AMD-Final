using Unity.Entities;
using Unity.Burst;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System;

[BurstCompile]
public partial class InputCaptureSystem : SystemBase
{
	private InputSystem_Actions inputMap;

	protected override void OnCreate()
	{
		inputMap = new InputSystem_Actions();

		RequireForUpdate<InputMove>();
		inputMap.Player.Move.performed += Handle_InputMovePerformed;
		inputMap.Player.Move.canceled += Handle_InputMoveCanceled;
		inputMap.Player.Attack.performed += Handle_InputAttackPerformed;
		inputMap.Player.Attack.canceled += Handle_InputAttackCanceled;

		inputMap.Enable();
	}

	protected override void OnUpdate() {}

	protected override void OnDestroy()
	{
		inputMap.Disable();

		inputMap.Player.Move.performed -= Handle_InputMovePerformed;
		inputMap.Player.Move.canceled -= Handle_InputMoveCanceled;
		inputMap.Player.Attack.performed -= Handle_InputAttackPerformed;
		inputMap.Player.Attack.canceled -= Handle_InputAttackCanceled;
	}

	private void Handle_InputMovePerformed(InputAction.CallbackContext context)
	{
		foreach(var (InMoveEnabled, InMoveComp, L2W) in SystemAPI.Query<EnabledRefRW<InputMove>, RefRW<InputMove>, RefRO<LocalToWorld>>()
																			.WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
		{
			if(!InMoveEnabled.ValueRO) { InMoveEnabled.ValueRW = true; }

			float2 Request2D = context.ReadValue<Vector2>();
			float3 RequestWorldSpace = (L2W.ValueRO.Forward * Request2D.y) + (L2W.ValueRO.Right * Request2D.x);
			InMoveComp.ValueRW.RequestedMove = RequestWorldSpace;
		}
	}

	private void Handle_InputMoveCanceled(InputAction.CallbackContext context)
	{
		foreach(var InMoveEnabled in SystemAPI.Query<EnabledRefRW<InputMove>>())
		{
			InMoveEnabled.ValueRW = false;
		}
	}

	private void Handle_InputAttackPerformed(InputAction.CallbackContext context)
	{
		foreach(var InAttackEnabled in SystemAPI.Query<EnabledRefRW<InputAttack>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
		{
			if(!InAttackEnabled.ValueRO) { InAttackEnabled.ValueRW = true; }
		}
	}

	private void Handle_InputAttackCanceled(InputAction.CallbackContext context)
	{
		foreach(var InAttackEnabled in SystemAPI.Query<EnabledRefRW<InputAttack>>())
		{
			InAttackEnabled.ValueRW = false;
		}
	}
}