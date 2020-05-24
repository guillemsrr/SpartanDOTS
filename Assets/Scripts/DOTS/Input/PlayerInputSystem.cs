using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.Jobs;

namespace Spartans.Input
{
	/// <summary>
	/// Detects the player input and communicates it to the entities
	/// </summary>
	public class PlayerInputSystem : SystemBase
	{
		/// <summary>
		/// Used to create the controls using the new Input System
		/// </summary>
		private PlayerInput _inputAction;

		//Static otherwise i wouldn't be able to use it in a static separate function -> Clean code! But will it work in the future?
		private static Camera _camera;

		protected override void OnCreate()
		{
			_inputAction = new PlayerInput();
			_inputAction.Enable();
			_inputAction.Player.Move.performed += OnMove;
			_inputAction.Player.StopMove.performed += OnStopMove;
		}

		protected override void OnStartRunning()
		{
			_camera = Camera.main;
		}

		protected override void OnDestroy()
		{
			_inputAction.Player.Move.performed -= OnMove;
			_inputAction.Player.StopMove.performed -= OnStopMove;
			_inputAction.Disable();
		}

		protected override void OnUpdate()
		{
		}

		/// <summary>
		/// Sets the direction for each agent.
		/// </summary>
		/// <param name="context">Passed context using delegate</param>
		private void OnMove(InputAction.CallbackContext context)
		{
			if (!_camera) return;

			float2 vector2 = new float2(context.ReadValue<Vector2>());
			float3 moveInput = vector2.x * _camera.transform.right + vector2.y * Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1)).normalized;
			JobHandle job = Entities.WithAll<SpartanData>().ForEach((ref AgentData agent) =>
			{
				agent.direction = moveInput;
				//TODO randomize the seek weight so that they walk differently
				agent.seekWeight = 0f; // new Unity.Mathematics.Random(1).NextFloat(1f - 0.1f, 1f + 0.1f);
			}).Schedule(Dependency);

			job.Complete();
		}

		private void OnStopMove(InputAction.CallbackContext context)
		{
			JobHandle job = Entities.ForEach((ref AgentData agent) =>
			{
				agent.direction = float3.zero;
			}).Schedule(Dependency);

			job.Complete();
		}
	}
}
