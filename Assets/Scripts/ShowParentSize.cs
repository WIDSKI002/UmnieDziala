using UnityEngine;

[ExecuteInEditMode]
public class ShowParentSize : MonoBehaviour
{
#if UNITY_EDITOR
	private RectTransform rect;
	[SerializeField] private Vector2 size;

	private void Awake()
	{
		TryGetComponent(out rect);
	}

	private void Update()
	{
		if (rect != null && rect.parent != null)
		{
			RectTransform parentRect = rect.parent as RectTransform;

			if (parentRect != null)
			{
				size.x = parentRect.rect.width;
				size.y = parentRect.rect.height;
			}
		}
	}
#endif
}