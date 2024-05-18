using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class HeightDisplay : Entity
	{
		private int index;

		private string text = "";

		private string leftText = "";

		private string rightText = "";

		private float leftSize;

		private float rightSize;

		private float numberSize;

		private Vector2 size;

		private int height;

		private float approach;

		private float ease;

		private float pulse;

		private string spawnedLevel;

		private bool setAudioProgression;

		private bool easingCamera = true;

		private bool drawText
		{
			get
			{
				if (index >= 0 && ease > 0f)
				{
					return !string.IsNullOrEmpty(text);
				}
				return false;
			}
		}

		public HeightDisplay(int index)
		{
			base.Tag = (int)Tags.HUD | (int)Tags.Persistent;
			this.index = index;
			string txt = "CH7_HEIGHT_" + ((index < 0) ? "START" : index.ToString());
			if (index >= 0 && Dialog.Has(txt))
			{
				text = Dialog.Get(txt);
				text = text.ToUpper();
				height = (index + 1) * 500;
				approach = index * 500;
				int numberPos = text.IndexOf("{X}");
				leftText = text.Substring(0, numberPos);
				leftSize = ActiveFont.Measure(leftText).X;
				rightText = text.Substring(numberPos + 3);
				numberSize = ActiveFont.Measure(height.ToString()).X;
				rightSize = ActiveFont.Measure(rightText).X;
				size = ActiveFont.Measure(leftText + height + rightText);
			}
			Add(new Coroutine(Routine()));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			spawnedLevel = (scene as Level).Session.Level;
		}

		private IEnumerator Routine()
		{
			Player player;
			while (true)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && (base.Scene as Level).Session.Level != spawnedLevel)
				{
					break;
				}
				yield return null;
			}
			StepAudioProgression();
			easingCamera = false;
			yield return 0.1f;
			Add(new Coroutine(CameraUp()));
			if (!string.IsNullOrEmpty(text) && index >= 0)
			{
				Audio.Play("event:/game/07_summit/altitude_count");
			}
			while ((ease += Engine.DeltaTime / 0.15f) < 1f)
			{
				yield return null;
			}
			while (approach < (float)height && !player.OnGround())
			{
				yield return null;
			}
			approach = height;
			pulse = 1f;
			while ((pulse -= Engine.DeltaTime * 4f) > 0f)
			{
				yield return null;
			}
			pulse = 0f;
			yield return 1f;
			while ((ease -= Engine.DeltaTime / 0.15f) > 0f)
			{
				yield return null;
			}
			RemoveSelf();
		}

		private IEnumerator CameraUp()
		{
			easingCamera = true;
			Level level = base.Scene as Level;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)
			{
				level.Camera.Y = (float)(level.Bounds.Bottom - 180) + 64f * (1f - Ease.CubeOut(p));
				yield return null;
			}
		}

		private void StepAudioProgression()
		{
			Session session = (base.Scene as Level).Session;
			if (!setAudioProgression && index >= 0 && session.Area.Mode == AreaMode.Normal)
			{
				setAudioProgression = true;
				int progress = index + 1;
				if (progress <= 5)
				{
					session.Audio.Music.Progress = progress;
				}
				else
				{
					session.Audio.Music.Event = "event:/music/lvl7/final_ascent";
				}
				session.Audio.Apply();
			}
		}

		public override void Update()
		{
			if (index >= 0 && ease > 0f)
			{
				if ((float)height - approach > 100f)
				{
					approach += 1000f * Engine.DeltaTime;
				}
				else if ((float)height - approach > 25f)
				{
					approach += 200f * Engine.DeltaTime;
				}
				else if ((float)height - approach > 5f)
				{
					approach += 50f * Engine.DeltaTime;
				}
				else if ((float)height - approach > 0f)
				{
					approach += 10f * Engine.DeltaTime;
				}
				else
				{
					approach = height;
				}
			}
			Level level = base.Scene as Level;
			if (!easingCamera)
			{
				level.Camera.Y = level.Bounds.Bottom - 180 + 64;
			}
			base.Update();
		}

		public override void Render()
		{
			if (!base.Scene.Paused && drawText)
			{
				Vector2 center = new Vector2(1920f, 1080f) / 2f;
				float fontScale = 1.2f + pulse * 0.2f;
				Vector2 s = size * fontScale;
				float e = Ease.SineInOut(ease);
				Vector2 scale = new Vector2(1f, e);
				Draw.Rect(center.X - (s.X + 64f) * 0.5f * scale.X, center.Y - (s.Y + 32f) * 0.5f * scale.Y, (s.X + 64f) * scale.X, (s.Y + 32f) * scale.Y, Color.Black);
				Vector2 left = center + new Vector2((0f - s.X) * 0.5f, 0f);
				Vector2 textScale = scale * fontScale;
				Color textColor = Color.White * e;
				ActiveFont.Draw(leftText, left, new Vector2(0f, 0.5f), textScale, textColor);
				ActiveFont.Draw(rightText, left + Vector2.UnitX * (leftSize + numberSize) * fontScale, new Vector2(0f, 0.5f), textScale, textColor);
				ActiveFont.Draw(((int)approach).ToString(), left + Vector2.UnitX * (leftSize + numberSize * 0.5f) * fontScale, new Vector2(0.5f, 0.5f), textScale, textColor);
			}
		}

		public override void Removed(Scene scene)
		{
			StepAudioProgression();
			base.Removed(scene);
		}
	}
}
