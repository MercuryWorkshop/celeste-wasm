using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AmbienceParamTrigger : Trigger
	{
		public string Parameter;

		public float From;

		public float To;

		public PositionModes PositionMode;

		public AmbienceParamTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Parameter = data.Attr("parameter");
			From = data.Float("from");
			To = data.Float("to");
			PositionMode = data.Enum("direction", PositionModes.NoEffect);
		}

		public override void OnStay(Player player)
		{
			float value = Calc.ClampedMap(GetPositionLerp(player, PositionMode), 0f, 1f, From, To);
			Level obj = base.Scene as Level;
			obj.Session.Audio.Ambience.Param(Parameter, value);
			obj.Session.Audio.Apply();
		}
	}
}
