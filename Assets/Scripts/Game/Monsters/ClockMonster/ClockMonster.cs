using FishNet.Example.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using UmnieDziala.Game.Player;
using UmnieDziala.Game.UI;
using UnityEngine;
using UnityEngine.AI;

namespace UmnieDziala.Game.Monsters
{
	public partial class ClockMonster : NetworkBehaviour, ISaveable
	{
		public static ClockMonster instance;
		[Header("References")]
		[SerializeField] private Animator Animator;
		[SerializeField] private NavMeshAgent navAgent;
		[SerializeField] private AudioSource myAudioSource;
		[SerializeField] private AudioClip TickClip1;
		[SerializeField] private AudioClip TickClip2;

		[Header("Detection Settings")]
		[SerializeField] private float detectionRadius = 10f;
		[SerializeField] private float viewAngle = 120f;
		[SerializeField] private LayerMask LookedAtLayer;
		[SerializeField] private LayerMask playerLayer;
		[SerializeField] private LayerMask obstacleLayer;
		[SerializeField] private float catchDistance = 1.5f;
		[SerializeField] private float targetMemoryTime = 3f;
		private float detectionTimer = 0f;
		[SerializeField] private float detectionInterval = 0.2f;

		[Header("Wandering Settings")]
		[SerializeField] private float RandomWalkRange = 20f;
		[SerializeField] public float tickInterval = 0.5f;
		[SerializeField] private float lookedAtTickIntervalMultiplier = 1.5f;
		[SerializeField] public float normalSpeed = 30f;
		[SerializeField] private float moveTime = 0.15f;
		[SerializeField] private int maxNavAttempts = 5;

		[Header("Realtime")]
		[SerializeField] private Transform CurrentTarget { get => CurrentTargetSync.Value?.transform; set { CurrentTargetSync.Value = value?.gameObject; } }

		public readonly SyncVar<GameObject> CurrentTargetSync = new(null);
		private bool tickortock = false;
		private float lostTargetTimer = 0f;
		private Vector3 lastPlayerPos;
		private bool LookingForPlayer = false;
		[Header("Jumpscare")]
		[SerializeField] private AudioSource JumpscareSound;
		[SerializeField] private Transform CameraTargetOnJumpscare;
		Vector3 lastpos;

		bool isLocalJumpscared;
		bool canceledJumpscare;
		public readonly SyncVar<bool> WalkAnim = new();
		[SerializeField, Range(0.1f, 5f)] float WalkSpeedScalar = 2f;
		public float TimerAfterStart = 1f;
		private void Awake()
		{
			instance = this;
			lastpos = transform.position;
		}
		public override void OnStartServer()
		{
			base.OnStartServer();

			if (!IsServerStarted)
				return;

			Save(0);
			TimerAfterStart = 1f;
			SetRandomDestination();
			StartCoroutine(ClockTickMovement());
		}

		[Server]
		private IEnumerator ClockTickMovement() //tu sobie jest g��wna p�tla potwora od poruszania sie
		{
			while (true)
			{
				float finaltick = tickInterval;
				if (IsBeingLookedAt())
					finaltick *= lookedAtTickIntervalMultiplier;

				yield return new WaitForSeconds(finaltick);

				while (GameManager.instance.pauseTimer.Value > 0f)
					yield return new WaitForSecondsRealtime(1f); //niech ta mikstura zatrzymuje potwora

				if (TimerAfterStart>=0f)
					yield return new WaitForSecondsRealtime(TimerAfterStart);
				TimerAfterStart = 0f;

				PlayTickSound();
				float baseSpeed = normalSpeed;
				navAgent.speed = baseSpeed;

				yield return new WaitForSeconds(moveTime);

				navAgent.speed = 0f;
			}
		}
		[Server]
		private bool IsBeingLookedAt()
		{
			foreach (PlayerCamera pc in PlayerCamera.ActiveCameras) //bo ka�dy gracz mo�e patrze�
			{
				Transform camTransform = pc.CameraTransform;
				if (camTransform == null)
					continue;

				float distance = Vector3.Distance(camTransform.position, transform.position);
				if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, distance, LookedAtLayer))
				{
					if (hit.transform == transform || hit.transform.IsChildOf(transform))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void SetRandomDestination()
		{
			if (!IsServerStarted)
				return;

			bool found = false;
			int att = 0;
			while (!found && att < maxNavAttempts)
			{
				att++;
				//insideunitsphere to chyba z tego co rozumiem w kuli dookoła kierunek, mogłoby być circle w sumie
				Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * RandomWalkRange;
				if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, RandomWalkRange, NavMesh.AllAreas))
				{
					NavMeshPath path = new NavMeshPath();
					navAgent.CalculatePath(hit.position, path);

					if (path.status == NavMeshPathStatus.PathComplete)
					{
						navAgent.SetDestination(hit.position);
						found = true;
					}
				}
			}
			if (!found)
			{
				//Debug.LogWarning($"Nadal nie moglo znalezc punktu na navmeshu");
				//Debug.LogWarning("I still couldn't find a valid navmesh position.");
			}
		}

		[Server]
		private void DetectPlayers()
		{
			if (!IsServerStarted)
				return;

			if (CurrentTarget != null)
				return;

			Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
			foreach (Collider col in hitColliders)
			{
				Transform player = col.transform;
				if (!player.TryGetComponent(out GamePlayer plr))
					continue;
				if (!plr.IsAlive.Value)
					continue;

				Vector3 directionToPlayer = (player.position - transform.position).normalized;
				float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

				if (angleToPlayer <= viewAngle)
				{
					if (HasLineOfSightTo(player) && plr.AntiJumpscareTimer<=0f)
					{
						CurrentTarget = player;
						navAgent.SetDestination(player.position);
						LookingForPlayer = false;
						lostTargetTimer = 0f;
						break;
					}
				}
			}
		}
		private void OnEnable()
		{
			WalkAnim.OnChange += changedWalkAnim;
		}
		private void OnDisable()
		{
			WalkAnim.OnChange -= changedWalkAnim;
		}

		private void changedWalkAnim(bool prev, bool next, bool asServer)
		{
			Animator.SetBool("Walk", next);
		}

		private void Update()
		{
			Animator.SetFloat("AnimSpeed", (transform.position-lastpos).magnitude * 10f * WalkSpeedScalar);
			lastpos= transform.position;
			if (!IsServerStarted)
				return;
			WalkAnim.Value = navAgent.enabled;
			if (navAgent.enabled)
			{
				detectionTimer += Time.deltaTime;
				if (detectionTimer >= detectionInterval) //bo chyba by troch� lagowa�o wi�c odst�py czasu mi�dzy wykrywaniem ale nie chcia�em przy tickach tego robic
				{
					DetectPlayers();
					detectionTimer = 0f;
				}

				CheckCatchTarget();

				if (CurrentTarget == null && navAgent.enabled && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && TimerAfterStart<=0f)
				{
					SetRandomDestination();
				}
			}
		}
		[Server]
		private bool HasLineOfSightTo(Transform target)
		{
			Vector3 directionToTarget = (target.position - transform.position).normalized;
			Vector3 targetPosition = target.position + Vector3.up;
			float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
			Vector3 startPosition = transform.position + Vector3.up * 1.7f;

			Ray ray = new Ray(startPosition, directionToTarget);
			if (!Physics.Raycast(ray, out RaycastHit hitInfo, distanceToTarget, obstacleLayer))
			{
				return true;
			}
			return false;
		}

		[Server]
		private void CheckCatchTarget()
		{
			if (!IsServerStarted || CurrentTarget == null)
				return;

			if (!HasLineOfSightTo(CurrentTarget))
			{
				if (lostTargetTimer == 0f)
				{
					lastPlayerPos = CurrentTarget.position;
				}
				lostTargetTimer += Time.deltaTime;
				if (lostTargetTimer >= targetMemoryTime)
				{
					LookingForPlayer = true;
					navAgent.SetDestination(lastPlayerPos);
					CurrentTarget = null;
					lostTargetTimer = 0f;
				}
				return;
			}

			lostTargetTimer = 0f;
			navAgent.SetDestination(CurrentTarget.position);

			var plr = CurrentTarget.GetComponent<GamePlayer>();//getcomponent co klatke :( bo nie ma czasu
			float dist = Vector3.Distance(transform.position, CurrentTarget.position);
			if (dist <= catchDistance && plr.AntiJumpscareTimer<=0f && plr.IsAlive.Value) 
			{
				OnPlayerCaught();
			}
		}


		[Server]
		private void PlayTickSound()
		{
			if (!IsServerStarted)
				return;

			tickortock = !tickortock;
			RpcPlayTickSound(tickortock);
		}

		[ObserversRpc]
		private void RpcPlayTickSound(bool which)
		{
			var clip = which ? TickClip2 : TickClip1;
			float speedMultiplier = 1f;

			switch (GameManager.instance.CurrentHour.Value)
			{
				case 1:
					speedMultiplier = 1.2f;
					break;
				case 2:
					speedMultiplier = 1.5f;
					break;
				case 3:
					speedMultiplier = 1.8f;
					break;
				case 4:
					speedMultiplier = 2f;
					break;
				case 5:
					speedMultiplier = 2.5f;
					break;
				default:
					speedMultiplier = 1f;
					break;
			}

			myAudioSource.pitch = speedMultiplier; // Ustawienie prędkości odtwarzania dźwięku
			myAudioSource.PlayOneShot(clip);
		}

		[Server]
		public void OverrideDestination(Vector3 position)
		{
			if (!IsServerStarted)
				return;

			if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
			{
				CurrentTarget = null;
				LookingForPlayer = false;
				navAgent.SetDestination(hit.position);
			}
		}
		[Server]
		public void FailedPuzzle(Vector3 position)
		{
			if (CurrentTarget == null)
				OverrideDestination(position);
		}
		[Server]
		public void OnPlayerCaught()
		{
			if (!IsServerStarted)
				return;
			Debug.Log($"Current Target: {CurrentTarget}");
			StartCoroutine(KillPlayer(CurrentTarget.gameObject));

			CurrentTarget = null;
			LookingForPlayer = false;
			
		}
		[Server]
		private IEnumerator KillPlayer(GameObject plrobj)
		{
			if (!plrobj.TryGetComponent(out GamePlayer plr)) yield break;

			navAgent.enabled = false;
			plr.IsAlive.Value = false;
			plr.MovementScript.ForcePosition.Value = plrobj.transform.position;
			plr.MovementScript.ForceRotation.Value = plrobj.transform.rotation;
			RpcStartJumpscare(plrobj);
			float t = 0f;
			DateTime originalLoadTime = lastLoad;
			while (t <= 4f && lastLoad == originalLoadTime)
			{
				yield return new WaitForEndOfFrame();
				t += Time.deltaTime;
			}
			if (t < 4f)
			{
				RpcCancelJumpscare(plrobj);
			}
			else
			{
				navAgent.enabled = true;
				SetRandomDestination();
			}
		}
		[ObserversRpc(RunLocally =true)]
		void RpcStartJumpscare(GameObject plro)
		{
			isLocalJumpscared = false;
			canceledJumpscare = false;
			JumpscareSound.Play();
			GamePlayer myPlayer = GamePlayer.Local;
			Animator.SetBool("Jumpscare", true);
			if (plro == myPlayer.gameObject)
			{
				isLocalJumpscared = true;
				myPlayer.CameraScript.CameraFollow = CameraTargetOnJumpscare;
			}
		}
		[ObserversRpc(RunLocally =true)]
		void RpcCancelJumpscare(GameObject plro)
		{
			Debug.Log($"RPC CANCEL JUMPSCARE");
			isLocalJumpscared = false;
			canceledJumpscare = true;
			JumpscareSound.Stop();
			GamePlayer myPlayer = GamePlayer.Local;
			Animator.SetBool("Jumpscare", false);
			if (plro == myPlayer.gameObject)
			{
				myPlayer.CameraScript.CameraFollow = null;
				DeathUI.instance.SetPlay(false);
			}
		}
		public void EndedJumpscare()
		{
			if (canceledJumpscare) 
			{
				Debug.Log($"END JUMPSCARE CANCEL");
				return; 
			}
			if (isLocalJumpscared)
			{
				GamePlayer.Local.CameraScript.CameraFollow = null;
				DeathUI.instance.SetPlay(true);
				isLocalJumpscared = false;
			}
			Animator.SetBool("Jumpscare", false);
		}
	}
}
