using FishNet.Object;
using System;
using UmnieDziala.Game.Player;
using UmnieDziala.Game.UI;
using UmnieDziala.Lobby;
using UnityEngine;

public class EndGameInteract : NetworkBehaviour, IInteractable
{
	[SerializeField] private Note NoteToOpen;
	bool ended = false;
	public void Interact()
	{

		CmdEndGame();
	}
	[ServerRpc(RequireOwnership =false)]
	private void CmdEndGame()
	{
		if (ended) return;
		ended = true;
		RpcOpenNote();
		foreach(GamePlayer plr in FindObjectsByType<GamePlayer>(FindObjectsSortMode.None))
		{
			plr.MovementScript.ForceNoMovement.Value = true;
		}
		Invoke("CloseServer", 15f);
	}
	void CloseServer()
	{
		CancelInvoke("CloseServer");
		PauseMenu.instance.Disconnect();
	}
	[ObserversRpc]
	private void RpcOpenNote()
	{
		NoteUI.instance.Open(NoteToOpen);
		NoteUI.instance.DisallowClose = true;
	}
}
