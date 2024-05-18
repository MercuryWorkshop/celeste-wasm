using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PlayerSprite : Sprite
	{
		public const string Idle = "idle";

		public const string Shaking = "shaking";

		public const string FrontEdge = "edge";

		public const string LookUp = "lookUp";

		public const string Walk = "walk";

		public const string RunSlow = "runSlow";

		public const string RunFast = "runFast";

		public const string RunWind = "runWind";

		public const string RunStumble = "runStumble";

		public const string JumpSlow = "jumpSlow";

		public const string FallSlow = "fallSlow";

		public const string Fall = "fall";

		public const string JumpFast = "jumpFast";

		public const string FallFast = "fallFast";

		public const string FallBig = "bigFall";

		public const string LandInPose = "fallPose";

		public const string Tired = "tired";

		public const string TiredStill = "tiredStill";

		public const string WallSlide = "wallslide";

		public const string ClimbUp = "climbUp";

		public const string ClimbDown = "climbDown";

		public const string ClimbLookBackStart = "climbLookBackStart";

		public const string ClimbLookBack = "climbLookBack";

		public const string Dangling = "dangling";

		public const string Duck = "duck";

		public const string Dash = "dash";

		public const string Sleep = "sleep";

		public const string Sleeping = "asleep";

		public const string Flip = "flip";

		public const string Skid = "skid";

		public const string DreamDashIn = "dreamDashIn";

		public const string DreamDashLoop = "dreamDashLoop";

		public const string DreamDashOut = "dreamDashOut";

		public const string SwimIdle = "swimIdle";

		public const string SwimUp = "swimUp";

		public const string SwimDown = "swimDown";

		public const string StartStarFly = "startStarFly";

		public const string StarFly = "starFly";

		public const string StarMorph = "starMorph";

		public const string IdleCarry = "idle_carry";

		public const string RunCarry = "runSlow_carry";

		public const string JumpCarry = "jumpSlow_carry";

		public const string FallCarry = "fallSlow_carry";

		public const string PickUp = "pickup";

		public const string Throw = "throw";

		public const string Launch = "launch";

		public const string TentacleGrab = "tentacle_grab";

		public const string TentacleGrabbed = "tentacle_grabbed";

		public const string TentaclePull = "tentacle_pull";

		public const string TentacleDangling = "tentacle_dangling";

		public const string SitDown = "sitDown";

		private string spriteName;

		public int HairCount = 4;

		private static Dictionary<string, PlayerAnimMetadata> FrameMetadata = new Dictionary<string, PlayerAnimMetadata>(StringComparer.OrdinalIgnoreCase);

		public PlayerSpriteMode Mode { get; private set; }

		public Vector2 HairOffset
		{
			get
			{
				if (Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out var at))
				{
					return at.HairOffset;
				}
				return Vector2.Zero;
			}
		}

		public float CarryYOffset
		{
			get
			{
				if (Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out var at))
				{
					return (float)at.CarryYOffset * Scale.Y;
				}
				return 0f;
			}
		}

		public int HairFrame
		{
			get
			{
				if (Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out var at))
				{
					return at.Frame;
				}
				return 0;
			}
		}

		public bool HasHair
		{
			get
			{
				if (Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out var at))
				{
					return at.HasHair;
				}
				return false;
			}
		}

		public bool Running
		{
			get
			{
				if (base.LastAnimationID != null)
				{
					if (!(base.LastAnimationID == "flip"))
					{
						return base.LastAnimationID.StartsWith("run");
					}
					return true;
				}
				return false;
			}
		}

		public bool DreamDashing
		{
			get
			{
				if (base.LastAnimationID != null)
				{
					return base.LastAnimationID.StartsWith("dreamDash");
				}
				return false;
			}
		}

		public PlayerSprite(PlayerSpriteMode mode)
			: base(null, null)
		{
			Mode = mode;
			string sprite = "";
			switch (mode)
			{
			case PlayerSpriteMode.Madeline:
				sprite = "player";
				break;
			case PlayerSpriteMode.MadelineNoBackpack:
				sprite = "player_no_backpack";
				break;
			case PlayerSpriteMode.Badeline:
				sprite = "badeline";
				break;
			case PlayerSpriteMode.MadelineAsBadeline:
				sprite = "player_badeline";
				break;
			case PlayerSpriteMode.Playback:
				sprite = "player_playback";
				break;
			}
			spriteName = sprite;
			GFX.SpriteBank.CreateOn(this, sprite);
		}

		public override void Render()
		{
			Vector2 was = base.RenderPosition;
			base.RenderPosition = base.RenderPosition.Floor();
			base.Render();
			base.RenderPosition = was;
		}

		public static void CreateFramesMetadata(string sprite)
		{
			foreach (SpriteDataSource source in GFX.SpriteBank.SpriteData[sprite].Sources)
			{
				XmlElement xml = source.XML["Metadata"];
				string path = source.Path;
				if (xml == null)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(source.OverridePath))
				{
					path = source.OverridePath;
				}
				foreach (XmlElement e in xml.GetElementsByTagName("Frames"))
				{
					string animation = path + e.Attr("path", "");
					string[] hair = e.Attr("hair").Split('|');
					string[] carry = e.Attr("carry", "").Split(',');
					for (int i = 0; i < Math.Max(hair.Length, carry.Length); i++)
					{
						PlayerAnimMetadata metadata = new PlayerAnimMetadata();
						string key = animation + ((i < 10) ? "0" : "") + i;
						if (i == 0 && !GFX.Game.Has(key))
						{
							key = animation;
						}
						FrameMetadata[key] = metadata;
						if (i < hair.Length)
						{
							if (hair[i].Equals("x", StringComparison.OrdinalIgnoreCase) || hair[i].Length <= 0)
							{
								metadata.HasHair = false;
							}
							else
							{
								string[] parts = hair[i].Split(':');
								string[] sides = parts[0].Split(',');
								metadata.HasHair = true;
								metadata.HairOffset = new Vector2(Convert.ToInt32(sides[0]), Convert.ToInt32(sides[1]));
								metadata.Frame = ((parts.Length >= 2) ? Convert.ToInt32(parts[1]) : 0);
							}
						}
						if (i < carry.Length && carry[i].Length > 0)
						{
							metadata.CarryYOffset = int.Parse(carry[i]);
						}
					}
				}
			}
		}

		public static void ClearFramesMetadata()
		{
			FrameMetadata.Clear();
		}
	}
}
