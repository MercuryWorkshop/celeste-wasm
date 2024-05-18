using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Pooled]
	[Tracked(false)]
	public class FinalBossBeam : Entity
	{
		public static ParticleType P_Dissipate;

		public const float ChargeTime = 1.4f;

		public const float FollowTime = 0.9f;

		public const float ActiveTime = 0.12f;

		private const float AngleStartOffset = 100f;

		private const float RotationSpeed = 200f;

		private const float CollideCheckSep = 2f;

		private const float BeamLength = 2000f;

		private const float BeamStartDist = 12f;

		private const int BeamsDrawn = 15;

		private const float SideDarknessAlpha = 0.35f;

		private FinalBoss boss;

		private Player player;

		private Sprite beamSprite;

		private Sprite beamStartSprite;

		private float chargeTimer;

		private float followTimer;

		private float activeTimer;

		private float angle;

		private float beamAlpha;

		private float sideFadeAlpha;

		private VertexPositionColor[] fade = new VertexPositionColor[24];

		public FinalBossBeam()
		{
			Add(beamSprite = GFX.SpriteBank.Create("badeline_beam"));
			beamSprite.OnLastFrame = delegate(string anim)
			{
				if (anim == "shoot")
				{
					Destroy();
				}
			};
			Add(beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
			beamSprite.Visible = false;
			base.Depth = -1000000;
		}

		public FinalBossBeam Init(FinalBoss boss, Player target)
		{
			this.boss = boss;
			chargeTimer = 1.4f;
			followTimer = 0.9f;
			activeTimer = 0.12f;
			beamSprite.Play("charge");
			sideFadeAlpha = 0f;
			beamAlpha = 0f;
			int sign = ((target.Y <= boss.Y + 16f) ? 1 : (-1));
			if (target.X >= boss.X)
			{
				sign *= -1;
			}
			angle = Calc.Angle(boss.BeamOrigin, target.Center);
			Vector2 at = Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f), target.Center);
			at += (target.Center - boss.BeamOrigin).Perpendicular().SafeNormalize(100f) * sign;
			angle = Calc.Angle(boss.BeamOrigin, at);
			return this;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (boss.Moving)
			{
				RemoveSelf();
			}
		}

		public override void Update()
		{
			base.Update();
			player = base.Scene.Tracker.GetEntity<Player>();
			beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
			if (chargeTimer > 0f)
			{
				sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
				if (player != null && !player.Dead)
				{
					followTimer -= Engine.DeltaTime;
					chargeTimer -= Engine.DeltaTime;
					if (followTimer > 0f && player.Center != boss.BeamOrigin)
					{
						Vector2 at = Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f), player.Center);
						Vector2 target = player.Center;
						at = Calc.Approach(at, target, 200f * Engine.DeltaTime);
						angle = Calc.Angle(boss.BeamOrigin, at);
					}
					else if (beamSprite.CurrentAnimationID == "charge")
					{
						beamSprite.Play("lock");
					}
					if (chargeTimer <= 0f)
					{
						SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
						Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						DissipateParticles();
					}
				}
			}
			else if (activeTimer > 0f)
			{
				sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
				if (beamSprite.CurrentAnimationID != "shoot")
				{
					beamSprite.Play("shoot");
					beamStartSprite.Play("shoot", restart: true);
				}
				activeTimer -= Engine.DeltaTime;
				if (activeTimer > 0f)
				{
					PlayerCollideCheck();
				}
			}
		}

		private void DissipateParticles()
		{
			Level level = SceneAs<Level>();
			Vector2 center = level.Camera.Position + new Vector2(160f, 90f);
			Vector2 from = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
			Vector2 to = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
			Vector2 perp = (to - from).Perpendicular().SafeNormalize();
			Vector2 normal = (to - from).SafeNormalize();
			Vector2 rangeA = -perp * 1f;
			Vector2 rangeB = perp * 1f;
			float dirA = perp.Angle();
			float dirB = (-perp).Angle();
			float distance = Vector2.Distance(center, from) - 12f;
			center = Calc.ClosestPointOnLine(from, to, center);
			for (int i = 0; i < 200; i += 12)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					level.ParticlesFG.Emit(P_Dissipate, center + normal * i + perp * 2f * j + Calc.Random.Range(rangeA, rangeB), dirA);
					level.ParticlesFG.Emit(P_Dissipate, center + normal * i - perp * 2f * j + Calc.Random.Range(rangeA, rangeB), dirB);
					if (i != 0 && (float)i < distance)
					{
						level.ParticlesFG.Emit(P_Dissipate, center - normal * i + perp * 2f * j + Calc.Random.Range(rangeA, rangeB), dirA);
						level.ParticlesFG.Emit(P_Dissipate, center - normal * i - perp * 2f * j + Calc.Random.Range(rangeA, rangeB), dirB);
					}
				}
			}
		}

		private void PlayerCollideCheck()
		{
			Vector2 from = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
			Vector2 to = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
			Vector2 perp = (to - from).Perpendicular().SafeNormalize(2f);
			Player hit = base.Scene.CollideFirst<Player>(from + perp, to + perp);
			if (hit == null)
			{
				hit = base.Scene.CollideFirst<Player>(from - perp, to - perp);
			}
			if (hit == null)
			{
				hit = base.Scene.CollideFirst<Player>(from, to);
			}
			hit?.Die((hit.Center - boss.BeamOrigin).SafeNormalize());
		}

		public override void Render()
		{
			Vector2 pos = boss.BeamOrigin;
			Vector2 dir = Calc.AngleToVector(angle, beamSprite.Width);
			beamSprite.Rotation = angle;
			beamSprite.Color = Color.White * beamAlpha;
			beamStartSprite.Rotation = angle;
			beamStartSprite.Color = Color.White * beamAlpha;
			if (beamSprite.CurrentAnimationID == "shoot")
			{
				pos += Calc.AngleToVector(angle, 8f);
			}
			for (int i = 0; i < 15; i++)
			{
				beamSprite.RenderPosition = pos;
				beamSprite.Render();
				pos += dir;
			}
			if (beamSprite.CurrentAnimationID == "shoot")
			{
				beamStartSprite.RenderPosition = boss.BeamOrigin;
				beamStartSprite.Render();
			}
			GameplayRenderer.End();
			Vector2 norm = dir.SafeNormalize();
			Vector2 perp = norm.Perpendicular();
			Color color = Color.Black * sideFadeAlpha * 0.35f;
			Color empty = Color.Transparent;
			norm *= 4000f;
			perp *= 120f;
			int v = 0;
			Quad(ref v, pos, -norm + perp * 2f, norm + perp * 2f, norm + perp, -norm + perp, color, color);
			Quad(ref v, pos, -norm + perp, norm + perp, norm, -norm, color, empty);
			Quad(ref v, pos, -norm, norm, norm - perp, -norm - perp, empty, color);
			Quad(ref v, pos, -norm - perp, norm - perp, norm - perp * 2f, -norm - perp * 2f, color, color);
			GFX.DrawVertices((base.Scene as Level).Camera.Matrix, fade, fade.Length);
			GameplayRenderer.Begin();
		}

		private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd)
		{
			fade[v].Position.X = offset.X + a.X;
			fade[v].Position.Y = offset.Y + a.Y;
			fade[v++].Color = ab;
			fade[v].Position.X = offset.X + b.X;
			fade[v].Position.Y = offset.Y + b.Y;
			fade[v++].Color = ab;
			fade[v].Position.X = offset.X + c.X;
			fade[v].Position.Y = offset.Y + c.Y;
			fade[v++].Color = cd;
			fade[v].Position.X = offset.X + a.X;
			fade[v].Position.Y = offset.Y + a.Y;
			fade[v++].Color = ab;
			fade[v].Position.X = offset.X + c.X;
			fade[v].Position.Y = offset.Y + c.Y;
			fade[v++].Color = cd;
			fade[v].Position.X = offset.X + d.X;
			fade[v].Position.Y = offset.Y + d.Y;
			fade[v++].Color = cd;
		}

		public void Destroy()
		{
			RemoveSelf();
		}
	}
}
