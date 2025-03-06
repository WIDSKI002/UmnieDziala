using FishNet;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UmnieDziala.Menu
{
	public class MenuScript : MonoBehaviour
	{
		public static MenuScript instance;
		public static string PlayerName;

		public ushort DefaultPort = 7770;

		[SerializeField] private GameObject MainPanel;
		[SerializeField] private GameObject HostPanel;
		[SerializeField] private GameObject JoinPanel;

		[SerializeField] private TMP_InputField PNameInputHost;
		[SerializeField] private TMP_InputField PNameInputJoin;

		[SerializeField] private TMP_InputField HostPortInput;
		[SerializeField] private TMP_InputField JoinIpInput;
		[SerializeField] private TMP_InputField JoinPortInput;

		[SerializeField] private GameObject ErrorObject;
		[SerializeField] private TextMeshProUGUI ErrorText;
		[SerializeField] private GameObject HostButton;
		[SerializeField] private GameObject JoinButton;

		private void OnEnable()
		{
			PNameInputHost.onValueChanged.AddListener((t) => { if (!PNameInputJoin.gameObject.activeInHierarchy) { PNameInputJoin.text = t; } });
			PNameInputJoin.onValueChanged.AddListener((t) => { if (!PNameInputHost.gameObject.activeInHierarchy) { PNameInputHost.text = t; } });

			InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
		}

		private void OnDisable()
		{
			if(InstanceFinder.ClientManager!=null)
				InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
		}

		private void OnClientConnectionState(ClientConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				ShowError("Stopped (maybe failed) Connection.");
				
			}
		}

		public void Error_Close()
		{
			ErrorObject.SetActive(false);
			HostButton.SetActive(true);
			JoinButton.SetActive(true);
		}

		public void ShowError(string t)
		{
			ErrorObject.SetActive(true);
			ErrorText.text = t;
		}

		public void PressMain_Host()
		{
			MainPanel.SetActive(false);
			HostPanel.SetActive(true);
			HostPortInput.text = $"{DefaultPort}";
			PNameInputHost.text = PlayerName;
			if (string.IsNullOrEmpty(HostPortInput.text)) HostPortInput.text = DefaultPort.ToString();
		}

		public void PressMain_Join()
		{
			MainPanel.SetActive(false);
			JoinPanel.SetActive(true);
			PNameInputJoin.text = PlayerName;
			if (string.IsNullOrEmpty(JoinPortInput.text)) JoinPortInput.text = DefaultPort.ToString();
		}

		public void PressMain_Credits()
		{
			SceneManager.LoadScene("Credits");
		}

		public void PressMain_Quit()
		{
			Application.Quit();
		}

		public void PressHost_Host()
		{
			NetworkManager nm = InstanceFinder.NetworkManager;
			if (nm == null) return;
			if (string.IsNullOrEmpty(HostPortInput.text)) HostPortInput.text = DefaultPort.ToString();
			JoinButton.SetActive(false);
			HostButton.SetActive(false);
			PlayerName = PNameInputHost.text.Trim();
			PlayerName = PlayerName.Substring(0, Mathf.Min(PlayerName.Length, 20));
			if (ushort.TryParse(HostPortInput.text, out var port))
			{
				nm.TransportManager.Transport.SetPort(port);
				if (nm.ServerManager.StartConnection())
				{
					nm.TransportManager.Transport.SetClientAddress("localhost");
					if (!nm.ClientManager.StartConnection())
					{
						ShowError("Nie uda³o siê uruchomiæ klienta.");
						nm.TransportManager.Transport.StopConnection(true);
						nm.TransportManager.Transport.StopConnection(false);
					}
				}
				else
				{
					ShowError("Nie uda³o siê uruchomiæ serwera.");
				}
			}
			else
			{
				ShowError("Nie uda³o siê przetworzyæ portu.");
			}
		}

		public void PressJoin_Join()
		{
			NetworkManager nm = InstanceFinder.NetworkManager;
			if (nm == null) return;
			if (string.IsNullOrEmpty(JoinPortInput.text)) JoinPortInput.text = DefaultPort.ToString();

			JoinButton.SetActive(false);
			HostButton.SetActive(false); 
			PlayerName = PNameInputJoin.text.Trim();
			PlayerName = PlayerName.Substring(0, Mathf.Min(PlayerName.Length, 20));

			if (ushort.TryParse(JoinPortInput.text.Trim(), out var port))
			{
				nm.TransportManager.Transport.SetPort(port);
				nm.TransportManager.Transport.SetClientAddress(JoinIpInput.text.Trim());
				if (!nm.ClientManager.StartConnection())
				{
					ShowError("Nie uda³o siê uruchomiæ klienta.");
					nm.TransportManager.Transport.StopConnection(false);
				}
			}
			else
			{
				ShowError("Nie uda³o siê przetworzyæ portu.");
			}
		}

		public void ReturnToMain()
		{
			MainPanel.SetActive(true);
			JoinPanel.SetActive(false);
			HostPanel.SetActive(false);
		}
	}
}
