using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RumbleTrigger : Trigger
	{
		private bool manualTrigger;

		private bool started;

		private bool persistent;

		private EntityID id;

		private float rumble;

		private float left;

		private float right;

		private List<Decal> decals = new List<Decal>();

		private List<CrumbleWallOnRumble> crumbles = new List<CrumbleWallOnRumble>();

		public RumbleTrigger(EntityData data, Vector2 offset, EntityID id)
			: base(data, offset)
		{
			manualTrigger = data.Bool("manualTrigger");
			persistent = data.Bool("persistent");
			this.id = id;
			Vector2[] nodes = data.NodesOffset(offset);
			if (nodes.Length >= 2)
			{
				left = Math.Min(nodes[0].X, nodes[1].X);
				right = Math.Max(nodes[0].X, nodes[1].X);
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level level = base.Scene as Level;
			bool remove = false;
			if (persistent && level.Session.GetFlag(id.ToString()))
			{
				remove = true;
			}
			foreach (CrumbleWallOnRumble crumble in scene.Tracker.GetEntities<CrumbleWallOnRumble>())
			{
				if (crumble.X >= left && crumble.X <= right)
				{
					if (remove)
					{
						crumble.RemoveSelf();
					}
					else
					{
						crumbles.Add(crumble);
					}
				}
			}
			if (!remove)
			{
				foreach (Decal decal in scene.Entities.FindAll<Decal>())
				{
					if (decal.IsCrack && decal.X >= left && decal.X <= right)
					{
						decal.Visible = false;
						decals.Add(decal);
					}
				}
				crumbles.Sort((CrumbleWallOnRumble a, CrumbleWallOnRumble b) => (!Calc.Random.Chance(0.5f)) ? 1 : (-1));
			}
			if (remove)
			{
				RemoveSelf();
			}
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (!manualTrigger)
			{
				Invoke();
			}
		}

		private void Invoke(float delay = 0f)
		{
			if (!started)
			{
				started = true;
				if (persistent)
				{
					(base.Scene as Level).Session.SetFlag(id.ToString());
				}
				Add(new Coroutine(RumbleRoutine(delay)));
				Add(new DisplacementRenderHook(RenderDisplacement));
			}
		}

		private IEnumerator RumbleRoutine(float delay)
		{
			yield return delay;
			_ = base.Scene;
			rumble = 1f;
			Audio.Play("event:/new_content/game/10_farewell/quake_onset", Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			foreach (Decal decal in decals)
			{
				decal.Visible = true;
			}
			foreach (CrumbleWallOnRumble crumble in crumbles)
			{
				crumble.Break();
				yield return 0.05f;
			}
		}

		public override void Update()
		{
			base.Update();
			rumble = Calc.Approach(rumble, 0f, Engine.DeltaTime * 0.7f);
		}

		private void RenderDisplacement()
		{
			if (!(rumble <= 0f) && Settings.Instance.ScreenShake != 0)
			{
				Camera cam = (base.Scene as Level).Camera;
				int num = (int)(cam.Left / 8f) - 1;
				int right = (int)(cam.Right / 8f) + 1;
				for (int tx = num; tx <= right; tx++)
				{
					float sin = (float)Math.Sin(base.Scene.TimeActive * 60f + (float)tx * 0.4f) * 0.06f * rumble;
					Draw.Rect(color: new Color(0.5f, 0.5f + sin, 0f, 1f), x: tx * 8, y: cam.Top - 2f, width: 8f, height: 184f);
				}
			}
		}

		public static void ManuallyTrigger(float x, float delay)
		{
			foreach (RumbleTrigger trigger in Engine.Scene.Entities.FindAll<RumbleTrigger>())
			{
				if (trigger.manualTrigger && x >= trigger.left && x <= trigger.right)
				{
					trigger.Invoke(delay);
				}
			}
		}
	}
}
