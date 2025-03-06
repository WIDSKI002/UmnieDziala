using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit;
using System;
using System.Collections;
using UmnieDziala.Game.Items;
using UmnieDziala.Game.Player;
using Unity.Multiplayer.Center.Common;
using UnityEngine;
using UnityEngine.Events;
using static UmnieDziala.Game.Items.Cable;

namespace UmnieDziala.Game.Puzzles
{
	public class CableBox : NetworkBehaviour, ISaveable, IInteractable
	{
		[SerializeField] private CableColor[] StartCableColors = new CableColor[3];

		[SerializeField] private bool RequireSequentialCableOrder = false;

		public readonly SyncVar<CableColor> Cable1 = new();
		public readonly SyncVar<CableColor> Cable2 = new();
		public readonly SyncVar<CableColor> Cable3 = new();

		public readonly SyncVar<bool> Cable1Done = new();
		public readonly SyncVar<bool> Cable2Done = new();
		public readonly SyncVar<bool> Cable3Done = new();

		[SerializeField] private Renderer Cable1R;
		[SerializeField] private Renderer Cable2R;
		[SerializeField] private Renderer Cable3R;

		[SerializeField] private UnityEvent ServerOnAllCablesDone;

		[SerializeField] private Renderer[] ShowWhatColorRenderers;

		[SerializeField] private CableBox[] RequireMoreCableBoxesToFireEvent;

		private DateTime lastload;

		private void OnEnable()
		{
			Cable1.OnChange += OnCableColorChange;
			Cable2.OnChange += OnCableColorChange;
			Cable3.OnChange += OnCableColorChange;

			Cable1Done.OnChange += OnCableDoneChange;
			Cable2Done.OnChange += OnCableDoneChange;
			Cable3Done.OnChange += OnCableDoneChange;
		}

		private void OnDisable()
		{
			Cable1.OnChange -= OnCableColorChange;
			Cable2.OnChange -= OnCableColorChange;
			Cable3.OnChange -= OnCableColorChange;

			Cable1Done.OnChange -= OnCableDoneChange;
			Cable2Done.OnChange -= OnCableDoneChange;
			Cable3Done.OnChange -= OnCableDoneChange;
		}

		private void OnCableColorChange(CableColor prev, CableColor next, bool asServer)
		{
			UpdateCableRenderers();
		}

		private void OnCableDoneChange(bool prev, bool next, bool asServer)
		{
			UpdateCableRenderers();
		}

		private void UpdateCableRenderers()
		{
			SetCableProperties(Cable1R, Cable1.Value, Cable1Done.Value);
			SetCableProperties(Cable2R, Cable2.Value, Cable2Done.Value);
			SetCableProperties(Cable3R, Cable3.Value, Cable3Done.Value);

			ShowWhatColorRenderers[0].material.color = GetColorFromCable(Cable1.Value);
			ShowWhatColorRenderers[1].material.color = GetColorFromCable(Cable2.Value);
			ShowWhatColorRenderers[2].material.color = GetColorFromCable(Cable3.Value);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			Cable1.Value = StartCableColors[0];
			Cable2.Value = StartCableColors[1];
			Cable3.Value = StartCableColors[2];

			Save(0);
		}

		private void SetCableProperties(Renderer renderer, CableColor color, bool isActive)
		{
			if (renderer == null) return;

			renderer.material.color = GetColorFromCable(color);
			renderer.enabled = isActive;
		}

		private Color GetColorFromCable(CableColor color)
		{
			return color switch
			{
				CableColor.Red => Color.red,
				CableColor.Green => Color.green,
				CableColor.Blue => Color.blue,
				CableColor.Black => Color.black,
				CableColor.Purple => new Color(0.5f, 0f, 0.5f),
				CableColor.Yellow => Color.yellow,
				CableColor.Pink => new Color(1f, 0.4f, 0.7f),
				CableColor.Brown => new Color(0.6f, 0.3f, 0f),
				CableColor.Orange => new Color(1f, 0.5f, 0f),
				CableColor.White => Color.white,
				_ => Color.gray,
			};
		}

		public void Interact()
		{
			if (GamePlayer.Local == null) return;
			var inv = GamePlayer.Local.Inventory;
			if (inv.SelectedItem == null) return;

			if (inv.SelectedItem.TryGetComponent(out Cable cable))
			{
				if (NeedsCable(cable))
				{
					CmdRequestCablePut(cable.gameObject);
				}
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void CmdRequestCablePut(GameObject cableObject)
		{
			if (cableObject.TryGetComponent(out Cable cable))
			{
				if (cable.IsUsed.Value) return;
				if (NeedsCable(cable))
				{
					AddFirstNeededCable(cable);
				}
			}
		}

		private bool NeedsCable(Cable cable)
		{
			if (RequireSequentialCableOrder)
			{
				if (!Cable1Done.Value && Cable1.Value == cable.SyncColor.Value)
					return true;
				if (!Cable2Done.Value && Cable2.Value == cable.SyncColor.Value)
					return true;
				if (!Cable3Done.Value && Cable3.Value == cable.SyncColor.Value)
					return true;

				return false;
			}
			else
			{
				return (!Cable1Done.Value && Cable1.Value == cable.SyncColor.Value) ||
					   (!Cable2Done.Value && Cable2.Value == cable.SyncColor.Value) ||
					   (!Cable3Done.Value && Cable3.Value == cable.SyncColor.Value);
			}
		}
		public override void OnStartClient()
		{
			base.OnStartClient();
			UpdateCableRenderers();
		}
		private void AddFirstNeededCable(Cable cable)
		{
			if (!Cable1Done.Value && Cable1.Value == cable.SyncColor.Value)
			{
				Cable1Done.Value = true;
			}
			else if (!Cable2Done.Value && Cable2.Value == cable.SyncColor.Value)
			{
				Cable2Done.Value = true;
			}
			else if (!Cable3Done.Value && Cable3.Value == cable.SyncColor.Value)
			{
				Cable3Done.Value = true;
			}

			cable.HideMe();
			OnCableAdded();
		}

		[Server]
		private void OnCableAdded()
		{
			if (IsDone())
			{
				AllCablesDone();
			}
		}

		[Server]
		private void AllCablesDone()
		{
			StartCoroutine(DoneEvent());
		}

		private IEnumerator DoneEvent()
		{
			if (!IsDone())
				yield break;

			if (RequireMoreCableBoxesToFireEvent.Length > 0)
			{
				var lt = lastload;
				int done = 0;
				bool[] completed = new bool[RequireMoreCableBoxesToFireEvent.Length];

				while (done != RequireMoreCableBoxesToFireEvent.Length)
				{
					for (int i = 0; i < RequireMoreCableBoxesToFireEvent.Length; i++)
					{
						if (!completed[i] && RequireMoreCableBoxesToFireEvent[i].IsDone())
						{
							completed[i] = true;
							done++;
						}
					}
					yield return new WaitForSecondsRealtime(1);
					if (lastload != lt)
						yield break;
				}
			}
			ServerOnAllCablesDone.Invoke();
		}

		private bool IsDone()
		{
			return Cable1Done.Value && Cable2Done.Value && Cable3Done.Value;
		}
		private CableBoxSave[] cableboxsaves = new CableBoxSave[6];
		private struct CableBoxSave
		{
			public bool IsSaved;
			public CableColor Cable1;
			public CableColor Cable2;
			public CableColor Cable3;
			public bool Cable1Done;
			public bool Cable2Done;
			public bool Cable3Done;

			public CableBoxSave(CableColor cable1, CableColor cable2, CableColor cable3, bool cable1Done, bool cable2Done, bool cable3Done)
			{
				IsSaved = true;
				Cable1 = cable1;
				Cable2 = cable2;
				Cable3 = cable3;
				Cable1Done = cable1Done;
				Cable2Done = cable2Done;
				Cable3Done = cable3Done;
			}
		}

		public void Save(int hour)
		{
			if (IsServerStarted)
			{
				cableboxsaves[hour] = new CableBoxSave(Cable1.Value, Cable2.Value, Cable3.Value, Cable1Done.Value, Cable2Done.Value, Cable3Done.Value);
			}
		}

		public void Load(int hour)
		{
			var save = cableboxsaves[hour];
			if (!save.IsSaved) return;

			if (IsServerStarted)
			{
				Cable1.Value = save.Cable1;
				Cable2.Value = save.Cable2;
				Cable3.Value = save.Cable3;
				Cable1Done.Value = save.Cable1Done;
				Cable2Done.Value = save.Cable2Done;
				Cable3Done.Value = save.Cable3Done;
				lastload = DateTime.Now;
			}
		}
	}
}