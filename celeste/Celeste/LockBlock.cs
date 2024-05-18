using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LockBlock : Solid
	{
		public static ParticleType P_Appear;

		public EntityID ID;

		public bool UnlockingRegistered;

		private Sprite sprite;

		private bool opening;

		private bool stepMusicProgress;

		private string unlockSfxName;

		public LockBlock(Vector2 position, EntityID id, bool stepMusicProgress, string spriteName, string unlock_sfx)
			: base(position, 32f, 32f, safe: false)
		{
			ID = id;
			DisableLightsInside = false;
			this.stepMusicProgress = stepMusicProgress;
			Add(new PlayerCollider(OnPlayer, new Circle(60f, 16f, 16f)));
			Add(sprite = GFX.SpriteBank.Create("lockdoor_" + spriteName));
			sprite.Play("idle");
			sprite.Position = new Vector2(base.Width / 2f, base.Height / 2f);
			if (string.IsNullOrWhiteSpace(unlock_sfx))
			{
				unlockSfxName = "event:/game/03_resort/key_unlock";
				if (spriteName == "temple_a")
				{
					unlockSfxName = "event:/game/05_mirror_temple/key_unlock_light";
				}
				else if (spriteName == "temple_b")
				{
					unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
				}
			}
			else
			{
				unlockSfxName = SFX.EventnameByHandle(unlock_sfx);
			}
		}

		public LockBlock(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, id, data.Bool("stepMusicProgress"), data.Attr("sprite", "wood"), data.Attr("unlock_sfx", null))
		{
		}

		public void Appear()
		{
			Visible = true;
			sprite.Play("appear");
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				Level level = base.Scene as Level;
				if (!CollideCheck<Solid>(Position - Vector2.UnitX))
				{
					level.Particles.Emit(P_Appear, 16, Position + new Vector2(3f, 16f), new Vector2(2f, 10f), (float)Math.PI);
					level.Particles.Emit(P_Appear, 16, Position + new Vector2(29f, 16f), new Vector2(2f, 10f), 0f);
				}
				level.Shake();
			}, 0.25f, start: true));
		}

		private void OnPlayer(Player player)
		{
			if (opening)
			{
				return;
			}
			foreach (Follower fol in player.Leader.Followers)
			{
				if (fol.Entity is Key && !(fol.Entity as Key).StartedUsing)
				{
					TryOpen(player, fol);
					break;
				}
			}
		}

		private void TryOpen(Player player, Follower fol)
		{
			Collidable = false;
			if (!base.Scene.CollideCheck<Solid>(player.Center, base.Center))
			{
				opening = true;
				(fol.Entity as Key).StartedUsing = true;
				Add(new Coroutine(UnlockRoutine(fol)));
			}
			Collidable = true;
		}

		private IEnumerator UnlockRoutine(Follower fol)
		{
			SoundEmitter emitter = SoundEmitter.Play(unlockSfxName, this);
			emitter.Source.DisposeOnTransition = true;
			Level level = SceneAs<Level>();
			Key key = fol.Entity as Key;
			Add(new Coroutine(key.UseRoutine(base.Center + new Vector2(0f, 2f))));
			yield return 1.2f;
			UnlockingRegistered = true;
			if (stepMusicProgress)
			{
				level.Session.Audio.Music.Progress++;
				level.Session.Audio.Apply();
			}
			level.Session.DoNotLoad.Add(ID);
			key.RegisterUsed();
			while (key.Turning)
			{
				yield return null;
			}
			base.Tag |= Tags.TransitionUpdate;
			Collidable = false;
			emitter.Source.DisposeOnTransition = false;
			yield return sprite.PlayRoutine("open");
			level.Shake();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			yield return sprite.PlayRoutine("burst");
			RemoveSelf();
		}
	}
}
