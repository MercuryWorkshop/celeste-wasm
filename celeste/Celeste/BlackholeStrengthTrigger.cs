using Microsoft.Xna.Framework;

namespace Celeste
{
	public class BlackholeStrengthTrigger : Trigger
	{
		private BlackholeBG.Strengths strength;

		public BlackholeStrengthTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			strength = data.Enum("strength", BlackholeBG.Strengths.Mild);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			(base.Scene as Level).Background.Get<BlackholeBG>()?.NextStrength(base.Scene as Level, strength);
		}
	}
}
