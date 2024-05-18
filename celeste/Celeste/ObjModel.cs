using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class ObjModel : IDisposable
	{
		public class Mesh
		{
			public string Name = "";

			public ObjModel Model;

			public int VertexStart;

			public int VertexCount;
		}

		public List<Mesh> Meshes = new List<Mesh>();

		public VertexBuffer Vertices;

		private VertexPositionTexture[] verts;

		private bool ResetVertexBuffer()
		{
			if (Vertices == null || Vertices.IsDisposed || Vertices.GraphicsDevice.IsDisposed)
			{
				Vertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionTexture), verts.Length, BufferUsage.None);
				Vertices.SetData(verts);
				return true;
			}
			return false;
		}

		public void ReassignVertices()
		{
			if (!ResetVertexBuffer())
			{
				Vertices.SetData(verts);
			}
		}

		public void Draw(Effect effect)
		{
			ResetVertexBuffer();
			Engine.Graphics.GraphicsDevice.SetVertexBuffer(Vertices);
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.VertexCount / 3);
			}
		}

		public void Dispose()
		{
			Vertices.Dispose();
			Meshes = null;
		}

		public static ObjModel Create(string filename)
		{
			Path.GetDirectoryName(filename);
			ObjModel model = new ObjModel();
			List<VertexPositionTexture> verts = new List<VertexPositionTexture>();
			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			Mesh mesh = null;
			if (File.Exists(filename + ".export"))
			{
				using BinaryReader binaryReader = new BinaryReader(File.OpenRead(filename + ".export"));
				int count = binaryReader.ReadInt32();
				for (int m = 0; m < count; m++)
				{
					if (mesh != null)
					{
						mesh.VertexCount = verts.Count - mesh.VertexStart;
					}
					mesh = new Mesh();
					mesh.Name = binaryReader.ReadString();
					mesh.VertexStart = verts.Count;
					model.Meshes.Add(mesh);
					int vertexCount = binaryReader.ReadInt32();
					for (int k = 0; k < vertexCount; k++)
					{
						float a = binaryReader.ReadSingle();
						float b = binaryReader.ReadSingle();
						float c = binaryReader.ReadSingle();
						vertices.Add(new Vector3(a, b, c));
					}
					int uvCount = binaryReader.ReadInt32();
					for (int j = 0; j < uvCount; j++)
					{
						float a2 = binaryReader.ReadSingle();
						float b2 = binaryReader.ReadSingle();
						uvs.Add(new Vector2(a2, b2));
					}
					int faceCount = binaryReader.ReadInt32();
					for (int i = 0; i < faceCount; i++)
					{
						int pos = binaryReader.ReadInt32() - 1;
						int uv2 = binaryReader.ReadInt32() - 1;
						verts.Add(new VertexPositionTexture
						{
							Position = vertices[pos],
							TextureCoordinate = uvs[uv2]
						});
					}
				}
			}
			else
			{
				using StreamReader reader = new StreamReader(filename);
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					string[] data = line.Split(' ');
					if (data.Length == 0)
					{
						continue;
					}
					switch (data[0])
					{
					case "o":
						if (mesh != null)
						{
							mesh.VertexCount = verts.Count - mesh.VertexStart;
						}
						mesh = new Mesh();
						mesh.Name = data[1];
						mesh.VertexStart = verts.Count;
						model.Meshes.Add(mesh);
						break;
					case "v":
					{
						Vector3 position = new Vector3(Float(data[1]), Float(data[2]), Float(data[3]));
						vertices.Add(position);
						break;
					}
					case "vt":
					{
						Vector2 uv = new Vector2(Float(data[1]), Float(data[2]));
						uvs.Add(uv);
						break;
					}
					case "f":
					{
						for (int l = 1; l < Math.Min(4, data.Length); l++)
						{
							VertexPositionTexture vertex = default(VertexPositionTexture);
							string[] component = data[l].Split('/');
							if (component[0].Length > 0)
							{
								vertex.Position = vertices[int.Parse(component[0]) - 1];
							}
							if (component[1].Length > 0)
							{
								vertex.TextureCoordinate = uvs[int.Parse(component[1]) - 1];
							}
							verts.Add(vertex);
						}
						break;
					}
					}
				}
			}
			if (mesh != null)
			{
				mesh.VertexCount = verts.Count - mesh.VertexStart;
			}
			model.verts = verts.ToArray();
			model.ResetVertexBuffer();
			return model;
		}

		private static float Float(string data)
		{
			return float.Parse(data, CultureInfo.InvariantCulture);
		}
	}
}
