using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BestTimeDisplay : Component
	{
		public enum Modes
		{
			Best,
			BestFullClear,
			Current
		}

		private static readonly Color IconColor = Color.Lerp(Calc.HexToColor("7CFF70"), Color.Black, 0.25f);

		private static readonly Color FullClearColor = Color.Lerp(Calc.HexToColor("FF3D57"), Color.Black, 0.25f);

		public Vector2 Position;

		private TimeSpan time;

		private string sTime;

		private Wiggler wiggler;

		private MTexture icon;

		private float flashTimer;

		private Color iconColor;

		public TimeSpan Time
		{
			get
			{
				return time;
			}
			set
			{
				if (time != value)
				{
					time = value;
					UpdateString();
					wiggler.Start();
					flashTimer = 0.5f;
				}
			}
		}

		public Vector2 RenderPosition => (base.Entity.Position + Position).Round();

		public bool WillRender => time > TimeSpan.Zero;

		public BestTimeDisplay(Modes mode, TimeSpan time)
			: base(active: true, visible: true)
		{
			this.time = time;
			UpdateString();
			wiggler = Wiggler.Create(0.5f, 3f);
			wiggler.UseRawDeltaTime = true;
			switch (mode)
			{
			default:
				icon = GFX.Game["gui/bestTime"];
				iconColor = IconColor;
				break;
			case Modes.BestFullClear:
				icon = GFX.Game["gui/bestFullClearTime"];
				iconColor = FullClearColor;
				break;
			case Modes.Current:
				icon = null;
				break;
			}
		}

		private void UpdateString()
		{
			sTime = time.ShortGameplayFormat();
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
			if (flashTimer > 0f)
			{
				flashTimer -= Engine.RawDeltaTime;
			}
		}

		public override void Render()
		{
			if (WillRender)
			{
				Vector2 pos = RenderPosition;
				pos -= Vector2.UnitY * wiggler.Value * 3f;
				Color color = Color.White;
				if (flashTimer > 0f && base.Scene.BetweenRawInterval(0.05f))
				{
					color = StrawberriesCounter.FlashColor;
				}
				if (icon != null)
				{
					icon.DrawOutlineCentered(pos + new Vector2(-4f, -3f), iconColor);
				}
				ActiveFont.DrawOutline(sTime, pos + new Vector2(0f, 4f), new Vector2(0.5f, 0f), Vector2.One, color, 2f, Color.Black);
			}
		}
	}
}
