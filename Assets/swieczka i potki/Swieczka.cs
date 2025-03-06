using FishNet.Object.Synchronizing;
using UmnieDziala.Game.Items;
using UnityEngine;

public class Swieczka : ItemBase
{
    Animator ani;
    public Light fireLight;
    public float minIntensity = 1.0f;
    public float maxIntensity = 2.0f; 
    public float flickerSpeed = 1.5f;
    public Color minColor = new Color(1.0f, 0.4f, 0.0f);
    public Color maxColor = new Color(1.0f, 0.6f, 0.0f);
    public float heightOffset = 0.2f;
    public readonly SyncVar<bool> IsBurning = new();
    [SerializeField] private GameObject fireParticle;

	protected override void Awake()
	{
        base.Awake();
        TryGetComponent(out ani);
	}
	void Start()
    {
        SetLightPositionOnTop();
    }
	public override void UseServer()
	{
		base.UseServer();
        if (!IsBurning.Value)
            IsBurning.Value = true;
	}
	public void DestroyAfterAnimation()
    {
        if (IsServerStarted)
            HideMe();
    }
    void Update()
    {
		fireLight.enabled = IsBurning.Value;
		fireParticle.SetActive(IsBurning.Value);
        if (!IsBurning.Value)
        {
			ani.speed = 0f;
			return;
        }
		ani.SetBool("Trzyma", true);
		ani.speed = 1f;
		float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.0f);
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        fireLight.color = Color.Lerp(minColor, maxColor, noise);
    }
    void SetLightPositionOnTop()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            float objectHeight = renderer.bounds.size.y;
            Vector3 topPosition = transform.position + Vector3.up * (objectHeight / 2 + heightOffset);

            fireLight.transform.position = topPosition;
        }
        else
        {
            Debug.LogWarning("Obiekt nie posiada Renderera. Pozycja światła nie została ustawiona.");
        }
    }
	public override void Save(int hour)
	{
		base.Save(hour);
		AnimatorStateInfo stateInfo = ani.GetCurrentAnimatorStateInfo(0); 
		float animTime = stateInfo.normalizedTime * stateInfo.length;
		candleSaves[hour] = new CandleSaveData(animTime, IsBurning.Value);
	}
	public override void Load(int hour)
	{
		base.Load(hour);
        var save = candleSaves[hour];
        if (save.IsSaved)
        {
            float savedTime = save.AnimTime;
            AnimatorStateInfo stateInfo = ani.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = savedTime / stateInfo.length;
            ani.Play(stateInfo.fullPathHash, 0, normalizedTime);
            if (IsServerStarted)
            {
                IsBurning.Value = save.IsBurning;
            }
            ani.SetBool("Trzyma", save.IsBurning);
        }
	}
	CandleSaveData[] candleSaves = new CandleSaveData[6];
	private struct CandleSaveData
    {
        public bool IsSaved;
        public float AnimTime;
        public bool IsBurning;
        public CandleSaveData(float t, bool burning)
        {
            IsSaved = true;
            AnimTime = t;
            IsBurning = burning;
        }
    }
}
