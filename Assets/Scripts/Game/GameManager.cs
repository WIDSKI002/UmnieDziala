using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UmnieDziala.Game.Monsters;
using UmnieDziala.Game.Player;
using UmnieDziala.Lobby;
#if UNITY_EDITOR

using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

namespace UmnieDziala.Game
{
	public partial class GameManager : NetworkBehaviour, ISaveable
	{
		public static GameManager instance;
		[Header("Config")]
		[SerializeField, Range(30, 600)] private int SecondsPerHour = 300;
		[Header("References")]
		[SerializeField] private GameObject[] DisableOnRuntime;
		[SerializeField] private GameObject InGamePlayerPrefab = null;

		[SerializeField] private Transform[] SpawnOrderList;
		private Queue<Transform> SpawnsQueue;

		[SerializeField] private float sv_CurrentTime;
		public readonly SyncVar<byte> CurrentHour = new();
		public readonly SyncVar<byte> CurrentMins = new();

		private readonly SyncDictionary<byte, int> HourSeeds = new();
		public readonly SyncVar<float> pauseTimer = new();
		private bool isTimerPaused => pauseTimer.Value > 0f;

		private void Awake()
		{
			instance = this;
			foreach (var dis in DisableOnRuntime)
			{
				if (dis == null) continue;
				dis.SetActive(false);
			}
			SpawnsQueue = new Queue<Transform>(SpawnOrderList);
		}
		private void OnEnable()
		{
			CurrentHour.OnChange += OnHourChanged;
		}
		private void OnDisable()
		{
			CurrentHour.OnChange -= OnHourChanged;
		}
		private void OnHourChanged(byte prev, byte next, bool asServer)
		{
			if (prev < next) // tylko jeżeli godzina do przodu ruszyła
				if (IsServerStarted)
					RpcSaveHour(next);
			if(IsServerStarted)
				AdjustMonsterSpeed();
		}
		[Server]
		private void AdjustMonsterSpeed()
		{
			if (ClockMonster.instance == null)
			{
				Debug.LogWarning("There is no clock monster.");
				return;
			}
			switch (CurrentHour.Value)
			{
				case 1:
					ClockMonster.instance.normalSpeed = 15f;
					ClockMonster.instance.tickInterval = 0.7f;
					break;
				case 2:
					ClockMonster.instance.normalSpeed = 17f;
					ClockMonster.instance.tickInterval = 0.6f;
					break;
				case 3:
					ClockMonster.instance.normalSpeed = 20f;
					ClockMonster.instance.tickInterval = 0.5f;
					break;
				case 4:
					ClockMonster.instance.normalSpeed = 23f;
					ClockMonster.instance.tickInterval = 0.45f;
					break;
				case 5:
					ClockMonster.instance.normalSpeed = 25f;
					ClockMonster.instance.tickInterval = 0.4f;
					break;
				default:
					ClockMonster.instance.normalSpeed = 20f;
					ClockMonster.instance.tickInterval = 0.7f;
					break;
			}
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			foreach (LobbyPlayer lp in LobbyPlayer.LobbyPlayers)
			{
				Transform spawnpoint = SpawnsQueue.Dequeue();
				var plrObject = Instantiate(InGamePlayerPrefab, spawnpoint.position + Vector3.up*0.1f, spawnpoint.rotation);
				Debug.Log($"Spawned player {lp.PlayerName.Value} at {plrObject.transform.position}");
				plrObject.TryGetComponent(out GamePlayer plr);
				plr.LobbyPlayerObject.Value = lp.gameObject;
				ServerManager.Spawn(plrObject, lp.Owner);
			}
			sv_CurrentTime = 0;

			for (byte i = 0; i <= 5; i++)
			{
				HourSeeds[i] = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			}

			UnityEngine.Random.InitState(HourSeeds[0]);
			CurrentHour.Value = 0;
			CurrentMins.Value = 0;
			Save(0);
		}

		private void Update()
		{
			if (IsServerStarted)
				ServerUpdate();
		}

		[Server]
		private void ServerUpdate()
		{
			if (isTimerPaused) // Sprawdzenie, czy timer jest wstrzymany
			{
				pauseTimer.Value -= Time.deltaTime;
				return;
			}
			sv_CurrentTime += Time.deltaTime;
			var hour = (byte)(Mathf.FloorToInt(sv_CurrentTime / SecondsPerHour));
			if (hour >= 6)
			{
				RpcLoadHour(0);
				return;
			}
			var min = (byte)(Mathf.FloorToInt((sv_CurrentTime % SecondsPerHour) / (SecondsPerHour / 60f)));
			if (hour != CurrentHour.Value)
				CurrentHour.Value = hour;
			if (min != CurrentMins.Value)
				CurrentMins.Value = min;
		}

		[ObserversRpc(RunLocally = true)]
		public void RpcSaveHour(byte hour)
		{
			foreach (var sav in ISaveable.FindAll())
			{
				sav.Save(hour);
			}
			if (IsServerStarted)
			{
				Debug.Log($"Saved Hour {hour}, Seed: {HourSeeds[hour]}");
			}
		}

		[ObserversRpc(RunLocally = true)]
		public void RpcLoadHour(byte hour)
		{
			if(hour > CurrentHour.Value)
			{
				Debug.LogWarning($"[Game Manager] Loading the future is not allowed.");
				return;
			}

			if (HourSeeds.ContainsKey(hour))
			{
				UnityEngine.Random.InitState(HourSeeds[hour]);
				Debug.Log($"Loaded Hour {hour}, Seed: {HourSeeds[hour]}");
			}
			else
			{
				Debug.LogWarning($"No seed found for Hour {hour}");
			}
			GetComponent<AudioSource>().Play();
			foreach (var sav in ISaveable.FindAll())
			{
				sav.Load(hour);
			}
			if (IsServerStarted)
			{
				sv_CurrentTime = hour * SecondsPerHour;
			}
		}

		[Server]
		public void PauseTimer()
		{
			pauseTimer.Value += 30;
		}
		[Server]
		public void ServerOnPlayerDied(GamePlayer plr)
		{
			if (HowManyPlayersAlive() == 0)
			{
				StartCoroutine(DelayedBackHours(10f));
			}
		}

		private IEnumerator DelayedBackHours(float timeToWait)
		{
			yield return new WaitForSecondsRealtime(timeToWait);
			if (HowManyPlayersAlive() == 0)
			{
				RpcLoadHour((byte)Mathf.Clamp(CurrentHour.Value - 3, 0, 5));
			}
			yield break;
		}

		public int HowManyPlayersAlive()
		{
			int alive = 0;
			foreach (GamePlayer aplr in FindObjectsByType<GamePlayer>(FindObjectsSortMode.None))
			{
				if (aplr.IsAlive.Value)
					alive++;
			}
			return alive;
		}
		internal void Started(GamePlayer gamePlayer)
		{
		}
		[ServerRpc(RequireOwnership =false)]
		internal void CmdRequestRewind(byte targetHour, GameObject player)
		{
			if (player == null) return;
			if (!player.TryGetComponent(out GamePlayer plr)) return;
			if (!plr.IsAlive.Value) return;
			if (targetHour > CurrentHour.Value) return;
			RpcLoadHour(targetHour);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GameManager gameManager = (GameManager)target;

			GUILayout.Space(10);

			if (GUILayout.Button("Load Hour 0"))
			{
				gameManager.RpcLoadHour(0);
			}

			if (GUILayout.Button("Load Hour 1"))
			{
				gameManager.RpcLoadHour(1);
			}

			if (GUILayout.Button("Load Hour 2"))
			{
				gameManager.RpcLoadHour(2);
			}

			if (GUILayout.Button("Load Hour 3"))
			{
				gameManager.RpcLoadHour(3);
			}

			if (GUILayout.Button("Load Hour 4"))
			{
				gameManager.RpcLoadHour(4);
			}

			if (GUILayout.Button("Load Hour 5"))
			{
				gameManager.RpcLoadHour(5);
			}
		}
	}
#endif
}
