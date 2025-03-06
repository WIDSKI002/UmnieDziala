using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UmnieDziala.Game.Player;
using UnityEngine;

namespace UmnieDziala.Game.Items
{
	//podczas pisania tego kodu momentami mialem ochote wyrzucic komputer przez okno ~Grzesiek
	[RequireComponent(typeof(ItemSyncPosition))]
	public abstract partial class ItemBase : NetworkBehaviour, ISaveable
	{
		public Sprite itemIcon; // Przypisujesz ikonê w Inspectorze
		public string ItemName = "UnknownItem"; // Przypisujesz ikonê w Inspectorze
		public readonly SyncVar<GameObject> PickedUpBy = new(null);
		public GamePlayer Holder;
		[SerializeField, Range(0f,5f)] private float ScaleWhenHeldForOthers = 0.5f;
		[SerializeField, Range(0f, 5f)] private float ScaleWhenHeldForHolder = 1f;
		public readonly SyncVar<bool> IsSelected = new(false);
		public readonly SyncVar<bool> IsUsed = new(false);
		public ItemSyncPosition syncPos;
		[SerializeField] protected GameObject[] Visuals;
		[SerializeField] private GameObject[] DisableWhenHeld;
		[SerializeField] private GameObject[] DisableWhenUsed;
		[SerializeField] private Animator[] AnimatorsToDisableWhenHeld;
		[SerializeField] protected Collider[] Colliders;
		[SerializeField] private bool HideAfterUsage = true;
		public bool IsHolder => IsOwner || (GamePlayer.Local != null && Holder == GamePlayer.Local.gameObject);
		private Dictionary<Transform, (Vector3 pos, Quaternion rot)> StartStates = new();
		Vector3 startscale;
		[SerializeField] private bool StartHidden = false;
		protected virtual void Awake()
		{
			TryGetComponent(out syncPos);
			Colliders = GetComponentsInChildren<Collider>(true);
			startscale = transform.localScale;
			foreach (var anim in AnimatorsToDisableWhenHeld)
			{
				ProcessAnimator(anim.transform);
			}
		}
		private void ProcessAnimator(Transform transform)
		{
			var animator = transform.GetComponent<Animator>();
			if (animator != null)
			{
				StartStates[transform] = (transform.localPosition, transform.localRotation);
				foreach (Transform child in transform)
				{
					if (child.GetComponent<Animator>() == null)
					{
						ProcessAnimator(child);
					}
				}
			}
			else
			{
				StartStates[transform] = (transform.localPosition, transform.localRotation);
				foreach (Transform child in transform)
				{
					ProcessAnimator(child);
				}
			}
		}
		public void EnableAnimators(bool enable)
		{
			foreach(var anim in AnimatorsToDisableWhenHeld)
			{
				anim.enabled=enable;
			}

			if (enable) return;
			
			foreach(var t in StartStates)
			{
				t.Key.localPosition = t.Value.pos;
				t.Key.localRotation = t.Value.rot;
			}
		}
		[ServerRpc]
		public void CmdUseItem()
		{
			if (IsUsed.Value) return;
			UseServer();
			if (HideAfterUsage)
				HideMe();
		}
		[Client(RequireOwnership = true)]
		public virtual void UseClient() { }

		[Server]
		public virtual void UseServer() { }
		[ServerRpc(RequireOwnership = true)]
		public virtual void CmdItemDropped(Vector3 direction)
		{
			transform.SetParent(null);
			if(Holder)
				transform.position = Holder.Inventory.holdingPlace.position;
			PickedUpBy.Value = null;
			syncPos.EnableSyncPosition.Value = true;
			syncPos.myRigidbody.isKinematic = false;
			syncPos.myRigidbody.AddForce(direction * 8f, ForceMode.Impulse);
		}
		[Server]
		public void HideMe()
		{
			//Juz nie usuwam przedmiotow bo one potem sa cofane w czasie.
			
			/*ServerManager.Despawn(gameObject, DespawnType.Destroy);
			if (gameObject != null)
				Destroy(gameObject);*/

			PickedUpBy.Value = null;
			IsSelected.Value = false;
			IsUsed.Value = true;
		}
		private void OnDestroy()
		{
			//chwila moment po co ja to zrobilem:

			/*if (GamePlayer.Local == null) return;
			for (int i = 0; i< GamePlayer.Local.Inventory.itemSlots.Length; i++)
			{
				if (GamePlayer.Local.Inventory.itemSlots[i] == gameObject)
					GamePlayer.Local.Inventory.itemSlots[i] = null;
			}*/
		}
		[ServerRpc (RequireOwnership = false)]
		internal void CmdItemPickedUp(GameObject PlayerObject)
		{
			Debug.Log($"CMD ITEM PICKED UP BY {PlayerObject}");
			if (PickedUpBy.Value != null) return;
			PickedUpBy.Value = PlayerObject;
		}
		private void OnEnable()
		{
			PickedUpBy.OnChange += OnChangedHolder;
			IsSelected.OnChange += OnSelectedChanged;
			IsUsed.OnChange += OnUsedChange;
		}


		private void OnDisable()
		{
			PickedUpBy.OnChange -= OnChangedHolder;
			IsSelected.OnChange -= OnSelectedChanged;
			IsUsed.OnChange -= OnUsedChange;
		}

		private void OnUsedChange(bool prev, bool next, bool asServer)
		{
			if(asServer && next)
			{
				if (PickedUpBy.Value)
					PickedUpBy.Value = null;
				syncPos.myRigidbody.isKinematic = true;
			}
			SetVisible(!next);
			foreach (var obj in DisableWhenUsed)
			{
				obj.SetActive(!next);
			}
			if (next)
			{
				if (GamePlayer.Local)
				{
					ItemEq inventory = GamePlayer.Local.Inventory;
					for (int i = 0; i < inventory.itemSlots.Length; i++)
					{
						if (inventory.itemSlots[i] == gameObject)
							inventory.itemSlots[i] = null;
					}
				}
			}
		}

		private void OnSelectedChanged(bool prev, bool next, bool asServer)
		{
			//Debug.Log($"{next}, {Holder}, {PickedUpBy.Value}"); to czêœæ mojego 2 godzinnego za³amania psychicznego :)
			SetVisible(next || Holder == null || PickedUpBy.Value == null);
		}
		public void SetVisible(bool visible)
		{
			foreach (var obj in Visuals)
			{
				//Debug.Log($"SetVisible({visible}) - {obj.gameObject}"); to te¿i to
				if (IsUsed.Value)
				{
					//Debug.Log($" FALSE 1");i to
					obj.SetActive(false);
				}
				else if (DisableWhenHeld.Contains(obj) && Holder != null)
				{
					//Debug.Log($" FALSE 2");
					obj.SetActive(false);
				}
				else
				{
					//Debug.Log($" visible"); i to
					obj.SetActive(visible);
				}
			}
		}
		[ServerRpc(RequireOwnership = false)] //false bo w sumie nie zalezy mi az tak na zabezpieczaniu a bedzie szybciej, edit: ...
		public void CmdSetSelected(bool isSelected, NetworkConnection conn = null)
		{
			//Debug.Log($"CONN: {conn}");
			if (Holder != null && Holder.Owner != conn) return;
			//Debug.Log($"CMD SELECTED {isSelected}");
			IsSelected.Value = isSelected;
		}

		private void OnChangedHolder(GameObject prev, GameObject next, bool asServer)
		{
			if (next != null)
			{
				next.TryGetComponent(out Holder);
				foreach (var col in Colliders)
					col.enabled = false;
			}
			else
			{
				transform.localScale = startscale;
				if (IsServerStarted)
				{
					IsSelected.Value = false;
					//Debug.Log("SELECTED = FALSE");
				}
				Holder = null;
				foreach (var col in Colliders)
					col.enabled = (!IsUsed.Value);
			}

			if (asServer && IsServerStarted)
				OnServerHolderChanged();

			RefreshVisualState();
			if (IsClientStarted)
			{
				if (Holder != null)
				{
					if (IsHolder)
						OnBecomeHolder();
					else
						OnSomebodyBecomeHolder();
				}
				else
					OnClientNoHolder();
			}
		}
		private void RefreshVisualState()
		{
			if (Holder != null)
			{
				
				EnableAnimators(false);
				foreach (var dis in DisableWhenHeld)
					dis.SetActive(false);
			}
			else
			{
				EnableAnimators(true);
				foreach (var dis in DisableWhenHeld)
					dis.SetActive(true);
			}
		}
		private void OnClientNoHolder()
		{
			transform.localScale = startscale;
			ChangeItemLayer(10);
			SetVisible(true);
			foreach (var col in Colliders)
				col.enabled = (!IsUsed.Value);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			if (StartHidden)
				HideMe();
			Save(0);
		}
		[Server]
		public void SpawnInPosition(Vector3 position)
		{
			if (!IsUsed.Value) return;
			IsUsed.Value = false;
			transform.position = position;
		}
		protected virtual void OnServerHolderChanged()
		{
			if (PickedUpBy.Value != null)
			{
				RemoveOwnership();
				GiveOwnership(PickedUpBy.Value.GetComponent<NetworkObject>().Owner);
				syncPos.EnableSyncPosition.Value = false;
				syncPos.myRigidbody.isKinematic = true;
			}
			else
			{
				RemoveOwnership();
				syncPos.EnableSyncPosition.Value = !IsUsed.Value;
				syncPos.myRigidbody.isKinematic = IsUsed.Value;
			}
			transform.localScale = startscale * ScaleWhenHeldForHolder;
		}
		public void SetToleranceForClient()
		{
			if (syncPos == null)
			{
				Debug.LogError("syncPos is null!");
				return;
			}

			if (InstanceFinder.TimeManager == null)
			{
				Debug.LogError("TimeManager is null!");
				return;
			}

			Debug.Log($"RoundTripTime: {InstanceFinder.TimeManager.RoundTripTime}");

			syncPos.ClientToleranceTimer = ((InstanceFinder.TimeManager.RoundTripTime / 1000f) * 1.5f) + 0.2f;

			Debug.Log($"ClientToleranceTimer set to: {syncPos.ClientToleranceTimer}");
		}

		protected virtual void OnSomebodyBecomeHolder()
		{
			ChangeItemLayer(10);
			SetToleranceForClient();
			transform.SetParent(Holder.Inventory.holdingPlaceForOthers);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = startscale * ScaleWhenHeldForOthers;
		}
		protected virtual void OnBecomeHolder()
		{
			ChangeItemLayer(8);
			Debug.Log($"I became the holder of {ItemName}.");
			RefreshVisualState();
			transform.SetParent(GamePlayer.Local.Inventory.holdingPlace);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = startscale * ScaleWhenHeldForHolder;
			GamePlayer.Local.Inventory.RefreshIcons();
		}
		void ChangeItemLayer(int layer)
		{
			ChangeLayerRecursive(transform, layer);
		}

		void ChangeLayerRecursive(Transform obj, int layer)
		{
			obj.gameObject.layer = layer;
			var shadowmode = layer == 8 ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;

			if (obj.TryGetComponent(out Renderer rend))
			{
				rend.shadowCastingMode = shadowmode;
			}

			foreach (Transform child in obj)
			{
				ChangeLayerRecursive(child, layer);
			}
		}

	}
}