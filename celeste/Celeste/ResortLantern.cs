using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ResortLantern : Entity
	{
		private Image holder;

		private Sprite lantern;

		private float collideTimer;

		private int mult;

		private Wiggler wiggler;

		private VertexLight light;

		private BloomPoint bloom;

		private float alphaTimer;

		private SoundSource sfx;

		public ResortLantern(Vector2 position)
			: base(position)
		{
			base.Collider = new Hitbox(8f, 8f, -4f, -4f);
			base.Depth = 2000;
			Add(new PlayerCollider(OnPlayer));
			holder = new Image(GFX.Game["objects/resortLantern/holder"]);
			holder.CenterOrigin();
			Add(holder);
			lantern = new Sprite(GFX.Game, "objects/resortLantern/");
			lantern.AddLoop("light", "lantern", 0.3f, 0, 0, 1, 2, 1);
			lantern.Play("light");
			lantern.Origin = new Vector2(7f, 7f);
			lantern.Position = new Vector2(-1f, -5f);
			Add(lantern);
			wiggler = Wiggler.Create(2.5f, 1.2f, delegate(float v)
			{
				lantern.Rotation = v * (float)mult * ((float)Math.PI / 180f) * 30f;
			});
			wiggler.StartZero = true;
			Add(wiggler);
			Add(light = new VertexLight(Color.White, 0.95f, 32, 64));
			Add(bloom = new BloomPoint(0.8f, 8f));
			Add(sfx = new SoundSource());
		}

		public ResortLantern(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (CollideCheck<Solid>(Position + Vector2.UnitX * 8f))
			{
				holder.Scale.X = -1f;
				lantern.Scale.X = -1f;
				lantern.X += 2f;
			}
		}

		public override void Update()
		{
			base.Update();
			if (collideTimer > 0f)
			{
				collideTimer -= Engine.DeltaTime;
			}
			alphaTimer += Engine.DeltaTime;
			bloom.Alpha = (light.Alpha = 0.95f + (float)Math.Sin(alphaTimer * 1f) * 0.05f);
		}

		private void OnPlayer(Player player)
		{
			if (collideTimer <= 0f)
			{
				if (player.Speed != Vector2.Zero)
				{
					sfx.Play("event:/game/03_resort/lantern_bump");
					collideTimer = 0.5f;
					mult = Calc.Random.Choose(1, -1);
					wiggler.Start();
				}
			}
			else
			{
				collideTimer = 0.5f;
			}
		}
	}
}
