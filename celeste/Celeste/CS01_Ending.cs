using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS01_Ending : CutsceneEntity
	{
		private Player player;

		private Bonfire bonfire;

		public CS01_Ending(Player player)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			bonfire = base.Scene.Tracker.GetEntity<Bonfire>();
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.Dashes = 1;
			level.Session.Audio.Music.Layer(3, value: false);
			level.Session.Audio.Apply();
			yield return 0.5f;
			yield return player.DummyWalkTo(bonfire.X + 40f);
			yield return 1.5f;
			player.Facing = Facings.Left;
			yield return 0.5f;
			yield return Textbox.Say("CH1_END", EndCityTrigger);
			yield return 0.3f;
			EndCutscene(level);
		}

		private IEnumerator EndCityTrigger()
		{
			yield return 0.2f;
			yield return player.DummyWalkTo(bonfire.X - 12f);
			yield return 0.2f;
			player.Facing = Facings.Right;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("duck");
			yield return 0.5f;
			if (bonfire != null)
			{
				bonfire.SetMode(Bonfire.Mode.Lit);
			}
			yield return 1f;
			player.Sprite.Play("idle");
			yield return 0.4f;
			player.DummyAutoAnimate = true;
			yield return player.DummyWalkTo(bonfire.X - 24f);
			yield return 0.4f;
			player.DummyAutoAnimate = false;
			player.Facing = Facings.Right;
			player.Sprite.Play("sleep");
			Audio.Play("event:/char/madeline/campfire_sit", player.Position);
			yield return 4f;
			BirdNPC bird = new BirdNPC(player.Position + new Vector2(88f, -200f), BirdNPC.Modes.None);
			base.Scene.Add(bird);
			EventInstance? instance = Audio.Play("event:/game/general/bird_in", bird.Position);
			bird.Facing = Facings.Left;
			bird.Sprite.Play("fall");
			Vector2 from = bird.Position;
			Vector2 to = player.Position + new Vector2(1f, -12f);
			float percent = 0f;
			while (percent < 1f)
			{
				bird.Position = from + (to - from) * Ease.QuadOut(percent);
				Audio.Position(instance, bird.Position);
				if (percent > 0.5f)
				{
					bird.Sprite.Play("fly");
				}
				percent += Engine.DeltaTime * 0.5f;
				yield return null;
			}
			bird.Position = to;
			bird.Sprite.Play("idle");
			yield return 0.5f;
			bird.Sprite.Play("croak");
			yield return 0.6f;
			Audio.Play("event:/game/general/bird_squawk", bird.Position);
			yield return 0.9f;
			bird.Sprite.Play("sleep");
			yield return null;
			yield return 2f;
		}

		public override void OnEnd(Level level)
		{
			level.CompleteArea();
		}
	}
}
