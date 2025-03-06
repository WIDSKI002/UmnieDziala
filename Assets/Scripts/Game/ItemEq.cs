using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UmnieDziala.Game.Items;
using FishNet.Object;
using UmnieDziala.Game.Player;
using System;
using FishNet;
using UmnieDziala.Game.UI;
using NUnit.Framework.Interfaces;
using UmnieDziala.Game;

public partial class ItemEq : NetworkBehaviour, ISaveable
{
	[SerializeField] private SyncAnimator syncAnimator;
	private GamePlayer PlayerScript;
	private PlayerCamera cameraScript;
	public Transform holdingPlace;
	public Transform holdingPlaceForOthers;
	public LayerMask groundLayer;
	public float pickupDistance = 3f;
	public float dropDistance = 5f;

	public Image crosshair => ItemUI.instance?.Crosshair;
	public Color normalColor = Color.red;
	public Color highlightColor = Color.green;

	public GameObject[] itemSlots = new GameObject[3];
	[SerializeField] private int selectedSlot = 0;
	public ItemBase SelectedItem =>
		selectedSlot >= 0 && selectedSlot < itemSlots.Length && itemSlots[selectedSlot] != null
			? itemSlots[selectedSlot].GetComponent<ItemBase>()
			: null;

	public Image[] itemSlotIcons => ItemUI.instance?.itemSlotIcons;
	public Image[] itemSlotBackground => ItemUI.instance?.itemSlotBackground;

	public InputActionReference pickupAction;
	public InputActionReference dropAction;
	public InputActionReference useAction;
	public InputActionReference scrollAction;
	public InputActionReference slot1Action;
	public InputActionReference slot2Action;
	public InputActionReference slot3Action;
	[SerializeField] private LayerMask PickupLayer;
	ItemBase lastSelected;
	[SerializeField] private GameObject RaycastNow;
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (IsOwner)
		{
			Save(0);
		}
	}
	private void Awake()
	{
		TryGetComponent(out cameraScript);
		TryGetComponent(out PlayerScript);
	}
	void Update()
	{
		if (!IsOwner) return;
		if (crosshair == null) { Debug.LogWarning("Crosshair is null for the inventory."); return; }
		if (!PlayerScript.IsAlive.Value) return;
		HandleSlotSelection();
		UpdateCrosshair();

		if (pickupAction.action.WasPressedThisFrame())
		{
			HandlePickup();
		}
		if (dropAction.action.WasPressedThisFrame())
		{
			PlaceItem();
		}
		if (useAction.action.WasPressedThisFrame())
		{
			UseItem();
		}

		SelectedSync();
	}

	void HandlePickup()
	{
		if (!CursorManager.IsLockedMouse()) return;

		RaycastHit hit;
		var playerCamera = cameraScript.CameraTransform;

		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickupDistance, PickupLayer))
		{
			RaycastNow = hit.transform.gameObject;
			Transform item = hit.transform;

			do
			{
				if (item.TryGetComponent(out ItemBase foundItemScript))
				{
					PickUpItem(foundItemScript.gameObject);
					return;
				}
				else if (item.CompareTag("przycisk"))
				{
					var przyciskScript = item.GetComponent<PrzyciskLiczba>();
					if (przyciskScript != null)
					{
						GameObject.FindFirstObjectByType<kod>().ZmianaKodu(przyciskScript.liczba.ToString());
					}
					return;
				}
				else if (item.CompareTag("przycisk_open"))
				{
					item.GetComponent<PrzyciskOpen>()?.open();
					return;
				}
				else if (item.CompareTag("RewindClock"))
				{
					RewindMenu.instance.Open(true);
					return;
				}
				else if (item.TryGetComponent(out IInteractable interactable))
				{
					interactable.Interact();
					return;
				}

				item = item.parent;

			} while (item != null);
		}
		else
		{
			RaycastNow = null;
		}
	}

	void Klikniecie(){

	}
	void HandleSlotSelection()
	{

		if (slot1Action.action.WasReleasedThisFrame())
			selectedSlot = 0;
		if (slot2Action.action.WasReleasedThisFrame())
			selectedSlot = 1;
		if (slot3Action.action.WasReleasedThisFrame())
			selectedSlot = 2;

		float scroll = scrollAction.action.ReadValue<Vector2>().y;

		if (scroll < 0)
			selectedSlot = (selectedSlot + 1) % 3;
		if (scroll > 0)
			selectedSlot = (selectedSlot + 2) % 3;

		UpdateSlotHighlight();
	}


	void UpdateCrosshair()
	{
		RaycastHit hit;
		var playerCamera = cameraScript.CameraTransform;
		crosshair.color = normalColor;

		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickupDistance, PickupLayer))
		{
			Transform item = hit.transform;

			do
			{
				if (item.TryGetComponent(out ItemBase _) ||
					item.CompareTag("przycisk") ||
					item.CompareTag("przycisk_open") ||
					item.CompareTag("RewindClock") ||
					item.TryGetComponent(out IInteractable _))
				{
					crosshair.color = highlightColor;
					return;
				}

				item = item.parent;

			} while (item != null);
		}
	}

	void UpdateSlotHighlight()
	{
		bool anySelected = false;
		for (int i = 0; i < itemSlotBackground.Length; i++)
		{
			if (itemSlotBackground[i] != null)
				itemSlotBackground[i].color = (i == selectedSlot) ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 1f, 1f, 0.2f);

			if (itemSlots[i] != null)
			{
				bool isSelected = (i == selectedSlot);
				itemSlots[i].SetActive(isSelected);
				anySelected = isSelected || anySelected;
			}
			else
			{
				itemSlotIcons[i].enabled = false;
			}
		}
		syncAnimator.CmdSetItem(anySelected);
		
	}
	void SelectedSync()
	{
		if (itemSlots[selectedSlot] != null)
		{
			ItemBase newSelected = itemSlots[selectedSlot].GetComponent<ItemBase>();

			if (lastSelected != newSelected)
			{
				if (lastSelected != null)
				{
					Debug.Log($"Deselecting item: {lastSelected.ItemName} (ID: {lastSelected.GetInstanceID()})");
					lastSelected.CmdSetSelected(false);
				}

				lastSelected = newSelected;

				if (lastSelected != null)
				{
					Debug.Log($"Selecting new item: {lastSelected}, {lastSelected?.ItemName} (ID: {lastSelected?.GetInstanceID()})");
					lastSelected.CmdSetSelected(true);
				}
			}
		}
		else if (lastSelected != null)
		{
			Debug.Log($"Deselecting item: {lastSelected.ItemName} (ID: {lastSelected.GetInstanceID()})");
			lastSelected.CmdSetSelected(false);
			lastSelected = null;
		}
	}
	void PickUpItem(GameObject item)
	{
		if (!CursorManager.IsLockedMouse()) return;
		UpdateItemIcon(selectedSlot);
		if (itemSlots[selectedSlot] != null) return;

		itemSlots[selectedSlot] = item;
		if (itemSlots[selectedSlot].TryGetComponent(out ItemBase itemScript))
		{
			itemScript.SetToleranceForClient();
			itemScript.CmdItemPickedUp(gameObject);
		}
		UpdateItemIcon(selectedSlot);
	}
	void PlaceItem(bool forced = false)
	{
		if (!CursorManager.IsLockedMouse() && !forced) return;
		if (itemSlots[selectedSlot] == null) return;

		if (itemSlots[selectedSlot].TryGetComponent(out ItemBase item))
		{
			item.CmdItemDropped(cameraScript.CameraTransform.forward);
		}
		itemSlots[selectedSlot] = null;
		UpdateItemIcon(selectedSlot);
	}
	[Client]
	void UpdateItemIcon(int slotIndex)
	{
		if (itemSlotIcons[slotIndex] != null)
		{
			if (itemSlots[slotIndex] != null && itemSlots[slotIndex].TryGetComponent(out ItemBase itemScript))
			{
				itemSlotIcons[slotIndex].sprite = itemScript.itemIcon;
				itemSlotIcons[slotIndex].enabled = itemSlotIcons[slotIndex].sprite != null;
			}
			else
			{
				itemSlotIcons[slotIndex].sprite = null;
				itemSlotIcons[slotIndex].enabled = false;
			}
		}
	}
	[Client]
	void UseItem()
	{
		if (!CursorManager.IsLockedMouse()) return;
		if (itemSlots[selectedSlot] == null) return;
		if (itemSlots[selectedSlot].TryGetComponent(out ItemBase item))
		{
			item.UseClient(); // tu lokalnie na kliencie cos robie uzywajac jesli cos jest
			item.CmdUseItem(); // wysylam do serwera ze uzywam jakby cos sie mialo dziac na serwerze
		}
	}
	internal void RefreshIcons()
	{
		for (int i = 0; i < itemSlots.Length; i++)
		{
			UpdateItemIcon(i);
		}
	}

	internal void DropAllItems()
	{
		for (int i = 0; i < itemSlots.Length; i++)
		{
			selectedSlot = i;
			PlaceItem();
		}
	}
}
