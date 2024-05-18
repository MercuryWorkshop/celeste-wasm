using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CameraAdvanceTargetTrigger : Trigger
	{
		public Vector2 Target;

		public Vector2 LerpStrength;

		public PositionModes PositionModeX;

		public PositionModes PositionModeY;

		public bool XOnly;

		public bool YOnly;

		public CameraAdvanceTargetTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Target = data.Nodes[0] + offset - new Vector2(320f, 180f) * 0.5f;
			LerpStrength.X = data.Float("lerpStrengthX");
			LerpStrength.Y = data.Float("lerpStrengthY");
			PositionModeX = data.Enum("positionModeX", PositionModes.NoEffect);
			PositionModeY = data.Enum("positionModeY", PositionModes.NoEffect);
			XOnly = data.Bool("xOnly");
			YOnly = data.Bool("yOnly");
		}

		public override void OnStay(Player player)
		{
			player.CameraAnchor = Target;
			player.CameraAnchorLerp.X = MathHelper.Clamp(LerpStrength.X * GetPositionLerp(player, PositionModeX), 0f, 1f);
			player.CameraAnchorLerp.Y = MathHelper.Clamp(LerpStrength.Y * GetPositionLerp(player, PositionModeY), 0f, 1f);
			player.CameraAnchorIgnoreX = YOnly;
			player.CameraAnchorIgnoreY = XOnly;
		}

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			bool isInOther = false;
			foreach (CameraTargetTrigger entity in base.Scene.Tracker.GetEntities<CameraTargetTrigger>())
			{
				if (entity.PlayerIsInside)
				{
					isInOther = true;
					break;
				}
			}
			if (!isInOther)
			{
				foreach (CameraAdvanceTargetTrigger entity2 in base.Scene.Tracker.GetEntities<CameraAdvanceTargetTrigger>())
				{
					if (entity2.PlayerIsInside)
					{
						isInOther = true;
						break;
					}
				}
			}
			if (!isInOther)
			{
				player.CameraAnchorLerp = Vector2.Zero;
			}
		}
	}
}
