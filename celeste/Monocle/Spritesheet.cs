using System;
using System.Collections.Generic;

namespace Monocle
{
	public class Spritesheet<T> : Image
	{
		private struct Animation
		{
			public float Delay;

			public int[] Frames;

			public bool Loop;
		}

		public int CurrentFrame;

		public float Rate = 1f;

		public bool UseRawDeltaTime;

		public Action<T> OnFinish;

		public Action<T> OnLoop;

		public Action<T> OnAnimate;

		private Dictionary<T, Animation> animations;

		private Animation currentAnimation;

		private float animationTimer;

		private bool played;

		public MTexture[] Frames { get; private set; }

		public bool Animating { get; private set; }

		public T CurrentAnimationID { get; private set; }

		public int CurrentAnimationFrame { get; private set; }

		public override float Width
		{
			get
			{
				if (Frames.Length != 0)
				{
					return Frames[0].Width;
				}
				return 0f;
			}
		}

		public override float Height
		{
			get
			{
				if (Frames.Length != 0)
				{
					return Frames[0].Height;
				}
				return 0f;
			}
		}

		public Spritesheet(MTexture texture, int frameWidth, int frameHeight, int frameSep = 0)
			: base(texture, active: true)
		{
			SetFrames(texture, frameWidth, frameHeight, frameSep);
			animations = new Dictionary<T, Animation>();
		}

		public void SetFrames(MTexture texture, int frameWidth, int frameHeight, int frameSep = 0)
		{
			List<MTexture> frames = new List<MTexture>();
			int x = 0;
			int y = 0;
			while (y <= texture.Height - frameHeight)
			{
				for (; x <= texture.Width - frameWidth; x += frameWidth + frameSep)
				{
					frames.Add(texture.GetSubtexture(x, y, frameWidth, frameHeight));
				}
				y += frameHeight + frameSep;
				x = 0;
			}
			Frames = frames.ToArray();
		}

		public override void Update()
		{
			if (!Animating || !(currentAnimation.Delay > 0f))
			{
				return;
			}
			if (UseRawDeltaTime)
			{
				animationTimer += Engine.RawDeltaTime * Rate;
			}
			else
			{
				animationTimer += Engine.DeltaTime * Rate;
			}
			if (!(Math.Abs(animationTimer) >= currentAnimation.Delay))
			{
				return;
			}
			CurrentAnimationFrame += Math.Sign(animationTimer);
			animationTimer -= (float)Math.Sign(animationTimer) * currentAnimation.Delay;
			if (CurrentAnimationFrame < 0 || CurrentAnimationFrame >= currentAnimation.Frames.Length)
			{
				if (currentAnimation.Loop)
				{
					CurrentAnimationFrame -= Math.Sign(CurrentAnimationFrame) * currentAnimation.Frames.Length;
					CurrentFrame = currentAnimation.Frames[CurrentAnimationFrame];
					if (OnAnimate != null)
					{
						OnAnimate(CurrentAnimationID);
					}
					if (OnLoop != null)
					{
						OnLoop(CurrentAnimationID);
					}
				}
				else
				{
					if (CurrentAnimationFrame < 0)
					{
						CurrentAnimationFrame = 0;
					}
					else
					{
						CurrentAnimationFrame = currentAnimation.Frames.Length - 1;
					}
					Animating = false;
					animationTimer = 0f;
					if (OnFinish != null)
					{
						OnFinish(CurrentAnimationID);
					}
				}
			}
			else
			{
				CurrentFrame = currentAnimation.Frames[CurrentAnimationFrame];
				if (OnAnimate != null)
				{
					OnAnimate(CurrentAnimationID);
				}
			}
		}

		public override void Render()
		{
			Texture = Frames[CurrentFrame];
			base.Render();
		}

		public void Add(T id, bool loop, float delay, params int[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = frames,
				Loop = loop
			};
		}

		public void Add(T id, float delay, params int[] frames)
		{
			Add(id, loop: true, delay, frames);
		}

		public void Add(T id, int frame)
		{
			Add(id, false, 0f, frame);
		}

		public void ClearAnimations()
		{
			animations.Clear();
		}

		public bool IsPlaying(T id)
		{
			if (!played)
			{
				return false;
			}
			if (CurrentAnimationID == null)
			{
				return id == null;
			}
			return CurrentAnimationID.Equals(id);
		}

		public void Play(T id, bool restart = false)
		{
			if (!IsPlaying(id) || restart)
			{
				CurrentAnimationID = id;
				currentAnimation = animations[id];
				animationTimer = 0f;
				CurrentAnimationFrame = 0;
				played = true;
				Animating = currentAnimation.Frames.Length > 1;
				CurrentFrame = currentAnimation.Frames[0];
			}
		}

		public void Reverse(T id, bool restart = false)
		{
			Play(id, restart);
			if (Rate > 0f)
			{
				Rate *= -1f;
			}
		}

		public void Stop()
		{
			Animating = false;
			played = false;
		}
	}
}
