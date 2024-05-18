using Microsoft.Xna.Framework;

namespace Celeste
{
	public class OshiroTrigger : Trigger
	{
		public bool State;

		public OshiroTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			State = data.Bool("state", defaultValue: true);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (State)
			{
				Level level = SceneAs<Level>();
				Vector2 at = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
				base.Scene.Add(new AngryOshiro(at, fromCutscene: false));
				RemoveSelf();
			}
			else
			{
				base.Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
				RemoveSelf();
			}
		}
	}
}
