using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Leader : Component
	{
		public const int MaxPastPoints = 350;

		public List<Follower> Followers = new List<Follower>();

		public List<Vector2> PastPoints = new List<Vector2>();

		public Vector2 Position;

		private static List<Strawberry> storedBerries;

		private static List<Vector2> storedOffsets;

		public Leader()
			: base(active: true, visible: false)
		{
		}

		public Leader(Vector2 position)
			: base(active: true, visible: false)
		{
			Position = position;
		}

		public void GainFollower(Follower follower)
		{
			Followers.Add(follower);
			follower.OnGainLeaderUtil(this);
		}

		public void LoseFollower(Follower follower)
		{
			Followers.Remove(follower);
			follower.OnLoseLeaderUtil();
		}

		public void LoseFollowers()
		{
			foreach (Follower follower in Followers)
			{
				follower.OnLoseLeaderUtil();
			}
			Followers.Clear();
		}

		public override void Update()
		{
			Vector2 nextTarget = base.Entity.Position + Position;
			if (base.Scene.OnInterval(0.02f) && (PastPoints.Count == 0 || (nextTarget - PastPoints[0]).Length() >= 3f))
			{
				PastPoints.Insert(0, nextTarget);
				if (PastPoints.Count > 350)
				{
					PastPoints.RemoveAt(PastPoints.Count - 1);
				}
			}
			int index = 5;
			foreach (Follower follower in Followers)
			{
				if (index >= PastPoints.Count)
				{
					break;
				}
				Vector2 target = PastPoints[index];
				if (follower.DelayTimer <= 0f && follower.MoveTowardsLeader)
				{
					follower.Entity.Position = follower.Entity.Position + (target - follower.Entity.Position) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
				}
				index += 5;
			}
		}

		public bool HasFollower<T>()
		{
			foreach (Follower follower in Followers)
			{
				if (follower.Entity is T)
				{
					return true;
				}
			}
			return false;
		}

		public void TransferFollowers()
		{
			for (int i = 0; i < Followers.Count; i++)
			{
				Follower follower = Followers[i];
				if (!follower.Entity.TagCheck(Tags.Persistent))
				{
					LoseFollower(follower);
					i--;
				}
			}
		}

		public static void StoreStrawberries(Leader leader)
		{
			storedBerries = new List<Strawberry>();
			storedOffsets = new List<Vector2>();
			foreach (Follower follower in leader.Followers)
			{
				if (follower.Entity is Strawberry)
				{
					storedBerries.Add(follower.Entity as Strawberry);
					storedOffsets.Add(follower.Entity.Position - leader.Entity.Position);
				}
			}
			foreach (Strawberry berry in storedBerries)
			{
				leader.Followers.Remove(berry.Follower);
				berry.Follower.Leader = null;
				berry.AddTag(Tags.Global);
			}
		}

		public static void RestoreStrawberries(Leader leader)
		{
			leader.PastPoints.Clear();
			for (int i = 0; i < storedBerries.Count; i++)
			{
				Strawberry berry = storedBerries[i];
				leader.GainFollower(berry.Follower);
				berry.Position = leader.Entity.Position + storedOffsets[i];
				berry.RemoveTag(Tags.Global);
			}
		}
	}
}
