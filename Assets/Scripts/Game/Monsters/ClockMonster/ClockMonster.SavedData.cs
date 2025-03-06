using FishNet.Object;
using System;
using UmnieDziala.Game.Player;
using UnityEngine;

namespace UmnieDziala.Game.Monsters
{
	public partial class ClockMonster
	{
		[SerializeField] private SavedData[] HourSaves = new SavedData[6];
		private DateTime lastLoad;
		public struct SavedData
		{
			public bool IsSaved;
			public Vector3 Position;
			public Vector3 Rotation;
			public Vector3 Destination;
			public bool IsInvestigating;
			public float TimeSinceTargetLost;
			public Vector3 LastKnownPlayerPosition;
			public bool HasTarget;
			public int TargetInstanceID;
			public bool agentEnabled;
			public SavedData(Vector3 pos, Vector3 rot, Vector3 destination, bool isInvestigating, float timeSinceTargetLost, Vector3 lastKnownPlayerPosition, bool hasTarget, int targetInstanceID, bool agentenable)
			{
				IsSaved = true;
				Position = pos;
				Rotation = rot;
				Destination = destination;
				IsInvestigating = isInvestigating;
				TimeSinceTargetLost = timeSinceTargetLost;
				LastKnownPlayerPosition = lastKnownPlayerPosition;
				HasTarget = hasTarget;
				TargetInstanceID = targetInstanceID;
				agentEnabled= agentenable;
			}
		}
		[Server]
		public void Save(int hour)
		{
			if (!IsServerStarted) return;

			int targetID = CurrentTarget ? CurrentTarget.GetInstanceID() : -1;

			var saved = new SavedData
			(
				transform.position,
				transform.eulerAngles,
				navAgent.destination,
				LookingForPlayer,
				lostTargetTimer,
				lastPlayerPos,
				CurrentTarget != null,
				targetID,
				navAgent.enabled
			);
			TimerAfterStart = 1f;
			HourSaves[hour] = saved;
		}
		[Server]
		public void Load(int hour)
		{
			if (!IsServerStarted) return;

			var save = HourSaves[hour];
			if (!save.IsSaved) return;
			navAgent.enabled = false;
			transform.position = save.Position;
			transform.eulerAngles = save.Rotation;
			LookingForPlayer = save.IsInvestigating;
			lostTargetTimer = save.TimeSinceTargetLost;
			lastPlayerPos = save.LastKnownPlayerPosition;
			navAgent.enabled = save.agentEnabled;
			if (save.HasTarget)
			{
				CurrentTarget = FindTransformByInstanceID(save.TargetInstanceID);
			}
			else
			{
				CurrentTarget = null;
			}
			navAgent.SetDestination(save.Destination);
			lastLoad = DateTime.Now;
			TimerAfterStart = 1f;
		}
		private Transform FindTransformByInstanceID(int instanceID)
		{
			foreach (var player in FindObjectsByType<GamePlayer>(FindObjectsSortMode.None))
			{
				if (player.transform.GetInstanceID() == instanceID)
				{
					return player.transform;
				}
			}
			return null;
		}
	}
}
