using FishNet.Object.Synchronizing;
using UnityEngine;

namespace UmnieDziala.Game.Items
{
	public partial class ItemSyncPosition
	{
		private SaveData[] saves = new SaveData[6];
		public void Load(int hour)
		{
			var save = saves[hour];
			if (!save.IsSaved) return;

			if (IsServerStarted)
			{
				bool waskinematic = myRigidbody.isKinematic;
				myRigidbody.isKinematic = true;
				transform.SetParent(null);
				transform.position = save.Pos;
				transform.rotation = save.Rot;
				myRigidbody.isKinematic = !EnableSyncPosition.Value;
				EnableSyncPosition.Value = save.Sync;
				EnableSyncPosition.DirtyAll();
				myRigidbody.isKinematic = waskinematic;
				Debug.Log($"LOAD POS: {transform.position}, {transform.eulerAngles}");
			}
		}

		public void Save(int hour)
		{
			saves[hour] = new SaveData(EnableSyncPosition.Value, transform.position, transform.rotation);
		}
		public struct SaveData
		{
			public bool IsSaved;
			public bool Sync;
			public Vector3 Pos;
			public Quaternion Rot;

			public SaveData(bool enableSyncPosition, Vector3 position, Quaternion rotation)
			{
				IsSaved = true;
				this.Sync = enableSyncPosition;
				this.Pos = position;
				this.Rot = rotation;
			}
		}
	}
}