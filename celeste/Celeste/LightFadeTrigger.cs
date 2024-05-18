using Microsoft.Xna.Framework;

namespace Celeste
{
	public class LightFadeTrigger : Trigger
	{
		public float LightAddFrom;

		public float LightAddTo;

		public PositionModes PositionMode;

		public LightFadeTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			AddTag(Tags.TransitionUpdate);
			LightAddFrom = data.Float("lightAddFrom");
			LightAddTo = data.Float("lightAddTo");
			PositionMode = data.Enum("positionMode", PositionModes.NoEffect);
		}

		public override void OnStay(Player player)
		{
			Level level = base.Scene as Level;
			float lightAdd = (level.Session.LightingAlphaAdd = LightAddFrom + (LightAddTo - LightAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f));
			level.Lighting.Alpha = level.BaseLightingAlpha + lightAdd;
		}
	}
}
