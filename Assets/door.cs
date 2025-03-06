using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UmnieDziala.Game;
using UnityEngine;

public class door : NetworkBehaviour, ISaveable
{
    Animator anim;
    public readonly SyncVar<bool> IsOpened = new();
    [SerializeField] private bool AllowCloseWhenPermanentOpen = false;
    [SerializeField] private bool disableClosing = false;
    [SerializeField] private float CloseTimer = 0f; //a w sumie wlasnie se tak mysle po co ja to zrobilem,
                                   //to akurat niepotrzebne ale moze sie przyda w sumie jednak to nie usuwam
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
		Save(0);
	}
	public override void OnStartServer()
	{
		base.OnStartServer();
		Save(0);
	}
	private void OnEnable()
	{
        IsOpened.OnChange += OpenChanged;
	}
	private void OnDisable()
	{
        IsOpened.OnChange -= OpenChanged;
	}

	private void Update()
	{
        if (IsServerStarted)
        {
            if (CloseTimer > 0f && CloseTimer != float.MaxValue)
                CloseTimer -= Time.deltaTime;
            IsOpened.Value = CloseTimer > 0f;
        }
	}
	private void OpenChanged(bool prev, bool next, bool asServer)
	{
		anim.SetBool("otworz", next);
	}
	[Server]
	public void OpenDoorPermanent()
	{
		OpenDoor();
	}
    [Server]
    public void OpenDoor(bool open = true, float time = float.MaxValue) // musialem w sumie przerobic ten skrypt bo drzwi tez niby mozna otworzyc tymczasowo itd.
    {
        if (!open)
            CloseTimer = 0f;
        else if (time == -1f)
            CloseTimer = float.MaxValue;
        else
        {
            CloseTimer = time;
            disableClosing = !AllowCloseWhenPermanentOpen;
        }
        Debug.Log($"Door opening: {CloseTimer}");
    }
    [ServerRpc (RequireOwnership =false)]
	public void CmdRequestOpenDoor(bool open, float timeToCloseDoor)
	{
        if (disableClosing && !open) return;
		OpenDoor(open, timeToCloseDoor);
	}
    DoorSave[] doorsaves = new DoorSave[6];
    public struct DoorSave
    {
        public bool IsSaved;
        public float CloseTimer;
        public bool WasDisabledClosing;
		public bool wasAnimOpened;
		public float AnimationTime;
	}
	public void Save(int hour)
	{
		AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
		doorsaves[hour] = new DoorSave
		{
			IsSaved = true,
			CloseTimer = CloseTimer,
			WasDisabledClosing = disableClosing,
			wasAnimOpened = anim.GetBool("otworz"),
			AnimationTime = stateInfo.normalizedTime
		};
	}

	public void Load(int hour)
	{
		var savedState = doorsaves[hour];
		if (savedState.IsSaved)
		{
			anim.Play(savedState.wasAnimOpened ? "open" : "Idle", 0, savedState.AnimationTime);
			if (IsServerStarted)
			{
				CloseTimer = savedState.CloseTimer;
				disableClosing = savedState.WasDisabledClosing;
				IsOpened.Value = CloseTimer > 0f;
				IsOpened.DirtyAll();
			}
		}
	}
}
