using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Switch : Component
	{
		public bool GroundReset;

		public Action OnActivate;

		public Action OnDeactivate;

		public Action OnFinish;

		public Action OnStartFinished;

		public bool Activated { get; private set; }

		public bool Finished { get; private set; }

		public Switch(bool groundReset)
			: base(active: true, visible: false)
		{
			GroundReset = groundReset;
		}

		public override void EntityAdded(Scene scene)
		{
			base.EntityAdded(scene);
			if (CheckLevelFlag(SceneAs<Level>()))
			{
				StartFinished();
			}
		}

		public override void Update()
		{
			base.Update();
			if (GroundReset && Activated && !Finished)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.OnGround())
				{
					Deactivate();
				}
			}
		}

		public bool Activate()
		{
			if (!Finished && !Activated)
			{
				Activated = true;
				if (OnActivate != null)
				{
					OnActivate();
				}
				return FinishedCheck(SceneAs<Level>());
			}
			return false;
		}

		public void Deactivate()
		{
			if (!Finished && Activated)
			{
				Activated = false;
				if (OnDeactivate != null)
				{
					OnDeactivate();
				}
			}
		}

		public void Finish()
		{
			Finished = true;
			if (OnFinish != null)
			{
				OnFinish();
			}
		}

		public void StartFinished()
		{
			if (!Finished)
			{
				bool finished = (Activated = true);
				Finished = finished;
				if (OnStartFinished != null)
				{
					OnStartFinished();
				}
			}
		}

		public static bool Check(Scene scene)
		{
			return scene.Tracker.GetComponent<Switch>()?.Finished ?? false;
		}

		private static bool FinishedCheck(Level level)
		{
			foreach (Switch component in level.Tracker.GetComponents<Switch>())
			{
				if (!component.Activated)
				{
					return false;
				}
			}
			foreach (Switch component2 in level.Tracker.GetComponents<Switch>())
			{
				component2.Finish();
			}
			return true;
		}

		public static bool CheckLevelFlag(Level level)
		{
			return level.Session.GetFlag("switches_" + level.Session.Level);
		}

		public static void SetLevelFlag(Level level)
		{
			level.Session.SetFlag("switches_" + level.Session.Level);
		}
	}
}
