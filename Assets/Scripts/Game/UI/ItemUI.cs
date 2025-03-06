using UnityEngine;
using UnityEngine.UI;

namespace UmnieDziala.Game.UI
{
	public class ItemUI : MonoBehaviour
	{
		public static ItemUI instance;
		public Image[] itemSlotIcons = new Image[3];
		public Image[] itemSlotBackground = new Image[3];
		public Image Crosshair;
		public GameObject Content;

		public void SetVisible(bool vis)
		{
			Content.SetActive(vis);
		}
		private void Awake()
		{
			instance = this;
		}
	}
}