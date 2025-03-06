using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UmnieDziala.Lobby
{
	public class LobbyScript : NetworkBehaviour
	{
		public static LobbyScript instance;

		[Header("Config")]
		[SerializeField, Range(0, 126)] private sbyte Config_TimeToStart = 5;

		[Header("References")]
		[SerializeField] private Transform PlayersParent;
		[SerializeField] private GameObject LobbyEntryPrefab = null;
		[SerializeField] private TextMeshProUGUI HeaderText;
		[SerializeField] private TextMeshProUGUI ReadyButtonText;

		private bool cancelCountdown = false;
		sbyte countdownValue = -1;
		protected override void OnValidate()
		{
			base.OnValidate();
			Config_TimeToStart = (sbyte)Mathf.Clamp(Config_TimeToStart, 0, 126);
		}

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			UpdateReadyText();
		}
		[ObserversRpc (BufferLast =true)]
		private void RpcCountdownChanged(sbyte timeRemaining)
		{
			if (!IsClientStarted) return;
			
			HeaderText.text = timeRemaining < 0
				? "Waiting for players..."
				: $"Starting in <b>{timeRemaining}</b>...";
		}

		[Server]
		private void SwitchToGameScene()
		{
			var sld = new SceneLoadData("Game");
			sld.ReplaceScenes = ReplaceOption.All;
			InstanceFinder.SceneManager.LoadGlobalScenes(sld);
		}

		public void UpdateReadyText()
		{
			ReadyButtonText.text = $"Ready ({LobbyPlayer.HowManyReadyPlayers()}/2)";
		}

		[Server]
		public void StartCountdown()
		{
			if (countdownValue >= 0) return;
			StartCoroutine(CountdownRoutine());
		}

		public void PlayerLeftOrNotReady()
		{
			cancelCountdown = true;
		}

		[Client]
		public void ReadyButtonPress()
		{
			LobbyPlayer.Local.CmdPressReady();
		}

		public void LeaveButtonPress()
		{
			if (ServerManager.Started)
				ServerManager.StopConnection(true);
			if(ClientManager.Started)
				ClientManager.StopConnection();

		}

		[Server]
		private IEnumerator CountdownRoutine()
		{
			if (countdownValue >= 0) yield break;
			Debug.Log($"Starting START Countdown");
			cancelCountdown = false;
			countdownValue = Config_TimeToStart;
			RpcCountdownChanged(countdownValue);
			while (countdownValue > 0)
			{
				if (cancelCountdown)
					break;
				yield return new WaitForSeconds(1f);
				countdownValue--;
				RpcCountdownChanged(countdownValue);
			}
			if (cancelCountdown)
			{
				countdownValue = -1;
				RpcCountdownChanged(-1);
				cancelCountdown = false;
			}
			else
			{
				SwitchToGameScene();
			}
		}

		public LobbyPlayerVisual SpawnPlayerEntry(LobbyPlayer player)
		{
			LobbyPlayerVisual entry = null;
			var spawned = Instantiate(LobbyEntryPrefab, PlayersParent);
			spawned.TryGetComponent(out entry);
			if (entry == null)
				Destroy(spawned);
			else
				entry.Initialize(player);

			return entry;
		}
	}
}
