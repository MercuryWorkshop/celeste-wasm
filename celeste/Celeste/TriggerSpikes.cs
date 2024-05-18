using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TriggerSpikes : Entity
	{
		public enum Directions
		{
			Up,
			Down,
			Left,
			Right
		}

		private struct SpikeInfo
		{
			public TriggerSpikes Parent;

			public int Index;

			public Vector2 WorldPosition;

			public bool Triggered;

			public float RetractTimer;

			public float DelayTimer;

			public float Lerp;

			public float ParticleTimerOffset;

			public int TextureIndex;

			public float TextureRotation;

			public int DustOutDistance;

			public int TentacleColor;

			public float TentacleFrame;

			public void Update()
			{
				if (Triggered)
				{
					if (DelayTimer > 0f)
					{
						DelayTimer -= Engine.DeltaTime;
						if (DelayTimer <= 0f)
						{
							if (PlayerCheck())
							{
								DelayTimer = 0.05f;
							}
							else
							{
								Audio.Play("event:/game/03_resort/fluff_tendril_emerge", WorldPosition);
							}
						}
					}
					else
					{
						Lerp = Calc.Approach(Lerp, 1f, 8f * Engine.DeltaTime);
					}
					TextureRotation += Engine.DeltaTime * 1.2f;
				}
				else
				{
					Lerp = Calc.Approach(Lerp, 0f, 4f * Engine.DeltaTime);
					TentacleFrame += Engine.DeltaTime * 12f;
					if (Lerp <= 0f)
					{
						Triggered = false;
					}
				}
			}

			public bool PlayerCheck()
			{
				return Parent.PlayerCheck(Index);
			}

			public bool OnPlayer(Player player, Vector2 outwards)
			{
				if (!Triggered)
				{
					Audio.Play("event:/game/03_resort/fluff_tendril_touch", WorldPosition);
					Triggered = true;
					DelayTimer = 0.4f;
					RetractTimer = 6f;
				}
				else if (Lerp >= 1f)
				{
					player.Die(outwards);
					return true;
				}
				return false;
			}
		}

		private const float RetractTime = 6f;

		private const float DelayTime = 0.4f;

		private Directions direction;

		private Vector2 outwards;

		private Vector2 offset;

		private PlayerCollider pc;

		private Vector2 shakeOffset;

		private SpikeInfo[] spikes;

		private List<MTexture> dustTextures;

		private List<MTexture> tentacleTextures;

		private Color[] tentacleColors;

		private int size;

		public TriggerSpikes(Vector2 position, int size, Directions direction)
			: base(position)
		{
			this.size = size;
			this.direction = direction;
			switch (direction)
			{
			case Directions.Up:
				tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
				outwards = new Vector2(0f, -1f);
				offset = new Vector2(0f, -1f);
				base.Collider = new Hitbox(size, 4f, 0f, -4f);
				Add(new SafeGroundBlocker());
				Add(new LedgeBlocker(UpSafeBlockCheck));
				break;
			case Directions.Down:
				tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
				outwards = new Vector2(0f, 1f);
				base.Collider = new Hitbox(size, 4f);
				break;
			case Directions.Left:
				tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
				outwards = new Vector2(-1f, 0f);
				base.Collider = new Hitbox(4f, size, -4f);
				Add(new SafeGroundBlocker());
				Add(new LedgeBlocker(SideSafeBlockCheck));
				break;
			case Directions.Right:
				tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
				outwards = new Vector2(1f, 0f);
				offset = new Vector2(1f, 0f);
				base.Collider = new Hitbox(4f, size);
				Add(new SafeGroundBlocker());
				Add(new LedgeBlocker(SideSafeBlockCheck));
				break;
			}
			Add(pc = new PlayerCollider(OnCollide));
			Add(new StaticMover
			{
				OnShake = OnShake,
				SolidChecker = IsRiding,
				JumpThruChecker = IsRiding
			});
			Add(new DustEdge(RenderSpikes));
			base.Depth = -50;
		}

		public TriggerSpikes(EntityData data, Vector2 offset, Directions dir)
			: this(data.Position + offset, GetSize(data, dir), dir)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Vector3[] edgeColors = DustStyles.Get(scene).EdgeColors;
			dustTextures = GFX.Game.GetAtlasSubtextures("danger/dustcreature/base");
			tentacleColors = new Color[edgeColors.Length];
			for (int j = 0; j < tentacleColors.Length; j++)
			{
				tentacleColors[j] = Color.Lerp(new Color(edgeColors[j]), Color.DarkSlateBlue, 0.4f);
			}
			Vector2 perp = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
			spikes = new SpikeInfo[size / 4];
			for (int i = 0; i < spikes.Length; i++)
			{
				spikes[i].Parent = this;
				spikes[i].Index = i;
				spikes[i].WorldPosition = Position + perp * (2 + i * 4);
				spikes[i].ParticleTimerOffset = Calc.Random.NextFloat(0.25f);
				spikes[i].TextureIndex = Calc.Random.Next(dustTextures.Count);
				spikes[i].DustOutDistance = Calc.Random.Choose(3, 4, 6);
				spikes[i].TentacleColor = Calc.Random.Next(tentacleColors.Length);
				spikes[i].TentacleFrame = Calc.Random.NextFloat(tentacleTextures.Count);
			}
		}

		private void OnShake(Vector2 amount)
		{
			shakeOffset += amount;
		}

		private bool UpSafeBlockCheck(Player player)
		{
			int add = 8 * (int)player.Facing;
			int min = (int)((player.Left + (float)add - base.Left) / 4f);
			int max = (int)((player.Right + (float)add - base.Left) / 4f);
			if (max < 0 || min >= spikes.Length)
			{
				return false;
			}
			min = Math.Max(min, 0);
			max = Math.Min(max, spikes.Length - 1);
			for (int i = min; i <= max; i++)
			{
				if (spikes[i].Lerp >= 1f)
				{
					return true;
				}
			}
			return false;
		}

		private bool SideSafeBlockCheck(Player player)
		{
			int min = (int)((player.Top - base.Top) / 4f);
			int max = (int)((player.Bottom - base.Top) / 4f);
			if (max < 0 || min >= spikes.Length)
			{
				return false;
			}
			min = Math.Max(min, 0);
			max = Math.Min(max, spikes.Length - 1);
			for (int i = min; i <= max; i++)
			{
				if (spikes[i].Lerp >= 1f)
				{
					return true;
				}
			}
			return false;
		}

		private void OnCollide(Player player)
		{
			GetPlayerCollideIndex(player, out var min, out var max);
			if (max >= 0 && min < spikes.Length)
			{
				min = Math.Max(min, 0);
				max = Math.Min(max, spikes.Length - 1);
				for (int i = min; i <= max && !spikes[i].OnPlayer(player, outwards); i++)
				{
				}
			}
		}

		private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex)
		{
			minIndex = (maxIndex = -1);
			switch (direction)
			{
			case Directions.Up:
				if (player.Speed.Y >= 0f)
				{
					minIndex = (int)((player.Left - base.Left) / 4f);
					maxIndex = (int)((player.Right - base.Left) / 4f);
				}
				break;
			case Directions.Down:
				if (player.Speed.Y <= 0f)
				{
					minIndex = (int)((player.Left - base.Left) / 4f);
					maxIndex = (int)((player.Right - base.Left) / 4f);
				}
				break;
			case Directions.Left:
				if (player.Speed.X >= 0f)
				{
					minIndex = (int)((player.Top - base.Top) / 4f);
					maxIndex = (int)((player.Bottom - base.Top) / 4f);
				}
				break;
			case Directions.Right:
				if (player.Speed.X <= 0f)
				{
					minIndex = (int)((player.Top - base.Top) / 4f);
					maxIndex = (int)((player.Bottom - base.Top) / 4f);
				}
				break;
			}
		}

		private bool PlayerCheck(int spikeIndex)
		{
			Player player = CollideFirst<Player>();
			if (player != null)
			{
				GetPlayerCollideIndex(player, out var min, out var max);
				if (min <= spikeIndex + 1)
				{
					return max >= spikeIndex - 1;
				}
				return false;
			}
			return false;
		}

		private static int GetSize(EntityData data, Directions dir)
		{
			if ((uint)dir > 1u)
			{
				_ = dir - 2;
				_ = 1;
				return data.Height;
			}
			return data.Width;
		}

		public override void Update()
		{
			base.Update();
			for (int i = 0; i < spikes.Length; i++)
			{
				spikes[i].Update();
			}
		}

		public override void Render()
		{
			base.Render();
			Vector2 perp = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
			int frames = tentacleTextures.Count;
			Vector2 scale = Vector2.One;
			Vector2 justify = new Vector2(0f, 0.5f);
			if (direction == Directions.Left)
			{
				scale.X = -1f;
			}
			else if (direction == Directions.Up)
			{
				scale.Y = -1f;
			}
			if (direction == Directions.Up || direction == Directions.Down)
			{
				justify = new Vector2(0.5f, 0f);
			}
			for (int i = 0; i < spikes.Length; i++)
			{
				if (!spikes[i].Triggered)
				{
					MTexture mTexture = tentacleTextures[(int)(spikes[i].TentacleFrame % (float)frames)];
					Vector2 pos = Position + perp * (2 + i * 4);
					mTexture.DrawJustified(pos + perp, justify, Color.Black, scale, 0f);
					mTexture.DrawJustified(pos, justify, tentacleColors[spikes[i].TentacleColor], scale, 0f);
				}
			}
			RenderSpikes();
		}

		private void RenderSpikes()
		{
			Vector2 perp = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
			for (int i = 0; i < spikes.Length; i++)
			{
				if (spikes[i].Triggered)
				{
					MTexture mTexture = dustTextures[spikes[i].TextureIndex];
					Vector2 pos = Position + outwards * (-4f + spikes[i].Lerp * (float)spikes[i].DustOutDistance) + perp * (2 + i * 4);
					mTexture.DrawCentered(pos, Color.White, 0.5f * spikes[i].Lerp, spikes[i].TextureRotation);
				}
			}
		}

		private bool IsRiding(Solid solid)
		{
			return direction switch
			{
				Directions.Up => CollideCheckOutside(solid, Position + Vector2.UnitY), 
				Directions.Down => CollideCheckOutside(solid, Position - Vector2.UnitY), 
				Directions.Left => CollideCheckOutside(solid, Position + Vector2.UnitX), 
				Directions.Right => CollideCheckOutside(solid, Position - Vector2.UnitX), 
				_ => false, 
			};
		}

		private bool IsRiding(JumpThru jumpThru)
		{
			if (direction != 0)
			{
				return false;
			}
			return CollideCheck(jumpThru, Position + Vector2.UnitY);
		}
	}
}
