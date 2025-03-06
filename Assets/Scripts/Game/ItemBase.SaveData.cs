using System;
using System.Collections.Generic;
using UmnieDziala.Game.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace UmnieDziala.Game.Items
{
	public abstract partial class ItemBase
	{
		private SaveData[] saves = new SaveData[6];
		public virtual void Save(int hour)
		{
			if (IsServerStarted)
			{
				var save = new SaveData(PickedUpBy.Value, IsUsed.Value, IsSelected.Value);
				saves[hour] = save;
			}
		}

		public virtual void Load(int hour)
		{
			var save = saves[hour];
			if (!save.IsSaved) return;
			if (IsServerStarted)
			{
				IsUsed.Value = save.WasUsed;
				Dictionary<int, GameObject> allObjects = new Dictionary<int, GameObject>();
				foreach (var obj in FindObjectsByType<GamePlayer>(FindObjectsSortMode.None))
				{
					allObjects[obj.gameObject.GetInstanceID()] = obj.gameObject;
				}
				if (save.PickedUpById != -1)
				{
					if (allObjects.TryGetValue(save.PickedUpById, out GameObject holder))
					{
						PickedUpBy.Value = holder;
						IsSelected.Value = save.WasSelected;
					}
					else
					{
						Debug.LogError($"[ITEMBASE] For some reason player (holder) is gone."); //aha
						PickedUpBy.Value = null;
						IsSelected.Value = false;
					}
				}
				else
				{
					PickedUpBy.Value = null;
					IsSelected.Value = false;
				}
				if (!save.WasUsed && save.PickedUpById == -1)
				{
					syncPos.myRigidbody.isKinematic = false;
					foreach (var col in Colliders)
						col.enabled = true;
				}

				IsUsed.DirtyAll();
				PickedUpBy.DirtyAll();
				IsSelected.DirtyAll();
			}
		}
		public struct SaveData
		{
			public bool IsSaved;
			public int PickedUpById;
			public bool WasUsed;
			public bool WasSelected;

			public SaveData(GameObject holderObject, bool isUsed, bool wasSelected)
			{
				this.IsSaved = true;
				this.PickedUpById = holderObject != null ? holderObject.GetInstanceID() : -1;
				this.WasUsed = isUsed;
				this.WasSelected = wasSelected;
			}
		}
	}
}