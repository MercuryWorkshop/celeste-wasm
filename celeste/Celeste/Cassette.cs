using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Cassette : Entity
	{
		private class UnlockedBSide : Entity
		{
			private float alpha;

			private string text;

			private bool waitForKeyPress;

			private float timer;

			public override void Added(Scene scene)
			{
				base.Added(scene);
				base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
				text = ActiveFont.FontSize.AutoNewline(Dialog.Clean("UI_REMIX_UNLOCKED"), 900);
				base.Depth = -10000;
			}

			public IEnumerator EaseIn()
			{
				_ = base.Scene;
				while ((alpha += Engine.DeltaTime / 0.5f) < 1f)
				{
					yield return null;
				}
				alpha = 1f;
				yield return 1.5f;
				waitForKeyPress = true;
			}

			public IEnumerator EaseOut()
			{
				waitForKeyPress = false;
				while ((alpha -= Engine.DeltaTime / 0.5f) > 0f)
				{
					yield return null;
				}
				alpha = 0f;
				RemoveSelf();
			}

			public override void Update()
			{
				timer += Engine.DeltaTime;
				base.Update();
			}

			public override void Render()
			{
				float e = Ease.CubeOut(alpha);
				Vector2 center = Celeste.TargetCenter + new Vector2(0f, 64f);
				Vector2 ease = Vector2.UnitY * 64f * (1f - e);
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * e * 0.8f);
				GFX.Gui["collectables/cassette"].DrawJustified(center - ease + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * e);
				ActiveFont.Draw(text, center + ease, new Vector2(0.5f, 0f), Vector2.One, Color.White * e);
				if (waitForKeyPress)
				{
					GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((timer % 1f < 0.25f) ? 6 : 0)));
				}
			}
		}

		public static ParticleType P_Shine;

		public static ParticleType P_Collect;

		public bool IsGhost;

		private Sprite sprite;

		private SineWave hover;

		private BloomPoint bloom;

		private VertexLight light;

		private Wiggler scaleWiggler;

		private bool collected;

		private Vector2[] nodes;

		private EventInstance? remixSfx;

		private bool collecting;

		public Cassette(Vector2 position, Vector2[] nodes)
			: base(position)
		{
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			this.nodes = nodes;
			Add(new PlayerCollider(OnPlayer));
		}

		public Cassette(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.NodesOffset(offset))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			IsGhost = SaveData.Instance.Areas[SceneAs<Level>().Session.Area.ID].Cassette;
			Add(sprite = GFX.SpriteBank.Create(IsGhost ? "cassetteGhost" : "cassette"));
			sprite.Play("idle");
			Add(scaleWiggler = Wiggler.Create(0.25f, 4f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(bloom = new BloomPoint(0.25f, 16f));
			Add(light = new VertexLight(Color.White, 0.4f, 32, 64));
			Add(hover = new SineWave(0.5f));
			hover.OnUpdate = delegate(float f)
			{
				Sprite obj = sprite;
				VertexLight vertexLight = light;
				float num2 = (bloom.Y = f * 2f);
				float num5 = (obj.Y = (vertexLight.Y = num2));
			};
			if (IsGhost)
			{
				sprite.Color = Color.White * 0.8f;
			}
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Audio.Stop(remixSfx);
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Audio.Stop(remixSfx);
		}

		public override void Update()
		{
			base.Update();
			if (!collecting && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().Particles.Emit(P_Shine, 1, base.Center, new Vector2(12f, 10f));
			}
		}

		private void OnPlayer(Player player)
		{
			if (!collected)
			{
				player?.RefillStamina();
				Audio.Play("event:/game/general/cassette_get", Position);
				collected = true;
				Celeste.Freeze(0.1f);
				Add(new Coroutine(CollectRoutine(player)));
			}
		}

		private IEnumerator CollectRoutine(Player player)
		{
			collecting = true;
			Level level = base.Scene as Level;
			CassetteBlockManager cbm = base.Scene.Tracker.GetEntity<CassetteBlockManager>();
			level.PauseLock = true;
			level.Frozen = true;
			base.Tag = Tags.FrozenUpdate;
			level.Session.Cassette = true;
			level.Session.RespawnPoint = level.GetSpawnPoint(nodes[1]);
			level.Session.UpdateLevelStartDashes();
			SaveData.Instance.RegisterCassette(level.Session.Area);
			cbm?.StopBlocks();
			base.Depth = -1000000;
			level.Shake();
			level.Flash(Color.White);
			level.Displacement.Clear();
			Vector2 camWas = level.Camera.Position;
			Vector2 camTo = (Position - new Vector2(160f, 90f)).Clamp(level.Bounds.Left - 64, level.Bounds.Top - 32, level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
			level.Camera.Position = camTo;
			level.ZoomSnap((Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2f);
			sprite.Play("spin", restart: true);
			sprite.Rate = 2f;
			for (float p3 = 0f; p3 < 1.5f; p3 += Engine.DeltaTime)
			{
				sprite.Rate += Engine.DeltaTime * 4f;
				yield return null;
			}
			sprite.Rate = 0f;
			sprite.SetAnimationFrame(0);
			scaleWiggler.Start();
			yield return 0.25f;
			Vector2 from = Position;
			Vector2 to = new Vector2(base.X, level.Camera.Top - 16f);
			float duration2 = 0.4f;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
			{
				sprite.Scale.X = MathHelper.Lerp(1f, 0.1f, p3);
				sprite.Scale.Y = MathHelper.Lerp(1f, 3f, p3);
				Position = Vector2.Lerp(from, to, Ease.CubeIn(p3));
				yield return null;
			}
			Visible = false;
			remixSfx = Audio.Play("event:/game/general/cassette_preview", "remix", level.Session.Area.ID);
			UnlockedBSide message = new UnlockedBSide();
			base.Scene.Add(message);
			yield return message.EaseIn();
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			Audio.SetParameter(remixSfx, "end", 1f);
			yield return message.EaseOut();
			duration2 = 0.25f;
			Add(new Coroutine(level.ZoomBack(duration2 - 0.05f)));
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
			{
				level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p3));
				yield return null;
			}
			if (!player.Dead && nodes != null && nodes.Length >= 2)
			{
				Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
				player.StartCassetteFly(nodes[1], nodes[0]);
			}
			foreach (SandwichLava item in level.Entities.FindAll<SandwichLava>())
			{
				item.Leave();
			}
			level.Frozen = false;
			yield return 0.25f;
			cbm?.Finish();
			level.PauseLock = false;
			level.ResetZoom();
			RemoveSelf();
		}
	}
}
