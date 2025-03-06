using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UmnieDziala.Game.UI
{
	public class NoteUI : MonoBehaviour
	{
		public static NoteUI instance;
		public Image Crosshair;
		[SerializeField] private TextMeshProUGUI noteTextUI;
		[SerializeField] private TextMeshProUGUI noteSignatureUI;
		[SerializeField] private GameObject noteParent;
		public bool DisallowClose = false;
		internal void Close()
		{
			if (DisallowClose) return;
			noteParent.SetActive(false);
		}

		internal void Open(Note note)
		{
			noteTextUI.text = note.noteText;
			noteSignatureUI.text = note.noteSignatureText;
			noteParent.SetActive(true);
		}
		private void Awake()
		{
			instance = this;
		}
	}
}