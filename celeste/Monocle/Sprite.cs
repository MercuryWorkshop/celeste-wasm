using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Sprite : Image
	{
		private class Animation
		{
			public float Delay;

			public MTexture[] Frames;

			public Chooser<string> Goto;
		}

		public float Rate = 1f;

		public bool UseRawDeltaTime;

		public Vector2? Justify;

		public Action<string> OnFinish;

		public Action<string> OnLoop;

		public Action<string> OnFrameChange;

		public Action<string> OnLastFrame;

		public Action<string, string> OnChange;

		private Atlas atlas;

		public string Path;

		private Dictionary<string, Animation> animations;

		private Animation currentAnimation;

		private float animationTimer;

		private int width;

		private int height;

		public Vector2 Center => new Vector2(Width / 2f, Height / 2f);

		public bool Animating { get; private set; }

		public string CurrentAnimationID { get; private set; }

		public string LastAnimationID { get; private set; }

		public int CurrentAnimationFrame { get; private set; }

		public int CurrentAnimationTotalFrames
		{
			get
			{
				if (currentAnimation != null)
				{
					return currentAnimation.Frames.Length;
				}
				return 0;
			}
		}

		public override float Width => width;

		public override float Height => height;

		public Sprite(Atlas atlas, string path)
			: base(null, active: true)
		{
			this.atlas = atlas;
			Path = path;
			animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
			CurrentAnimationID = "";
		}

		public void Reset(Atlas atlas, string path)
		{
			this.atlas = atlas;
			Path = path;
			animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
			currentAnimation = null;
			CurrentAnimationID = "";
			OnFinish = null;
			OnLoop = null;
			OnFrameChange = null;
			OnChange = null;
			Animating = false;
		}

		public MTexture GetFrame(string animation, int frame)
		{
			return animations[animation].Frames[frame];
		}

		public override void Update()
		{
			if (!Animating)
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
				string currentAnimationID = CurrentAnimationID;
				if (OnLastFrame != null)
				{
					OnLastFrame(CurrentAnimationID);
				}
				if (!(currentAnimationID == CurrentAnimationID))
				{
					return;
				}
				if (currentAnimation.Goto != null)
				{
					CurrentAnimationID = currentAnimation.Goto.Choose();
					if (OnChange != null)
					{
						OnChange(LastAnimationID, CurrentAnimationID);
					}
					LastAnimationID = CurrentAnimationID;
					currentAnimation = animations[LastAnimationID];
					if (CurrentAnimationFrame < 0)
					{
						CurrentAnimationFrame = currentAnimation.Frames.Length - 1;
					}
					else
					{
						CurrentAnimationFrame = 0;
					}
					SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
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
					string id = CurrentAnimationID;
					CurrentAnimationID = "";
					currentAnimation = null;
					animationTimer = 0f;
					if (OnFinish != null)
					{
						OnFinish(id);
					}
				}
			}
			else
			{
				SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
			}
		}

		private void SetFrame(MTexture texture)
		{
			if (texture != Texture)
			{
				Texture = texture;
				if (width == 0)
				{
					width = texture.Width;
				}
				if (height == 0)
				{
					height = texture.Height;
				}
				if (Justify.HasValue)
				{
					Origin = new Vector2((float)Texture.Width * Justify.Value.X, (float)Texture.Height * Justify.Value.Y);
				}
				if (OnFrameChange != null)
				{
					OnFrameChange(CurrentAnimationID);
				}
			}
		}

		public void SetAnimationFrame(int frame)
		{
			animationTimer = 0f;
			CurrentAnimationFrame = frame % currentAnimation.Frames.Length;
			SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
		}

		public void AddLoop(string id, string path, float delay)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path),
				Goto = new Chooser<string>(id, 1f)
			};
		}

		public void AddLoop(string id, string path, float delay, params int[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path, frames),
				Goto = new Chooser<string>(id, 1f)
			};
		}

		public void AddLoop(string id, float delay, params MTexture[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = frames,
				Goto = new Chooser<string>(id, 1f)
			};
		}

		public void Add(string id, string path)
		{
			animations[id] = new Animation
			{
				Delay = 0f,
				Frames = GetFrames(path),
				Goto = null
			};
		}

		public void Add(string id, string path, float delay)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path),
				Goto = null
			};
		}

		public void Add(string id, string path, float delay, params int[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path, frames),
				Goto = null
			};
		}

		public void Add(string id, string path, float delay, string into)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path),
				Goto = Chooser<string>.FromString<string>(into)
			};
		}

		public void Add(string id, string path, float delay, Chooser<string> into)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path),
				Goto = into
			};
		}

		public void Add(string id, string path, float delay, string into, params int[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path, frames),
				Goto = Chooser<string>.FromString<string>(into)
			};
		}

		public void Add(string id, float delay, string into, params MTexture[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = frames,
				Goto = Chooser<string>.FromString<string>(into)
			};
		}

		public void Add(string id, string path, float delay, Chooser<string> into, params int[] frames)
		{
			animations[id] = new Animation
			{
				Delay = delay,
				Frames = GetFrames(path, frames),
				Goto = into
			};
		}

		private MTexture[] GetFrames(string path, int[] frames = null)
		{
			MTexture[] ret;
			if (frames == null || frames.Length == 0)
			{
				ret = atlas.GetAtlasSubtextures(Path + path).ToArray();
			}
			else
			{
				string fullPath = Path + path;
				MTexture[] finalFrames = new MTexture[frames.Length];
				for (int i = 0; i < frames.Length; i++)
				{
					MTexture frame = atlas.GetAtlasSubtexturesAt(fullPath, frames[i]);
					if (frame == null)
					{
						throw new Exception("Can't find sprite " + fullPath + " with index " + frames[i]);
					}
					finalFrames[i] = frame;
				}
				ret = finalFrames;
			}
			width = Math.Max(ret[0].Width, width);
			height = Math.Max(ret[0].Height, height);
			return ret;
		}

		public void ClearAnimations()
		{
			animations.Clear();
		}

		public void Play(string id, bool restart = false, bool randomizeFrame = false)
		{
			if (CurrentAnimationID != id || restart)
			{
				if (OnChange != null)
				{
					OnChange(LastAnimationID, id);
				}
				string text3 = (LastAnimationID = (CurrentAnimationID = id));
				currentAnimation = animations[id];
				Animating = currentAnimation.Delay > 0f;
				if (randomizeFrame)
				{
					animationTimer = Calc.Random.NextFloat(currentAnimation.Delay);
					CurrentAnimationFrame = Calc.Random.Next(currentAnimation.Frames.Length);
				}
				else
				{
					animationTimer = 0f;
					CurrentAnimationFrame = 0;
				}
				SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
			}
		}

		public void PlayOffset(string id, float offset, bool restart = false)
		{
			if (!(CurrentAnimationID != id || restart))
			{
				return;
			}
			if (OnChange != null)
			{
				OnChange(LastAnimationID, id);
			}
			string text3 = (LastAnimationID = (CurrentAnimationID = id));
			currentAnimation = animations[id];
			if (currentAnimation.Delay > 0f)
			{
				Animating = true;
				float at = currentAnimation.Delay * (float)currentAnimation.Frames.Length * offset;
				CurrentAnimationFrame = 0;
				while (at >= currentAnimation.Delay)
				{
					CurrentAnimationFrame++;
					at -= currentAnimation.Delay;
				}
				CurrentAnimationFrame %= currentAnimation.Frames.Length;
				animationTimer = at;
				SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
			}
			else
			{
				animationTimer = 0f;
				Animating = false;
				CurrentAnimationFrame = 0;
				SetFrame(currentAnimation.Frames[0]);
			}
		}

		public IEnumerator PlayRoutine(string id, bool restart = false)
		{
			Play(id, restart);
			return PlayUtil();
		}

		public IEnumerator ReverseRoutine(string id, bool restart = false)
		{
			Reverse(id, restart);
			return PlayUtil();
		}

		private IEnumerator PlayUtil()
		{
			while (Animating)
			{
				yield return null;
			}
		}

		public void Reverse(string id, bool restart = false)
		{
			Play(id, restart);
			if (Rate > 0f)
			{
				Rate *= -1f;
			}
		}

		public bool Has(string id)
		{
			if (id != null)
			{
				return animations.ContainsKey(id);
			}
			return false;
		}

		public void Stop()
		{
			Animating = false;
			currentAnimation = null;
			CurrentAnimationID = "";
		}

		internal Sprite()
			: base(null, active: true)
		{
		}

		internal Sprite CreateClone()
		{
			return CloneInto(new Sprite());
		}

		internal Sprite CloneInto(Sprite clone)
		{
			clone.Texture = Texture;
			clone.Position = Position;
			clone.Justify = Justify;
			clone.Origin = Origin;
			clone.animations = new Dictionary<string, Animation>(animations, StringComparer.OrdinalIgnoreCase);
			clone.currentAnimation = currentAnimation;
			clone.animationTimer = animationTimer;
			clone.width = width;
			clone.height = height;
			clone.Animating = Animating;
			clone.CurrentAnimationID = CurrentAnimationID;
			clone.LastAnimationID = LastAnimationID;
			clone.CurrentAnimationFrame = CurrentAnimationFrame;
			return clone;
		}

		public void DrawSubrect(Vector2 offset, Rectangle rectangle)
		{
			if (Texture != null)
			{
				Rectangle clip = Texture.GetRelativeRect(rectangle);
				Vector2 clipOffset = new Vector2(0f - Math.Min((float)rectangle.X - Texture.DrawOffset.X, 0f), 0f - Math.Min((float)rectangle.Y - Texture.DrawOffset.Y, 0f));
				Draw.SpriteBatch.Draw(Texture.Texture.Texture, base.RenderPosition + offset, clip, Color, Rotation, Origin - clipOffset, Scale, Effects, 0f);
			}
		}

		public void LogAnimations()
		{
			StringBuilder str = new StringBuilder();
			foreach (KeyValuePair<string, Animation> kv in animations)
			{
				Animation anim = kv.Value;
				str.Append(kv.Key);
				str.Append("\n{\n\t");
				object[] frames = anim.Frames;
				str.Append(string.Join("\n\t", frames));
				str.Append("\n}\n");
			}
			Calc.Log(str.ToString());
		}
	}
}
