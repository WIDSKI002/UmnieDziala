using UmnieDziala.Game.Monsters;
using UmnieDziala.Game.Player;
using UnityEngine;

public class ChaseMusic : MonoBehaviour
{
	[SerializeField] private AudioSource ChaseMusicSource;
	private float VolumeTarget
	{
		get
		{
			if (ClockMonster.instance == null) return 0;
			if (GamePlayer.Local == null) return 0;
			if (ClockMonster.instance.CurrentTargetSync.Value == GamePlayer.Local.gameObject) return 0.6f;
			return 0;
		}
	}
	private void Update()
	{
		ChaseMusicSource.volume = Mathf.Lerp(ChaseMusicSource.volume, VolumeTarget, Time.deltaTime);
		if(ChaseMusicSource.volume <= 0.01f)
		{
			if(ChaseMusicSource.isPlaying)
				ChaseMusicSource.Stop();
		}
		else if(!ChaseMusicSource.isPlaying)
		{
			ChaseMusicSource.Play();
		}
	}
}
