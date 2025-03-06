using UnityEngine;
using UnityEngine.Events;

public class PassInteraction : MonoBehaviour, IInteractable
{
	public UnityEvent OnInteract = new(); 
	public void Interact()
	{
		OnInteract.Invoke();
	}
}