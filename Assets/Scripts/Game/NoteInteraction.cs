using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Object;
using UmnieDziala.Game.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class NoteInteraction : NetworkBehaviour
{
	public Transform playerCamera;
	public float interactionDistance = 3f;
	public Image crosshair => NoteUI.instance?.Crosshair;
	public Color normalColor = Color.red;
	public Color highlightColor = Color.green;
	public InputActionReference interactAction;
	public InputActionReference closeNoteAction;
	[SerializeField] private LayerMask layerMask;

	private bool noteActive = false;

	void Update()
	{
		if (!IsOwner) return;
		if (crosshair == null) return;

		UpdateCrosshair();

		if (interactAction.action.WasReleasedThisFrame())
		{
			TryInteractWithNote();
		}
		else if (closeNoteAction.action.WasPressedThisFrame() || closeNoteAction.action.WasReleasedThisFrame())
		{
			CloseNote();
		}
	}

	void UpdateCrosshair()
	{
		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionDistance, layerMask))
		{
			if (hit.collider.CompareTag("note"))
			{
				crosshair.color = highlightColor;
				return;
			}
		}
		crosshair.color = normalColor;
	}

	void TryInteractWithNote()
	{
		if (!IsOwner) return;

		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionDistance, layerMask))
		{
			if (hit.collider.CompareTag("note"))
			{
				Note note = hit.collider.GetComponent<Note>();
				if (note != null)
				{
					OpenNote(note);
				}
			}
		}
	}

	void OpenNote(Note note)
	{
		if (!IsOwner) return;

		if (NoteUI.instance != null)
		{
			NoteUI.instance.Open(note);
			noteActive = true;
		}
	}

	void CloseNote()
	{
		if (!IsOwner) return;
		if (noteActive && NoteUI.instance != null)
		{
			NoteUI.instance.Close();
			noteActive = false;
		}
	}
}
