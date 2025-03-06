using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UmnieDziala.Game.Player
{
	public class PlayerCamera : NetworkBehaviour
	{
		private GamePlayer Player;
		public Transform CameraTransform;
		public Camera Camera;
		public static List<PlayerCamera> ActiveCameras = new List<PlayerCamera>();
		[SerializeField] private InputActionReference lookAction;
		[SerializeField] private float sensitivity = 1f;
		[SerializeField] private float maxLookAngle = 89.9f;
		internal Vector2 cameraRotation = Vector2.zero;
		private readonly SyncVar<float> SyncCameraRotationY = new();
		public Transform CameraFollow;
		private Vector3 startpos;
		[SerializeField] private Camera FirstPersonOverlayCamera;
		private LayerMask startMask;
		[SerializeField] private LayerMask OwnRagdollMask;
		private bool SeeOwnRagdoll = false;
		public override void OnStartClient()
		{
			base.OnStartClient();
			CameraTransform.gameObject.SetActive(IsOwner);
			startMask = Camera.cullingMask;
		}
		private void OnEnable()
		{
			if (!ActiveCameras.Contains(this))
				ActiveCameras.Add(this);
		}
		private void OnDisable()
		{
			ActiveCameras.Remove(this);
		}
		private void Update()
		{
			if (!Player.IsAlive.Value && IsOwner)
			{
				if (CameraFollow != null)
					HandleCameraFollow();

				if (!SeeOwnRagdoll)
				{
					Camera.cullingMask = startMask & ~OwnRagdollMask;
				}
				else
				{
					Camera.cullingMask = startMask;
				}
				return;
			}
			CameraTransform.localPosition = startpos;
			if (!IsOwner)
				HandleCameraSync();
			else
				HandleCameraRotation();
		}

		private void HandleCameraFollow()
		{
			Vector3 pos = Vector3.Lerp(CameraTransform.position, CameraFollow.transform.position, Time.deltaTime*20f);
			Quaternion rot = Quaternion.Lerp(CameraTransform.rotation, CameraFollow.transform.rotation, Time.deltaTime*20f);

			CameraTransform.transform.position = pos;
			CameraTransform.transform.rotation = rot;
		}

		private void HandleCameraSync()
		{
			if (IsOwner) return;
			Quaternion targetRot = Quaternion.Euler(SyncCameraRotationY.Value, 0f, 0f);
			CameraTransform.localRotation = Quaternion.Lerp(CameraTransform.localRotation, targetRot, Time.deltaTime * 10f);
		}

		private void HandleCameraRotation()
		{
			if (CursorManager.IsLockedMouse())
			{
				Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();

				cameraRotation.x += lookDelta.x * sensitivity;
				cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookDelta.y * sensitivity, -maxLookAngle, maxLookAngle);
			}
			transform.rotation = Quaternion.Euler(0f, cameraRotation.x, 0f); //na razie po prostu obracanie postaci instant,
																			 //potem moze sie zrobi jakos inaczej
			CameraTransform.localRotation = Quaternion.Euler(cameraRotation.y, 0f, 0f);
			if(Mathf.Abs(SyncCameraRotationY.Value - cameraRotation.y) >= 0.05f)
			{
				SyncCameraRotY(cameraRotation.y);
			}
		}
		[ServerRpc]
		void SyncCameraRotY(float y)
		{
			SyncCameraRotationY.Value = y;
		}
		private void Awake()
		{
			startpos = CameraTransform.localPosition;
			TryGetComponent(out Player);
		}

		internal void EnableFirstPersonOverlay(bool v)
		{
			FirstPersonOverlayCamera.enabled = v;
		}
	}
}
