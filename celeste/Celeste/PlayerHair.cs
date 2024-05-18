using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PlayerHair : Component
	{
		public const string Hair = "characters/player/hair00";

		public Color Color = Player.NormalHairColor;

		public Color Border = Color.Black;

		public float Alpha = 1f;

		public Facings Facing;

		public bool DrawPlayerSpriteOutline;

		public bool SimulateMotion = true;

		public Vector2 StepPerSegment = new Vector2(0f, 2f);

		public float StepInFacingPerSegment = 0.5f;

		public float StepApproach = 64f;

		public float StepYSinePerSegment;

		public PlayerSprite Sprite;

		public List<Vector2> Nodes = new List<Vector2>();

		private List<MTexture> bangs = GFX.Game.GetAtlasSubtextures("characters/player/bangs");

		private float wave;

		public PlayerHair(PlayerSprite sprite)
			: base(active: true, visible: true)
		{
			Sprite = sprite;
			for (int i = 0; i < sprite.HairCount; i++)
			{
				Nodes.Add(Vector2.Zero);
			}
		}

		public void Start()
		{
			Vector2 at = base.Entity.Position + new Vector2((0 - Facing) * 200, 200f);
			for (int i = 0; i < Nodes.Count; i++)
			{
				Nodes[i] = at;
			}
		}

		public void AfterUpdate()
		{
			Vector2 offset = Sprite.HairOffset * new Vector2((float)Facing, 1f);
			Nodes[0] = Sprite.RenderPosition + new Vector2(0f, -9f * Sprite.Scale.Y) + offset;
			Vector2 target = Nodes[0] + new Vector2((float)(0 - Facing) * StepInFacingPerSegment * 2f, (float)Math.Sin(wave) * StepYSinePerSegment) + StepPerSegment;
			Vector2 prev = Nodes[0];
			float maxdist = 3f;
			for (int i = 1; i < Sprite.HairCount; i++)
			{
				if (i >= Nodes.Count)
				{
					Nodes.Add(Nodes[i - 1]);
				}
				if (SimulateMotion)
				{
					float approach = (1f - (float)i / (float)Sprite.HairCount * 0.5f) * StepApproach;
					Nodes[i] = Calc.Approach(Nodes[i], target, approach * Engine.DeltaTime);
				}
				if ((Nodes[i] - prev).Length() > maxdist)
				{
					Nodes[i] = prev + (Nodes[i] - prev).SafeNormalize() * maxdist;
				}
				target = Nodes[i] + new Vector2((float)(0 - Facing) * StepInFacingPerSegment, (float)Math.Sin(wave + (float)i * 0.8f) * StepYSinePerSegment) + StepPerSegment;
				prev = Nodes[i];
			}
		}

		public override void Update()
		{
			wave += Engine.DeltaTime * 4f;
			base.Update();
		}

		public void MoveHairBy(Vector2 amount)
		{
			for (int i = 0; i < Nodes.Count; i++)
			{
				Nodes[i] += amount;
			}
		}

		public override void Render()
		{
			if (!Sprite.HasHair)
			{
				return;
			}
			Vector2 origin = new Vector2(5f, 5f);
			Color borderColor = Border * Alpha;
			Color mainColor = Color * Alpha;
			if (DrawPlayerSpriteOutline)
			{
				Color col = Sprite.Color;
				Vector2 pos = Sprite.Position;
				Sprite.Color = borderColor;
				Sprite.Position = pos + new Vector2(0f, -1f);
				Sprite.Render();
				Sprite.Position = pos + new Vector2(0f, 1f);
				Sprite.Render();
				Sprite.Position = pos + new Vector2(-1f, 0f);
				Sprite.Render();
				Sprite.Position = pos + new Vector2(1f, 0f);
				Sprite.Render();
				Sprite.Color = col;
				Sprite.Position = pos;
			}
			Nodes[0] = Nodes[0].Floor();
			if (borderColor.A > 0)
			{
				for (int j = 0; j < Sprite.HairCount; j++)
				{
					int hairIndex2 = Sprite.HairFrame;
					MTexture obj = ((j == 0) ? bangs[hairIndex2] : GFX.Game["characters/player/hair00"]);
					Vector2 scale = GetHairScale(j);
					obj.Draw(Nodes[j] + new Vector2(-1f, 0f), origin, borderColor, scale);
					obj.Draw(Nodes[j] + new Vector2(1f, 0f), origin, borderColor, scale);
					obj.Draw(Nodes[j] + new Vector2(0f, -1f), origin, borderColor, scale);
					obj.Draw(Nodes[j] + new Vector2(0f, 1f), origin, borderColor, scale);
				}
			}
			for (int i = Sprite.HairCount - 1; i >= 0; i--)
			{
				int hairIndex = Sprite.HairFrame;
				((i == 0) ? bangs[hairIndex] : GFX.Game["characters/player/hair00"]).Draw(Nodes[i], origin, mainColor, GetHairScale(i));
			}
		}

		private Vector2 GetHairScale(int index)
		{
			float scale = 0.25f + (1f - (float)index / (float)Sprite.HairCount) * 0.75f;
			return new Vector2(((index == 0) ? ((float)Facing) : scale) * Math.Abs(Sprite.Scale.X), scale);
		}
	}
}
