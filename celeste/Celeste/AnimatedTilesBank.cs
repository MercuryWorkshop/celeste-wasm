using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AnimatedTilesBank
	{
		public struct Animation
		{
			public int ID;

			public string Name;

			public float Delay;

			public Vector2 Offset;

			public Vector2 Origin;

			public MTexture[] Frames;
		}

		public Dictionary<string, Animation> AnimationsByName = new Dictionary<string, Animation>();

		public List<Animation> Animations = new List<Animation>();

		public void Add(string name, float delay, Vector2 offset, Vector2 origin, List<MTexture> textures)
		{
			Animation animation = default(Animation);
			animation.Name = name;
			animation.Delay = delay;
			animation.Offset = offset;
			animation.Origin = origin;
			animation.Frames = textures.ToArray();
			Animation anim = animation;
			anim.ID = Animations.Count;
			Animations.Add(anim);
			AnimationsByName.Add(name, anim);
		}
	}
}
