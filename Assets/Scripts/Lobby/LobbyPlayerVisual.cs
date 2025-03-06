using TMPro;
using UnityEngine;

namespace UmnieDziala.Lobby
{
	public class LobbyPlayerVisual : MonoBehaviour
	{
		public LobbyPlayer MyPlayer;
		public TextMeshProUGUI PlayerNameText;
		
		public void Initialize(LobbyPlayer plr)
		{
			MyPlayer = plr;
			UpdateName();
		}

		public void UpdateName()
		{
			if (MyPlayer == null)
				PlayerNameText.text = "Error!";
			else
				PlayerNameText.text = MyPlayer.PlayerName.Value;
		}
		public void UpdateName(string newname)
		{
			PlayerNameText.text = newname;
		}
	}
}