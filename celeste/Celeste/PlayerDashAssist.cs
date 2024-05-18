using System;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PlayerDashAssist : Entity
	{
		public float Direction;

		public float Scale;

		public Vector2 Offset;

		private List<MTexture> images;

		private EventInstance snapshot;

		private float timer;

		private bool paused;

		private int lastIndex;

		public PlayerDashAssist()
		{
			base.Tag = Tags.Global;
			base.Depth = -1000000;
			Visible = false;
			images = GFX.Game.GetAtlasSubtextures("util/dasharrow/dasharrow");
		}

		public override void Update()
		{
			if (!Engine.DashAssistFreeze)
			{
				if (paused)
				{
					if (!base.Scene.Paused)
					{
						Audio.PauseGameplaySfx = false;
					}
					DisableSnapshot();
					timer = 0f;
					paused = false;
				}
				return;
			}
			paused = true;
			Audio.PauseGameplaySfx = true;
			timer += Engine.RawDeltaTime;
			if (timer > 0.2f && snapshot == null)
			{
				EnableSnapshot();
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				float dirAngle = Input.GetAimVector(player.Facing).Angle();
				if (Calc.AbsAngleDiff(dirAngle, Direction) >= 1.5807964f)
				{
					Direction = dirAngle;
					Scale = 0f;
				}
				else
				{
					Direction = Calc.AngleApproach(Direction, dirAngle, (float)Math.PI * 6f * Engine.RawDeltaTime);
				}
				Scale = Calc.Approach(Scale, 1f, Engine.DeltaTime * 4f);
				int index = 1 + (8 + (int)Math.Round(dirAngle / ((float)Math.PI / 4f))) % 8;
				if (lastIndex != 0 && lastIndex != index)
				{
					Audio.Play("event:/game/general/assist_dash_aim", player.Center, "dash_direction", index);
				}
				lastIndex = index;
			}
		}

		public override void Render()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null || !Engine.DashAssistFreeze)
			{
				return;
			}
			MTexture tex = null;
			float diff = float.MaxValue;
			for (int i = 0; i < 8; i++)
			{
				float angleDiff = Calc.AngleDiff((float)Math.PI * 2f * ((float)i / 8f), Direction);
				if (Math.Abs(angleDiff) < Math.Abs(diff))
				{
					diff = angleDiff;
					tex = images[i];
				}
			}
			if (tex != null)
			{
				if (Math.Abs(diff) < 0.05f)
				{
					diff = 0f;
				}
				tex.DrawOutlineCentered((player.Center + Offset + Calc.AngleToVector(Direction, 20f)).Round(), Color.White, Ease.BounceOut(Scale), diff);
			}
		}

		private void EnableSnapshot()
		{
		}

		private void DisableSnapshot()
		{
			if (snapshot != null)
			{
				Audio.ReleaseSnapshot(snapshot);
				snapshot = null;
			}
		}

		public override void Removed(Scene scene)
		{
			DisableSnapshot();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			DisableSnapshot();
			base.SceneEnd(scene);
		}
	}
}
