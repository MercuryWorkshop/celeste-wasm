using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Killbox : Entity
	{
		public Killbox(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Collider = new Hitbox(data.Width, 32f);
			Collidable = false;
			Add(new PlayerCollider(OnPlayer));
		}

		private void OnPlayer(Player player)
		{
			if (SaveData.Instance.Assists.Invincible)
			{
				player.Play("event:/game/general/assist_screenbottom");
				player.Bounce(base.Top);
			}
			else
			{
				player.Die(Vector2.Zero);
			}
		}

		public override void Update()
		{
			if (!Collidable)
			{
				Player player2 = base.Scene.Tracker.GetEntity<Player>();
				if (player2 != null && player2.Bottom < base.Top - 32f)
				{
					Collidable = true;
				}
			}
			else
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Top > base.Bottom + 32f)
				{
					Collidable = false;
				}
			}
			base.Update();
		}
	}
}
