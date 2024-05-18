using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DetachStrawberryTrigger : Trigger
	{
		public Vector2 Target;

		public bool Global;

		public DetachStrawberryTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Vector2[] nodes = data.NodesOffset(offset);
			if (nodes.Length != 0)
			{
				Target = nodes[0];
			}
			Global = data.Bool("global", defaultValue: true);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			for (int i = player.Leader.Followers.Count - 1; i >= 0; i--)
			{
				if (player.Leader.Followers[i].Entity is Strawberry)
				{
					Add(new Coroutine(DetatchFollower(player.Leader.Followers[i])));
				}
			}
		}

		private IEnumerator DetatchFollower(Follower follower)
		{
			Leader leader = follower.Leader;
			Entity entity = follower.Entity;
			float distance = (entity.Position - Target).Length();
			float time = distance / 200f;
			if (entity is Strawberry strawb)
			{
				strawb.ReturnHomeWhenLost = false;
			}
			leader.LoseFollower(follower);
			entity.Active = false;
			entity.Collidable = false;
			if (Global)
			{
				entity.AddTag(Tags.Global);
				follower.OnGainLeader = (Action)Delegate.Combine(follower.OnGainLeader, (Action)delegate
				{
					entity.RemoveTag(Tags.Global);
				});
			}
			else
			{
				entity.AddTag(Tags.Persistent);
			}
			Audio.Play("event:/new_content/game/10_farewell/strawberry_gold_detach", entity.Position);
			Vector2 from = entity.Position;
			SimpleCurve curve = new SimpleCurve(from, Target, from + (Target - from) * 0.5f + new Vector2(0f, -64f));
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / time)
			{
				entity.Position = curve.GetPoint(Ease.CubeInOut(p));
				yield return null;
			}
			entity.Active = true;
			entity.Collidable = true;
		}
	}
}
