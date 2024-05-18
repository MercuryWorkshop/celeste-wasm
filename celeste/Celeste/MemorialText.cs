using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MemorialText : Entity
	{
		public bool Show;

		public bool Dreamy;

		public Memorial Memorial;

		private float index;

		private string message;

		private float alpha;

		private float timer;

		private float widestCharacter;

		private int firstLineLength;

		private SoundSource textSfx;

		private bool textSfxPlaying;

		public MemorialText(Memorial memorial, bool dreamy)
		{
			AddTag(Tags.HUD);
			AddTag(Tags.PauseUpdate);
			Add(textSfx = new SoundSource());
			Dreamy = dreamy;
			Memorial = memorial;
			message = Dialog.Clean("memorial");
			firstLineLength = CountToNewline(0);
			for (int i = 0; i < message.Length; i++)
			{
				float w = ActiveFont.Measure(message[i]).X;
				if (w > widestCharacter)
				{
					widestCharacter = w;
				}
			}
			widestCharacter *= 0.9f;
		}

		public override void Update()
		{
			base.Update();
			if ((base.Scene as Level).Paused)
			{
				textSfx.Pause();
				return;
			}
			timer += Engine.DeltaTime;
			if (!Show)
			{
				alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);
				if (alpha <= 0f)
				{
					index = firstLineLength;
				}
			}
			else
			{
				alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
				if (alpha >= 1f)
				{
					index = Calc.Approach(index, message.Length, 32f * Engine.DeltaTime);
				}
			}
			if (Show && alpha >= 1f && index < (float)message.Length)
			{
				if (!textSfxPlaying)
				{
					textSfxPlaying = true;
					textSfx.Play(Dreamy ? "event:/ui/game/memorial_dream_text_loop" : "event:/ui/game/memorial_text_loop");
					textSfx.Param("end", 0f);
				}
			}
			else if (textSfxPlaying)
			{
				textSfxPlaying = false;
				textSfx.Stop();
				textSfx.Param("end", 1f);
			}
			textSfx.Resume();
		}

		private int CountToNewline(int start)
		{
			int i;
			for (i = start; i < message.Length && message[i] != '\n'; i++)
			{
			}
			return i - start;
		}

		public override void Render()
		{
			if ((base.Scene as Level).FrozenOrPaused || (base.Scene as Level).Completed || !(index > 0f) || !(alpha > 0f))
			{
				return;
			}
			Camera camera = SceneAs<Level>().Camera;
			Vector2 position = new Vector2((Memorial.X - camera.X) * 6f, (Memorial.Y - camera.Y) * 6f - 350f - ActiveFont.LineHeight * 3.3f);
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				position.X = 1920f - position.X;
			}
			float ease = Ease.CubeInOut(alpha);
			int length = (int)Math.Min(message.Length, index);
			int step = 0;
			float y = 64f * (1f - ease);
			int chars = CountToNewline(0);
			for (int i = 0; i < length; i++)
			{
				char character = message[i];
				if (character == '\n')
				{
					step = 0;
					chars = CountToNewline(i + 1);
					y += ActiveFont.LineHeight * 1.1f;
					continue;
				}
				float scale = 1f;
				float x = (float)(-chars) * widestCharacter / 2f + ((float)step + 0.5f) * widestCharacter;
				float floaty = 0f;
				if (Dreamy && character != ' ' && character != '-' && character != '\n')
				{
					character = message[(i + (int)(Math.Sin(timer * 2f + (float)i / 8f) * 4.0) + message.Length) % message.Length];
					floaty = (float)Math.Sin(timer * 2f + (float)i / 8f) * 8f;
					scale = ((!(Math.Sin(timer * 4f + (float)i / 16f) < 0.0)) ? 1 : (-1));
				}
				ActiveFont.Draw(character, position + new Vector2(x, y + floaty), new Vector2(0.5f, 1f), new Vector2(scale, 1f), Color.White * ease);
				step++;
			}
		}
	}
}
