using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Oshiro_Lobby : NPC
	{
		public static ParticleType P_AppearSpark;

		private float startX;

		public NPC03_Oshiro_Lobby(Vector2 position)
			: base(position)
		{
			Add(Sprite = new OshiroSprite(-1));
			Sprite.Visible = false;
			MTexture hover = GFX.Gui["hover/resort"];
			if (GFX.Gui.Has("hover/resort_" + Settings.Instance.Language))
			{
				hover = GFX.Gui["hover/resort_" + Settings.Instance.Language];
			}
			Add(Talker = new TalkComponent(new Rectangle(-30, -16, 42, 32), new Vector2(-12f, -24f), OnTalk, new TalkComponent.HoverDisplay
			{
				Texture = hover,
				InputPosition = new Vector2(0f, -75f),
				SfxIn = "event:/ui/game/hotspot_note_in",
				SfxOut = "event:/ui/game/hotspot_note_out"
			}));
			Talker.PlayerMustBeFacing = false;
			MoveAnim = "move";
			IdleAnim = "idle";
			base.Depth = 9001;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("oshiro_resort_talked_1"))
			{
				base.Session.Audio.Music.Event = "event:/music/lvl3/explore";
				base.Session.Audio.Music.Progress = 1;
				base.Session.Audio.Apply();
				RemoveSelf();
			}
			else
			{
				base.Session.Audio.Music.Event = null;
				base.Session.Audio.Apply();
			}
			scene.Add(new OshiroLobbyBell(new Vector2(base.X - 14f, base.Y)));
			startX = Position.X;
		}

		private void OnTalk(Player player)
		{
			base.Scene.Add(new CS03_OshiroLobby(player, this));
			Talker.Enabled = false;
		}

		public override void Update()
		{
			base.Update();
			if (base.X >= startX + 12f)
			{
				base.Depth = 1000;
			}
		}
	}
}
