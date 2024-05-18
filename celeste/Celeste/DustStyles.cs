using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public static class DustStyles
	{
		public struct DustStyle
		{
			public Vector3[] EdgeColors;

			public Color EyeColor;

			public string EyeTextures;
		}

		public static Dictionary<int, DustStyle> Styles = new Dictionary<int, DustStyle>
		{
			{
				3,
				new DustStyle
				{
					EdgeColors = new Vector3[3]
					{
						Calc.HexToColor("f25a10").ToVector3(),
						Calc.HexToColor("ff0000").ToVector3(),
						Calc.HexToColor("f21067").ToVector3()
					},
					EyeColor = Color.Red,
					EyeTextures = "danger/dustcreature/eyes"
				}
			},
			{
				5,
				new DustStyle
				{
					EdgeColors = new Vector3[3]
					{
						Calc.HexToColor("245ebb").ToVector3(),
						Calc.HexToColor("17a0ff").ToVector3(),
						Calc.HexToColor("17a0ff").ToVector3()
					},
					EyeColor = Calc.HexToColor("245ebb"),
					EyeTextures = "danger/dustcreature/templeeyes"
				}
			}
		};

		public static DustStyle Get(Session session)
		{
			if (!Styles.ContainsKey(session.Area.ID))
			{
				return Styles[3];
			}
			return Styles[session.Area.ID];
		}

		public static DustStyle Get(Scene scene)
		{
			return Get((scene as Level).Session);
		}
	}
}
