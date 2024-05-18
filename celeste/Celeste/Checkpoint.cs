using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Checkpoint : Entity
	{
		private const float LightAlpha = 0.8f;

		private const float BloomAlpha = 0.5f;

		private const float TargetFade = 0.5f;

		private Image image;

		private Sprite sprite;

		private Sprite flash;

		private VertexLight light;

		private BloomPoint bloom;

		private bool triggered;

		private float sine = (float)Math.PI / 2f;

		private float fade = 1f;

		private string bg;

		public Vector2 SpawnOffset;

		public Checkpoint(Vector2 position, string bg = "", Vector2? spawnTarget = null)
			: base(position)
		{
			base.Depth = 9990;
			SpawnOffset = (spawnTarget.HasValue ? (spawnTarget.Value - Position) : Vector2.Zero);
			this.bg = bg;
		}

		public Checkpoint(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Attr("bg"), data.FirstNodeNullable(offset))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level level = base.Scene as Level;
			int area = level.Session.Area.ID;
			string img = "";
			img = ((!string.IsNullOrWhiteSpace(bg)) ? ("objects/checkpoint/bg/" + bg) : ("objects/checkpoint/bg/" + area));
			if (GFX.Game.Has(img))
			{
				Add(image = new Image(GFX.Game[img]));
				image.JustifyOrigin(0.5f, 1f);
			}
			Add(sprite = GFX.SpriteBank.Create("checkpoint_highlight"));
			sprite.Play("off");
			Add(flash = GFX.SpriteBank.Create("checkpoint_flash"));
			flash.Visible = false;
			flash.Color = Color.White * 0.6f;
			if (SaveData.Instance.HasCheckpoint(level.Session.Area, level.Session.Level))
			{
				TurnOn(animate: false);
			}
		}

		public override void Update()
		{
			if (!triggered)
			{
				Level level = base.Scene as Level;
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && !level.Transitioning)
				{
					if (!player.CollideCheck<CheckpointBlockerTrigger>() && SaveData.Instance.SetCheckpoint(level.Session.Area, level.Session.Level))
					{
						level.AutoSave();
						TurnOn(animate: true);
					}
					triggered = true;
				}
			}
			if (triggered && sprite.CurrentAnimationID == "on")
			{
				sine += Engine.DeltaTime * 2f;
				fade = Calc.Approach(fade, 0.5f, Engine.DeltaTime);
				float ease = (float)(1.0 + Math.Sin(sine)) / 2f;
				sprite.Color = Color.White * (0.5f + ease * 0.5f) * fade;
			}
			base.Update();
		}

		private void TurnOn(bool animate)
		{
			triggered = true;
			Add(light = new VertexLight(Color.White, 0f, 16, 32));
			Add(bloom = new BloomPoint(0f, 16f));
			light.Position = new Vector2(0f, -8f);
			bloom.Position = new Vector2(0f, -8f);
			flash.Visible = true;
			flash.Play("flash", restart: true);
			if (animate)
			{
				sprite.Play("turnOn");
				Add(new Coroutine(EaseLightsOn()));
				fade = 1f;
			}
			else
			{
				fade = 0.5f;
				sprite.Play("on");
				sprite.Color = Color.White * 0.5f;
				light.Alpha = 0.8f;
				bloom.Alpha = 0.5f;
			}
		}

		private IEnumerator EaseLightsOn()
		{
			float lightStartRadius = light.StartRadius;
			float lightEndRadius = light.EndRadius;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.5f)
			{
				float ease = Ease.BigBackOut(p);
				light.Alpha = 0.8f * ease;
				light.StartRadius = (int)(lightStartRadius + Calc.YoYo(p) * 8f);
				light.EndRadius = (int)(lightEndRadius + Calc.YoYo(p) * 16f);
				bloom.Alpha = 0.5f * ease;
				yield return null;
			}
		}
	}
}
