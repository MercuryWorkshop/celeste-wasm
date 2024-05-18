using System;
using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CSGEN_StrawberrySeeds : CutsceneEntity
	{
		private Strawberry strawberry;

		private Vector2 cameraStart;

		private ParticleSystem system;

		private EventInstance? snapshot;

		private EventInstance? sfx;

		public CSGEN_StrawberrySeeds(Strawberry strawberry)
		{
			this.strawberry = strawberry;
		}

		public override void OnBegin(Level level)
		{
			cameraStart = level.Camera.Position;
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			sfx = Audio.Play("event:/game/general/seed_complete_main", Position);
			snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				cameraStart = player.CameraTarget;
			}
			foreach (StrawberrySeed seed2 in strawberry.Seeds)
			{
				seed2.OnAllCollected();
			}
			strawberry.Depth = -2000002;
			strawberry.AddTag(Tags.FrozenUpdate);
			yield return 0.35f;
			base.Tag = (int)Tags.FrozenUpdate | (int)Tags.HUD;
			level.Frozen = true;
			level.FormationBackdrop.Display = true;
			level.FormationBackdrop.Alpha = 0.5f;
			level.Displacement.Clear();
			level.Displacement.Enabled = false;
			Audio.BusPaused("bus:/gameplay_sfx/ambience", true);
			Audio.BusPaused("bus:/gameplay_sfx/char", true);
			Audio.BusPaused("bus:/gameplay_sfx/game/general/yes_pause", true);
			Audio.BusPaused("bus:/gameplay_sfx/game/chapters", true);
			yield return 0.1f;
			system = new ParticleSystem(-2000002, 50);
			system.Tag = Tags.FrozenUpdate;
			level.Add(system);
			float angleSep = (float)Math.PI * 2f / (float)strawberry.Seeds.Count;
			float angle = (float)Math.PI / 2f;
			Vector2 avg = Vector2.Zero;
			foreach (StrawberrySeed seed in strawberry.Seeds)
			{
				avg += seed.Position;
			}
			avg /= (float)strawberry.Seeds.Count;
			foreach (StrawberrySeed seed3 in strawberry.Seeds)
			{
				seed3.StartSpinAnimation(avg, strawberry.Position, angle, 4f);
				angle -= angleSep;
			}
			Vector2 target = strawberry.Position - new Vector2(160f, 90f);
			target = target.Clamp(level.Bounds.Left, level.Bounds.Top, level.Bounds.Right - 320, level.Bounds.Bottom - 180);
			Add(new Coroutine(CutsceneEntity.CameraTo(target, 3.5f, Ease.CubeInOut)));
			yield return 4f;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			Audio.Play("event:/game/general/seed_complete_berry", strawberry.Position);
			foreach (StrawberrySeed seed4 in strawberry.Seeds)
			{
				seed4.StartCombineAnimation(strawberry.Position, 0.6f, system);
			}
			yield return 0.6f;
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			foreach (StrawberrySeed seed5 in strawberry.Seeds)
			{
				seed5.RemoveSelf();
			}
			strawberry.CollectedSeeds();
			yield return 0.5f;
			float dist = (level.Camera.Position - cameraStart).Length();
			yield return CutsceneEntity.CameraTo(cameraStart, dist / 180f);
			if (dist > 80f)
			{
				yield return 0.25f;
			}
			level.EndCutscene();
			OnEnd(level);
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped)
			{
				Audio.Stop(sfx);
			}
			level.OnEndOfFrame += delegate
			{
				if (WasSkipped)
				{
					foreach (StrawberrySeed seed in strawberry.Seeds)
					{
						seed.RemoveSelf();
					}
					strawberry.CollectedSeeds();
					level.Camera.Position = cameraStart;
				}
				strawberry.Depth = -100;
				strawberry.RemoveTag(Tags.FrozenUpdate);
				level.Frozen = false;
				level.FormationBackdrop.Display = false;
				level.Displacement.Enabled = true;
			};
			RemoveSelf();
		}

		private void EndSfx()
		{
			Audio.BusPaused("bus:/gameplay_sfx/ambience", false);
			Audio.BusPaused("bus:/gameplay_sfx/char", false);
			Audio.BusPaused("bus:/gameplay_sfx/game/general/yes_pause", false);
			Audio.BusPaused("bus:/gameplay_sfx/game/chapters", false);
			Audio.ReleaseSnapshot(snapshot);
		}

		public override void Removed(Scene scene)
		{
			EndSfx();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			EndSfx();
			base.SceneEnd(scene);
		}
	}
}
