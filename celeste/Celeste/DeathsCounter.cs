using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DeathsCounter : Component
	{
		private const int IconWidth = 60;

		public Vector2 Position;

		public bool CenteredX;

		public bool CanWiggle = true;

		public float Alpha = 1f;

		public float Scale = 1f;

		public float Stroke = 2f;

		public Color Color = Color.White;

		private int amount;

		private int minDigits;

		private Wiggler wiggler;

		private Wiggler iconWiggler;

		private float flashTimer;

		private string sAmount;

		private MTexture icon;

		private MTexture x;

		public int Amount
		{
			get
			{
				return amount;
			}
			set
			{
				if (amount != value)
				{
					amount = value;
					UpdateString();
					if (CanWiggle)
					{
						wiggler.Start();
						flashTimer = 0.5f;
					}
				}
			}
		}

		public int MinDigits
		{
			get
			{
				return minDigits;
			}
			set
			{
				if (minDigits != value)
				{
					minDigits = value;
					UpdateString();
				}
			}
		}

		public Vector2 RenderPosition => (((base.Entity != null) ? base.Entity.Position : Vector2.Zero) + Position).Round();

		public DeathsCounter(AreaMode mode, bool centeredX, int amount, int minDigits = 0)
			: base(active: true, visible: true)
		{
			CenteredX = centeredX;
			this.amount = amount;
			this.minDigits = minDigits;
			UpdateString();
			wiggler = Wiggler.Create(0.5f, 3f);
			wiggler.StartZero = true;
			wiggler.UseRawDeltaTime = true;
			iconWiggler = Wiggler.Create(0.5f, 3f);
			iconWiggler.UseRawDeltaTime = true;
			SetMode(mode);
			x = GFX.Gui["x"];
		}

		private void UpdateString()
		{
			if (minDigits > 0)
			{
				sAmount = amount.ToString("D" + minDigits);
			}
			else
			{
				sAmount = amount.ToString();
			}
		}

		public void SetMode(AreaMode mode)
		{
			switch (mode)
			{
			case AreaMode.Normal:
				icon = GFX.Gui["collectables/skullBlue"];
				break;
			case AreaMode.BSide:
				icon = GFX.Gui["collectables/skullRed"];
				break;
			default:
				icon = GFX.Gui["collectables/skullGold"];
				break;
			}
			iconWiggler.Start();
		}

		public void Wiggle()
		{
			wiggler.Start();
			flashTimer = 0.5f;
		}

		public override void Update()
		{
			base.Update();
			if (wiggler.Active)
			{
				wiggler.Update();
			}
			if (iconWiggler.Active)
			{
				iconWiggler.Update();
			}
			if (flashTimer > 0f)
			{
				flashTimer -= Engine.RawDeltaTime;
			}
		}

		public override void Render()
		{
			Vector2 pos = RenderPosition;
			float tw = ActiveFont.Measure(sAmount).X;
			float width = 62f + (float)x.Width + 2f + tw;
			Color color = Color;
			Color outline = Color.Black;
			if (flashTimer > 0f && base.Scene != null && base.Scene.BetweenRawInterval(0.05f))
			{
				color = StrawberriesCounter.FlashColor;
			}
			if (Alpha < 1f)
			{
				color *= Alpha;
				outline *= Alpha;
			}
			if (CenteredX)
			{
				pos -= Vector2.UnitX * (width / 2f) * Scale;
			}
			icon.DrawCentered(pos + new Vector2(30f, 0f) * Scale, Color.White * Alpha, Scale * (1f + iconWiggler.Value * 0.2f));
			x.DrawCentered(pos + new Vector2(62f + (float)(x.Width / 2), 2f) * Scale, color, Scale);
			ActiveFont.DrawOutline(sAmount, pos + new Vector2(width - tw / 2f, (0f - wiggler.Value) * 18f) * Scale, new Vector2(0.5f, 0.5f), Vector2.One * Scale, color, Stroke, outline);
		}
	}
}
