using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CutsceneNode : Entity
	{
		public string Name;

		public CutsceneNode(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Name = data.Attr("nodeName");
		}

		public static CutsceneNode Find(string name)
		{
			foreach (CutsceneNode node in Engine.Scene.Tracker.GetEntities<CutsceneNode>())
			{
				if (node.Name != null && node.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return node;
				}
			}
			return null;
		}
	}
}
