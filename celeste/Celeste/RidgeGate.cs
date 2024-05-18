using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RidgeGate : Solid
	{
		private Vector2? node;

		public RidgeGate(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("ridge", defaultValue: true))
		{
		}

		public RidgeGate(Vector2 position, float width, float height, Vector2? node, bool ridgeImage = true)
			: base(position, width, height, safe: true)
		{
			this.node = node;
			Add(new Image(GFX.Game[ridgeImage ? "objects/ridgeGate" : "objects/farewellGate"]));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (node.HasValue && CollideCheck<Player>())
			{
				Visible = (Collidable = false);
				Vector2 moveTo = Position;
				Position = node.Value;
				Add(new Coroutine(EnterSequence(moveTo)));
			}
		}

		private IEnumerator EnterSequence(Vector2 moveTo)
		{
			Visible = (Collidable = true);
			yield return 0.25f;
			Audio.Play("event:/game/04_cliffside/stone_blockade", Position);
			yield return 0.25f;
			Vector2 start = Position;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				MoveTo(Vector2.Lerp(start, moveTo, t.Eased));
			};
			Add(tween);
		}
	}
}
