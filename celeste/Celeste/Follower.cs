using System;
using Monocle;

namespace Celeste
{
	public class Follower : Component
	{
		public EntityID ParentEntityID;

		public Leader Leader;

		public Action OnGainLeader;

		public Action OnLoseLeader;

		public bool PersistentFollow = true;

		public float FollowDelay = 0.5f;

		public float DelayTimer;

		public bool MoveTowardsLeader = true;

		public bool HasLeader => Leader != null;

		public int FollowIndex
		{
			get
			{
				if (Leader == null)
				{
					return -1;
				}
				return Leader.Followers.IndexOf(this);
			}
		}

		public Follower(Action onGainLeader = null, Action onLoseLeader = null)
			: base(active: true, visible: false)
		{
			OnGainLeader = onGainLeader;
			OnLoseLeader = onLoseLeader;
		}

		public Follower(EntityID entityID, Action onGainLeader = null, Action onLoseLeader = null)
			: base(active: true, visible: false)
		{
			ParentEntityID = entityID;
			OnGainLeader = onGainLeader;
			OnLoseLeader = onLoseLeader;
		}

		public override void Update()
		{
			base.Update();
			if (DelayTimer > 0f)
			{
				DelayTimer -= Engine.DeltaTime;
			}
		}

		public void OnLoseLeaderUtil()
		{
			if (PersistentFollow)
			{
				base.Entity.RemoveTag(Tags.Persistent);
			}
			Leader = null;
			if (OnLoseLeader != null)
			{
				OnLoseLeader();
			}
		}

		public void OnGainLeaderUtil(Leader leader)
		{
			if (PersistentFollow)
			{
				base.Entity.AddTag(Tags.Persistent);
			}
			Leader = leader;
			DelayTimer = FollowDelay;
			if (OnGainLeader != null)
			{
				OnGainLeader();
			}
		}
	}
}
