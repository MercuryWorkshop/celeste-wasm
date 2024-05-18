using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS07_Ascend : CutsceneEntity
	{
		private int index;

		private string cutscene;

		private BadelineDummy badeline;

		private Player player;

		private Vector2 origin;

		private bool spinning;

		private bool dark;

		public CS07_Ascend(int index, string cutscene, bool dark)
		{
			this.index = index;
			this.cutscene = cutscene;
			this.dark = dark;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene()));
		}

		private IEnumerator Cutscene()
		{
			while ((player = base.Scene.Tracker.GetEntity<Player>()) == null)
			{
				yield return null;
			}
			origin = player.Position;
			Audio.Play("event:/char/badeline/maddy_split");
			player.CreateSplitParticles();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f);
			player.Dashes = 1;
			player.Facing = Facings.Right;
			base.Scene.Add(badeline = new BadelineDummy(player.Position));
			badeline.AutoAnimator.Enabled = false;
			spinning = true;
			Add(new Coroutine(SpinCharacters()));
			yield return Textbox.Say(cutscene);
			Audio.Play("event:/char/badeline/maddy_join");
			spinning = false;
			yield return 0.25f;
			badeline.RemoveSelf();
			player.Dashes = 2;
			player.CreateSplitParticles();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f);
			EndCutscene(Level);
		}

		private IEnumerator SpinCharacters()
		{
			float dist = 0f;
			Vector2 center = player.Position;
			float timer = (float)Math.PI / 2f;
			player.Sprite.Play("spin");
			badeline.Sprite.Play("spin");
			badeline.Sprite.Scale.X = 1f;
			while (spinning || dist > 0f)
			{
				dist = Calc.Approach(dist, spinning ? 1f : 0f, Engine.DeltaTime * 4f);
				int frame = (int)(timer / ((float)Math.PI * 2f) * 14f + 10f);
				float sin = (float)Math.Sin(timer);
				float cos = (float)Math.Cos(timer);
				float len = Ease.CubeOut(dist) * 32f;
				player.Sprite.SetAnimationFrame(frame);
				badeline.Sprite.SetAnimationFrame(frame + 7);
				player.Position = center - new Vector2(sin * len, cos * dist * 8f);
				badeline.Position = center + new Vector2(sin * len, cos * dist * 8f);
				timer -= Engine.DeltaTime * 2f;
				if (timer <= 0f)
				{
					timer += (float)Math.PI * 2f;
				}
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			if (badeline != null)
			{
				badeline.RemoveSelf();
			}
			if (player != null)
			{
				player.Dashes = 2;
				player.Position = origin;
			}
			if (!dark)
			{
				level.Add(new HeightDisplay(index));
			}
		}
	}
}
