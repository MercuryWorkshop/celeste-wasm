using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DashSwitch : Solid
	{
		public enum Sides
		{
			Up,
			Down,
			Left,
			Right
		}

		public static ParticleType P_PressA;

		public static ParticleType P_PressB;

		public static ParticleType P_PressAMirror;

		public static ParticleType P_PressBMirror;

		private Sides side;

		private Vector2 pressedTarget;

		private bool pressed;

		private Vector2 pressDirection;

		private float speedY;

		private float startY;

		private bool persistent;

		private EntityID id;

		private bool mirrorMode;

		private bool playerWasOn;

		private bool allGates;

		private Sprite sprite;

		private string FlagName => GetFlagName(id);

		public DashSwitch(Vector2 position, Sides side, bool persistent, bool allGates, EntityID id, string spriteName)
			: base(position, 0f, 0f, safe: true)
		{
			this.side = side;
			this.persistent = persistent;
			this.allGates = allGates;
			this.id = id;
			mirrorMode = spriteName != "default";
			Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
			sprite.Play("idle");
			if (side == Sides.Up || side == Sides.Down)
			{
				base.Collider.Width = 16f;
				base.Collider.Height = 8f;
			}
			else
			{
				base.Collider.Width = 8f;
				base.Collider.Height = 16f;
			}
			switch (side)
			{
			case Sides.Down:
				sprite.Position = new Vector2(8f, 8f);
				sprite.Rotation = (float)Math.PI / 2f;
				pressedTarget = Position + Vector2.UnitY * 8f;
				pressDirection = Vector2.UnitY;
				startY = base.Y;
				break;
			case Sides.Up:
				sprite.Position = new Vector2(8f, 0f);
				sprite.Rotation = -(float)Math.PI / 2f;
				pressedTarget = Position + Vector2.UnitY * -8f;
				pressDirection = -Vector2.UnitY;
				break;
			case Sides.Right:
				sprite.Position = new Vector2(8f, 8f);
				sprite.Rotation = 0f;
				pressedTarget = Position + Vector2.UnitX * 8f;
				pressDirection = Vector2.UnitX;
				break;
			case Sides.Left:
				sprite.Position = new Vector2(0f, 8f);
				sprite.Rotation = (float)Math.PI;
				pressedTarget = Position + Vector2.UnitX * -8f;
				pressDirection = -Vector2.UnitX;
				break;
			}
			OnDashCollide = OnDashed;
		}

		public static DashSwitch Create(EntityData data, Vector2 offset, EntityID id)
		{
			Vector2 at = data.Position + offset;
			bool per = data.Bool("persistent");
			bool all = data.Bool("allGates");
			string spriteName = data.Attr("sprite", "default");
			if (data.Name.Equals("dashSwitchH"))
			{
				if (data.Bool("leftSide"))
				{
					return new DashSwitch(at, Sides.Left, per, all, id, spriteName);
				}
				return new DashSwitch(at, Sides.Right, per, all, id, spriteName);
			}
			if (data.Bool("ceiling"))
			{
				return new DashSwitch(at, Sides.Up, per, all, id, spriteName);
			}
			return new DashSwitch(at, Sides.Down, per, all, id, spriteName);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!persistent || !SceneAs<Level>().Session.GetFlag(FlagName))
			{
				return;
			}
			sprite.Play("pushed");
			Position = pressedTarget - pressDirection * 2f;
			pressed = true;
			Collidable = false;
			if (allGates)
			{
				foreach (TempleGate gate in base.Scene.Tracker.GetEntities<TempleGate>())
				{
					if (gate.Type == TempleGate.Types.NearestSwitch && gate.LevelID == id.Level)
					{
						gate.StartOpen();
					}
				}
				return;
			}
			GetGate()?.StartOpen();
		}

		public override void Update()
		{
			base.Update();
			if (pressed || side != Sides.Down)
			{
				return;
			}
			Player player = GetPlayerOnTop();
			if (player != null)
			{
				if (player.Holding != null)
				{
					OnDashed(player, Vector2.UnitY);
				}
				else
				{
					if (speedY < 0f)
					{
						speedY = 0f;
					}
					speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
					MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
					if (!playerWasOn)
					{
						Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
					}
				}
			}
			else
			{
				if (speedY > 0f)
				{
					speedY = 0f;
				}
				speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
				MoveTowardsY(startY, (0f - speedY) * Engine.DeltaTime);
				if (playerWasOn)
				{
					Audio.Play("event:/game/05_mirror_temple/button_return", Position);
				}
			}
			playerWasOn = player != null;
		}

		public DashCollisionResults OnDashed(Player player, Vector2 direction)
		{
			if (!pressed && direction == pressDirection)
			{
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
				sprite.Play("push");
				pressed = true;
				MoveTo(pressedTarget);
				Collidable = false;
				Position -= pressDirection * 2f;
				SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressAMirror : P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
				SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressBMirror : P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
				if (allGates)
				{
					foreach (TempleGate gate in base.Scene.Tracker.GetEntities<TempleGate>())
					{
						if (gate.Type == TempleGate.Types.NearestSwitch && gate.LevelID == id.Level)
						{
							gate.SwitchOpen();
						}
					}
				}
				else
				{
					GetGate()?.SwitchOpen();
				}
				base.Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(base.X - (float)(base.Scene as Level).Bounds.Center.X));
				if (persistent)
				{
					SceneAs<Level>().Session.SetFlag(FlagName);
				}
			}
			return DashCollisionResults.NormalCollision;
		}

		private TempleGate GetGate()
		{
			List<Entity> entities = base.Scene.Tracker.GetEntities<TempleGate>();
			TempleGate nearest = null;
			float nearestDistSq = 0f;
			foreach (TempleGate gate in entities)
			{
				if (gate.Type == TempleGate.Types.NearestSwitch && !gate.ClaimedByASwitch && gate.LevelID == id.Level)
				{
					float distSq = Vector2.DistanceSquared(Position, gate.Position);
					if (nearest == null || distSq < nearestDistSq)
					{
						nearest = gate;
						nearestDistSq = distSq;
					}
				}
			}
			if (nearest != null)
			{
				nearest.ClaimedByASwitch = true;
			}
			return nearest;
		}

		public static string GetFlagName(EntityID id)
		{
			return "dashSwitch_" + id.Key;
		}
	}
}
