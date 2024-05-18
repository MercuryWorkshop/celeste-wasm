using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ReflectionTentacles : Entity
	{
		private struct Tentacle
		{
			public Vector2 Position;

			public float Width;

			public float Length;

			public float Approach;

			public float WaveOffset;

			public int TexIndex;

			public int FillerTexIndex;

			public Vector2 LerpPositionFrom;

			public float LerpPercent;

			public float LerpDuration;
		}

		public int Index;

		public List<Vector2> Nodes = new List<Vector2>();

		private Vector2 outwards;

		private Vector2 lastOutwards;

		private float ease;

		private Vector2 p;

		private Player player;

		private float fearDistance;

		private float offset;

		private bool createdFromLevel;

		private int slideUntilIndex;

		private int layer;

		private const int NodesPerTentacle = 10;

		private Tentacle[] tentacles;

		private int tentacleCount;

		private VertexPositionColorTexture[] vertices;

		private int vertexCount;

		private Color color = Color.Purple;

		private float soundDelay = 0.25f;

		private List<MTexture[]> arms = new List<MTexture[]>();

		private List<MTexture> fillers;

		public ReflectionTentacles()
		{
		}

		public ReflectionTentacles(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Nodes.Add(Position);
			Vector2[] nodes = data.Nodes;
			foreach (Vector2 node in nodes)
			{
				Nodes.Add(offset + node);
			}
			switch (data.Attr("fear_distance"))
			{
			case "close":
				fearDistance = 16f;
				break;
			case "medium":
				fearDistance = 40f;
				break;
			case "far":
				fearDistance = 80f;
				break;
			}
			int slideUntilIndex = data.Int("slide_until");
			Create(fearDistance, slideUntilIndex, 0, Nodes);
			createdFromLevel = true;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (createdFromLevel)
			{
				for (int i = 1; i < 4; i++)
				{
					ReflectionTentacles t = new ReflectionTentacles();
					t.Create(fearDistance, slideUntilIndex, i, Nodes);
					scene.Add(t);
				}
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			bool retreated = false;
			while (player != null && Index < Nodes.Count - 1)
			{
				Vector2 closest = (p = Calc.ClosestPointOnLine(Nodes[Index], Nodes[Index] + outwards * 10000f, player.Center));
				if (!((Nodes[Index] - closest).Length() < fearDistance))
				{
					break;
				}
				retreated = true;
				Retreat();
			}
			if (retreated)
			{
				ease = 1f;
				SnapTentacles();
			}
		}

		public void Create(float fearDistance, int slideUntilIndex, int layer, List<Vector2> startNodes)
		{
			Nodes = new List<Vector2>();
			foreach (Vector2 node in startNodes)
			{
				Nodes.Add(node + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-8, 8)));
			}
			base.Tag = Tags.TransitionUpdate;
			Position = Nodes[0];
			outwards = (Nodes[0] - Nodes[1]).SafeNormalize();
			this.fearDistance = fearDistance;
			this.slideUntilIndex = slideUntilIndex;
			this.layer = layer;
			switch (layer)
			{
			case 0:
				base.Depth = -1000000;
				color = Calc.HexToColor("3f2a4f");
				offset = 110f;
				break;
			case 1:
				base.Depth = 8990;
				color = Calc.HexToColor("7b3555");
				offset = 80f;
				break;
			case 2:
				base.Depth = 10010;
				color = Calc.HexToColor("662847");
				offset = 50f;
				break;
			case 3:
				base.Depth = 10011;
				color = Calc.HexToColor("492632");
				offset = 20f;
				break;
			}
			foreach (MTexture tex in GFX.Game.GetAtlasSubtextures("scenery/tentacles/arms"))
			{
				MTexture[] splits = new MTexture[10];
				int split = tex.Width / 10;
				for (int k = 0; k < 10; k++)
				{
					splits[k] = tex.GetSubtexture(split * (10 - k - 1), 0, split, tex.Height);
				}
				arms.Add(splits);
			}
			fillers = GFX.Game.GetAtlasSubtextures("scenery/tentacles/filler");
			tentacles = new Tentacle[100];
			float w = 0f;
			int j = 0;
			while (j < tentacles.Length && w < 440f)
			{
				tentacles[j].Approach = 0.25f + Calc.Random.NextFloat() * 0.75f;
				tentacles[j].Length = 32f + Calc.Random.NextFloat(64f);
				tentacles[j].Width = 4f + Calc.Random.NextFloat(16f);
				tentacles[j].Position = TargetTentaclePosition(tentacles[j], Nodes[0], w);
				tentacles[j].WaveOffset = Calc.Random.NextFloat();
				tentacles[j].TexIndex = Calc.Random.Next(arms.Count);
				tentacles[j].FillerTexIndex = Calc.Random.Next(fillers.Count);
				tentacles[j].LerpDuration = 0.5f + Calc.Random.NextFloat(0.25f);
				w += tentacles[j].Width;
				j++;
				tentacleCount++;
			}
			vertices = new VertexPositionColorTexture[tentacleCount * 12 * 6];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].Color = color;
			}
		}

		private Vector2 TargetTentaclePosition(Tentacle tentacle, Vector2 position, float along)
		{
			Vector2 center;
			Vector2 vector = (center = position - outwards * offset);
			if (player != null)
			{
				Vector2 perp = outwards.Perpendicular();
				center = Calc.ClosestPointOnLine(center - perp * 200f, center + perp * 200f, player.Position);
			}
			Vector2 target = vector + outwards.Perpendicular() * (-220f + along + tentacle.Width * 0.5f);
			float dist = (center - target).Length();
			return target + outwards * dist * 0.6f;
		}

		public void Retreat()
		{
			if (Index < Nodes.Count - 1)
			{
				lastOutwards = outwards;
				ease = 0f;
				Index++;
				if (layer == 0 && soundDelay <= 0f)
				{
					Audio.Play(((Nodes[Index - 1] - Nodes[Index]).Length() > 180f) ? "event:/game/06_reflection/scaryhair_whoosh" : "event:/game/06_reflection/scaryhair_move");
				}
				for (int i = 0; i < tentacleCount; i++)
				{
					tentacles[i].LerpPercent = 0f;
					tentacles[i].LerpPositionFrom = tentacles[i].Position;
				}
			}
		}

		public override void Update()
		{
			soundDelay -= Engine.DeltaTime;
			if (slideUntilIndex > Index)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					Vector2 closest2 = (p = Calc.ClosestPointOnLine(Nodes[Index] - outwards * 10000f, Nodes[Index] + outwards * 10000f, player.Center));
					if ((closest2 - Nodes[Index]).Length() < 32f)
					{
						Retreat();
						outwards = (Nodes[Index - 1] - Nodes[Index]).SafeNormalize();
					}
					else
					{
						MoveTentacles(closest2 - outwards * 190f);
					}
				}
			}
			else
			{
				FinalBoss entity = base.Scene.Tracker.GetEntity<FinalBoss>();
				player = base.Scene.Tracker.GetEntity<Player>();
				if (entity == null && player != null && Index < Nodes.Count - 1)
				{
					Vector2 closest = (p = Calc.ClosestPointOnLine(Nodes[Index], Nodes[Index] + outwards * 10000f, player.Center));
					if ((Nodes[Index] - closest).Length() < fearDistance)
					{
						Retreat();
					}
				}
				if (Index > 0)
				{
					ease = Calc.Approach(ease, 1f, (float)((Index != Nodes.Count - 1) ? 1 : 2) * Engine.DeltaTime);
					outwards = Calc.AngleToVector(Calc.AngleLerp(lastOutwards.Angle(), (Nodes[Index - 1] - Nodes[Index]).Angle(), Ease.QuadOut(ease)), 1f);
					float w = 0f;
					for (int j = 0; j < tentacleCount; j++)
					{
						Vector2 target = TargetTentaclePosition(tentacles[j], Nodes[Index], w);
						if (tentacles[j].LerpPercent < 1f)
						{
							tentacles[j].LerpPercent += Engine.DeltaTime / tentacles[j].LerpDuration;
							tentacles[j].Position = Vector2.Lerp(tentacles[j].LerpPositionFrom, target, Ease.CubeInOut(tentacles[j].LerpPercent));
						}
						else
						{
							tentacles[j].Position += (target - tentacles[j].Position) * (1f - (float)Math.Pow(0.1f * tentacles[j].Approach, Engine.DeltaTime));
						}
						w += tentacles[j].Width;
					}
				}
				else
				{
					MoveTentacles(Nodes[Index]);
				}
			}
			if (Index == Nodes.Count - 1)
			{
				Color lerp = color * (1f - ease);
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i].Color = lerp;
				}
			}
			UpdateVertices();
		}

		private void MoveTentacles(Vector2 pos)
		{
			float w = 0f;
			for (int i = 0; i < tentacleCount; i++)
			{
				Vector2 target = TargetTentaclePosition(tentacles[i], pos, w);
				tentacles[i].Position += (target - tentacles[i].Position) * (1f - (float)Math.Pow(0.1f * tentacles[i].Approach, Engine.DeltaTime));
				w += tentacles[i].Width;
			}
		}

		public void SnapTentacles()
		{
			float w = 0f;
			for (int i = 0; i < tentacleCount; i++)
			{
				tentacles[i].LerpPercent = 1f;
				tentacles[i].Position = TargetTentaclePosition(tentacles[i], Nodes[Index], w);
				w += tentacles[i].Width;
			}
		}

		private void UpdateVertices()
		{
			Vector2 perp = -outwards.Perpendicular();
			int v = 0;
			for (int i = 0; i < tentacleCount; i++)
			{
				Vector2 start = tentacles[i].Position;
				Vector2 width = perp * (tentacles[i].Width * 0.5f + 2f);
				MTexture[] tex = arms[tentacles[i].TexIndex];
				Quad(ref v, start + width, start + width * 1.5f - outwards * 240f, start - width * 1.5f - outwards * 240f, start - width, fillers[tentacles[i].FillerTexIndex]);
				Vector2 last = start;
				Vector2 lastSize = width;
				float step = tentacles[i].Length / 10f;
				step += Calc.YoYo(tentacles[i].LerpPercent) * 6f;
				for (int j = 1; j <= 10; j++)
				{
					float percent = (float)j / 10f;
					float waveSpeed = base.Scene.TimeActive * tentacles[i].WaveOffset * (float)Math.Pow(1.100000023841858, j) * 2f;
					float waveOffset = tentacles[i].WaveOffset * 3f + (float)j * 0.05f;
					float waveAmp = 2f + 4f * percent;
					Vector2 wave = perp * (float)Math.Sin(waveSpeed + waveOffset) * waveAmp;
					Vector2 next = last + outwards * step + wave;
					Vector2 nextSize = width * (1f - percent);
					Quad(ref v, next - nextSize, last - lastSize, last + lastSize, next + nextSize, tex[j - 1]);
					last = next;
					lastSize = nextSize;
				}
			}
			vertexCount = v;
		}

		private void Quad(ref int n, Vector2 a, Vector2 b, Vector2 c, Vector2 d, MTexture subtexture = null)
		{
			if (subtexture == null)
			{
				subtexture = GFX.Game["util/pixel"];
			}
			float px = 1f / (float)subtexture.Texture.Texture.Width;
			float py = 1f / (float)subtexture.Texture.Texture.Height;
			Vector2 topleft = new Vector2((float)subtexture.ClipRect.Left * px, (float)subtexture.ClipRect.Top * py);
			Vector2 topright = new Vector2((float)subtexture.ClipRect.Right * px, (float)subtexture.ClipRect.Top * py);
			Vector2 botleft = new Vector2((float)subtexture.ClipRect.Left * px, (float)subtexture.ClipRect.Bottom * py);
			Vector2 botright = new Vector2((float)subtexture.ClipRect.Right * px, (float)subtexture.ClipRect.Bottom * py);
			vertices[n].Position = new Vector3(a, 0f);
			vertices[n++].TextureCoordinate = topleft;
			vertices[n].Position = new Vector3(b, 0f);
			vertices[n++].TextureCoordinate = topright;
			vertices[n].Position = new Vector3(d, 0f);
			vertices[n++].TextureCoordinate = botleft;
			vertices[n].Position = new Vector3(d, 0f);
			vertices[n++].TextureCoordinate = botleft;
			vertices[n].Position = new Vector3(b, 0f);
			vertices[n++].TextureCoordinate = topright;
			vertices[n].Position = new Vector3(c, 0f);
			vertices[n++].TextureCoordinate = botright;
		}

		public override void Render()
		{
			if (vertexCount > 0)
			{
				GameplayRenderer.End();
				Engine.Graphics.GraphicsDevice.Textures[0] = arms[0][0].Texture.Texture;
				GFX.DrawVertices((base.Scene as Level).Camera.Matrix, vertices, vertexCount, GFX.FxTexture);
				GameplayRenderer.Begin();
			}
		}
	}
}
