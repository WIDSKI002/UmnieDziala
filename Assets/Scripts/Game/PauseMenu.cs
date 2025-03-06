using FishNet;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UmnieDziala.Game.UI
{
	public class PauseMenu : MonoBehaviour
	{
		[SerializeField] private InputActionReference PauseMenuAction;
		[SerializeField] private GameObject Content;
		public static PauseMenu instance;
		private void Awake()
		{
			PauseMenuAction.action.performed += (ctx) => { SwitchPause(); };
			Content.SetActive(false);
			instance = this;
		}
		
		void SwitchPause()
		{
			if (Content == null) return;
			Content.SetActive(!Content.activeInHierarchy);
			CursorManager.instance.IsPauseMenu = Content.activeInHierarchy;
			if (Content.activeInHierarchy)
			{
				RewindMenu.instance.Open(false);
			}
		}
		public void Resume()
		{
			if (Content.activeInHierarchy)
			{
				SwitchPause();
			}
		}
		public void Disconnect()
		{
			if (InstanceFinder.ServerManager && InstanceFinder.ServerManager.Started)
				InstanceFinder.ServerManager.StopConnection(true);
			if (InstanceFinder.ClientManager && InstanceFinder.ClientManager.Started)
				InstanceFinder.ClientManager.StopConnection();
		}
		public void QuitToDesktop()
		{
			Disconnect();
			Application.Quit();
		}
	}
}