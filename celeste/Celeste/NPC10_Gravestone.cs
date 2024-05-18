using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC10_Gravestone : NPC
	{
		private const string Flag = "gravestone";

		private Player player;

		private Vector2 boostTarget;

		private TalkComponent talk;

		public NPC10_Gravestone(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			boostTarget = data.FirstNodeNullable(offset) ?? Vector2.Zero;
			Add(talk = new TalkComponent(new Rectangle(-24, -8, 32, 8), new Vector2(-0.5f, -20f), Interact));
			talk.PlayerMustBeFacing = false;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (Level.Session.GetFlag("gravestone"))
			{
				Level.Add(new BadelineBoost(new Vector2[1] { boostTarget }, lockCamera: false));
				talk.RemoveSelf();
			}
		}

		private void Interact(Player player)
		{
			Level.Session.SetFlag("gravestone");
			base.Scene.Add(new CS10_Gravestone(player, this, boostTarget));
			talk.Enabled = false;
		}
	}
}
