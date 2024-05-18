using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class TrailManager : Entity
	{
		[Pooled]
		[Tracked(false)]
		public class Snapshot : Entity
		{
			public TrailManager Manager;

			public Image Sprite;

			public Vector2 SpriteScale;

			public PlayerHair Hair;

			public int Index;

			public Color Color;

			public float Percent;

			public float Duration;

			public bool Drawn;

			public bool UseRawDeltaTime;

			public Snapshot()
			{
				Add(new MirrorReflection());
			}

			public void Init(TrailManager manager, int index, Vector2 position, Image sprite, PlayerHair hair, Vector2 scale, Color color, float duration, int depth, bool frozenUpdate, bool useRawDeltaTime)
			{
				base.Tag = Tags.Global;
				if (frozenUpdate)
				{
					base.Tag |= Tags.FrozenUpdate;
				}
				Manager = manager;
				Index = index;
				Position = position;
				Sprite = sprite;
				SpriteScale = scale;
				Hair = hair;
				Color = color;
				Percent = 0f;
				Duration = duration;
				base.Depth = depth;
				Drawn = false;
				UseRawDeltaTime = useRawDeltaTime;
			}

			public override void Update()
			{
				if (Duration <= 0f)
				{
					if (Drawn)
					{
						RemoveSelf();
					}
					return;
				}
				if (Percent >= 1f)
				{
					RemoveSelf();
				}
				Percent += (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / Duration;
			}

			public override void Render()
			{
				VirtualRenderTarget buffer = Manager.buffer;
				Rectangle bounds = new Rectangle(Index % 8 * 64, Index / 8 * 64, 64, 64);
				float alpha = ((Duration > 0f) ? (0.75f * (1f - Ease.CubeOut(Percent))) : 1f);
				if (buffer != null)
				{
					Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Position, bounds, Color * alpha, 0f, new Vector2(64f, 64f) * 0.5f, Vector2.One, SpriteEffects.None, 0f);
				}
			}

			public override void Removed(Scene scene)
			{
				if (Manager != null)
				{
					Manager.snapshots[Index] = null;
				}
				base.Removed(scene);
			}
		}

		private static BlendState MaxBlendState = new BlendState
		{
			ColorSourceBlend = Blend.DestinationAlpha,
			AlphaSourceBlend = Blend.DestinationAlpha
		};

		private const int size = 64;

		private const int columns = 8;

		private const int rows = 8;

		private Snapshot[] snapshots = new Snapshot[64];

		private VirtualRenderTarget buffer;

		private bool dirty;

		public TrailManager()
		{
			base.Tag = Tags.Global;
			base.Depth = 10;
			Add(new BeforeRenderHook(BeforeRender));
			Add(new MirrorReflection());
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

		private void BeforeRender()
		{
			if (!dirty)
			{
				return;
			}
			if (buffer == null)
			{
				buffer = VirtualContent.CreateRenderTarget("trail-manager", 512, 512);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, LightingRenderer.OccludeBlendState);
			for (int i = 0; i < snapshots.Length; i++)
			{
				if (snapshots[i] != null && !snapshots[i].Drawn)
				{
					Draw.Rect(i % 8 * 64, i / 8 * 64, 64f, 64f, Color.Transparent);
				}
			}
			Draw.SpriteBatch.End();
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
			for (int j = 0; j < snapshots.Length; j++)
			{
				if (snapshots[j] == null || snapshots[j].Drawn)
				{
					continue;
				}
				Snapshot snapshot = snapshots[j];
				Vector2 diff = new Vector2(((float)(j % 8) + 0.5f) * 64f, ((float)(j / 8) + 0.5f) * 64f) - snapshot.Position;
				if (snapshot.Hair != null)
				{
					for (int l = 0; l < snapshot.Hair.Nodes.Count; l++)
					{
						snapshot.Hair.Nodes[l] += diff;
					}
					snapshot.Hair.Render();
					for (int k = 0; k < snapshot.Hair.Nodes.Count; k++)
					{
						snapshot.Hair.Nodes[k] -= diff;
					}
				}
				Vector2 was = snapshot.Sprite.Scale;
				snapshot.Sprite.Scale = snapshot.SpriteScale;
				snapshot.Sprite.Position += diff;
				snapshot.Sprite.Render();
				snapshot.Sprite.Scale = was;
				snapshot.Sprite.Position -= diff;
				snapshot.Drawn = true;
			}
			Draw.SpriteBatch.End();
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, MaxBlendState);
			Draw.Rect(0f, 0f, buffer.Width, buffer.Height, new Color(1f, 1f, 1f, 1f));
			Draw.SpriteBatch.End();
			dirty = false;
		}

		public static void Add(Entity entity, Color color, float duration = 1f, bool frozenUpdate = false, bool useRawDeltaTime = false)
		{
			Image sprite = entity.Get<PlayerSprite>();
			if (sprite == null)
			{
				sprite = entity.Get<Sprite>();
			}
			PlayerHair hair = entity.Get<PlayerHair>();
			Add(entity.Position, sprite, hair, sprite.Scale, color, entity.Depth + 1, duration, frozenUpdate, useRawDeltaTime);
		}

		public static void Add(Entity entity, Vector2 scale, Color color, float duration = 1f)
		{
			Image sprite = entity.Get<PlayerSprite>();
			if (sprite == null)
			{
				sprite = entity.Get<Sprite>();
			}
			PlayerHair hair = entity.Get<PlayerHair>();
			Add(entity.Position, sprite, hair, scale, color, entity.Depth + 1, duration);
		}

		public static void Add(Vector2 position, Image image, Color color, int depth, float duration = 1f)
		{
			Add(position, image, null, image.Scale, color, depth, duration);
		}

		public static Snapshot Add(Vector2 position, Image sprite, PlayerHair hair, Vector2 scale, Color color, int depth, float duration = 1f, bool frozenUpdate = false, bool useRawDeltaTime = false)
		{
			TrailManager manager = Engine.Scene.Tracker.GetEntity<TrailManager>();
			if (manager == null)
			{
				manager = new TrailManager();
				Engine.Scene.Add(manager);
			}
			for (int index = 0; index < manager.snapshots.Length; index++)
			{
				if (manager.snapshots[index] == null)
				{
					Snapshot snapshot = Engine.Pooler.Create<Snapshot>();
					snapshot.Init(manager, index, position, sprite, hair, scale, color, duration, depth, frozenUpdate, useRawDeltaTime);
					manager.snapshots[index] = snapshot;
					manager.dirty = true;
					Engine.Scene.Add(snapshot);
					return snapshot;
				}
			}
			return null;
		}

		public static void Clear()
		{
			TrailManager manager = Engine.Scene.Tracker.GetEntity<TrailManager>();
			if (manager == null)
			{
				return;
			}
			for (int i = 0; i < manager.snapshots.Length; i++)
			{
				if (manager.snapshots[i] != null)
				{
					manager.snapshots[i].RemoveSelf();
				}
			}
		}
	}
}
