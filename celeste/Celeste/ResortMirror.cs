using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class ResortMirror : Entity
	{
		private bool smashed;

		private Image bg;

		private Image frame;

		private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];

		private Sprite breakingGlass;

		private VirtualRenderTarget mirror;

		private float shineAlpha = 0.7f;

		private float mirrorAlpha = 0.7f;

		private BadelineDummy evil;

		private bool shardReflection;

		public ResortMirror(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(new BeforeRenderHook(BeforeRender));
			base.Depth = 9500;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			smashed = SceneAs<Level>().Session.GetFlag("oshiro_resort_suite");
			Entity frameEntity = new Entity(Position);
			frameEntity.Depth = 9000;
			frameEntity.Add(frame = new Image(GFX.Game["objects/mirror/resortframe"]));
			frame.JustifyOrigin(0.5f, 1f);
			base.Scene.Add(frameEntity);
			MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
			int w = (int)frame.Width - 2;
			int h = (int)frame.Height - 12;
			if (!smashed)
			{
				mirror = VirtualContent.CreateRenderTarget("resort-mirror", w, h);
			}
			else
			{
				glassbg = GFX.Game["objects/mirror/glassbreak09"];
			}
			Add(bg = new Image(glassbg.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h)));
			bg.JustifyOrigin(0.5f, 1f);
			List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask");
			MTexture temp = new MTexture();
			foreach (MTexture shard in atlasSubtextures)
			{
				MirrorSurface surface = new MirrorSurface();
				surface.OnRender = delegate
				{
					shard.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h, temp).DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (shardReflection ? 1 : 0));
				};
				surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
				Add(surface);
			}
		}

		public void EvilAppear()
		{
			Add(new Coroutine(EvilAppearRoutine()));
			Add(new Coroutine(FadeLights()));
		}

		private IEnumerator EvilAppearRoutine()
		{
			evil = new BadelineDummy(new Vector2(mirror.Width + 8, mirror.Height));
			yield return evil.WalkTo(mirror.Width / 2);
		}

		private IEnumerator FadeLights()
		{
			Level level = SceneAs<Level>();
			while (level.Lighting.Alpha != 0.35f)
			{
				level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, 0.35f, Engine.DeltaTime * 0.1f);
				yield return null;
			}
		}

		public IEnumerator SmashRoutine()
		{
			yield return evil.FloatTo(new Vector2(mirror.Width / 2, mirror.Height - 8));
			breakingGlass = GFX.SpriteBank.Create("glass");
			breakingGlass.Position = new Vector2(mirror.Width / 2, mirror.Height);
			breakingGlass.Play("break");
			breakingGlass.Color = Color.White * shineAlpha;
			Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
			while (breakingGlass.CurrentAnimationID == "break")
			{
				if (breakingGlass.CurrentAnimationFrame == 7)
				{
					SceneAs<Level>().Shake();
				}
				shineAlpha = Calc.Approach(shineAlpha, 1f, Engine.DeltaTime * 2f);
				mirrorAlpha = Calc.Approach(mirrorAlpha, 1f, Engine.DeltaTime * 2f);
				yield return null;
			}
			SceneAs<Level>().Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			for (float x = (0f - breakingGlass.Width) / 2f; x < breakingGlass.Width / 2f; x += 8f)
			{
				for (float y = 0f - breakingGlass.Height; y < 0f; y += 8f)
				{
					if (Calc.Random.Chance(0.5f))
					{
						(base.Scene as Level).Particles.Emit(DreamMirror.P_Shatter, 2, Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
					}
				}
			}
			shardReflection = true;
			evil = null;
		}

		public void Broken()
		{
			evil = null;
			smashed = true;
			shardReflection = true;
			MTexture tex = GFX.Game["objects/mirror/glassbreak09"];
			bg.Texture = tex.GetSubtexture((int)((float)tex.Width - bg.Width) / 2, tex.Height - (int)bg.Height, (int)bg.Width, (int)bg.Height);
		}

		private void BeforeRender()
		{
			if (smashed || mirror == null)
			{
				return;
			}
			Level level = SceneAs<Level>();
			Engine.Graphics.GraphicsDevice.SetRenderTarget(mirror);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			NPC oshiro = base.Scene.Entities.FindFirst<NPC>();
			if (oshiro != null)
			{
				Vector2 prev = oshiro.Sprite.RenderPosition;
				oshiro.Sprite.RenderPosition = prev - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, -4f);
				oshiro.Sprite.Render();
				oshiro.Sprite.RenderPosition = prev;
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				Vector2 prev2 = player.Position;
				player.Position = prev2 - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, 0f);
				Vector2 diff = player.Position - prev2;
				for (int j = 0; j < player.Hair.Nodes.Count; j++)
				{
					player.Hair.Nodes[j] += diff;
				}
				player.Render();
				for (int i = 0; i < player.Hair.Nodes.Count; i++)
				{
					player.Hair.Nodes[i] -= diff;
				}
				player.Position = prev2;
			}
			if (evil != null)
			{
				evil.Update();
				evil.Hair.Facing = (Facings)Math.Sign(evil.Sprite.Scale.X);
				evil.Hair.AfterUpdate();
				evil.Render();
			}
			if (breakingGlass != null)
			{
				breakingGlass.Color = Color.White * shineAlpha;
				breakingGlass.Update();
				breakingGlass.Render();
			}
			else
			{
				int shineOffset = -(int)(level.Camera.Y * 0.8f % (float)glassfg.Height);
				glassfg.DrawJustified(new Vector2(mirror.Width / 2, shineOffset), new Vector2(0.5f, 1f), Color.White * shineAlpha);
				glassfg.DrawJustified(new Vector2(mirror.Height / 2, shineOffset - glassfg.Height), new Vector2(0.5f, 1f), Color.White * shineAlpha);
			}
			Draw.SpriteBatch.End();
		}

		public override void Render()
		{
			bg.Render();
			if (!smashed)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)mirror, Position + new Vector2(-mirror.Width / 2, -mirror.Height), Color.White * mirrorAlpha);
			}
			frame.Render();
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		public override void Removed(Scene scene)
		{
			Dispose();
			base.Removed(scene);
		}

		private void Dispose()
		{
			if (mirror != null)
			{
				mirror.Dispose();
			}
			mirror = null;
		}
	}
}
