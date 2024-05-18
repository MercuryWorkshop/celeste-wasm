using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ReturnMapHint : Entity
	{
		private MTexture checkpoint;

		public ReturnMapHint()
		{
			base.Tag = Tags.HUD;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session session = (base.Scene as Level).Session;
			AreaKey area = session.Area;
			HashSet<string> saveDataCheckpoints = SaveData.Instance.GetCheckpoints(area);
			CheckpointData furthest = null;
			ModeProperties a = AreaData.Areas[area.ID].Mode[(int)area.Mode];
			if (a.Checkpoints != null)
			{
				CheckpointData[] checkpoints = a.Checkpoints;
				foreach (CheckpointData cp in checkpoints)
				{
					if (session.LevelFlags.Contains(cp.Level) && saveDataCheckpoints.Contains(cp.Level))
					{
						furthest = cp;
					}
				}
			}
			string lookup = area.ToString();
			if (furthest != null)
			{
				lookup = lookup + "_" + furthest.Level;
			}
			if (MTN.Checkpoints.Has(lookup))
			{
				checkpoint = MTN.Checkpoints[lookup];
			}
		}

		public static string GetCheckpointPreviewName(AreaKey area, string level)
		{
			if (level == null)
			{
				return area.ToString();
			}
			return area.ToString() + "_" + level;
		}

		private MTexture GetCheckpointPreview(AreaKey area, string level)
		{
			string name = GetCheckpointPreviewName(area, level);
			if (MTN.Checkpoints.Has(name))
			{
				return MTN.Checkpoints[name];
			}
			return null;
		}

		public override void Render()
		{
			MTexture icon = GFX.Gui["checkpoint"];
			string text = Dialog.Clean("MENU_RETURN_INFO");
			MTexture polaroid = MTN.Checkpoints["polaroid"];
			float textWidth = ActiveFont.Measure(text).X * 0.75f;
			if (checkpoint != null)
			{
				float polaroidWidth = (float)polaroid.Width * 0.25f;
				Vector2 pos2 = new Vector2((1920f - textWidth - polaroidWidth - 64f) / 2f, 730f);
				float checkpointScale = 720f / (float)checkpoint.ClipRect.Width;
				ActiveFont.DrawOutline(text, pos2 + new Vector2(textWidth / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
				pos2.X += textWidth + 64f;
				polaroid.DrawCentered(pos2 + new Vector2(polaroidWidth / 2f, 0f), Color.White, 0.25f, 0.1f);
				checkpoint.DrawCentered(pos2 + new Vector2(polaroidWidth / 2f, 0f), Color.White, 0.25f * checkpointScale, 0.1f);
				icon.DrawCentered(pos2 + new Vector2(polaroidWidth * 0.8f, (float)polaroid.Height * 0.25f * 0.5f * 0.8f), Color.White, 0.75f);
			}
			else
			{
				float iconWidth = (float)icon.Width * 0.75f;
				Vector2 pos = new Vector2((1920f - textWidth - iconWidth - 64f) / 2f, 730f);
				ActiveFont.DrawOutline(text, pos + new Vector2(textWidth / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
				pos.X += textWidth + 64f;
				icon.DrawCentered(pos + new Vector2(iconWidth * 0.5f, 0f), Color.White, 0.75f);
			}
		}
	}
}
