using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CoreMessage : Entity
	{
		private string text;

		private float alpha;

		public CoreMessage(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Tag = Tags.HUD;
			string[] lines = Dialog.Clean("app_ending").Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			text = lines[data.Int("line")];
		}

		public override void Update()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				alpha = Ease.CubeInOut(Calc.ClampedMap(Math.Abs(base.X - player.X), 0f, 128f, 1f, 0f));
			}
			base.Update();
		}

		public override void Render()
		{
			Vector2 cam = (base.Scene as Level).Camera.Position;
			Vector2 camCenter = cam + new Vector2(160f, 90f);
			Vector2 pos = (Position - cam + (Position - camCenter) * 0.2f) * 6f;
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				pos.X = 1920f - pos.X;
			}
			ActiveFont.Draw(text, pos, new Vector2(0.5f, 0.5f), Vector2.One * 1.25f, Color.White * alpha);
		}
	}
}
