using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Theo_Vents : NPC
	{
		private class Grate : Entity
		{
			private Image sprite;

			private float shake;

			private Vector2 speed;

			private bool falling;

			private float alpha = 1f;

			public Grate(Vector2 position)
				: base(position)
			{
				Add(sprite = new Image(GFX.Game["scenery/grate"]));
				sprite.JustifyOrigin(0.5f, 0f);
			}

			public void Shake()
			{
				if (!falling)
				{
					Audio.Play("event:/char/theo/resort_ceilingvent_shake", Position);
					shake = 0.5f;
				}
			}

			public void Fall()
			{
				Audio.Play("event:/char/theo/resort_ceilingvent_popoff", Position);
				falling = true;
				speed = new Vector2(40f, 200f);
				base.Collider = new Hitbox(2f, 2f, -1f);
			}

			public override void Update()
			{
				if (shake > 0f)
				{
					shake -= Engine.DeltaTime;
					if (base.Scene.OnInterval(0.05f))
					{
						sprite.X = 1f - sprite.X;
					}
				}
				if (falling)
				{
					speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 80f);
					speed.Y += 200f * Engine.DeltaTime;
					Position += speed * Engine.DeltaTime;
					if (CollideCheck<Solid>(Position + new Vector2(0f, 2f)) && speed.Y > 0f)
					{
						speed.Y = (0f - speed.Y) * 0.25f;
					}
					alpha -= Engine.DeltaTime;
					sprite.Rotation += Engine.DeltaTime;
					sprite.Color = Color.White * alpha;
					if (alpha <= 0f)
					{
						RemoveSelf();
					}
				}
				base.Update();
			}
		}

		private const string AppeardFlag = "theoVentsAppeared";

		private const string TalkedFlag = "theoVentsTalked";

		private const int SpriteAppearY = -8;

		private float particleDelay;

		private bool appeared;

		private Grate grate;

		public NPC03_Theo_Vents(Vector2 position)
			: base(position)
		{
			base.Tag = Tags.TransitionUpdate;
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			Sprite.Scale.Y = -1f;
			Sprite.Scale.X = -1f;
			Visible = false;
			Maxspeed = 48f;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("theoVentsTalked"))
			{
				RemoveSelf();
			}
			else
			{
				Add(new Coroutine(Appear()));
			}
		}

		public override void Update()
		{
			base.Update();
			if (appeared)
			{
				return;
			}
			particleDelay -= Engine.DeltaTime;
			if (particleDelay <= 0f)
			{
				Level.ParticlesFG.Emit(ParticleTypes.VentDust, 8, Position, new Vector2(6f, 0f));
				if (grate != null)
				{
					grate.Shake();
				}
				particleDelay = Calc.Random.Choose(1f, 2f, 3f);
			}
		}

		private IEnumerator Appear()
		{
			if (!base.Session.GetFlag("theoVentsAppeared"))
			{
				grate = new Grate(Position);
				base.Scene.Add(grate);
				Player player;
				do
				{
					yield return null;
					player = base.Scene.Tracker.GetEntity<Player>();
				}
				while (player == null || !(player.X > base.X - 32f));
				Audio.Play("event:/char/theo/resort_ceilingvent_hey", Position);
				Level.ParticlesFG.Emit(ParticleTypes.VentDust, 24, Position, new Vector2(6f, 0f));
				grate.Fall();
				int from = -24;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
				{
					yield return null;
					Visible = true;
					Sprite.Y = (float)from + (float)(-8 - from) * Ease.CubeOut(p);
				}
				base.Session.SetFlag("theoVentsAppeared");
			}
			appeared = true;
			Sprite.Y = -8f;
			Visible = true;
			Add(Talker = new TalkComponent(new Rectangle(-16, 0, 32, 100), new Vector2(0f, -8f), OnTalk));
		}

		private void OnTalk(Player player)
		{
			Level.StartCutscene(OnTalkEnd);
			Add(new Coroutine(Talk(player)));
		}

		private IEnumerator Talk(Player player)
		{
			yield return PlayerApproach(player, turnToFace: true, 10f, -1);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			yield return CutsceneEntity.CameraTo(new Vector2(Level.Bounds.Right - 320, Level.Bounds.Top), 0.5f);
			yield return Level.ZoomTo(new Vector2(240f, 70f), 2f, 0.5f);
			yield return Textbox.Say("CH3_THEO_VENTS");
			yield return Disappear();
			yield return 0.25f;
			yield return Level.ZoomBack(0.5f);
			Level.EndCutscene();
			OnTalkEnd(Level);
		}

		private void OnTalkEnd(Level level)
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.DummyAutoAnimate = true;
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
			base.Session.SetFlag("theoVentsTalked");
			RemoveSelf();
		}

		private IEnumerator Disappear()
		{
			Audio.Play("event:/char/theo/resort_ceilingvent_seeya", Position);
			int to = -24;
			float from = Sprite.Y;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
			{
				yield return null;
				Level.ParticlesFG.Emit(ParticleTypes.VentDust, 1, Position, new Vector2(6f, 0f));
				Sprite.Y = from + ((float)to - from) * Ease.BackIn(p);
			}
		}
	}
}
