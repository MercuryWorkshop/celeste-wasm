using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class TempleMirrorPortal : Entity
	{
		private struct Debris
		{
			public Vector2 Direction;

			public float Percent;

			public float Duration;

			public bool Enabled;
		}

		private class Bg : Entity
		{
			private MirrorSurface surface;

			private Vector2[] offsets;

			private List<MTexture> textures;

			public Bg(Vector2 position)
				: base(position)
			{
				base.Depth = 9500;
				textures = GFX.Game.GetAtlasSubtextures("objects/temple/portal/reflection");
				Vector2 offset = new Vector2(10f, 4f);
				offsets = new Vector2[textures.Count];
				for (int i = 0; i < offsets.Length; i++)
				{
					offsets[i] = offset + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
				}
				Add(surface = new MirrorSurface());
				surface.OnRender = delegate
				{
					for (int j = 0; j < textures.Count; j++)
					{
						surface.ReflectionOffset = offsets[j];
						textures[j].DrawCentered(Position, surface.ReflectionColor);
					}
				};
			}

			public override void Render()
			{
				GFX.Game["objects/temple/portal/surface"].DrawCentered(Position);
			}
		}

		private class Curtain : Solid
		{
			public Sprite Sprite;

			public Curtain(Vector2 position)
				: base(position, 140f, 12f, safe: true)
			{
				Add(Sprite = GFX.SpriteBank.Create("temple_portal_curtain"));
				base.Depth = 1999;
				base.Collider.Position.X = -70f;
				base.Collider.Position.Y = 33f;
				Collidable = false;
				SurfaceSoundIndex = 17;
			}

			public override void Update()
			{
				base.Update();
				if (Collidable)
				{
					Player player;
					if ((player = CollideFirst<Player>(Position + new Vector2(-1f, 0f))) != null && player.OnGround() && Input.Aim.Value.X > 0f)
					{
						player.MoveV(base.Top - player.Bottom);
						player.MoveH(1f);
					}
					else if ((player = CollideFirst<Player>(Position + new Vector2(1f, 0f))) != null && player.OnGround() && Input.Aim.Value.X < 0f)
					{
						player.MoveV(base.Top - player.Bottom);
						player.MoveH(-1f);
					}
				}
			}

			public void Drop()
			{
				Sprite.Play("fall");
				base.Depth = -8999;
				Collidable = true;
				bool hit = false;
				Player player;
				while ((player = CollideFirst<Player>(Position)) != null && !hit)
				{
					Collidable = false;
					hit = player.MoveV(-1f);
					Collidable = true;
				}
			}
		}

		public static ParticleType P_CurtainDrop;

		public float DistortionFade = 1f;

		private bool canTrigger;

		private int switchCounter;

		private VirtualRenderTarget buffer;

		private float bufferAlpha;

		private float bufferTimer;

		private Debris[] debris = new Debris[50];

		private Color debrisColorFrom = Calc.HexToColor("f442d4");

		private Color debrisColorTo = Calc.HexToColor("000000");

		private MTexture debrisTexture = GFX.Game["particles/blob"];

		private Curtain curtain;

		private TemplePortalTorch leftTorch;

		private TemplePortalTorch rightTorch;

		public TempleMirrorPortal(Vector2 position)
			: base(position)
		{
			base.Depth = 2000;
			base.Collider = new Hitbox(120f, 64f, -60f, -32f);
			Add(new PlayerCollider(OnPlayer));
		}

		public TempleMirrorPortal(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(curtain = new Curtain(Position));
			scene.Add(new Bg(Position));
			scene.Add(leftTorch = new TemplePortalTorch(Position + new Vector2(-90f, 0f)));
			scene.Add(rightTorch = new TemplePortalTorch(Position + new Vector2(90f, 0f)));
		}

		public void OnSwitchHit(int side)
		{
			Add(new Coroutine(OnSwitchRoutine(side)));
		}

		private IEnumerator OnSwitchRoutine(int side)
		{
			yield return 0.4f;
			if (side < 0)
			{
				leftTorch.Light(switchCounter);
			}
			else
			{
				rightTorch.Light(switchCounter);
			}
			switchCounter++;
			if ((base.Scene as Level).Session.Area.Mode == AreaMode.Normal)
			{
				LightingRenderer lighting = (base.Scene as Level).Lighting;
				float lightTarget = Math.Max(0f, lighting.Alpha - 0.2f);
				while ((lighting.Alpha -= Engine.DeltaTime) > lightTarget)
				{
					yield return null;
				}
			}
			yield return 0.15f;
			if (switchCounter < 2)
			{
				yield break;
			}
			yield return 0.1f;
			Audio.Play("event:/game/05_mirror_temple/mainmirror_reveal", Position);
			curtain.Drop();
			canTrigger = true;
			yield return 0.1f;
			Level level = SceneAs<Level>();
			for (int i = 0; i < 120; i += 12)
			{
				for (int j = 0; j < 60; j += 6)
				{
					level.Particles.Emit(P_CurtainDrop, 1, curtain.Position + new Vector2(-57 + i, -27 + j), new Vector2(6f, 3f));
				}
			}
		}

		public void Activate()
		{
			Add(new Coroutine(ActivateRoutine()));
		}

		private IEnumerator ActivateRoutine()
		{
			Level level = base.Scene as Level;
			LightingRenderer light = level.Lighting;
			float debrisStart = 0f;
			Add(new BeforeRenderHook(BeforeRender));
			Add(new DisplacementRenderHook(RenderDisplacement));
			while (true)
			{
				bufferAlpha = Calc.Approach(bufferAlpha, 1f, Engine.DeltaTime);
				bufferTimer += 4f * Engine.DeltaTime;
				light.Alpha = Calc.Approach(light.Alpha, 0.2f, Engine.DeltaTime * 0.25f);
				if (debrisStart < (float)debris.Length)
				{
					int index = (int)debrisStart;
					debris[index].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
					debris[index].Enabled = true;
					debris[index].Duration = 0.5f + Calc.Random.NextFloat(0.7f);
				}
				debrisStart += Engine.DeltaTime * 10f;
				for (int i = 0; i < debris.Length; i++)
				{
					if (debris[i].Enabled)
					{
						debris[i].Percent %= 1f;
						debris[i].Percent += Engine.DeltaTime / debris[i].Duration;
					}
				}
				yield return null;
			}
		}

		private void BeforeRender()
		{
			if (buffer == null)
			{
				buffer = VirtualContent.CreateRenderTarget("temple-portal", 120, 64);
			}
			Vector2 center = new Vector2(buffer.Width, buffer.Height) / 2f;
			MTexture tex = GFX.Game["objects/temple/portal/portal"];
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			Engine.Graphics.GraphicsDevice.Clear(Color.Black);
			Draw.SpriteBatch.Begin();
			for (int i = 0; (float)i < 10f; i++)
			{
				float percent = bufferTimer % 1f * 0.1f + (float)i / 10f;
				Color color = Color.Lerp(Color.Black, Color.Purple, percent);
				float scale = percent;
				float rotation = (float)Math.PI * 2f * percent;
				tex.DrawCentered(center, color, scale, rotation);
			}
			Draw.SpriteBatch.End();
		}

		private void RenderDisplacement()
		{
			Draw.Rect(base.X - 60f, base.Y - 32f, 120f, 64f, new Color(0.5f, 0.5f, 0.25f * DistortionFade * bufferAlpha, 1f));
		}

		public override void Render()
		{
			base.Render();
			if (buffer != null)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Position + new Vector2((0f - base.Collider.Width) / 2f, (0f - base.Collider.Height) / 2f), Color.White * bufferAlpha);
			}
			GFX.Game["objects/temple/portal/portalframe"].DrawCentered(Position);
			Level level = base.Scene as Level;
			for (int i = 0; i < debris.Length; i++)
			{
				Debris d = debris[i];
				if (d.Enabled)
				{
					float ease = Ease.SineOut(d.Percent);
					Vector2 pos = Position + d.Direction * (1f - ease) * (190f - level.Zoom * 30f);
					Color color = Color.Lerp(debrisColorFrom, debrisColorTo, ease);
					float scale = Calc.LerpClamp(1f, 0.2f, ease);
					debrisTexture.DrawCentered(pos, color, scale, (float)i * 0.05f);
				}
			}
		}

		private void OnPlayer(Player player)
		{
			if (canTrigger)
			{
				canTrigger = false;
				base.Scene.Add(new CS04_MirrorPortal(player, this));
			}
		}

		public override void Removed(Scene scene)
		{
			Dispose();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		private void Dispose()
		{
			if (buffer != null)
			{
				buffer.Dispose();
			}
			buffer = null;
		}
	}
}
