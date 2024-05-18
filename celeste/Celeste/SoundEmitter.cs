using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SoundEmitter : Entity
	{
		public SoundSource Source { get; private set; }

		public static SoundEmitter Play(string sfx)
		{
			SoundEmitter emitter = new SoundEmitter(sfx);
			Engine.Scene.Add(emitter);
			return emitter;
		}

		public static SoundEmitter Play(string sfx, Entity follow, Vector2? offset = null)
		{
			SoundEmitter emitter = new SoundEmitter(sfx, follow, offset.HasValue ? offset.Value : Vector2.Zero);
			Engine.Scene.Add(emitter);
			return emitter;
		}

		private SoundEmitter(string sfx)
		{
			Add(Source = new SoundSource());
			Source.Play(sfx);
			Source.DisposeOnTransition = false;
			base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
			Add(new LevelEndingHook(OnLevelEnding));
		}

		private SoundEmitter(string sfx, Entity follow, Vector2 offset)
		{
			Add(Source = new SoundSource());
			Position = follow.Position + offset;
			Source.Play(sfx);
			Source.DisposeOnTransition = false;
			base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
			Add(new LevelEndingHook(OnLevelEnding));
		}

		public override void Update()
		{
			base.Update();
			if (!Source.Playing)
			{
				RemoveSelf();
			}
		}

		private void OnLevelEnding()
		{
			Source.Stop();
		}
	}
}
