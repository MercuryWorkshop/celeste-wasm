using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ChangeRespawnTrigger : Trigger
	{
		public Vector2 Target;

		public ChangeRespawnTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			base.Collider = new Hitbox(data.Width, data.Height);
			if (data.Nodes != null && data.Nodes.Length != 0)
			{
				Target = data.Nodes[0] + offset;
			}
			else
			{
				Target = base.Center;
			}
			Visible = (Active = false);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Target = SceneAs<Level>().GetSpawnPoint(Target);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			Session session = (base.Scene as Level).Session;
			if (SolidCheck() && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != Target))
			{
				session.HitCheckpoint = true;
				session.RespawnPoint = Target;
				session.UpdateLevelStartDashes();
			}
		}

		private bool SolidCheck()
		{
			Vector2 at = Target + Vector2.UnitY * -4f;
			if (base.Scene.CollideCheck<Solid>(at))
			{
				return base.Scene.CollideCheck<FloatySpaceBlock>(at);
			}
			return true;
		}
	}
}
