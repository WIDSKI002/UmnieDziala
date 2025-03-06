using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScript : MonoBehaviour
{
	public void GoBackToMenu()
	{
		SceneManager.LoadScene("Menu");
	}
}