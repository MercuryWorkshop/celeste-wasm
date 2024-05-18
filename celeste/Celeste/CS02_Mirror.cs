using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS02_Mirror : CutsceneEntity
	{
		private Player player;

		private DreamMirror mirror;

		private float playerEndX;

		private int direction = 1;

		private SoundSource sfx;

		public CS02_Mirror(Player player, DreamMirror mirror)
		{
			this.player = player;
			this.mirror = mirror;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			Add(sfx = new SoundSource());
			sfx.Position = mirror.Center;
			sfx.Play("event:/music/lvl2/dreamblock_sting_pt1");
			direction = Math.Sign(player.X - mirror.X);
			player.StateMachine.State = 11;
			playerEndX = 8 * direction;
			yield return 1f;
			player.Facing = (Facings)(-direction);
			yield return 0.4f;
			yield return player.DummyRunTo(mirror.X + playerEndX);
			yield return 0.5f;
			yield return level.ZoomTo(mirror.Position - level.Camera.Position - Vector2.UnitY * 24f, 2f, 1f);
			yield return 0.5f;
			yield return mirror.BreakRoutine(direction);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			Vector2 from = level.Camera.Position;
			Vector2 to = level.Camera.Position + new Vector2(0f, -80f);
			for (float ease = 0f; ease < 1f; ease += Engine.DeltaTime * 1.2f)
			{
				level.Camera.Position = from + (to - from) * Ease.CubeInOut(ease);
				yield return null;
			}
			Add(new Coroutine(ZoomBack()));
			using (List<Entity>.Enumerator enumerator = base.Scene.Tracker.GetEntities<DreamBlock>().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					DreamBlock block = (DreamBlock)enumerator.Current;
					yield return block.Activate();
				}
			}
			yield return 0.5f;
			EndCutscene(level);
		}

		private IEnumerator ZoomBack()
		{
			yield return 1.2f;
			yield return Level.ZoomBack(3f);
		}

		public override void OnEnd(Level level)
		{
			mirror.Broken(WasSkipped);
			if (WasSkipped)
			{
				SceneAs<Level>().ParticlesFG.Clear();
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.State = 0;
				player.DummyAutoAnimate = true;
				player.Speed = Vector2.Zero;
				player.X = mirror.X + playerEndX;
				if (direction != 0)
				{
					player.Facing = (Facings)(-direction);
				}
				else
				{
					player.Facing = Facings.Right;
				}
			}
			foreach (DreamBlock entity in base.Scene.Tracker.GetEntities<DreamBlock>())
			{
				entity.ActivateNoRoutine();
			}
			level.ResetZoom();
			level.Session.Inventory.DreamDash = true;
			level.Session.Audio.Music.Event = "event:/music/lvl2/mirror";
			level.Session.Audio.Apply();
		}
	}
}
