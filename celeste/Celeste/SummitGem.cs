using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SummitGem : Entity
	{
		private class BgFlash : Entity
		{
			private float alpha = 1f;

			public BgFlash()
			{
				base.Depth = 10100;
				base.Tag = Tags.Persistent;
			}

			public override void Update()
			{
				base.Update();
				alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 0.5f);
				if (alpha <= 0f)
				{
					RemoveSelf();
				}
			}

			public override void Render()
			{
				Vector2 cam = (base.Scene as Level).Camera.Position;
				Draw.Rect(cam.X - 10f, cam.Y - 10f, 340f, 200f, Color.Black * alpha);
			}
		}

		public static ParticleType P_Shatter;

		public static readonly Color[] GemColors = new Color[6]
		{
			Calc.HexToColor("9ee9ff"),
			Calc.HexToColor("54baff"),
			Calc.HexToColor("90ff2d"),
			Calc.HexToColor("ffd300"),
			Calc.HexToColor("ff609d"),
			Calc.HexToColor("c5e1ba")
		};

		public int GemID;

		public EntityID GID;

		private Sprite sprite;

		private Wiggler scaleWiggler;

		private Vector2 moveWiggleDir;

		private Wiggler moveWiggler;

		private float bounceSfxDelay;

		public SummitGem(EntityData data, Vector2 position, EntityID gid)
			: base(data.Position + position)
		{
			GID = gid;
			GemID = data.Int("gem");
			base.Collider = new Hitbox(12f, 12f, -6f, -6f);
			Add(sprite = new Sprite(GFX.Game, "collectables/summitgems/" + GemID + "/gem"));
			sprite.AddLoop("idle", "", 0.08f);
			sprite.Play("idle");
			sprite.CenterOrigin();
			if (SaveData.Instance.SummitGems != null && SaveData.Instance.SummitGems[GemID])
			{
				sprite.Color = Color.White * 0.5f;
			}
			Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.3f);
			}));
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
			Add(new PlayerCollider(OnPlayer));
		}

		private void OnPlayer(Player player)
		{
			Level level = base.Scene as Level;
			if (player.DashAttacking)
			{
				Add(new Coroutine(SmashRoutine(player, level)));
				return;
			}
			player.PointBounce(base.Center);
			moveWiggler.Start();
			scaleWiggler.Start();
			moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			if (bounceSfxDelay <= 0f)
			{
				Audio.Play("event:/game/general/crystalheart_bounce", Position);
				bounceSfxDelay = 0.1f;
			}
		}

		private IEnumerator SmashRoutine(Player player, Level level)
		{
			Visible = false;
			Collidable = false;
			player.Stamina = 110f;
			SoundEmitter.Play("event:/game/07_summit/gem_get", this);
			Session session = (base.Scene as Level).Session;
			session.DoNotLoad.Add(GID);
			session.SummitGems[GemID] = true;
			SaveData.Instance.RegisterSummitGem(GemID);
			level.Shake();
			Celeste.Freeze(0.1f);
			P_Shatter.Color = GemColors[GemID];
			float angle = player.Speed.Angle();
			level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, angle - (float)Math.PI / 2f);
			level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, angle + (float)Math.PI / 2f);
			SlashFx.Burst(Position, angle);
			for (int i = 0; i < 10; i++)
			{
				base.Scene.Add(new AbsorbOrb(Position, player));
			}
			level.Flash(Color.White, drawPlayerOver: true);
			base.Scene.Add(new BgFlash());
			Engine.TimeRate = 0.5f;
			while (Engine.TimeRate < 1f)
			{
				Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
				yield return null;
			}
			RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			bounceSfxDelay -= Engine.DeltaTime;
			sprite.Position = moveWiggleDir * moveWiggler.Value * -8f;
		}
	}
}
