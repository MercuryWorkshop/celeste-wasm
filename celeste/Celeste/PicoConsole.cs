using System.Collections;
using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PicoConsole : Entity
	{
		private Image sprite;

		private TalkComponent talk;

		private bool talking;

		private SoundSource sfx;

		public PicoConsole(Vector2 position)
			: base(position)
		{
			base.Depth = 1000;
			AddTag(Tags.TransitionUpdate);
			AddTag(Tags.PauseUpdate);
			Add(sprite = new Image(GFX.Game["objects/pico8Console"]));
			sprite.JustifyOrigin(0.5f, 1f);
			Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0f, -24f), OnInteract));
		}

		public PicoConsole(EntityData data, Vector2 position)
			: this(data.Position + position)
		{
		}

		public override void Update()
		{
			base.Update();
			if (sfx == null)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Y < base.Y + 16f)
				{
					Add(sfx = new SoundSource("event:/env/local/03_resort/pico8_machine"));
				}
			}
		}

		private void OnInteract(Player player)
		{
			if (!talking)
			{
				(base.Scene as Level).PauseLock = true;
				talking = true;
				Add(new Coroutine(InteractRoutine(player)));
			}
		}

		private IEnumerator InteractRoutine(Player player)
		{
			player.StateMachine.State = 11;
			yield return player.DummyWalkToExact((int)base.X - 6);
			player.Facing = Facings.Right;
			bool wasUnlocked = Settings.Instance.Pico8OnMainMenu;
			Settings.Instance.Pico8OnMainMenu = true;
			if (!wasUnlocked)
			{
				UserIO.SaveHandler(file: false, settings: true);
				while (UserIO.Saving)
				{
					yield return null;
				}
			}
			else
			{
				yield return 0.5f;
			}
			bool done = false;
			SpotlightWipe.FocusPoint = player.Position - (base.Scene as Level).Camera.Position + new Vector2(0f, -8f);
			new SpotlightWipe(base.Scene, wipeIn: false, delegate
			{
				if (!wasUnlocked)
				{
					base.Scene.Add(new UnlockedPico8Message(delegate
					{
						done = true;
					}));
				}
				else
				{
					done = true;
				}
				Engine.Scene = new Emulator(base.Scene as Level);
			});
			while (!done)
			{
				yield return null;
			}
			yield return 0.25f;
			talking = false;
			(base.Scene as Level).PauseLock = false;
			player.StateMachine.State = 0;
		}

		public override void SceneEnd(Scene scene)
		{
			if (sfx != null)
			{
				sfx.Stop();
				sfx.RemoveSelf();
				sfx = null;
			}
			base.SceneEnd(scene);
		}
	}
}
