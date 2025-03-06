using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UmnieDziala.Game.Monsters;
using UnityEngine;
using UnityEngine.Events;

namespace UmnieDziala.Game.Puzzles
{
	public class TickTockMachine : NetworkBehaviour, ISaveable
	{
		[SerializeField, Range(1, 5)] private byte HowManyRounds = 3;
		[SerializeField] private byte[] TicksInRounds;
		public readonly SyncVar<byte> Round = new();
		private List<bool> TicksThisRound;
		private List<bool> PlayerInputs = new();
		private int currentInputIndex;
		[SerializeField] private AudioSource TickTockAudioSource;
		[SerializeField] private AudioClip TickClip;
		[SerializeField] private AudioClip TockClip;
		[SerializeField] private AudioClip FailedClip;
		[SerializeField] private AudioClip CorrectClip;
		[SerializeField] private Renderer TickButton;
		[SerializeField] private Renderer TockButton;
		[SerializeField] private Renderer RequestButtonRenderer;
		public readonly SyncVar<bool> AllowInput = new();
		private bool isSequenceDisplaying = false;
		private byte lastActiveRound = 0;
		private bool _isGameFinished = false;

		private Color _originalRequestButtonColor;
		Color originalEmission;
		public UnityEvent OnSuccessPuzzle = new();
		[SerializeField] private bool IsRequiredToEnd = true;
		private static List<TickTockMachine> RequiredTickTocks = new();
		private void Start()
		{
			_originalRequestButtonColor = RequestButtonRenderer.material.GetColor("_EmissiveColor");
		}
		private void Awake()
		{
			originalEmission = TickButton.material.GetColor("_EmissiveColor");
		}
		private void OnEnable()
		{
			if(!RequiredTickTocks.Contains(this) && IsRequiredToEnd)
				RequiredTickTocks.Add(this);
			AllowInput.OnChange += ChangedAllowInput;
		}
		public override void OnStartServer()
		{
			base.OnStartServer();
			Save(0);
		}
		private void OnDisable()
		{
			RequiredTickTocks.Remove(this);
			AllowInput.OnChange -= ChangedAllowInput;
		}
		public static bool AreAllRequiredDone()
		{
			int done = 0;
			foreach(TickTockMachine tt in RequiredTickTocks)
			{
				if (tt._isGameFinished)
					done++;
			}
			return done >= RequiredTickTocks.Count;
		}
		[ServerRpc(RequireOwnership = false)]
		public void CmdRequestStart()
		{
			if (_isGameFinished) return;

			if (Round.Value == 0)
			{
				Round.Value = 1;
				lastActiveRound = 1;
				SetRound(1);
				return;
			}

			if (isSequenceDisplaying)
			{
				return;
			}

			if (PlayerInputs.Count > 0)
			{
				if (ClockMonster.instance != null)
					ClockMonster.instance.FailedPuzzle(transform.position);
				RpcOnFailed();
				return;
			}

			Round.Value = lastActiveRound;
			SetRound(lastActiveRound);
		}

		[ServerRpc(RequireOwnership = false)]
		private void CmdHandlePlayerInput(bool isTick)
		{
			if (_isGameFinished) return;
			HandlePlayerInput(isTick);
		}

		public void OnPressTick()
		{
			if (AllowInput.Value && !_isGameFinished)
			{
				RpcShowPlayerInput(true);
				CmdHandlePlayerInput(true);
			}
		}

		public void OnPressTock()
		{
			if (AllowInput.Value && !_isGameFinished)
			{
				CmdHandlePlayerInput(false);
			}
		}

		[ObserversRpc]
		private void RpcShowPlayerInput(bool isTick)
		{
			Renderer buttonRenderer = isTick ? TickButton : TockButton;
			float currentPitch = TickTockAudioSource.pitch;
			StartCoroutine(LightUpPlayerButton(buttonRenderer, currentPitch));
		}

		private IEnumerator LightUpPlayerButton(Renderer buttonRenderer, float pitch)
		{
			if (buttonRenderer != null)
			{
				Material mat = buttonRenderer.material;

				mat.SetColor("_EmissiveColor", originalEmission);

				mat.SetColor("_EmissiveColor", originalEmission * 5.0f);
				TickTockAudioSource.pitch = pitch;
				TickTockAudioSource.PlayOneShot(buttonRenderer == TickButton ? TickClip : TockClip);

				yield return new WaitForSeconds(0.4f);

				mat.SetColor("_EmissiveColor", originalEmission);
			}
		}

		public void OnPressRequest()
		{
			if (!_isGameFinished)
			{
				CmdRequestStart();
			}
		}

		private void ChangedAllowInput(bool prev, bool next, bool asServer)
		{
			Color emissionColor = next ? Color.green : _originalRequestButtonColor;
			RequestButtonRenderer.material.SetColor("_EmissiveColor", emissionColor);
		}

		[Server]
		private void SetRound(int round)
		{
			if (round <= 0 || round > TicksInRounds.Length)
			{
				return;
			}

			AllowInput.Value = false;
			TicksThisRound = GenerateTicks(TicksInRounds[round - 1]);
			PlayerInputs = new List<bool>();
			currentInputIndex = 0;
			StartCoroutine(ShowTicksCoroutine());
		}

		private IEnumerator ShowTicksCoroutine()
		{
			isSequenceDisplaying = true;

			RequestButtonRenderer.material.SetColor("_EmissiveColor", Color.yellow);

			foreach (var tick in TicksThisRound)
			{
				RpcShowTick(tick);
				yield return new WaitForSeconds(0.5f);
			}

			RequestButtonRenderer.material.SetColor("_EmissiveColor", _originalRequestButtonColor);

			yield return new WaitForSeconds(.25f);
			AllowInput.Value = true;
			isSequenceDisplaying = false;
		}

		[ObserversRpc]
		private void RpcShowTick(bool tick)
		{
			float pitch = 1.2f;
			TickTockAudioSource.pitch = pitch;
			TickTockAudioSource.PlayOneShot(tick ? TickClip : TockClip);
			StartCoroutine(LightUpButton(tick, pitch));
		}

		private IEnumerator LightUpButton(bool tick, float pitch)
		{
			Renderer buttonRenderer = tick ? TickButton : TockButton;
			if (buttonRenderer != null)
			{
				Material mat = buttonRenderer.material;
				Color originalEmission = mat.GetColor("_EmissiveColor");

				mat.SetColor("_EmissiveColor", originalEmission);

				mat.SetColor("_EmissiveColor", originalEmission * 5.0f);
				yield return new WaitForSeconds(0.4f);

				mat.SetColor("_EmissiveColor", originalEmission);
			}
		}

		private List<bool> GenerateTicks(int ticksCount)
		{
			var ticks = new List<bool>();
			for (int i = 0; i < ticksCount; i++)
				ticks.Add(Random.Range(0, 1f) > 0.5f);
			return ticks;
		}
		[Server]
		private void HandlePlayerInput(bool input)
		{
			if (!AllowInput.Value || _isGameFinished) return;
			RpcShowPlayerInput(input);
			PlayerInputs.Add(input);

			if (PlayerInputs[currentInputIndex] != TicksThisRound[currentInputIndex])
			{
				AllowInput.Value = false;
				PlayerInputs.Clear();
				currentInputIndex = 0;
				if (ClockMonster.instance != null)
					ClockMonster.instance.FailedPuzzle(transform.position);
				RpcOnFailed();
				return;
			}

			currentInputIndex++;

			if (currentInputIndex >= TicksThisRound.Count)
			{
				OnCorrect();
			}
		}

		[ObserversRpc]
		private void RpcOnFailed()
		{
			StartCoroutine(ShowFailureEffect());
		}

		[ObserversRpc]
		private void RpcOnSuccess()
		{
			StartCoroutine(ShowSuccessEffect());
		}

		private IEnumerator ShowSuccessEffect()
		{
			yield return new WaitForSeconds(0.4f);

			TickTockAudioSource.pitch = 1f;
			TickTockAudioSource.clip = (CorrectClip);
			TickTockAudioSource.Play();
			SetButtonEmissionColor(Color.green);

			yield return new WaitForSeconds(1.5f);
			SetButtonEmissionColor(_originalRequestButtonColor);

			if (Round.Value >= HowManyRounds)
			{
				_isGameFinished = true;
				AllowInput.Value = false;
				yield break;
			}

			yield return new WaitForSeconds(1f);
			Round.Value++;
			SetRound(Round.Value);
		}

		private IEnumerator ShowFailureEffect()
		{
			yield return new WaitForSeconds(0.4f);

			TickTockAudioSource.pitch = 1f;
			TickTockAudioSource.PlayOneShot(FailedClip);
			SetButtonEmissionColor(Color.red);

			yield return new WaitForSeconds(1.5f);
			SetButtonEmissionColor(_originalRequestButtonColor);

			yield return new WaitForSeconds(1f);
			SetRound(lastActiveRound);

			AllowInput.Value = true;
		}

		private void SetButtonEmissionColor(Color color)
		{
			RequestButtonRenderer.material.SetColor("_EmissiveColor", color);
			TickButton.material.SetColor("_EmissiveColor", color);
			TockButton.material.SetColor("_EmissiveColor", color);
		}

		[Server]
		public void OnCorrect()
		{
			AllowInput.Value = false;
			if (Round.Value < HowManyRounds)
			{
				StartCoroutine(DelayedNextRound());
			}
			else
			{
				_isGameFinished = true;
				RpcOnSuccess();
				OnSuccessPuzzle.Invoke();
			}
		}

		[Server]
		private IEnumerator DelayedNextRound()
		{
			yield return new WaitForSeconds(2f);
			Round.Value++;
			lastActiveRound = Round.Value;
			SetRound(Round.Value);
		}
		public struct TickTockSavedData
		{
			public bool IsSaved;
			public byte CurrentRound;
			public List<bool> TicksThisRound;
			public List<bool> PlayerInputs;
			public int CurrentInputIndex;
			public bool IsGameFinished;

			public TickTockSavedData(byte currentRound, List<bool> ticksThisRound, List<bool> playerInputs, int currentInputIndex, bool isGameFinished)
			{
				IsSaved = true;
				CurrentRound = currentRound;
				TicksThisRound = new List<bool>(ticksThisRound);
				PlayerInputs = new List<bool>(playerInputs);
				CurrentInputIndex = currentInputIndex;
				IsGameFinished = isGameFinished;
			}
		}
		TickTockSavedData[] HourSaves = new TickTockSavedData[6];
		public void Save(int hour)
		{
			if (!IsServerStarted) return;

			var savedData = new TickTockSavedData(
				Round.Value,
				TicksThisRound?? new List<bool>(),
				PlayerInputs??new List<bool>(),
				currentInputIndex,
				_isGameFinished
			);

			HourSaves[hour] = savedData;
		}

		public void Load(int hour)
		{
			var savedData = HourSaves[hour];
			if (!savedData.IsSaved) return;
			if (IsClientStarted)
			{
				TickTockAudioSource.Stop();
			}
			if (IsServerStarted)
			{
				Round.Value = savedData.CurrentRound;
				TicksThisRound = new List<bool>(savedData.TicksThisRound);
				PlayerInputs = new List<bool>(savedData.PlayerInputs);
				currentInputIndex = savedData.CurrentInputIndex;
				_isGameFinished = savedData.IsGameFinished;

			}
		}
	}
}
