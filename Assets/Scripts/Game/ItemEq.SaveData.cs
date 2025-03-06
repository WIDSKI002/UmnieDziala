using System;
using System.Collections.Generic;
using UmnieDziala.Game.Items;
using UnityEngine;

public partial class ItemEq
{
	private SaveData[] saveDatas = new SaveData[6];
	public void Save(int hour)
	{
		saveDatas[hour] = new SaveData(itemSlots, selectedSlot);
	}

	public void Load(int hour)
	{
		if (!saveDatas[hour].IsSaved) return;

		Dictionary<int, GameObject> allObjects = new Dictionary<int, GameObject>();
		foreach (var obj in FindObjectsByType<ItemBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			allObjects[obj.gameObject.GetInstanceID()] = obj.gameObject;
		}

		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (i < saveDatas[hour].itemIDs.Length)
			{
				int savedID = saveDatas[hour].itemIDs[i];

				if (allObjects.ContainsKey(savedID))
				{
					itemSlots[i] = allObjects[savedID];
					Debug.Log($"Item Slot {i} : {itemSlots[i]}");
				}
				else
				{
					itemSlots[i] = null;
				}
			}
		}

		selectedSlot = saveDatas[hour].SelectedSlot;
		for (int i = 0; i < itemSlots.Length; i++)
		{
			UpdateItemIcon(i);
		}
	}


	private struct SaveData
	{
		public bool IsSaved;
		public int SelectedSlot;
		public int[] itemIDs;

		public SaveData(GameObject[] items, int selslot)
		{
			IsSaved = true;
			SelectedSlot = selslot;
			itemIDs = new int[items.Length];

			for (int i = 0; i < items.Length; i++)
			{
				itemIDs[i] = items[i] != null ? items[i].GetInstanceID() : -1;
			}
		}
	}
}
