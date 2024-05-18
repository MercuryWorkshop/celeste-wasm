using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS07_Ending : CutsceneEntity
	{
		private Player player;

		private BadelineDummy badeline;

		private Vector2 target;

		public CS07_Ending(Player player, Vector2 target)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.player = player;
			this.target = target;
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			Audio.SetMusic(null);
			player.StateMachine.State = 11;
			yield return player.DummyWalkTo(target.X);
			yield return 0.25f;
			Add(new Coroutine(CutsceneEntity.CameraTo(target + new Vector2(-160f, -130f), 3f, Ease.CubeInOut)));
			player.Facing = Facings.Right;
			yield return 1f;
			player.Sprite.Play("idle");
			player.DummyAutoAnimate = false;
			player.Dashes = 1;
			level.Session.Inventory.Dashes = 1;
			level.Add(badeline = new BadelineDummy(player.Center));
			player.CreateSplitParticles();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			Level.Displacement.AddBurst(player.Center, 0.4f, 8f, 32f, 0.5f);
			badeline.Sprite.Scale.X = 1f;
			Audio.Play("event:/char/badeline/maddy_split", player.Position);
			yield return badeline.FloatTo(target + new Vector2(-10f, -30f), 1, faceDirection: false);
			yield return 0.5f;
			yield return Textbox.Say("CH7_ENDING", WaitABit, SitDown, BadelineApproaches);
			yield return 1f;
			EndCutscene(level);
		}

		private IEnumerator WaitABit()
		{
			yield return 3f;
		}

		private IEnumerator SitDown()
		{
			yield return 0.5f;
			player.DummyAutoAnimate = true;
			yield return player.DummyWalkTo(player.X + 16f, walkBackwards: false, 0.25f);
			yield return 0.1f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("sitDown");
			yield return 1f;
		}

		private IEnumerator BadelineApproaches()
		{
			yield return 0.5f;
			badeline.Sprite.Scale.X = -1f;
			yield return 1f;
			badeline.Sprite.Scale.X = 1f;
			yield return 1f;
			Add(new Coroutine(CutsceneEntity.CameraTo(Level.Camera.Position + new Vector2(88f, 0f), 6f, Ease.CubeInOut)));
			badeline.FloatSpeed = 40f;
			yield return badeline.FloatTo(new Vector2(player.X - 10f, player.Y - 4f));
			yield return 0.5f;
		}

		public override void OnEnd(Level level)
		{
			Audio.SetMusic(null);
			ScreenWipe wipe = level.CompleteArea(spotlightWipe: false);
			if (wipe != null)
			{
				wipe.Duration = 2f;
				wipe.EndTimer = 1f;
			}
		}
	}
}
