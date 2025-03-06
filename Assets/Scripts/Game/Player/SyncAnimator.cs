using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

namespace UmnieDziala.Game.Player
{
    public class SyncAnimator : NetworkBehaviour
    {
		private PlayerCamera CameraScript;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private Animator cameraAnimator;
		internal readonly SyncVar<bool> IsWalking = new();
		internal readonly SyncVar<bool> IsRunning = new();
		internal readonly SyncVar<bool> IsGrounded = new();
		internal readonly SyncVar<bool> HasItemInHands = new();
        internal readonly SyncVar<Vector2> WalkDir = new();

		int xHash;
		int yHash;
		private void Awake()
		{
			xHash = Animator.StringToHash("WalkDirX");
			yHash = Animator.StringToHash("WalkDirY");
			TryGetComponent(out CameraScript);
		}
		[ObserversRpc(ExcludeOwner = true)]
        private void RpcJump()
        {
			SetJump();
        }
		private void SetJump()
		{
			m_Animator.SetTrigger("Jump");
		}
		[ServerRpc(RequireOwnership = true, RunLocally = true)]
		internal void CmdJumpAnim()
        {
			if (IsOwner)
				SetJump();
			if (IsServerStarted)
				RpcJump();
		}
		[ServerRpc(RequireOwnership = true)]
		internal void CmdSetGrounded(bool grounded)
        {
			IsGrounded.Value = grounded;
        }
		[ServerRpc(RequireOwnership = true)]
		internal void CmdSetDirection(Vector2 dir)
        {
			WalkDir.Value = dir;
		}	
		[ServerRpc(RequireOwnership = true)]
		internal void CmdSetItem(bool val)
        {
			HasItemInHands.Value = val;
		}
		[ServerRpc(RequireOwnership = true)]
		internal void CmdSetWalk(bool val)
        {
			if (IsOwner)
				cameraAnimator.SetBool("IsWalking", val);
			if (IsServerStarted)
				IsWalking.Value = val;
		}	
		[ServerRpc(RequireOwnership = true, RunLocally = true)]
        internal void CmdSetRun(bool val)
        {
			if (IsOwner)
				cameraAnimator.SetBool("IsRunning", val);
			if(IsServerStarted)
				IsRunning.Value = val;
		}
		private void Update()
		{
            if (m_Animator == null) return;

			float x = m_Animator.GetFloat(xHash);
			float y = m_Animator.GetFloat(yHash);

			x = Mathf.Lerp(x, WalkDir.Value.x, Time.deltaTime * 15f);
			y = Mathf.Lerp(y, WalkDir.Value.y, Time.deltaTime * 15f);

			m_Animator.SetFloat(xHash, x);
			m_Animator.SetFloat(yHash, y);
		}
		private void OnEnable()
		{
            IsWalking.OnChange += WalkChanged;
			IsRunning.OnChange += RunningChanged;
			HasItemInHands.OnChange += HandsItemChanged;
		}

		private void HandsItemChanged(bool prev, bool next, bool asServer)
		{
			m_Animator.SetBool("HasItem", next);
		}

		private void WalkChanged(bool prev, bool next, bool asServer)
		{
			if (cameraAnimator != null && cameraAnimator.enabled)
				cameraAnimator.SetBool("IsWalking", next);
			if (m_Animator == null) return;
            m_Animator.SetBool("IsWalking", next);

		}

		private void RunningChanged(bool prev, bool next, bool asServer)
		{
			if (cameraAnimator != null && cameraAnimator.enabled)
				cameraAnimator.SetBool("IsRunning", next);
			if (m_Animator == null) return;
			m_Animator.SetBool("IsRunning", next);
		}

		private void OnDisable()
		{
			IsWalking.OnChange -= WalkChanged;
			IsRunning.OnChange -= RunningChanged;
			HasItemInHands.OnChange -= HandsItemChanged;
		}
	}
}