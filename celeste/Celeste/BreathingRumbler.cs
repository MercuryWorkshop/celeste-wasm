using Monocle;

namespace Celeste
{
	public class BreathingRumbler : Entity
	{
		private const float MaxRumble = 0.25f;

		public float Strength = 0.2f;

		private float currentRumble;

		public BreathingRumbler()
		{
			currentRumble = Strength;
		}

		public override void Update()
		{
			base.Update();
			currentRumble = Calc.Approach(currentRumble, Strength, 2f * Engine.DeltaTime);
			if (currentRumble > 0f)
			{
				Input.RumbleSpecific(currentRumble * 0.25f, 0.05f);
			}
		}
	}
}
