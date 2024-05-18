using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Pooled]
	public class TempleBigEyeballShockwave : Entity
	{
		private MTexture distortionTexture;

		private float distortionAlpha;

		private bool hasHitPlayer;

		public TempleBigEyeballShockwave()
		{
			base.Depth = -1000000;
			base.Collider = new Hitbox(48f, 200f, -30f, -100f);
			Add(new PlayerCollider(OnPlayer));
			MTexture tex = GFX.Game["util/displacementcirclehollow"];
			distortionTexture = tex.GetSubtexture(0, 0, tex.Width / 2, tex.Height);
			Add(new DisplacementRenderHook(RenderDisplacement));
		}

		public TempleBigEyeballShockwave Init(Vector2 position)
		{
			Position = position;
			Collidable = true;
			distortionAlpha = 0f;
			hasHitPlayer = false;
			return this;
		}

		public override void Update()
		{
			base.Update();
			base.X -= 300f * Engine.DeltaTime;
			distortionAlpha = Calc.Approach(distortionAlpha, 1f, Engine.DeltaTime * 4f);
			if (base.X < (float)(SceneAs<Level>().Bounds.Left - 20))
			{
				RemoveSelf();
			}
		}

		private void RenderDisplacement()
		{
			distortionTexture.DrawCentered(Position, Color.White * 0.8f * distortionAlpha, new Vector2(0.9f, 1.5f));
		}

		private void OnPlayer(Player player)
		{
			if (player.StateMachine.State != 2)
			{
				player.Speed.X = -100f;
				if (player.Speed.Y > 30f)
				{
					player.Speed.Y = 30f;
				}
				if (!hasHitPlayer)
				{
					Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
					Audio.Play("event:/game/05_mirror_temple/eye_pulse", player.Position);
					hasHitPlayer = true;
				}
			}
		}
	}
}
