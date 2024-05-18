using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC05_Badeline : NPC
	{
		public const string FirstLevel = "c-00";

		public const string SecondLevel = "c-01";

		public const string ThirdLevel = "c-01b";

		private BadelineDummy shadow;

		private Vector2[] nodes;

		private Rectangle levelBounds;

		private SoundSource moveSfx;

		public NPC05_Badeline(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			nodes = data.NodesOffset(offset);
			Add(moveSfx = new SoundSource());
			Add(new TransitionListener
			{
				OnOut = delegate(float f)
				{
					if (shadow != null)
					{
						shadow.Hair.Alpha = 1f - Math.Min(1f, f * 2f);
						shadow.Sprite.Color = Color.White * shadow.Hair.Alpha;
						shadow.Light.Alpha = shadow.Hair.Alpha;
					}
				}
			});
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (base.Session.Level.Equals("c-00"))
			{
				if (!base.Session.GetLevelFlag("c-01"))
				{
					scene.Add(shadow = new BadelineDummy(Position));
					shadow.Depth = -1000000;
					Add(new Coroutine(FirstScene()));
				}
				else
				{
					RemoveSelf();
				}
			}
			else if (base.Session.Level.Equals("c-01"))
			{
				if (!base.Session.GetLevelFlag("c-01b"))
				{
					int startIndex;
					for (startIndex = 0; startIndex < 4 && base.Session.GetFlag(CS05_Badeline.GetFlag(startIndex)); startIndex++)
					{
					}
					if (startIndex >= 4)
					{
						RemoveSelf();
					}
					else
					{
						Vector2 pos = Position;
						if (startIndex > 0)
						{
							pos = nodes[startIndex - 1];
						}
						scene.Add(shadow = new BadelineDummy(pos));
						shadow.Depth = -1000000;
						Add(new Coroutine(SecondScene(startIndex)));
					}
				}
				else
				{
					RemoveSelf();
				}
			}
			levelBounds = (scene as Level).Bounds;
		}

		private IEnumerator FirstScene()
		{
			shadow.Sprite.Scale.X = -1f;
			shadow.FloatSpeed = 150f;
			bool playerHasFallen = false;
			bool startedMusic = false;
			Player player;
			while (true)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Y > (float)(Level.Bounds.Top + 180) && !player.OnGround() && !playerHasFallen)
				{
					player.StateMachine.State = 20;
					playerHasFallen = true;
				}
				if (player != null && playerHasFallen && !startedMusic && player.OnGround())
				{
					Level.Session.Audio.Music.Event = "event:/music/lvl5/middle_temple";
					Level.Session.Audio.Apply();
					startedMusic = true;
				}
				if (player != null && player.X > base.X - 64f && player.Y > base.Y - 32f)
				{
					break;
				}
				yield return null;
			}
			MoveToNode(0, chatMove: false);
			while (shadow.X < (float)(Level.Bounds.Right + 8))
			{
				yield return null;
				if (player.X > shadow.X - 24f)
				{
					shadow.X = player.X + 24f;
				}
			}
			base.Scene.Remove(shadow);
			RemoveSelf();
		}

		private IEnumerator SecondScene(int startIndex)
		{
			shadow.Sprite.Scale.X = -1f;
			shadow.FloatSpeed = 300f;
			shadow.FloatAccel = 400f;
			yield return 0.1f;
			int index = startIndex;
			while (index < nodes.Length)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				while (player == null || (player.Position - shadow.Position).Length() > 70f)
				{
					yield return null;
				}
				if (index < 4 && !base.Session.GetFlag(CS05_Badeline.GetFlag(index)))
				{
					CS05_Badeline cutscene = new CS05_Badeline(player, this, shadow, index);
					base.Scene.Add(cutscene);
					yield return null;
					while (cutscene.Scene != null)
					{
						yield return null;
					}
					index++;
				}
			}
			base.Tag |= Tags.TransitionUpdate;
			shadow.Tag |= Tags.TransitionUpdate;
			base.Scene.Remove(shadow);
			RemoveSelf();
		}

		public void MoveToNode(int index, bool chatMove = true)
		{
			if (chatMove)
			{
				moveSfx.Play("event:/char/badeline/temple_move_chats");
			}
			else
			{
				SoundEmitter.Play("event:/char/badeline/temple_move_first", this);
			}
			Vector2 start = shadow.Position;
			Vector2 end = nodes[index];
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.5f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				shadow.Position = Vector2.Lerp(start, end, t.Eased);
				if (base.Scene.OnInterval(0.03f))
				{
					SceneAs<Level>().ParticlesFG.Emit(BadelineOldsite.P_Vanish, 2, shadow.Position + new Vector2(0f, -6f), Vector2.One * 2f);
				}
				if (t.Eased >= 0.1f && t.Eased <= 0.9f && base.Scene.OnInterval(0.05f))
				{
					TrailManager.Add(shadow, Color.Red, 0.5f);
				}
			};
			Add(tween);
		}

		public void SnapToNode(int index)
		{
			shadow.Position = nodes[index];
		}
	}
}
