using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IntroCar : JumpThru
	{
		private Image bodySprite;

		private Entity wheels;

		private float startY;

		private bool didHaveRider;

		public IntroCar(Vector2 position)
			: base(position, 25, safe: true)
		{
			startY = position.Y;
			base.Depth = 1;
			Add(bodySprite = new Image(GFX.Game["scenery/car/body"]));
			bodySprite.Origin = new Vector2(bodySprite.Width / 2f, bodySprite.Height);
			Hitbox roof = new Hitbox(25f, 4f, -15f, -17f);
			Hitbox hood = new Hitbox(19f, 4f, 8f, -11f);
			base.Collider = new ColliderList(roof, hood);
			SurfaceSoundIndex = 2;
		}

		public IntroCar(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Image sprite = new Image(GFX.Game["scenery/car/wheels"]);
			sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
			wheels = new Entity(Position);
			wheels.Add(sprite);
			wheels.Depth = 3;
			scene.Add(wheels);
			Level level = scene as Level;
			if (level.Session.Area.ID == 0)
			{
				IntroPavement pavement = new IntroPavement(new Vector2(level.Bounds.Left, base.Y), (int)(base.X - (float)level.Bounds.Left - 48f));
				pavement.Depth = -10001;
				level.Add(pavement);
				level.Add(new IntroCarBarrier(Position + new Vector2(32f, 0f), -10, Color.White));
				level.Add(new IntroCarBarrier(Position + new Vector2(41f, 0f), 5, Color.DarkGray));
			}
		}

		public override void Update()
		{
			bool hasRider = HasRider();
			if (base.Y > startY && (!hasRider || base.Y > startY + 1f))
			{
				float move = -10f * Engine.DeltaTime;
				MoveV(move);
			}
			if (base.Y <= startY && !didHaveRider && hasRider)
			{
				MoveV(2f);
			}
			if (didHaveRider && !hasRider)
			{
				Audio.Play("event:/game/00_prologue/car_up", Position);
			}
			didHaveRider = hasRider;
			base.Update();
		}

		public override int GetLandSoundIndex(Entity entity)
		{
			Audio.Play("event:/game/00_prologue/car_down", Position);
			return -1;
		}
	}
}
