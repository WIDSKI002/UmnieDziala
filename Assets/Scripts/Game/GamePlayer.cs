using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UmnieDziala.Game.UI;
using UmnieDziala.Lobby;
using UnityEditor;
using UnityEngine;
using static UmnieDziala.Game.Player.GamePlayer;

namespace UmnieDziala.Game.Player
{
	public partial class GamePlayer : NetworkBehaviour, ISaveable
	{
		public static GamePlayer Local;
		public readonly SyncVar<GameObject> LobbyPlayerObject = new();
		[SerializeField] private LobbyPlayer Player;
		[SerializeField] internal Renderer[] PlayerRenderers;
		[Header("Save")]
		[SerializeField] private SavedData[] HourSaves = new SavedData[6];
		[Header("My Scripts")]
		public PlayerMovement MovementScript;
		public PlayerCamera CameraScript;
		public ItemEq Inventory;
		[Header("Data")]
		[SerializeField] private int OwnerIdTest;

		[SerializeField] private GameObject RagdollPrefab;
		[SerializeField] private GameObject PlayerModel;
		[SerializeField] private GameObject MyRagdoll;
		public string PlayerName => Player.PlayerName.Value;

		public readonly SyncVar<bool> IsAlive = new(true);
		public float AntiJumpscareTimer = 1f;

		private void OnEnable()
		{
			IsAlive.OnChange += OnAliveChanged;
		}
		private void OnDisable()
		{
			IsAlive.OnChange -= OnAliveChanged;
		}
		public override void OnStopServer()
		{
			base.OnStopServer();
			if(!IsOwner && IsServerStarted)
			{
			}
		}
		private void OnAliveChanged(bool prev, bool next, bool asServer)
		{
			if (!next && (next != prev))
			{
				OnPlayerDied();
			}
			else if (next && MyRagdoll != null)
			{
				Destroy(MyRagdoll);
				PlayerModel.SetActive(true);
				if (IsOwner)
					DeathUI.instance.SetPlay(false);
				CameraScript.EnableFirstPersonOverlay(true);
			}
		}

		private void OnPlayerDied()
		{
			PlayerModel.SetActive(false);
			MyRagdoll = Instantiate(RagdollPrefab, transform.position, transform.rotation);
			Transform playerArmature = PlayerModel.transform.Find("Armature");
			Transform ragdollArmature = MyRagdoll.transform.Find("Armature");

			if (playerArmature != null && ragdollArmature != null)
			{
				CopyBonesTransform(playerArmature, ragdollArmature);
			}
			if (IsOwner)
			{
				RewindMenu.instance.Open(false);
				var syncanim = GetComponent<SyncAnimator>();
				syncanim.CmdSetRun(false);
				syncanim.CmdSetWalk(false);
				Inventory.DropAllItems();
				CameraScript.EnableFirstPersonOverlay(false);
				SetLayerRecursively(MyRagdoll, 11);
			}
			if (IsServerStarted)
			{
				GameManager.instance.ServerOnPlayerDied(this);
			}
		}
		void SetLayerRecursively(GameObject obj, int layer)
		{
			obj.layer = layer;
			foreach (Transform child in obj.transform)
			{
				SetLayerRecursively(child.gameObject, layer);
			}
		}
		private void CopyBonesTransform(Transform source, Transform target)
		{
			foreach (Transform sourceBone in source.GetComponentsInChildren<Transform>())
			{
				Transform targetBone = target.Find(sourceBone.name);
				if (targetBone != null)
				{
					targetBone.position = sourceBone.position;
					targetBone.rotation = sourceBone.rotation;
				}
			}
		}
		public override void OnStartServer()
		{
			base.OnStartServer();
			GameManager.instance.Started(this);
			Save(0);
		}
		public override void OnStartClient()
		{
			base.OnStartClient();
			if (Player == null && LobbyPlayerObject.Value != null)
				LobbyPlayerObject.Value.TryGetComponent(out Player);
			if (IsOwner)
			{
				Local = this;
				Save(0);
			}
			OwnerIdTest = OwnerId;
			SetVisibility(!IsOwner);
			
		}
		public void SetVisibility(bool vis)
		{
			foreach(Renderer r in PlayerRenderers)
			{
				if (r == null) continue;
				r.shadowCastingMode = vis? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
			}
		}
		private void Awake()
		{
			TryGetComponent(out Inventory);
		}
		private void Update()
		{
			if (AntiJumpscareTimer > 0f)
				AntiJumpscareTimer -= Time.deltaTime;
			if (Player==null && LobbyPlayerObject.Value!=null)
				LobbyPlayerObject.Value.TryGetComponent(out Player);
		}
	}
	#if UNITY_EDITOR
	[CustomEditor(typeof(GamePlayer))]
	public class GamePlayerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			GamePlayer gamePlayer = (GamePlayer)target;
			if (GUILayout.Button("Get Renderers in Children"))
			{
				Undo.RecordObject(gamePlayer, "GetRenderers");
				gamePlayer.PlayerRenderers = gamePlayer.GetComponentsInChildren<Renderer>();
				EditorUtility.SetDirty(gamePlayer);
			}
		}
	}
	#endif
}