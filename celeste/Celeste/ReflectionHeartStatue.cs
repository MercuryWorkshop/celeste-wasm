using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ReflectionHeartStatue : Entity
	{
		public class Torch : Entity
		{
			public string[] Code;

			private Sprite sprite;

			private Session session;

			public string Flag => "heartTorch_" + Index;

			public bool Activated => session.GetFlag(Flag);

			public int Index { get; private set; }

			public Torch(Session session, Vector2 position, int index, string[] code)
				: base(position)
			{
				Index = index;
				Code = code;
				base.Depth = 8999;
				this.session = session;
				Image hint = new Image(GFX.Game.GetAtlasSubtextures("objects/reflectionHeart/hint")[index]);
				hint.CenterOrigin();
				hint.Position = new Vector2(0f, 28f);
				Add(hint);
				Add(sprite = new Sprite(GFX.Game, "objects/reflectionHeart/torch"));
				sprite.AddLoop("idle", "", 0f, default(int));
				sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
				sprite.Play("idle");
				sprite.Origin = new Vector2(32f, 64f);
			}

			public override void Added(Scene scene)
			{
				base.Added(scene);
				if (Activated)
				{
					PlayLit();
				}
			}

			public void Activate()
			{
				session.SetFlag(Flag);
				Alarm.Set(this, 0.2f, delegate
				{
					Audio.Play("event:/game/06_reflection/supersecret_torch_" + (Index + 1), Position);
					PlayLit();
				});
			}

			private void PlayLit()
			{
				sprite.Play("lit");
				sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
				Add(new VertexLight(Color.LightSeaGreen, 1f, 24, 48));
				Add(new BloomPoint(0.6f, 16f));
			}
		}

		private static readonly string[] Code = new string[6] { "U", "L", "DR", "UR", "L", "UL" };

		private const string FlagPrefix = "heartTorch_";

		private List<string> currentInputs = new List<string>();

		private List<Torch> torches = new List<Torch>();

		private Vector2 offset;

		private Vector2[] nodes;

		private DashListener dashListener;

		private bool enabled;

		public ReflectionHeartStatue(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			this.offset = offset;
			nodes = data.Nodes;
			base.Depth = 8999;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session session = (base.Scene as Level).Session;
			Image img = new Image(GFX.Game["objects/reflectionHeart/statue"]);
			img.JustifyOrigin(0.5f, 1f);
			img.Origin.Y -= 1f;
			Add(img);
			List<string[]> codes = new List<string[]>();
			codes.Add(Code);
			codes.Add(FlipCode(h: true, v: false));
			codes.Add(FlipCode(h: false, v: true));
			codes.Add(FlipCode(h: true, v: true));
			for (int j = 0; j < 4; j++)
			{
				Torch torch = new Torch(session, offset + nodes[j], j, codes[j]);
				base.Scene.Add(torch);
				torches.Add(torch);
			}
			int length = Code.Length;
			Vector2 center = nodes[4] + offset - Position;
			for (int i = 0; i < length; i++)
			{
				Image gem = new Image(GFX.Game["objects/reflectionHeart/gem"]);
				gem.CenterOrigin();
				gem.Color = ForsakenCitySatellite.Colors[Code[i]];
				gem.Position = center + new Vector2(((float)i - (float)(length - 1) / 2f) * 24f, 0f);
				Add(gem);
				Add(new BloomPoint(gem.Position, 0.3f, 12f));
			}
			enabled = !session.HeartGem;
			if (!enabled)
			{
				return;
			}
			Add(dashListener = new DashListener());
			dashListener.OnDash = delegate(Vector2 dir)
			{
				string text = "";
				if (dir.Y < 0f)
				{
					text = "U";
				}
				else if (dir.Y > 0f)
				{
					text = "D";
				}
				if (dir.X < 0f)
				{
					text += "L";
				}
				else if (dir.X > 0f)
				{
					text += "R";
				}
				int num = 0;
				if (dir.X < 0f && dir.Y == 0f)
				{
					num = 1;
				}
				else if (dir.X < 0f && dir.Y < 0f)
				{
					num = 2;
				}
				else if (dir.X == 0f && dir.Y < 0f)
				{
					num = 3;
				}
				else if (dir.X > 0f && dir.Y < 0f)
				{
					num = 4;
				}
				else if (dir.X > 0f && dir.Y == 0f)
				{
					num = 5;
				}
				else if (dir.X > 0f && dir.Y > 0f)
				{
					num = 6;
				}
				else if (dir.X == 0f && dir.Y > 0f)
				{
					num = 7;
				}
				else if (dir.X < 0f && dir.Y > 0f)
				{
					num = 8;
				}
				Audio.Play("event:/game/06_reflection/supersecret_dashflavour", base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero, "dash_direction", num);
				currentInputs.Add(text);
				if (currentInputs.Count > Code.Length)
				{
					currentInputs.RemoveAt(0);
				}
				foreach (Torch current in torches)
				{
					if (!current.Activated && CheckCode(current.Code))
					{
						current.Activate();
					}
				}
				CheckIfAllActivated();
			};
			CheckIfAllActivated(skipActivateRoutine: true);
		}

		private string[] FlipCode(bool h, bool v)
		{
			string[] newCode = new string[Code.Length];
			for (int i = 0; i < Code.Length; i++)
			{
				string id = Code[i];
				if (h)
				{
					id = (id.Contains('L') ? id.Replace('L', 'R') : id.Replace('R', 'L'));
				}
				if (v)
				{
					id = (id.Contains('U') ? id.Replace('U', 'D') : id.Replace('D', 'U'));
				}
				newCode[i] = id;
			}
			return newCode;
		}

		private bool CheckCode(string[] code)
		{
			if (currentInputs.Count < code.Length)
			{
				return false;
			}
			for (int i = 0; i < code.Length; i++)
			{
				if (!currentInputs[i].Equals(code[i]))
				{
					return false;
				}
			}
			return true;
		}

		private void CheckIfAllActivated(bool skipActivateRoutine = false)
		{
			if (!enabled)
			{
				return;
			}
			bool allActivated = true;
			foreach (Torch torch in torches)
			{
				if (!torch.Activated)
				{
					allActivated = false;
				}
			}
			if (allActivated)
			{
				Activate(skipActivateRoutine);
			}
		}

		public void Activate(bool skipActivateRoutine)
		{
			enabled = false;
			if (skipActivateRoutine)
			{
				base.Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
			}
			else
			{
				Add(new Coroutine(ActivateRoutine()));
			}
		}

		private IEnumerator ActivateRoutine()
		{
			yield return 0.533f;
			Audio.Play("event:/game/06_reflection/supersecret_heartappear");
			Entity dummy = new Entity(Position + new Vector2(0f, -52f))
			{
				Depth = 1
			};
			base.Scene.Add(dummy);
			Image white = new Image(GFX.Game["collectables/heartgem/white00"]);
			white.CenterOrigin();
			white.Scale = Vector2.Zero;
			dummy.Add(white);
			BloomPoint glow = new BloomPoint(0f, 16f);
			dummy.Add(glow);
			List<Entity> absorbs = new List<Entity>();
			for (int i = 0; i < 20; i++)
			{
				AbsorbOrb orb = new AbsorbOrb(Position + new Vector2(0f, -20f), dummy);
				base.Scene.Add(orb);
				absorbs.Add(orb);
				yield return null;
			}
			yield return 0.8f;
			float duration = 0.6f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				white.Scale = Vector2.One * p;
				glow.Alpha = p;
				(base.Scene as Level).Shake();
				yield return null;
			}
			foreach (Entity item in absorbs)
			{
				item.RemoveSelf();
			}
			(base.Scene as Level).Flash(Color.White);
			base.Scene.Remove(dummy);
			base.Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
		}

		public override void Update()
		{
			if (dashListener != null && !enabled)
			{
				Remove(dashListener);
				dashListener = null;
			}
			base.Update();
		}
	}
}
