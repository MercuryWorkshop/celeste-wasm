using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_SaveTheo : CutsceneEntity
	{
		public const string Flag = "foundTheoInCrystal";

		private Player player;

		private TheoCrystal theo;

		private Vector2 playerEndPosition;

		private bool wasDashAssistOn;

		public CS05_SaveTheo(Player player)
		{
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			theo = level.Tracker.GetEntity<TheoCrystal>();
			playerEndPosition = theo.Position + new Vector2(-24f, 0f);
			wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			player.ForceCameraUpdate = true;
			level.Session.Audio.Music.Layer(6, 0f);
			level.Session.Audio.Apply();
			yield return player.DummyWalkTo(theo.X - 18f);
			player.Facing = Facings.Right;
			yield return Textbox.Say("ch5_found_theo", TryToBreakCrystal);
			yield return 0.25f;
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator TryToBreakCrystal()
		{
			base.Scene.Entities.FindFirst<TheoCrystalPedestal>().Collidable = true;
			yield return player.DummyWalkTo(theo.X);
			yield return 0.1f;
			yield return Level.ZoomTo(new Vector2(160f, 90f), 2f, 0.5f);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			yield return 1f;
			wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
			SaveData.Instance.Assists.DashAssist = false;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			MInput.Disabled = true;
			player.OverrideDashDirection = new Vector2(0f, -1f);
			player.StateMachine.Locked = false;
			player.StateMachine.State = player.StartDash();
			player.Dashes = 0;
			yield return 0.1f;
			while (!player.OnGround() || player.Speed.Y < 0f)
			{
				player.Dashes = 0;
				Input.MoveY.Value = -1;
				Input.MoveX.Value = 0;
				yield return null;
			}
			player.OverrideDashDirection = null;
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			MInput.Disabled = false;
			player.DummyAutoAnimate = true;
			yield return player.DummyWalkToExact((int)playerEndPosition.X, walkBackwards: true);
			yield return 1.5f;
		}

		public override void OnEnd(Level level)
		{
			SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
			player.Position = playerEndPosition;
			while (!player.OnGround())
			{
				player.MoveV(1f);
			}
			level.Camera.Position = player.CameraTarget;
			level.Session.SetFlag("foundTheoInCrystal");
			level.ResetZoom();
			level.Session.Audio.Music.Layer(6, 1f);
			level.Session.Audio.Apply();
			List<Follower> list = new List<Follower>(player.Leader.Followers);
			player.RemoveSelf();
			level.Add(player = new Player(player.Position, player.DefaultSpriteMode));
			foreach (Follower follower in list)
			{
				player.Leader.Followers.Add(follower);
				follower.Leader = player.Leader;
			}
			player.Facing = Facings.Right;
			player.IntroType = Player.IntroTypes.None;
			TheoCrystalPedestal theoCrystalPedestal = base.Scene.Entities.FindFirst<TheoCrystalPedestal>();
			theoCrystalPedestal.Collidable = false;
			theoCrystalPedestal.DroppedTheo = true;
			theo.Depth = 100;
			theo.OnPedestal = false;
			theo.Speed = Vector2.Zero;
			while (!theo.OnGround())
			{
				theo.MoveV(1f);
			}
		}
	}
}
