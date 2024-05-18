using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DustGraphic : Component
	{
		public class Node
		{
			public MTexture Base;

			public MTexture Overlay;

			public float Rotation;

			public Vector2 Angle;

			public bool Enabled;
		}

		private class Eyeballs : Entity
		{
			public DustGraphic Dust;

			public Color Color;

			public Eyeballs(DustGraphic dust)
			{
				Dust = dust;
				base.Depth = Dust.Entity.Depth - 1;
			}

			public override void Added(Scene scene)
			{
				base.Added(scene);
				Color = DustStyles.Get(scene).EyeColor;
			}

			public override void Update()
			{
				base.Update();
				if (Dust.Entity == null || Dust.Scene == null)
				{
					RemoveSelf();
				}
			}

			public override void Render()
			{
				if (Dust.Visible && Dust.Entity.Visible)
				{
					Vector2 perp = new Vector2(0f - Dust.EyeDirection.Y, Dust.EyeDirection.X).SafeNormalize();
					if (Dust.leftEyeVisible)
					{
						Dust.eyeTexture.DrawCentered(Dust.RenderPosition + (Dust.EyeDirection * 5f + perp * 3f) * Dust.Scale, Color, Dust.Scale);
					}
					if (Dust.rightEyeVisible)
					{
						Dust.eyeTexture.DrawCentered(Dust.RenderPosition + (Dust.EyeDirection * 5f - perp * 3f) * Dust.Scale, Color, Dust.Scale);
					}
				}
			}
		}

		public Vector2 Position;

		public float Scale = 1f;

		private MTexture center;

		public Action OnEstablish;

		private List<Node> nodes = new List<Node>();

		public List<Node> LeftNodes = new List<Node>();

		public List<Node> RightNodes = new List<Node>();

		public List<Node> TopNodes = new List<Node>();

		public List<Node> BottomNodes = new List<Node>();

		public Vector2 EyeTargetDirection;

		public Vector2 EyeDirection;

		public int EyeFlip = 1;

		private bool eyesExist;

		private int eyeTextureIndex;

		private MTexture eyeTexture;

		private Vector2 eyeLookRange;

		private bool eyesMoveByRotation;

		private bool autoControlEyes;

		private bool eyesFollowPlayer;

		private Coroutine blink;

		private bool leftEyeVisible = true;

		private bool rightEyeVisible = true;

		private Eyeballs eyes;

		private float timer;

		private float offset;

		private bool ignoreSolids;

		private bool autoExpandDust;

		private float shakeTimer;

		private Vector2 shakeValue;

		private int randomSeed;

		public bool Estableshed { get; private set; }

		public Vector2 RenderPosition => base.Entity.Position + Position + shakeValue;

		private bool InView
		{
			get
			{
				Camera cam = (base.Scene as Level).Camera;
				Vector2 pos = base.Entity.Position;
				if (!(pos.X + 16f < cam.Left) && !(pos.Y + 16f < cam.Top) && !(pos.X - 16f > cam.Right))
				{
					return !(pos.Y - 16f > cam.Bottom);
				}
				return false;
			}
		}

		public DustGraphic(bool ignoreSolids, bool autoControlEyes = false, bool autoExpandDust = false)
			: base(active: true, visible: true)
		{
			this.ignoreSolids = ignoreSolids;
			this.autoControlEyes = autoControlEyes;
			this.autoExpandDust = autoExpandDust;
			center = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("danger/dustcreature/center"));
			offset = Calc.Random.NextFloat() * 4f;
			timer = Calc.Random.NextFloat();
			EyeTargetDirection = (EyeDirection = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f));
			eyeTextureIndex = Calc.Random.Next(128);
			eyesExist = true;
			if (autoControlEyes)
			{
				eyesExist = Calc.Random.Chance(0.5f);
				eyesFollowPlayer = Calc.Random.Chance(0.3f);
			}
			randomSeed = Calc.Random.Next();
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			TransitionListener listener = new TransitionListener();
			listener.OnIn = delegate
			{
				AddDustNodesIfInCamera();
			};
			entity.Add(listener);
			entity.Add(new DustEdge(Render));
		}

		public override void Update()
		{
			timer += Engine.DeltaTime * 0.6f;
			bool visible = InView;
			if (shakeTimer > 0f)
			{
				shakeTimer -= Engine.DeltaTime;
				if (shakeTimer <= 0f)
				{
					shakeValue = Vector2.Zero;
				}
				else if (base.Scene.OnInterval(0.05f))
				{
					shakeValue = Calc.Random.ShakeVector();
				}
			}
			if (eyesExist)
			{
				if (EyeDirection != EyeTargetDirection && visible)
				{
					if (!eyesMoveByRotation)
					{
						EyeDirection = Calc.Approach(EyeDirection, EyeTargetDirection, 12f * Engine.DeltaTime);
					}
					else
					{
						float angle2 = EyeDirection.Angle();
						float target = EyeTargetDirection.Angle();
						angle2 = Calc.AngleApproach(angle2, target, 8f * Engine.DeltaTime);
						if (angle2 == target)
						{
							EyeDirection = EyeTargetDirection;
						}
						else
						{
							EyeDirection = Calc.AngleToVector(angle2, 1f);
						}
					}
				}
				if (eyesFollowPlayer && visible)
				{
					Player player = base.Entity.Scene.Tracker.GetEntity<Player>();
					if (player != null)
					{
						Vector2 dir = (player.Position - base.Entity.Position).SafeNormalize();
						if (eyesMoveByRotation)
						{
							float angle = dir.Angle();
							float from = eyeLookRange.Angle();
							EyeTargetDirection = Calc.AngleToVector(Calc.AngleApproach(from, angle, (float)Math.PI / 4f), 1f);
						}
						else
						{
							EyeTargetDirection = dir;
						}
					}
				}
				if (blink != null)
				{
					blink.Update();
				}
			}
			if (nodes.Count <= 0 && base.Entity.Scene != null && !Estableshed)
			{
				AddDustNodesIfInCamera();
				return;
			}
			foreach (Node node in nodes)
			{
				node.Rotation += Engine.DeltaTime * 0.5f;
			}
		}

		public void OnHitPlayer()
		{
			if (!SaveData.Instance.Assists.Invincible)
			{
				shakeTimer = 0.6f;
				if (eyesExist)
				{
					blink = null;
					leftEyeVisible = true;
					rightEyeVisible = true;
					eyeTexture = GFX.Game["danger/dustcreature/deadEyes"];
				}
			}
		}

		public void AddDustNodesIfInCamera()
		{
			if (nodes.Count > 0 || !InView || DustEdges.DustGraphicEstabledCounter > 25 || Estableshed)
			{
				return;
			}
			Calc.PushRandom(randomSeed);
			int ox = (int)base.Entity.X;
			int oy = (int)base.Entity.Y;
			Vector2 dir = new Vector2(1f, 1f).SafeNormalize();
			AddNode(new Vector2(0f - dir.X, 0f - dir.Y), ignoreSolids || !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(ox - 8, oy - 8, 8, 8)));
			AddNode(new Vector2(dir.X, 0f - dir.Y), ignoreSolids || !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(ox, oy - 8, 8, 8)));
			AddNode(new Vector2(0f - dir.X, dir.Y), ignoreSolids || !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(ox - 8, oy, 8, 8)));
			AddNode(new Vector2(dir.X, dir.Y), ignoreSolids || !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(ox, oy, 8, 8)));
			if (nodes[0].Enabled || nodes[2].Enabled)
			{
				Position.X -= 1f;
			}
			if (nodes[1].Enabled || nodes[3].Enabled)
			{
				Position.X += 1f;
			}
			if (nodes[0].Enabled || nodes[1].Enabled)
			{
				Position.Y -= 1f;
			}
			if (nodes[2].Enabled || nodes[3].Enabled)
			{
				Position.Y += 1f;
			}
			int count = 0;
			foreach (Node node in nodes)
			{
				if (node.Enabled)
				{
					count++;
				}
			}
			eyesMoveByRotation = count < 4;
			if (autoControlEyes && eyesExist && eyesMoveByRotation)
			{
				eyeLookRange = Vector2.Zero;
				if (nodes[0].Enabled)
				{
					eyeLookRange += new Vector2(-1f, -1f).SafeNormalize();
				}
				if (nodes[1].Enabled)
				{
					eyeLookRange += new Vector2(1f, -1f).SafeNormalize();
				}
				if (nodes[2].Enabled)
				{
					eyeLookRange += new Vector2(-1f, 1f).SafeNormalize();
				}
				if (nodes[3].Enabled)
				{
					eyeLookRange += new Vector2(1f, 1f).SafeNormalize();
				}
				if (count > 0 && eyeLookRange.Length() > 0f)
				{
					eyeLookRange /= (float)count;
					eyeLookRange = eyeLookRange.SafeNormalize();
				}
				EyeTargetDirection = (EyeDirection = eyeLookRange);
			}
			if (eyesExist)
			{
				blink = new Coroutine(BlinkRoutine());
				List<MTexture> textures = GFX.Game.GetAtlasSubtextures(DustStyles.Get(base.Scene).EyeTextures);
				eyeTexture = textures[eyeTextureIndex % textures.Count];
				base.Entity.Scene.Add(eyes = new Eyeballs(this));
			}
			DustEdges.DustGraphicEstabledCounter++;
			Estableshed = true;
			if (OnEstablish != null)
			{
				OnEstablish();
			}
			Calc.PopRandom();
		}

		private void AddNode(Vector2 angle, bool enabled)
		{
			Vector2 dist = new Vector2(1f, 1f);
			if (autoExpandDust)
			{
				int dx = Math.Sign(angle.X);
				int dy = Math.Sign(angle.Y);
				base.Entity.Collidable = false;
				if (base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.Entity.X - 4f + (float)(dx * 16)), (int)(base.Entity.Y - 4f + (float)(dy * 4)), 8, 8)) || base.Scene.CollideCheck<DustStaticSpinner>(new Rectangle((int)(base.Entity.X - 4f + (float)(dx * 16)), (int)(base.Entity.Y - 4f + (float)(dy * 4)), 8, 8)))
				{
					dist.X = 5f;
				}
				if (base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.Entity.X - 4f + (float)(dx * 4)), (int)(base.Entity.Y - 4f + (float)(dy * 16)), 8, 8)) || base.Scene.CollideCheck<DustStaticSpinner>(new Rectangle((int)(base.Entity.X - 4f + (float)(dx * 4)), (int)(base.Entity.Y - 4f + (float)(dy * 16)), 8, 8)))
				{
					dist.Y = 5f;
				}
				base.Entity.Collidable = true;
			}
			Node p = new Node();
			p.Base = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("danger/dustcreature/base"));
			p.Overlay = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("danger/dustcreature/overlay"));
			p.Rotation = Calc.Random.NextFloat((float)Math.PI * 2f);
			p.Angle = angle * dist;
			p.Enabled = enabled;
			nodes.Add(p);
			if (angle.X < 0f)
			{
				LeftNodes.Add(p);
			}
			else
			{
				RightNodes.Add(p);
			}
			if (angle.Y < 0f)
			{
				TopNodes.Add(p);
			}
			else
			{
				BottomNodes.Add(p);
			}
		}

		private IEnumerator BlinkRoutine()
		{
			while (true)
			{
				yield return 2f + Calc.Random.NextFloat(1.5f);
				leftEyeVisible = false;
				yield return 0.02f + Calc.Random.NextFloat(0.05f);
				rightEyeVisible = false;
				yield return 0.25f;
				leftEyeVisible = (rightEyeVisible = true);
			}
		}

		public override void Render()
		{
			if (!InView)
			{
				return;
			}
			Vector2 origin = RenderPosition;
			foreach (Node p in nodes)
			{
				if (p.Enabled)
				{
					p.Base.DrawCentered(origin + p.Angle * Scale, Color.White, Scale, p.Rotation);
					p.Overlay.DrawCentered(origin + p.Angle * Scale, Color.White, Scale, 0f - p.Rotation);
				}
			}
			center.DrawCentered(origin, Color.White, Scale, timer);
		}
	}
}
