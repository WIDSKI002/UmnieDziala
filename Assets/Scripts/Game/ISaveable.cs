using System.Collections.Generic;
using UnityEngine;
using static UmnieDziala.Game.Player.GamePlayer;

namespace UmnieDziala.Game
{
	public interface ISaveable
	{
		/// <summary>
		/// Saves the object's state for a given hour. 
		/// Must differentiate between server and client loading logic inside method in implementing class.
		/// </summary>
		public void Save(int hour);

		/// <summary>
		/// Loads the object's state for a given hour. 
		/// Must differentiate between server and client loading logic inside method in implementing class.
		/// </summary>
		public void Load(int hour);

		/// <summary>
		/// Finds all Saveable objects. 
		/// </summary>
		public static ISaveable[] FindAll()
		{
			var saveableObjects = new List<ISaveable>();
			foreach (var obj in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			{
				if (obj is ISaveable saveable)
					saveableObjects.Add(saveable);
			}
			return saveableObjects.ToArray();
		}
	}
}
