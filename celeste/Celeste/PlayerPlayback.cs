using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PlayerPlayback : Entity
	{
		public Vector2 LastPosition;

		public List<Player.ChaserState> Timeline;

		public PlayerSprite Sprite;

		public PlayerHair Hair;

		private Vector2 start;

		private float time;

		private int index;

		private float loopDelay;

		private float startDelay;

		public float TrimStart;

		public float TrimEnd;

		public readonly float Duration;

		private float rangeMinX = float.MinValue;

		private float rangeMaxX = float.MaxValue;

		private bool ShowTrail;

		public Vector2 DashDirection { get; private set; }

		public float Time => time;

		public int FrameIndex => index;

		public int FrameCount => Timeline.Count;

		public PlayerPlayback(EntityData e, Vector2 offset)
			: this(e.Position + offset, PlayerSpriteMode.Playback, PlaybackData.Tutorials[e.Attr("tutorial")])
		{
			if (e.Nodes != null && e.Nodes.Length != 0)
			{
				rangeMinX = base.X;
				rangeMaxX = base.X;
				Vector2[] array = e.NodesOffset(offset);
				for (int i = 0; i < array.Length; i++)
				{
					Vector2 node = array[i];
					rangeMinX = Math.Min(rangeMinX, node.X);
					rangeMaxX = Math.Max(rangeMaxX, node.X);
				}
			}
			startDelay = 1f;
		}

		public PlayerPlayback(Vector2 start, PlayerSpriteMode sprite, List<Player.ChaserState> timeline)
		{
			this.start = start;
			base.Collider = new Hitbox(8f, 11f, -4f, -11f);
			Timeline = timeline;
			Position = start;
			time = 0f;
			index = 0;
			Duration = timeline[timeline.Count - 1].TimeStamp;
			TrimStart = 0f;
			TrimEnd = Duration;
			Sprite = new PlayerSprite(sprite);
			Add(Hair = new PlayerHair(Sprite));
			Add(Sprite);
			base.Collider = new Hitbox(8f, 4f, -4f, -4f);
			if (sprite == PlayerSpriteMode.Playback)
			{
				ShowTrail = true;
			}
			base.Depth = 9008;
			SetFrame(0);
			for (int i = 0; i < 10; i++)
			{
				Hair.AfterUpdate();
			}
			Visible = false;
			index = Timeline.Count;
		}

		private void Restart()
		{
			Audio.Play("event:/new_content/char/tutorial_ghost/appear", Position);
			Visible = true;
			time = TrimStart;
			index = 0;
			loopDelay = 0.25f;
			while (time > Timeline[index].TimeStamp)
			{
				index++;
			}
			SetFrame(index);
		}

		public void SetFrame(int index)
		{
			Player.ChaserState state = Timeline[index];
			string lastAnim = Sprite.CurrentAnimationID;
			bool lastOnGround = base.Scene != null && CollideCheck<Solid>(Position + new Vector2(0f, 1f));
			_ = DashDirection;
			Position = start + state.Position;
			if (state.Animation != Sprite.CurrentAnimationID && state.Animation != null && Sprite.Has(state.Animation))
			{
				Sprite.Play(state.Animation, restart: true);
			}
			Sprite.Scale = state.Scale;
			if (Sprite.Scale.X != 0f)
			{
				Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
			}
			Hair.Color = state.HairColor;
			if (Sprite.Mode == PlayerSpriteMode.Playback)
			{
				Sprite.Color = Hair.Color;
			}
			DashDirection = state.DashDirection;
			if (base.Scene == null)
			{
				return;
			}
			if (!lastOnGround && base.Scene != null && CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
			{
				Audio.Play("event:/new_content/char/tutorial_ghost/land", Position);
			}
			if (!(lastAnim != Sprite.CurrentAnimationID))
			{
				return;
			}
			string anim = Sprite.CurrentAnimationID;
			int frame = Sprite.CurrentAnimationFrame;
			switch (anim)
			{
			case "jumpFast":
			case "jumpSlow":
				Audio.Play("event:/new_content/char/tutorial_ghost/jump", Position);
				break;
			case "dreamDashIn":
				Audio.Play("event:/new_content/char/tutorial_ghost/dreamblock_sequence", Position);
				break;
			case "dash":
				if (DashDirection.Y != 0f)
				{
					Audio.Play("event:/new_content/char/tutorial_ghost/jump_super", Position);
				}
				else if (state.Scale.X > 0f)
				{
					Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_right", Position);
				}
				else
				{
					Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_left", Position);
				}
				break;
			case "climbUp":
			case "climbDown":
			case "wallslide":
				Audio.Play("event:/new_content/char/tutorial_ghost/grab", Position);
				break;
			default:
				if ((anim.Equals("runSlow_carry") && (frame == 0 || frame == 6)) || (anim.Equals("runFast") && (frame == 0 || frame == 6)) || (anim.Equals("runSlow") && (frame == 0 || frame == 6)) || (anim.Equals("walk") && (frame == 0 || frame == 6)) || (anim.Equals("runStumble") && frame == 6) || (anim.Equals("flip") && frame == 4) || (anim.Equals("runWind") && (frame == 0 || frame == 6)) || (anim.Equals("idleC") && Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack && (frame == 3 || frame == 6 || frame == 8 || frame == 11)) || (anim.Equals("carryTheoWalk") && (frame == 0 || frame == 6)) || (anim.Equals("push") && (frame == 8 || frame == 15)))
				{
					Audio.Play("event:/new_content/char/tutorial_ghost/footstep", Position);
				}
				break;
			}
		}

		public override void Update()
		{
			if (startDelay > 0f)
			{
				startDelay -= Engine.DeltaTime;
			}
			LastPosition = Position;
			base.Update();
			if (index >= Timeline.Count - 1 || Time >= TrimEnd)
			{
				if (Visible)
				{
					Audio.Play("event:/new_content/char/tutorial_ghost/disappear", Position);
				}
				Visible = false;
				Position = start;
				loopDelay -= Engine.DeltaTime;
				if (loopDelay <= 0f)
				{
					Player player = ((base.Scene == null) ? null : base.Scene.Tracker.GetEntity<Player>());
					if (player == null || (player.X > rangeMinX && player.X < rangeMaxX))
					{
						Restart();
					}
				}
			}
			else if (startDelay <= 0f)
			{
				SetFrame(index);
				time += Engine.DeltaTime;
				while (index < Timeline.Count - 1 && time >= Timeline[index + 1].TimeStamp)
				{
					index++;
				}
			}
			if (Visible && ShowTrail && base.Scene != null && base.Scene.OnInterval(0.1f))
			{
				TrailManager.Add(Position, Sprite, Hair, Sprite.Scale, Hair.Color, base.Depth + 1);
			}
		}
	}
}
