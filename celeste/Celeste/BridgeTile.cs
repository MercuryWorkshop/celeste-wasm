using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BridgeTile : JumpThru
	{
		private List<Image> images;

		private Vector2 shakeOffset;

		private float shakeTimer;

		private float speedY;

		private float colorLerp;

		public bool Fallen { get; private set; }

		public BridgeTile(Vector2 position, Rectangle tileSize)
			: base(position, tileSize.Width, safe: false)
		{
			images = new List<Image>();
			if (tileSize.Width == 16)
			{
				int height = 24;
				int i = 0;
				while (i < tileSize.Height)
				{
					Image image2;
					Add(image2 = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize.X, i, tileSize.Width, height)));
					image2.Origin = new Vector2(image2.Width / 2f, 0f);
					image2.X = image2.Width / 2f;
					image2.Y = i - 8;
					images.Add(image2);
					i += height;
					height = 12;
				}
			}
			else
			{
				Image image;
				Add(image = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize)));
				image.Origin = new Vector2(image.Width / 2f, 0f);
				image.X = image.Width / 2f;
				image.Y = -8f;
				images.Add(image);
			}
		}

		public override void Update()
		{
			base.Update();
			bool isBeam = images[0].Width == 16f;
			if (!Fallen)
			{
				return;
			}
			if (shakeTimer > 0f)
			{
				shakeTimer -= Engine.DeltaTime;
				if (base.Scene.OnInterval(0.02f))
				{
					shakeOffset = Calc.Random.ShakeVector();
				}
				if (shakeTimer <= 0f)
				{
					Collidable = false;
					SceneAs<Level>().Shake(0.1f);
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					if (isBeam)
					{
						Audio.Play("event:/game/00_prologue/bridge_support_break", Position);
						foreach (Image image in images)
						{
							if (image.RenderPosition.Y > base.Y + 4f)
							{
								Dust.Burst(image.RenderPosition, -(float)Math.PI / 2f, 8);
							}
						}
					}
				}
				images[0].Position = new Vector2(images[0].Width / 2f, -8f) + shakeOffset;
				return;
			}
			colorLerp = Calc.Approach(colorLerp, 1f, 10f * Engine.DeltaTime);
			images[0].Color = Color.Lerp(Color.White, Color.Gray, colorLerp);
			shakeOffset = Vector2.Zero;
			if (isBeam)
			{
				int i = 0;
				foreach (Image image2 in images)
				{
					image2.Rotation -= (float)((i % 2 != 0) ? 1 : (-1)) * Engine.DeltaTime * (float)i * 2f;
					image2.Y += (float)i * Engine.DeltaTime * 16f;
					i++;
				}
				speedY = Calc.Approach(speedY, 120f, 600f * Engine.DeltaTime);
			}
			else
			{
				speedY = Calc.Approach(speedY, 200f, 900f * Engine.DeltaTime);
			}
			MoveV(speedY * Engine.DeltaTime);
			if (base.Top > 220f)
			{
				RemoveSelf();
			}
		}

		public void Fall(float timer = 0.2f)
		{
			if (!Fallen)
			{
				Fallen = true;
				shakeTimer = timer;
			}
		}
	}
}
