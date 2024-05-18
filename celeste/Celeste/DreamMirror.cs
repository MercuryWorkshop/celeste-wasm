using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class DreamMirror : Entity
	{
		public static ParticleType P_Shatter;

		private Image frame;

		private MTexture glassbg = GFX.Game["objects/mirror/glassbg"];

		private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];

		private Sprite breakingGlass;

		private Hitbox hitbox;

		private VirtualRenderTarget mirror;

		private float shineAlpha = 0.5f;

		private float shineOffset;

		private Entity reflection;

		private PlayerSprite reflectionSprite;

		private PlayerHair reflectionHair;

		private float reflectionAlpha = 0.7f;

		private bool autoUpdateReflection = true;

		private BadelineDummy badeline;

		private bool smashed;

		private bool smashEnded;

		private bool updateShine = true;

		private Coroutine smashCoroutine;

		private SoundSource sfx;

		private SoundSource sfxSting;

		public DreamMirror(Vector2 position)
			: base(position)
		{
			base.Depth = 9500;
			Add(breakingGlass = GFX.SpriteBank.Create("glass"));
			breakingGlass.Play("idle");
			Add(new BeforeRenderHook(BeforeRender));
			foreach (MTexture shard in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask"))
			{
				MirrorSurface surface = new MirrorSurface();
				surface.OnRender = delegate
				{
					shard.DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (smashEnded ? 1 : 0));
				};
				surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
				Add(surface);
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			smashed = SceneAs<Level>().Session.Inventory.DreamDash;
			if (smashed)
			{
				breakingGlass.Play("broken");
				smashEnded = true;
			}
			else
			{
				reflection = new Entity();
				reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
				reflectionHair = new PlayerHair(reflectionSprite);
				reflectionHair.Color = BadelineOldsite.HairColor;
				reflectionHair.Border = Color.Black;
				reflection.Add(reflectionHair);
				reflection.Add(reflectionSprite);
				reflectionHair.Start();
				reflectionSprite.OnFrameChange = delegate(string anim)
				{
					if (!smashed && CollideCheck<Player>())
					{
						int currentAnimationFrame = reflectionSprite.CurrentAnimationFrame;
						if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)))
						{
							Audio.Play("event:/char/badeline/footstep", base.Center);
						}
					}
				};
				Add(smashCoroutine = new Coroutine(InteractRoutine()));
			}
			Entity frameEntity = new Entity(Position);
			frameEntity.Depth = 9000;
			frameEntity.Add(frame = new Image(GFX.Game["objects/mirror/frame"]));
			frame.JustifyOrigin(0.5f, 1f);
			base.Scene.Add(frameEntity);
			base.Collider = (hitbox = new Hitbox((int)frame.Width - 16, (int)frame.Height + 32, -(int)frame.Width / 2 + 8, -(int)frame.Height - 32));
		}

		public override void Update()
		{
			base.Update();
			if (reflection != null)
			{
				reflection.Update();
				reflectionHair.Facing = (Facings)Math.Sign(reflectionSprite.Scale.X);
				reflectionHair.AfterUpdate();
			}
		}

		private void BeforeRender()
		{
			if (smashed)
			{
				return;
			}
			Level level = base.Scene as Level;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null)
			{
				return;
			}
			if (autoUpdateReflection && reflection != null)
			{
				reflection.Position = new Vector2(base.X - player.X, player.Y - base.Y) + breakingGlass.Origin;
				reflectionSprite.Scale.X = (float)(0 - player.Facing) * Math.Abs(player.Sprite.Scale.X);
				reflectionSprite.Scale.Y = player.Sprite.Scale.Y;
				if (reflectionSprite.CurrentAnimationID != player.Sprite.CurrentAnimationID && player.Sprite.CurrentAnimationID != null && reflectionSprite.Has(player.Sprite.CurrentAnimationID))
				{
					reflectionSprite.Play(player.Sprite.CurrentAnimationID);
				}
			}
			if (mirror == null)
			{
				mirror = VirtualContent.CreateRenderTarget("dream-mirror", glassbg.Width, glassbg.Height);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(mirror);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			if (updateShine)
			{
				shineOffset = glassfg.Height - (int)(level.Camera.Y * 0.8f % (float)glassfg.Height);
			}
			glassbg.Draw(Vector2.Zero);
			if (reflection != null)
			{
				reflection.Render();
			}
			glassfg.Draw(new Vector2(0f, shineOffset), Vector2.Zero, Color.White * shineAlpha);
			glassfg.Draw(new Vector2(0f, shineOffset - (float)glassfg.Height), Vector2.Zero, Color.White * shineAlpha);
			Draw.SpriteBatch.End();
		}

		private IEnumerator InteractRoutine()
		{
			Player player = null;
			while (player == null)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				yield return null;
			}
			while (!hitbox.Collide(player))
			{
				yield return null;
			}
			hitbox.Width += 32f;
			hitbox.Position.X -= 16f;
			Audio.SetMusic(null);
			while (hitbox.Collide(player))
			{
				yield return null;
			}
			base.Scene.Add(new CS02_Mirror(player, this));
		}

		public IEnumerator BreakRoutine(int direction)
		{
			autoUpdateReflection = false;
			reflectionSprite.Play("runFast");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			while (Math.Abs(reflection.X - breakingGlass.Width / 2f) > 3f)
			{
				reflection.X += (float)(direction * 32) * Engine.DeltaTime;
				yield return null;
			}
			reflectionSprite.Play("idle");
			yield return 0.65f;
			Add(sfx = new SoundSource());
			sfx.Play("event:/game/02_old_site/sequence_mirror");
			yield return 0.15f;
			Add(sfxSting = new SoundSource("event:/music/lvl2/dreamblock_sting_pt2"));
			Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
			updateShine = false;
			while (shineOffset != 33f || shineAlpha < 1f)
			{
				shineOffset = Calc.Approach(shineOffset, 33f, Engine.DeltaTime * 120f);
				shineAlpha = Calc.Approach(shineAlpha, 1f, Engine.DeltaTime * 4f);
				yield return null;
			}
			smashed = true;
			breakingGlass.Play("break");
			yield return 0.6f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			(base.Scene as Level).Shake();
			for (float x = (0f - breakingGlass.Width) / 2f; x < breakingGlass.Width / 2f; x += 8f)
			{
				for (float y = 0f - breakingGlass.Height; y < 0f; y += 8f)
				{
					if (Calc.Random.Chance(0.5f))
					{
						(base.Scene as Level).Particles.Emit(P_Shatter, 2, Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
					}
				}
			}
			smashEnded = true;
			badeline = new BadelineDummy(reflection.Position + Position - breakingGlass.Origin);
			badeline.Floatness = 0f;
			for (int i = 0; i < badeline.Hair.Nodes.Count; i++)
			{
				badeline.Hair.Nodes[i] = reflectionHair.Nodes[i];
			}
			base.Scene.Add(badeline);
			badeline.Sprite.Play("idle");
			badeline.Sprite.Scale = reflectionSprite.Scale;
			reflection = null;
			yield return 1.2f;
			float speed = (float)(-direction) * 32f;
			badeline.Sprite.Scale.X = -direction;
			badeline.Sprite.Play("runFast");
			while (Math.Abs(badeline.X - base.X) < 60f)
			{
				speed += Engine.DeltaTime * (float)(-direction) * 128f;
				badeline.X += speed * Engine.DeltaTime;
				yield return null;
			}
			badeline.Sprite.Play("jumpFast");
			while (Math.Abs(badeline.X - base.X) < 128f)
			{
				speed += Engine.DeltaTime * (float)(-direction) * 128f;
				badeline.X += speed * Engine.DeltaTime;
				badeline.Y -= Math.Abs(speed) * Engine.DeltaTime * 0.8f;
				yield return null;
			}
			badeline.RemoveSelf();
			badeline = null;
			yield return 1.5f;
		}

		public void Broken(bool wasSkipped)
		{
			updateShine = false;
			smashed = true;
			smashEnded = true;
			breakingGlass.Play("broken");
			if (wasSkipped && badeline != null)
			{
				badeline.RemoveSelf();
			}
			if (wasSkipped && sfx != null)
			{
				sfx.Stop();
			}
			if (wasSkipped && sfxSting != null)
			{
				sfxSting.Stop();
			}
		}

		public override void Render()
		{
			if (smashed)
			{
				breakingGlass.Render();
			}
			else
			{
				Draw.SpriteBatch.Draw(mirror.Target, Position - breakingGlass.Origin, Color.White * reflectionAlpha);
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
