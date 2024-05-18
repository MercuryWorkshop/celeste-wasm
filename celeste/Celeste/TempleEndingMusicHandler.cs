using System.Collections.Generic;
using System.Text.RegularExpressions;
using Monocle;

namespace Celeste
{
	public class TempleEndingMusicHandler : Entity
	{
		public const string StartLevel = "e-01";

		public const string EndLevel = "e-09";

		public const string ApplyIn = "e-*";

		private HashSet<string> levels = new HashSet<string>();

		private float startX;

		private float endX;

		public TempleEndingMusicHandler()
		{
			base.Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Regex match = new Regex(Regex.Escape("e-*").Replace("\\*", ".*") + "$");
			foreach (LevelData level in (scene as Level).Session.MapData.Levels)
			{
				if (level.Name.Equals("e-01"))
				{
					startX = level.Bounds.Left;
				}
				else if (level.Name.Equals("e-09"))
				{
					endX = level.Bounds.Right;
				}
				if (match.IsMatch(level.Name))
				{
					levels.Add(level.Name);
				}
			}
		}

		public override void Update()
		{
			base.Update();
			Level level = base.Scene as Level;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && levels.Contains(level.Session.Level) && Audio.CurrentMusic == "event:/music/lvl5/mirror")
			{
				float ease = Calc.Clamp((player.X - startX) / (endX - startX), 0f, 1f);
				level.Session.Audio.Music.Layer(1, 1f - ease);
				level.Session.Audio.Music.Layer(5, ease);
				level.Session.Audio.Apply();
			}
		}
	}
}
