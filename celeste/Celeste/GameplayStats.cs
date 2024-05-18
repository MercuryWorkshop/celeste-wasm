using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class GameplayStats : Entity
	{
		public float DrawLerp;

		public GameplayStats()
		{
			base.Depth = -101;
			base.Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
		}

		public override void Update()
		{
			base.Update();
			Level level = base.Scene as Level;
			DrawLerp = Calc.Approach(DrawLerp, (level.Paused && level.PauseMainMenuOpen && level.Wipe == null) ? 1 : 0, Engine.DeltaTime * 8f);
		}

		public override void Render()
		{
			if (DrawLerp <= 0f)
			{
				return;
			}
			float ease = Ease.CubeOut(DrawLerp);
			Level level = base.Scene as Level;
			AreaKey area = level.Session.Area;
			AreaModeStats saveStats = SaveData.Instance.Areas[area.ID].Modes[(int)area.Mode];
			if (!saveStats.Completed && !SaveData.Instance.CheatMode && !SaveData.Instance.DebugMode)
			{
				return;
			}
			ModeProperties mode = AreaData.Get(area).Mode[(int)area.Mode];
			int totalStrawberries = mode.TotalStrawberries;
			int spacing = 32;
			int strawbWidth = (totalStrawberries - 1) * spacing;
			int checkpointWidth = ((totalStrawberries > 0 && mode.Checkpoints != null) ? (mode.Checkpoints.Length * spacing) : 0);
			Vector2 pos = new Vector2((1920 - strawbWidth - checkpointWidth) / 2, 1016f + (1f - ease) * 80f);
			if (totalStrawberries <= 0)
			{
				return;
			}
			int checkpoints = ((mode.Checkpoints == null) ? 1 : (mode.Checkpoints.Length + 1));
			for (int c = 0; c < checkpoints; c++)
			{
				int checkpointTotal = ((c == 0) ? mode.StartStrawberries : mode.Checkpoints[c - 1].Strawberries);
				for (int i = 0; i < checkpointTotal; i++)
				{
					EntityData atCheckpoint = mode.StrawberriesByCheckpoint[c, i];
					if (atCheckpoint == null)
					{
						continue;
					}
					bool currentHas = false;
					foreach (EntityID strawb2 in level.Session.Strawberries)
					{
						if (atCheckpoint.ID == strawb2.ID && atCheckpoint.Level.Name == strawb2.Level)
						{
							currentHas = true;
						}
					}
					MTexture dot = GFX.Gui["dot"];
					if (currentHas)
					{
						if (area.Mode == AreaMode.CSide)
						{
							dot.DrawOutlineCentered(pos, Calc.HexToColor("f2ff30"), 1.5f);
						}
						else
						{
							dot.DrawOutlineCentered(pos, Calc.HexToColor("ff3040"), 1.5f);
						}
					}
					else
					{
						bool oldHas = false;
						foreach (EntityID strawb in saveStats.Strawberries)
						{
							if (atCheckpoint.ID == strawb.ID && atCheckpoint.Level.Name == strawb.Level)
							{
								oldHas = true;
							}
						}
						if (oldHas)
						{
							dot.DrawOutlineCentered(pos, Calc.HexToColor("4193ff"), 1f);
						}
						else
						{
							Draw.Rect(pos.X - (float)dot.ClipRect.Width * 0.5f, pos.Y - 4f, dot.ClipRect.Width, 8f, Color.DarkGray);
						}
					}
					pos.X += spacing;
				}
				if (mode.Checkpoints != null && c < mode.Checkpoints.Length)
				{
					Draw.Rect(pos.X - 3f, pos.Y - 16f, 6f, 32f, Color.DarkGray);
					pos.X += spacing;
				}
			}
		}
	}
}
