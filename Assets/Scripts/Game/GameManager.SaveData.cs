namespace UmnieDziala.Game
{
	public partial class GameManager
	{
		private SaveData[] saves = new SaveData[6];
		public void Save(int hour)
		{
			if (IsServerStarted)
			{
				saves[hour] = new SaveData(pauseTimer.Value);
			}
		}

		public void Load(int hour)
		{
			var save = saves[hour];
			if (!save.IsSaved) return;
			if (IsServerStarted)
			{
				pauseTimer.Value = save.PauseTimer;
			}
		}
		private struct SaveData
		{
			public bool IsSaved;
			public float PauseTimer;

			public SaveData(float pausetimer)
			{
				IsSaved = true;
				PauseTimer = pausetimer;
			}
		}
	}
}
