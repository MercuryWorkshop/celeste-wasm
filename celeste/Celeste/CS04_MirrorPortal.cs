using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS04_MirrorPortal : CutsceneEntity
	{
		private class Fader : Entity
		{
			public float Target;

			public bool Ended;

			private float fade;

			public Fader()
			{
				base.Depth = -1000000;
			}

			public override void Update()
			{
				fade = Calc.Approach(fade, Target, Engine.DeltaTime * 0.5f);
				if (Target <= 0f && fade <= 0f && Ended)
				{
					RemoveSelf();
				}
				base.Update();
			}

			public override void Render()
			{
				Camera camera = (base.Scene as Level).Camera;
				if (fade > 0f)
				{
					Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
				}
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && !player.OnGround(2))
				{
					player.Render();
				}
			}
		}

		private Player player;

		private TempleMirrorPortal portal;

		private Fader fader;

		private SoundSource sfx;

		public CS04_MirrorPortal(Player player, TempleMirrorPortal portal)
		{
			this.player = player;
			this.portal = portal;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
			level.Add(fader = new Fader());
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			player.Dashes = 1;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				Audio.SetMusic(null);
			}
			else
			{
				Add(new Coroutine(MusicFadeOutBSide()));
			}
			Add(sfx = new SoundSource());
			sfx.Position = portal.Center;
			sfx.Play("event:/music/lvl5/mirror_cutscene");
			Add(new Coroutine(CenterCamera()));
			yield return player.DummyWalkToExact((int)portal.X);
			yield return 0.25f;
			yield return player.DummyWalkToExact((int)portal.X - 16);
			yield return 0.5f;
			yield return player.DummyWalkToExact((int)portal.X + 16);
			yield return 0.25f;
			player.Facing = Facings.Left;
			yield return 0.25f;
			yield return player.DummyWalkToExact((int)portal.X);
			yield return 0.1f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			yield return 1f;
			player.DummyAutoAnimate = true;
			portal.Activate();
			Add(new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 3f, 12f)));
			yield return 0.25f;
			player.ForceStrongWindHair.X = -1f;
			yield return player.DummyWalkToExact((int)player.X + 12, walkBackwards: true);
			yield return 0.5f;
			player.Facing = Facings.Right;
			player.DummyAutoAnimate = false;
			player.DummyGravity = false;
			player.Sprite.Play("runWind");
			while (player.Sprite.Rate > 0f)
			{
				player.MoveH(player.Sprite.Rate * 10f * Engine.DeltaTime);
				player.MoveV((0f - (1f - player.Sprite.Rate)) * 6f * Engine.DeltaTime);
				player.Sprite.Rate -= Engine.DeltaTime * 0.15f;
				yield return null;
			}
			yield return 0.5f;
			player.Sprite.Play("fallFast");
			player.Sprite.Rate = 1f;
			Vector2 target = portal.Center + new Vector2(0f, 8f);
			Vector2 from = player.Position;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 2f)
			{
				player.Position = from + (target - from) * Ease.SineInOut(p2);
				yield return null;
			}
			player.ForceStrongWindHair.X = 0f;
			fader.Target = 1f;
			yield return 2f;
			player.Sprite.Play("sleep");
			yield return 1f;
			yield return level.ZoomBack(1f);
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				level.Session.ColorGrade = "templevoid";
				for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
				{
					Glitch.Value = p2 * 0.05f;
					level.ScreenPadding = 32f * p2;
					yield return null;
				}
			}
			while ((portal.DistortionFade -= Engine.DeltaTime * 2f) > 0f)
			{
				yield return null;
			}
			EndCutscene(level);
		}

		private IEnumerator CenterCamera()
		{
			Camera camera = Level.Camera;
			Vector2 target = portal.Center - new Vector2(160f, 90f);
			while ((camera.Position - target).Length() > 1f)
			{
				camera.Position += (target - camera.Position) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
				yield return null;
			}
		}

		private IEnumerator MusicFadeOutBSide()
		{
			for (float p = 1f; p > 0f; p -= Engine.DeltaTime)
			{
				Audio.SetMusicParam("fade", p);
				yield return null;
			}
			Audio.SetMusicParam("fade", 0f);
		}

		public override void OnEnd(Level level)
		{
			level.OnEndOfFrame += delegate
			{
				if (fader != null && !WasSkipped)
				{
					fader.Tag = Tags.Global;
					fader.Target = 0f;
					fader.Ended = true;
				}
				Leader.StoreStrawberries(player.Leader);
				level.Remove(player);
				level.UnloadLevel();
				level.Session.Dreaming = true;
				level.Session.Keys.Clear();
				if (level.Session.Area.Mode == AreaMode.Normal)
				{
					level.Session.Level = "void";
					level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
					level.LoadLevel(Player.IntroTypes.TempleMirrorVoid);
				}
				else
				{
					level.Session.Level = "c-00";
					level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
					level.LoadLevel(Player.IntroTypes.WakeUp);
					Audio.SetMusicParam("fade", 1f);
				}
				Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
				level.Camera.Y -= 8f;
				if (!WasSkipped && level.Wipe != null)
				{
					level.Wipe.Cancel();
				}
				if (fader != null)
				{
					fader.RemoveTag(Tags.Global);
				}
			};
		}
	}
}
