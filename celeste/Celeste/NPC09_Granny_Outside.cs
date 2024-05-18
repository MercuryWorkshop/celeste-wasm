using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC09_Granny_Outside : NPC
	{
		public const string Flag = "granny_outside";

		public Hahaha Hahaha;

		public GrannyLaughSfx LaughSfx;

		private bool talking;

		private Player player;

		private bool leaving;

		public NPC09_Granny_Outside(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Play("idle");
			Add(LaughSfx = new GrannyLaughSfx(Sprite));
			MoveAnim = "walk";
			IdleAnim = "idle";
			Maxspeed = 40f;
			SetupGrannySpriteSounds();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if ((scene as Level).Session.GetFlag("granny_outside"))
			{
				RemoveSelf();
			}
			scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
			Hahaha.Enabled = false;
		}

		public override void Update()
		{
			if (!talking)
			{
				player = Level.Tracker.GetEntity<Player>();
				if (player != null && player.X > base.X - 48f)
				{
					(base.Scene as Level).StartCutscene(EndTalking);
					Add(new Coroutine(TalkRoutine(player)));
					talking = true;
				}
			}
			Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
			base.Update();
		}

		private IEnumerator TalkRoutine(Player player)
		{
			player.StateMachine.State = 11;
			while (!player.OnGround())
			{
				yield return null;
			}
			Sprite.Scale.X = -1f;
			yield return player.DummyWalkToExact((int)base.X - 16);
			yield return 0.5f;
			yield return Level.ZoomTo(new Vector2(200f, 110f), 2f, 0.5f);
			yield return Textbox.Say("APP_OLDLADY_A", MoveRight, ExitRight);
			yield return Level.ZoomBack(0.5f);
			Sprite.Scale.X = 1f;
			if (!leaving)
			{
				yield return ExitRight();
			}
			while (base.X < (float)(Level.Bounds.Right + 8))
			{
				yield return null;
			}
			Level.EndCutscene();
			EndTalking(Level);
		}

		private IEnumerator MoveRight()
		{
			yield return MoveTo(new Vector2(base.X + 8f, base.Y));
		}

		private IEnumerator ExitRight()
		{
			leaving = true;
			Add(new Coroutine(MoveTo(new Vector2(Level.Bounds.Right + 16, base.Y))));
			yield return null;
		}

		private void EndTalking(Level level)
		{
			if (player != null)
			{
				player.StateMachine.State = 0;
			}
			Level.Session.SetFlag("granny_outside");
			RemoveSelf();
		}
	}
}
