using FishNet.Managing.Scened;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UmnieDziala.Menu;
using UnityEngine;
using UnityEngine.Android;

namespace UmnieDziala.Lobby
{
    public class LobbyPlayer : NetworkBehaviour
    {
		public static LobbyPlayer Local;
		public LobbyPlayer LocalNotStatic;
		public static List<LobbyPlayer> LobbyPlayers = new();
		public readonly SyncVar<string> PlayerName = new(new(WritePermission.ServerOnly));
		public readonly SyncVar<bool> Ready = new(new(WritePermission.ServerOnly));
		[Header("References")]
		private LobbyPlayerVisual currentLobbyEntry = null;

		bool SetData = false;
		public override void OnStartClient()
		{
			base.OnStartClient();

			try {
				if(currentLobbyEntry==null)
					currentLobbyEntry = LobbyScript.instance.SpawnPlayerEntry(this);
			}
			catch (Exception ex)
			{
				if (ServerManager.Started)
					ServerManager.StopConnection(true);
				if (ClientManager.Started)
					ClientManager.StopConnection();
				return;
			}
			if (IsOwner)
			{
				Local = this;
				CmdSendPlayerData(MenuScript.PlayerName);
			}
		}
	
		private void OnEnable()
		{
			if(!LobbyPlayers.Contains(this))
				LobbyPlayers.Add(this);
			PlayerName.OnChange += PlayerNameChanged;
			Ready.OnChange += ReadyPlayerChanged;
		}
		private void OnDisable()
		{
			if (LobbyPlayers.Contains(this))
				LobbyPlayers.Remove(this);
			PlayerName.OnChange -= PlayerNameChanged;
			Ready.OnChange -= ReadyPlayerChanged;
		}
		private void PlayerNameChanged(string prev, string next, bool asServer)
		{
			if (currentLobbyEntry)
				currentLobbyEntry.UpdateName(next);
			else
				Debug.LogWarning($"Player's Name has changed but lobby entry isn't spawned.");
		}
		private void ReadyPlayerChanged(bool prev, bool next, bool asServer)
		{
			LobbyScript.instance.UpdateReadyText();
			if (IsServerStarted)
			{
				if (next)
				{
					if (HowManyReadyPlayers() >= 2) 
					{
						LobbyScript.instance.StartCountdown();
					}
				}
				else
					LobbyScript.instance.PlayerLeftOrNotReady();
			}
		}
		private void Update()
		{
			LocalNotStatic = Local;
			if(currentLobbyEntry == null && LobbyScript.instance != null)
			{
				currentLobbyEntry = LobbyScript.instance.SpawnPlayerEntry(this);
			}
		}
		private void OnDestroy()
		{
			if (currentLobbyEntry != null)
				Destroy(currentLobbyEntry.gameObject);
			if (LobbyScript.instance != null)
			{
				LobbyScript.instance.PlayerLeftOrNotReady();
			}
		}
		public override void OnStopServer()
		{
			base.OnStopServer();
			if (LobbyScript.instance == null && SetData)
			{
				ReturnToLobby();
			}
		}
		[Server]
		private void ReturnToLobby()
		{
			var sld = new SceneLoadData("Lobby");
			sld.ReplaceScenes = ReplaceOption.All;
			InstanceFinder.SceneManager.LoadGlobalScenes(sld);
		}

		[ServerRpc (RequireOwnership = true)]
		public void CmdSendPlayerData(string playerName)
		{
			SetData = true;
			if (string.IsNullOrEmpty(playerName))
				playerName = $"Player ({OwnerId})";

			PlayerName.Value = playerName;
		}
		[ServerRpc(RequireOwnership = true)]
		public void CmdPressReady()
		{
			Ready.Value = !Ready.Value;
		}
		public static int HowManyReadyPlayers()
		{
			int rdy = 0;
			foreach(var plr in LobbyPlayers)
			{
				if (plr == null) continue;
				if (plr.Ready.Value)
					rdy++;
			}
			return rdy;
		}
	}
}