using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UmnieDziala.Game.Items
{
	public partial class ItemSyncPosition : NetworkBehaviour, ISaveable
	{
		public Rigidbody myRigidbody;

		public readonly SyncVar<bool> EnableSyncPosition = new();
		public readonly SyncVar<Vector3> Position = new();
		public readonly SyncVar<Quaternion> Rotation = new();
		[SerializeField, Range(0.1f,20f)] private float LerpSpeed = 20f;
		public float ClientToleranceTimer;
		private void Awake()
		{
			TryGetComponent(out myRigidbody);
		}
		public override void OnStartClient()
		{
			base.OnStartClient();
			Save(0);
			if (!IsServerStarted && myRigidbody != null)
				myRigidbody.isKinematic = true;
		}
		public override void OnStartServer()
		{
			base.OnStartServer();
			Save(0);
		}
		private void OnEnable()
		{
			EnableSyncPosition.OnChange += changedSyncBool;

		}
		private void OnDisable()
		{
			EnableSyncPosition.OnChange -= changedSyncBool;
		}

		private void changedSyncBool(bool prev, bool next, bool asServer)
		{
			ClientToleranceTimer = 0;
		}

		private void Update()
		{
			if (!EnableSyncPosition.Value || ClientToleranceTimer > 0f)
			{
				if (ClientToleranceTimer > 0f)
					ClientToleranceTimer -= Time.deltaTime;

				if (myRigidbody != null)
					myRigidbody.isKinematic = true;
				return;
			}
			if (transform.parent != null)
				transform.SetParent(null);
			if (!IsServerStarted)
			{
				transform.position = Vector3.Lerp(transform.position, Position.Value, LerpSpeed * Time.deltaTime);
				transform.rotation = Quaternion.Lerp(transform.rotation, Rotation.Value, LerpSpeed * Time.deltaTime);
			}
			else
			{
				if (myRigidbody != null)
					myRigidbody.isKinematic = false;
				Position.Value = transform.position;
				Rotation.Value = transform.rotation;
			}
		}
	}
}