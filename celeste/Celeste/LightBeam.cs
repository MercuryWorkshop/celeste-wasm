using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LightBeam : Entity
	{
		public static ParticleType P_Glow;

		private MTexture texture = GFX.Game["util/lightbeam"];

		private Color color = new Color(0.8f, 1f, 1f);

		private float alpha;

		public int LightWidth;

		public int LightLength;

		public float Rotation;

		public string Flag;

		private float timer = Calc.Random.NextFloat(1000f);

		public LightBeam(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Depth = -9998;
			LightWidth = data.Width;
			LightLength = data.Height;
			Flag = data.Attr("flag");
			Rotation = data.Float("rotation") * ((float)Math.PI / 180f);
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			Level level = base.Scene as Level;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag)))
			{
				Vector2 direction = Calc.AngleToVector(Rotation + (float)Math.PI / 2f, 1f);
				Vector2 closest = Calc.ClosestPointOnLine(Position, Position + direction * 10000f, player.Center);
				float target = Math.Min(1f, Math.Max(0f, (closest - Position).Length() - 8f) / (float)LightLength);
				if ((closest - player.Center).Length() > (float)LightWidth / 2f)
				{
					target = 1f;
				}
				if (level.Transitioning)
				{
					target = 0f;
				}
				alpha = Calc.Approach(alpha, target, Engine.DeltaTime * 4f);
			}
			if (alpha >= 0.5f && level.OnInterval(0.8f))
			{
				Vector2 add = Calc.AngleToVector(Rotation + (float)Math.PI / 2f, 1f);
				Vector2 from = Position - add * 4f;
				float at = Calc.Random.Next(LightWidth - 4) + 2 - LightWidth / 2;
				from += at * add.Perpendicular();
				level.Particles.Emit(P_Glow, from, Rotation + (float)Math.PI / 2f);
			}
			base.Update();
		}

		public override void Render()
		{
			if (alpha > 0f)
			{
				DrawTexture(0f, LightWidth, (float)(LightLength - 4) + (float)Math.Sin(timer * 2f) * 4f, 0.4f);
				for (int i = 0; i < LightWidth; i += 4)
				{
					float off = timer + (float)i * 0.6f;
					float width = 4f + (float)Math.Sin(off * 0.5f + 1.2f) * 4f;
					float position = (float)Math.Sin((double)((off + (float)(i * 32)) * 0.1f) + Math.Sin(off * 0.05f + (float)i * 0.1f) * 0.25) * ((float)LightWidth / 2f - width / 2f);
					float length = (float)LightLength + (float)Math.Sin(off * 0.25f) * 8f;
					float a = 0.6f + (float)Math.Sin(off + 0.8f) * 0.3f;
					DrawTexture(position, width, length, a);
				}
			}
		}

		private void DrawTexture(float offset, float width, float length, float a)
		{
			float rot = Rotation + (float)Math.PI / 2f;
			if (width >= 1f)
			{
				texture.Draw(Position + Calc.AngleToVector(Rotation, 1f) * offset, new Vector2(0f, 0.5f), color * a * alpha, new Vector2(1f / (float)texture.Width * length, width), rot);
			}
		}
	}
}
