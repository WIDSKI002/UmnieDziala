using UnityEngine;

namespace UmnieDziala.Game.Player
{
	public partial class GamePlayer
	{
		public struct SavedData
		{
			public bool IsSaved;
			public Vector3 Position;
			public Vector3 Rotation;
			public float Stamina;
			public float MaxStamina;
			public float TimeSinceLastRun;
			public Vector2 CameraRotation;
			public bool IsAlive;

			public SavedData(Vector3 pos, Vector3 rot, float stamina, float maxStamina, float runtimer, Vector2 camRot, bool alive)
			{
				IsSaved = true;
				this.Position= pos;
				this.Rotation= rot;
				this.Stamina = stamina;
				this.MaxStamina = maxStamina;
				this.TimeSinceLastRun= runtimer;
				this.CameraRotation= camRot;
				this.IsAlive= alive;
			}
		}
		public void Save(int hour)
		{
			if (!IsOwner && !IsServerStarted) return; //client auth, zapisuje i ładuje klient u siebie,
								  //nie ma czasu na server auth i zabezpieczenia :|
								  //ale w sumie to nawet nie trzeba
								  //edit: dodałem też warunek na serwer bo isalive jest po stronie serwera chyba

			var saved = new SavedData
			(
				transform.position,
				transform.eulerAngles,
				MovementScript.currentStamina,
				MovementScript.maxStamina,
				MovementScript.timeSinceLastRun,
				CameraScript.cameraRotation,
				IsAlive.Value //to sie przyda tylko do serwera a inne do klienta
			);
			HourSaves[hour] = saved;
		}

		public void Load(int Hour)
		{
			var save = HourSaves[Hour];
			if (!save.IsSaved) return;
			if (IsOwner)
			{
				transform.position = save.Position;
				transform.eulerAngles = save.Rotation;
				MovementScript.currentStamina = save.Stamina;
				MovementScript.maxStamina = save.MaxStamina;
				MovementScript.timeSinceLastRun = save.TimeSinceLastRun;
				CameraScript.cameraRotation = save.CameraRotation;
			}
			if (IsServerStarted)
			{
				IsAlive.Value = save.IsAlive;
				AntiJumpscareTimer = 1f;
			}
		}
	}
}