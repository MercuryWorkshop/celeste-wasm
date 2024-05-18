using Microsoft.Xna.Framework;

namespace Celeste
{
	public class BloomFadeTrigger : Trigger
	{
		public float BloomAddFrom;

		public float BloomAddTo;

		public PositionModes PositionMode;

		public BloomFadeTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			BloomAddFrom = data.Float("bloomAddFrom");
			BloomAddTo = data.Float("bloomAddTo");
			PositionMode = data.Enum("positionMode", PositionModes.NoEffect);
		}

		public override void OnStay(Player player)
		{
			Level level = base.Scene as Level;
			float bloomAdd = (level.Session.BloomBaseAdd = BloomAddFrom + (BloomAddTo - BloomAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f));
			level.Bloom.Base = AreaData.Get(level).BloomBase + bloomAdd;
		}
	}
}
