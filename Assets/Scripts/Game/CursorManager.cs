using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
	public static CursorManager instance;
	public bool IsRewindMenu;
	public bool IsRewindMenuHoldingMouse;

	public bool IsPauseMenu;

	private void Awake()
	{
		instance = this;
	}
	public static bool IsLockedMouse()
	{
		if (instance == null)
			return false;
		if (instance.IsRewindMenu) return false;
		if (instance.IsPauseMenu) return false;
		return true;
	}
	public static bool IsLockedMouseButVisually()
	{
		if (instance == null)
			return false;
		if (instance.IsPauseMenu) return false;
		if (instance.IsRewindMenuHoldingMouse) return true;
		return false;
	}
	private void Update()
	{
		Cursor.lockState = IsLockedMouse() || IsLockedMouseButVisually() ? CursorLockMode.Locked : CursorLockMode.None;
	}
	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
	}
}