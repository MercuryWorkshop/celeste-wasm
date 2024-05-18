using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class VirtualTexture : VirtualAsset
	{
		private const int ByteArraySize = 524288;

		private const int ByteArrayCheckSize = 524256;

		internal static readonly byte[] buffer = new byte[67108864];

		internal static readonly byte[] bytes = new byte[524288];

		public Texture2D Texture;

		private Color color;

		public string Path { get; private set; }

		public bool IsDisposed
		{
			get
			{
				if (Texture != null && !Texture.IsDisposed)
				{
					return Texture.GraphicsDevice.IsDisposed;
				}
				return true;
			}
		}

		internal VirtualTexture(string path)
		{
			base.Name = (Path = path);
			Reload();
		}

		internal VirtualTexture(string name, int width, int height, Color color)
		{
			base.Name = name;
			base.Width = width;
			base.Height = height;
			this.color = color;
			Reload();
		}

		internal override void Unload()
		{
			if (Texture != null && !Texture.IsDisposed)
			{
				Texture.Dispose();
			}
			Texture = null;
		}

		internal unsafe override void Reload()
		{
			Unload();
			if (string.IsNullOrEmpty(Path))
			{
				Texture = new Texture2D(Engine.Instance.GraphicsDevice, base.Width, base.Height);
				Color[] data = new Color[base.Width * base.Height];
				fixed (Color* dataPtr = data)
				{
					for (int l = 0; l < data.Length; l++)
					{
						dataPtr[l] = color;
					}
				}
				Texture.SetData(data);
				return;
			}
			switch (System.IO.Path.GetExtension(Path))
			{
			case ".data":
			{
				using (FileStream stream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
				{
					stream.Read(bytes, 0, 524288);
					int pos = 0;
					int width = BitConverter.ToInt32(bytes, pos);
					int height = BitConverter.ToInt32(bytes, pos + 4);
					bool transparency = bytes[pos + 8] == 1;
					pos += 9;
					int size = width * height * 4;
					int index = 0;
					try
					{
						fixed (byte* bytesPtr = bytes)
						{
							fixed (byte* bufferPtr2 = VirtualTexture.buffer)
							{
								while (index < size)
								{
									int run = bytesPtr[pos] * 4;
									if (transparency)
									{
										byte alpha = bytesPtr[pos + 1];
										if (alpha > 0)
										{
											bufferPtr2[index] = bytesPtr[pos + 4];
											bufferPtr2[index + 1] = bytesPtr[pos + 3];
											bufferPtr2[index + 2] = bytesPtr[pos + 2];
											bufferPtr2[index + 3] = alpha;
											pos += 5;
										}
										else
										{
											bufferPtr2[index] = 0;
											bufferPtr2[index + 1] = 0;
											bufferPtr2[index + 2] = 0;
											bufferPtr2[index + 3] = 0;
											pos += 2;
										}
									}
									else
									{
										bufferPtr2[index] = bytesPtr[pos + 3];
										bufferPtr2[index + 1] = bytesPtr[pos + 2];
										bufferPtr2[index + 2] = bytesPtr[pos + 1];
										bufferPtr2[index + 3] = byte.MaxValue;
										pos += 4;
									}
									if (run > 4)
									{
										int j = index + 4;
										for (int m = index + run; j < m; j += 4)
										{
											bufferPtr2[j] = bufferPtr2[index];
											bufferPtr2[j + 1] = bufferPtr2[index + 1];
											bufferPtr2[j + 2] = bufferPtr2[index + 2];
											bufferPtr2[j + 3] = bufferPtr2[index + 3];
										}
									}
									index += run;
									if (pos > 524256)
									{
										int reset = 524288 - pos;
										for (int i = 0; i < reset; i++)
										{
											bytesPtr[i] = bytesPtr[pos + i];
										}
										stream.Read(bytes, reset, 524288 - reset);
										pos = 0;
									}
								}
							}
						}
					}
					finally
					{
					}
					Texture = new Texture2D(Engine.Graphics.GraphicsDevice, width, height);
					Texture.SetData(VirtualTexture.buffer, 0, size);
				}
				break;
			}
			case ".png":
			{
				using (FileStream stream3 = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
				{
					Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, stream3);
				}
				int size2 = Texture.Width * Texture.Height;
				Color[] buffer = new Color[size2];
				Texture.GetData(buffer, 0, size2);
				fixed (Color* bufferPtr = buffer)
				{
					for (int k = 0; k < size2; k++)
					{
						bufferPtr[k].R = (byte)((float)(int)bufferPtr[k].R * ((float)(int)bufferPtr[k].A / 255f));
						bufferPtr[k].G = (byte)((float)(int)bufferPtr[k].G * ((float)(int)bufferPtr[k].A / 255f));
						bufferPtr[k].B = (byte)((float)(int)bufferPtr[k].B * ((float)(int)bufferPtr[k].A / 255f));
					}
				}
				Texture.SetData(buffer, 0, size2);
				break;
			}
			case ".xnb":
			{
				string path = Path.Replace(".xnb", "");
				Texture = Engine.Instance.Content.Load<Texture2D>(path);
				break;
			}
			default:
			{
				using (FileStream stream2 = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
				{
					Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, stream2);
				}
				break;
			}
			}
			base.Width = Texture.Width;
			base.Height = Texture.Height;
		}

		public override void Dispose()
		{
			Unload();
			Texture = null;
			VirtualContent.Remove(this);
		}
	}
}
