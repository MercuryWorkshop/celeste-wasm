using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public static class PlaybackData
	{
		public static Dictionary<string, List<Player.ChaserState>> Tutorials = new Dictionary<string, List<Player.ChaserState>>();

		public static void Load()
		{
			string[] files = Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Tutorials"));
			foreach (string path in files)
			{
				string name = Path.GetFileNameWithoutExtension(path);
				List<Player.ChaserState> timeline = Import(File.ReadAllBytes(path));
				Tutorials[name] = timeline;
			}
		}

		public static void Export(List<Player.ChaserState> list, string path)
		{
			float startTime = list[0].TimeStamp;
			Vector2 startPosition = list[0].Position;
			using BinaryWriter writer = new BinaryWriter(File.OpenWrite(path));
			writer.Write("TIMELINE");
			writer.Write(2);
			writer.Write(list.Count);
			foreach (Player.ChaserState item in list)
			{
				writer.Write(item.Position.X - startPosition.X);
				writer.Write(item.Position.Y - startPosition.Y);
				writer.Write(item.TimeStamp - startTime);
				writer.Write(item.Animation);
				writer.Write((int)item.Facing);
				writer.Write(item.OnGround);
				Color hairColor = item.HairColor;
				writer.Write(hairColor.R);
				hairColor = item.HairColor;
				writer.Write(hairColor.G);
				hairColor = item.HairColor;
				writer.Write(hairColor.B);
				writer.Write(item.Depth);
				writer.Write(item.Scale.X);
				writer.Write(item.Scale.Y);
				writer.Write(item.DashDirection.X);
				writer.Write(item.DashDirection.Y);
			}
		}

		public static List<Player.ChaserState> Import(byte[] buffer)
		{
			List<Player.ChaserState> list = new List<Player.ChaserState>();
			using BinaryReader reader = new BinaryReader(new MemoryStream(buffer));
			int version = 1;
			if (reader.ReadString() == "TIMELINE")
			{
				version = reader.ReadInt32();
			}
			else
			{
				reader.BaseStream.Seek(0L, SeekOrigin.Begin);
			}
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Player.ChaserState state = default(Player.ChaserState);
				state.Position.X = reader.ReadSingle();
				state.Position.Y = reader.ReadSingle();
				state.TimeStamp = reader.ReadSingle();
				state.Animation = reader.ReadString();
				state.Facing = (Facings)reader.ReadInt32();
				state.OnGround = reader.ReadBoolean();
				state.HairColor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);
				state.Depth = reader.ReadInt32();
				state.Sounds = 0;
				if (version == 1)
				{
					state.Scale = new Vector2((float)state.Facing, 1f);
					state.DashDirection = Vector2.Zero;
				}
				else
				{
					state.Scale.X = reader.ReadSingle();
					state.Scale.Y = reader.ReadSingle();
					state.DashDirection.X = reader.ReadSingle();
					state.DashDirection.Y = reader.ReadSingle();
				}
				list.Add(state);
			}
			return list;
		}
	}
}
