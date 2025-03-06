using FishNet.Object;
using FishNet.Object.Synchronizing;
using UmnieDziala.Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

namespace UmnieDziala.Game.Player
{
	public class PlayerMovement : NetworkBehaviour
	{
		//nie wiem czy robic wszystko client auth, nawet stamine, ale to gra na 2 osoby z kolegami wiêc jak ktoœ bêdzie cheatowaæ to trudno

		private GamePlayer Player;
		[Header("Movement")]
		private float DisableMovementTimer = 10f;
		[SerializeField] private CharacterController characterController;
		[SerializeField] private InputActionReference moveAction;
		[SerializeField] private InputActionReference jumpAction;
		[SerializeField] private InputActionReference runAction;
		[SerializeField] private float walkSpeed = 5f;
		[SerializeField] private float runSpeed = 8f;
		[SerializeField] private float jumpForce = 2f;
		private float gravity => Physics.gravity.y;
		[SerializeField] private float GravityMultiplier = 2f;

		[SerializeField] private float groundAcceleration = 50f;
		[SerializeField] private float groundFriction = 8f;
		[SerializeField] private float airAcceleration = 12f;
		[SerializeField][Range(0f, 1f)] private float airControl = 0.5f;

		[Header("Stamina")]
		[SerializeField] internal float maxStamina = 100f;
		[SerializeField] private float staminaDrainRate = 10f;
		[SerializeField] private float staminaRegenRate = 5f;
		[SerializeField] private float staminaRegenDelay = 2f;

		private Vector3 velocity;
		[SerializeField] internal float currentStamina; //byc moze potem przeniose na osobny skrypt, np. na skrypt z statystykami i HP itd.
		internal float timeSinceLastRun; //od kiedy nie biega zeby opoznienie zrobic na regeneracje staminy
		private bool isGrounded => characterController.isGrounded;
		private bool isRunning;

		[Header("Other")]
		[SerializeField] private SyncAnimator Animations;
		public readonly SyncVar<Vector3> ForcePosition = new(Vector3.zero);
		public readonly SyncVar<Quaternion> ForceRotation = new(Quaternion.identity);
		public readonly SyncVar<bool> ForceNoMovement = new();
		private void Awake()
		{
			currentStamina = maxStamina;
			TryGetComponent(out Player);
		}

		private void Update()
		{
			if (PauseMenu.instance == null) return;
			if (CursorManager.instance == null) return;
			characterController.enabled = (IsOwner || IsServerStarted) && Player.IsAlive.Value;
			if (!IsOwner) return;
			if (DisableMovementTimer >= 0f)
			{ 
				DisableMovementTimer -= Time.deltaTime;
				return;
			}
			if (ForceNoMovement.Value)
				return;
			if (!Player.IsAlive.Value)
			{
				if (ForcePosition.Value != Vector3.zero && ForceRotation.Value != Quaternion.identity)
				{
					transform.position = ForcePosition.Value;
					transform.rotation = ForceRotation.Value;
				}
				return;
			}
			//isGrounded = characterController.isGrounded;
			HandleMovement();
			HandleJump();
			HandleStamina();
			ApplyGravity();
			characterController.Move(velocity * Time.deltaTime); //nwm czy nie bedzie trzeba na fixedupdate przeniesc w sumie w koncu tego wszystkiego potem
		}

		private void HandleMovement()
		{
			Vector2 input = moveAction.action.ReadValue<Vector2>();
			isRunning = runAction.action.IsPressed() && currentStamina > 0 && input.magnitude > 0.1f;
			float targetSpeed = isRunning ? runSpeed : walkSpeed;

			Vector3 targetDirection = transform.TransformDirection(new Vector3(input.x, 0f, input.y));
			Vector3 targetVelocity = targetDirection * targetSpeed;

			if (isGrounded)
			{
				velocity = Vector3.MoveTowards(velocity, targetVelocity, groundAcceleration * Time.deltaTime);

				if (input.magnitude < 0.1f)
					velocity = Vector3.MoveTowards(velocity, new Vector3(0f, velocity.y, 0f), groundFriction * Time.deltaTime);
			}
			else
			{
				Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
				Vector3 airDesiredVelocity = targetVelocity * airControl;
				Vector3 newVelocity = horizontalVelocity + airDesiredVelocity * airAcceleration * Time.deltaTime;

				if (newVelocity.magnitude > targetSpeed)
					newVelocity = newVelocity.normalized * targetSpeed;

				velocity = new Vector3(newVelocity.x, velocity.y, newVelocity.z);
			}
			if (Vector2.Distance(Animations.WalkDir.Value, input) > 0.05f)
				Animations.CmdSetDirection(input);

			if (Animations.IsWalking.Value != (targetDirection != Vector3.zero))
				Animations.CmdSetWalk((targetDirection != Vector3.zero));

			if (Animations.IsRunning.Value != isRunning)
				Animations.CmdSetRun(isRunning);	
			
			if (Animations.IsGrounded.Value != isGrounded)
				Animations.CmdSetGrounded(isGrounded);
		}

		private void HandleJump()
		{
			if (isGrounded && jumpAction.action.triggered)
			{
				velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity * GravityMultiplier);
				Animations.CmdJumpAnim();
			}
		}

		private void ApplyGravity()
		{
			if (isGrounded)
			{
				if (velocity.y < 0f)
				{
					velocity.y = -2f;
				}
			}
			else
			{
				velocity.y += gravity * GravityMultiplier * Time.deltaTime;
			}
		}

		private void HandleStamina()
		{
			if (isRunning)
			{
				currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
				timeSinceLastRun = 0f;
			}
			else if ((timeSinceLastRun += Time.deltaTime) >= staminaRegenDelay)
				currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
		}
	}
}