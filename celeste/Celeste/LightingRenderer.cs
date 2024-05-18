using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class LightingRenderer : Renderer
	{
		private struct VertexPositionColorMaskTexture : IVertexType
		{
			public Vector3 Position;

			public Color Color;

			public Color Mask;

			public Vector2 Texcoord;

			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 1), new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

			VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
		}

		public static BlendState GradientBlendState = new BlendState
		{
			AlphaBlendFunction = BlendFunction.Max,
			ColorBlendFunction = BlendFunction.Max,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One
		};

		public static BlendState OccludeBlendState = new BlendState
		{
			AlphaBlendFunction = BlendFunction.Min,
			ColorBlendFunction = BlendFunction.Min,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One
		};

		public const int TextureSize = 1024;

		public const int TextureSplit = 4;

		public const int Channels = 4;

		public const int Padding = 8;

		public const int CircleSegments = 20;

		private const int Cells = 16;

		private const int MaxLights = 64;

		private const int Radius = 128;

		private const int LightRadius = 120;

		public Color BaseColor = Color.Black;

		public float Alpha = 0.1f;

		private VertexPositionColor[] verts = new VertexPositionColor[11520];

		private VertexPositionColorMaskTexture[] resultVerts = new VertexPositionColorMaskTexture[384];

		private int[] indices = new int[11520];

		private int vertexCount;

		private int indexCount;

		private VertexLight[] lights;

		private VertexLight spotlight;

		private bool inSpotlight;

		private float nonSpotlightAlphaMultiplier = 1f;

		private Vector3[] angles = new Vector3[20];

		public LightingRenderer()
		{
			lights = new VertexLight[64];
			for (int i = 0; i < 20; i++)
			{
				angles[i] = new Vector3(Calc.AngleToVector((float)i / 20f * ((float)Math.PI * 2f), 1f), 0f);
			}
		}

		public VertexLight SetSpotlight(VertexLight light)
		{
			spotlight = light;
			inSpotlight = true;
			return light;
		}

		public void UnsetSpotlight()
		{
			inSpotlight = false;
		}

		public override void Update(Scene scene)
		{
			nonSpotlightAlphaMultiplier = Calc.Approach(nonSpotlightAlphaMultiplier, inSpotlight ? 0f : 1f, Engine.DeltaTime * 2f);
			base.Update(scene);
		}

		public override void BeforeRender(Scene scene)
		{
			Level level = scene as Level;
			Camera camera = level.Camera;
			for (int j = 0; j < 64; j++)
			{
				if (lights[j] != null && lights[j].Entity.Scene != scene)
				{
					lights[j].Index = -1;
					lights[j] = null;
				}
			}
			foreach (VertexLight light2 in scene.Tracker.GetComponents<VertexLight>())
			{
				if (light2.Entity != null && light2.Entity.Visible && light2.Visible && light2.Alpha > 0f && light2.Color.A > 0 && light2.Center.X + light2.EndRadius > camera.X && light2.Center.Y + light2.EndRadius > camera.Y && light2.Center.X - light2.EndRadius < camera.X + 320f && light2.Center.Y - light2.EndRadius < camera.Y + 180f)
				{
					if (light2.Index < 0)
					{
						light2.Dirty = true;
						for (int i = 0; i < 64; i++)
						{
							if (lights[i] == null)
							{
								lights[i] = light2;
								light2.Index = i;
								break;
							}
						}
					}
					if (light2.LastPosition != light2.Position || light2.LastEntityPosition != light2.Entity.Position || light2.Dirty)
					{
						light2.LastPosition = light2.Position;
						light2.InSolid = false;
						foreach (Solid item in scene.CollideAll<Solid>(light2.Center))
						{
							if (item.DisableLightsInside)
							{
								light2.InSolid = true;
								break;
							}
						}
						if (!light2.InSolid)
						{
							light2.LastNonSolidPosition = light2.Center;
						}
						if (light2.InSolid && !light2.Started)
						{
							light2.InSolidAlphaMultiplier = 0f;
						}
					}
					if (light2.Entity.Position != light2.LastEntityPosition)
					{
						light2.Dirty = true;
						light2.LastEntityPosition = light2.Entity.Position;
					}
					light2.Started = true;
				}
				else if (light2.Index >= 0)
				{
					lights[light2.Index] = null;
					light2.Index = -1;
					light2.Started = false;
				}
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.LightBuffer);
			Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			Matrix matrix = Matrix.CreateScale(0.0009765625f) * Matrix.CreateScale(2f, -2f, 1f) * Matrix.CreateTranslation(-1f, 1f, 0f);
			ClearDirtyLights(matrix);
			DrawLightGradients(matrix);
			DrawLightOccluders(matrix, level);
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Light);
			Engine.Graphics.GraphicsDevice.Clear(BaseColor);
			Engine.Graphics.GraphicsDevice.Textures[0] = (RenderTarget2D)GameplayBuffers.LightBuffer;
			StartDrawingPrimitives();
			for (int index = 0; index < 64; index++)
			{
				VertexLight light = lights[index];
				if (light == null)
				{
					continue;
				}
				light.Dirty = false;
				float alpha = light.Alpha * light.InSolidAlphaMultiplier;
				if (nonSpotlightAlphaMultiplier < 1f && light != spotlight)
				{
					alpha *= nonSpotlightAlphaMultiplier;
				}
				if (alpha > 0f && light.Color.A > 0 && light.EndRadius >= 2f)
				{
					int radius = 128;
					while (light.EndRadius <= (float)(radius / 2))
					{
						radius /= 2;
					}
					DrawLight(index, light.InSolid ? light.LastNonSolidPosition : light.Center, light.Color * alpha, radius);
				}
			}
			if (vertexCount > 0)
			{
				GFX.DrawIndexedVertices(camera.Matrix, resultVerts, vertexCount, indices, indexCount / 3, GFX.FxLighting, BlendState.Additive);
			}
			GaussianBlur.Blur((RenderTarget2D)GameplayBuffers.Light, GameplayBuffers.TempA, GameplayBuffers.Light);
		}

		private void ClearDirtyLights(Matrix matrix)
		{
			StartDrawingPrimitives();
			for (int index = 0; index < 64; index++)
			{
				VertexLight light = lights[index];
				if (light != null && light.Dirty)
				{
					SetClear(index);
				}
			}
			if (vertexCount <= 0)
			{
				return;
			}
			Engine.Instance.GraphicsDevice.BlendState = OccludeBlendState;
			GFX.FxPrimitive.Parameters["World"].SetValue(matrix);
			foreach (EffectPass pass in GFX.FxPrimitive.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, vertexCount, indices, 0, indexCount / 3);
			}
		}

		private void DrawLightGradients(Matrix matrix)
		{
			StartDrawingPrimitives();
			int count = 0;
			for (int index = 0; index < 64; index++)
			{
				VertexLight light = lights[index];
				if (light != null && light.Dirty)
				{
					count++;
					SetGradient(index, Calc.Clamp(light.StartRadius, 0f, 120f), Calc.Clamp(light.EndRadius, 0f, 120f));
				}
			}
			if (vertexCount <= 0)
			{
				return;
			}
			Engine.Instance.GraphicsDevice.BlendState = GradientBlendState;
			GFX.FxPrimitive.Parameters["World"].SetValue(matrix);
			foreach (EffectPass pass in GFX.FxPrimitive.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, vertexCount, indices, 0, indexCount / 3);
			}
		}

		private void DrawLightOccluders(Matrix matrix, Level level)
		{
			StartDrawingPrimitives();
			Rectangle mapBounds = level.Session.MapData.TileBounds;
			List<Component> occluders = level.Tracker.GetComponents<LightOcclude>();
			List<Component> cutouts = level.Tracker.GetComponents<EffectCutout>();
			foreach (LightOcclude occluder2 in occluders)
			{
				if (occluder2.Visible && occluder2.Entity.Visible)
				{
					occluder2.RenderBounds = new Rectangle(occluder2.Left, occluder2.Top, occluder2.Width, occluder2.Height);
				}
			}
			for (int index = 0; index < 64; index++)
			{
				VertexLight light = lights[index];
				if (light == null || !light.Dirty)
				{
					continue;
				}
				Vector2 position = (light.InSolid ? light.LastNonSolidPosition : light.Center);
				Rectangle lightBounds = new Rectangle((int)(position.X - light.EndRadius), (int)(position.Y - light.EndRadius), (int)light.EndRadius * 2, (int)light.EndRadius * 2);
				Vector3 center = GetCenter(index);
				Color mask = GetMask(index, 0f, 1f);
				foreach (LightOcclude occluder in occluders)
				{
					if (!occluder.Visible || !occluder.Entity.Visible || occluder.Alpha <= 0f)
					{
						continue;
					}
					Rectangle bounds2 = occluder.RenderBounds;
					if (bounds2.Intersects(lightBounds))
					{
						bounds2 = bounds2.ClampTo(lightBounds);
						Color occluderMask = GetMask(index, 1f - occluder.Alpha, 1f);
						if (bounds2.Bottom > lightBounds.Top && bounds2.Bottom < lightBounds.Center.Y)
						{
							SetOccluder(center, occluderMask, position, new Vector2(bounds2.Left, bounds2.Bottom), new Vector2(bounds2.Right, bounds2.Bottom));
						}
						if (bounds2.Top < lightBounds.Bottom && bounds2.Top > lightBounds.Center.Y)
						{
							SetOccluder(center, occluderMask, position, new Vector2(bounds2.Left, bounds2.Top), new Vector2(bounds2.Right, bounds2.Top));
						}
						if (bounds2.Right > lightBounds.Left && bounds2.Right < lightBounds.Center.X)
						{
							SetOccluder(center, occluderMask, position, new Vector2(bounds2.Right, bounds2.Top), new Vector2(bounds2.Right, bounds2.Bottom));
						}
						if (bounds2.Left < lightBounds.Right && bounds2.Left > lightBounds.Center.X)
						{
							SetOccluder(center, occluderMask, position, new Vector2(bounds2.Left, bounds2.Top), new Vector2(bounds2.Left, bounds2.Bottom));
						}
					}
				}
				int left = lightBounds.Left / 8 - mapBounds.Left;
				int top = lightBounds.Top / 8 - mapBounds.Top;
				int height = lightBounds.Height / 8;
				int width = lightBounds.Width / 8;
				int right = left + width;
				int bottom = top + height;
				for (int ty5 = top; ty5 < top + height / 2; ty5++)
				{
					for (int tx = left; tx < right; tx++)
					{
						if (level.SolidsData.SafeCheck(tx, ty5) != '0' && level.SolidsData.SafeCheck(tx, ty5 + 1) == '0')
						{
							int start = tx;
							do
							{
								tx++;
							}
							while (tx < right && level.SolidsData.SafeCheck(tx, ty5) != '0' && level.SolidsData.SafeCheck(tx, ty5 + 1) == '0');
							SetOccluder(center, mask, position, new Vector2(mapBounds.X + start, mapBounds.Y + ty5 + 1) * 8f, new Vector2(mapBounds.X + tx, mapBounds.Y + ty5 + 1) * 8f);
						}
					}
				}
				for (int tx5 = left; tx5 < left + width / 2; tx5++)
				{
					for (int ty = top; ty < bottom; ty++)
					{
						if (level.SolidsData.SafeCheck(tx5, ty) != '0' && level.SolidsData.SafeCheck(tx5 + 1, ty) == '0')
						{
							int start2 = ty;
							do
							{
								ty++;
							}
							while (ty < bottom && level.SolidsData.SafeCheck(tx5, ty) != '0' && level.SolidsData.SafeCheck(tx5 + 1, ty) == '0');
							SetOccluder(center, mask, position, new Vector2(mapBounds.X + tx5 + 1, mapBounds.Y + start2) * 8f, new Vector2(mapBounds.X + tx5 + 1, mapBounds.Y + ty) * 8f);
						}
					}
				}
				for (int ty4 = top + height / 2; ty4 < bottom; ty4++)
				{
					for (int tx2 = left; tx2 < right; tx2++)
					{
						if (level.SolidsData.SafeCheck(tx2, ty4) != '0' && level.SolidsData.SafeCheck(tx2, ty4 - 1) == '0')
						{
							int start3 = tx2;
							do
							{
								tx2++;
							}
							while (tx2 < right && level.SolidsData.SafeCheck(tx2, ty4) != '0' && level.SolidsData.SafeCheck(tx2, ty4 - 1) == '0');
							SetOccluder(center, mask, position, new Vector2(mapBounds.X + start3, mapBounds.Y + ty4) * 8f, new Vector2(mapBounds.X + tx2, mapBounds.Y + ty4) * 8f);
						}
					}
				}
				for (int tx4 = left + width / 2; tx4 < right; tx4++)
				{
					for (int ty2 = top; ty2 < bottom; ty2++)
					{
						if (level.SolidsData.SafeCheck(tx4, ty2) != '0' && level.SolidsData.SafeCheck(tx4 - 1, ty2) == '0')
						{
							int start4 = ty2;
							do
							{
								ty2++;
							}
							while (ty2 < bottom && level.SolidsData.SafeCheck(tx4, ty2) != '0' && level.SolidsData.SafeCheck(tx4 - 1, ty2) == '0');
							SetOccluder(center, mask, position, new Vector2(mapBounds.X + tx4, mapBounds.Y + start4) * 8f, new Vector2(mapBounds.X + tx4, mapBounds.Y + ty2) * 8f);
						}
					}
				}
				foreach (EffectCutout cutout in cutouts)
				{
					if (cutout.Visible && cutout.Entity.Visible && !(cutout.Alpha <= 0f))
					{
						Rectangle bounds = cutout.Bounds;
						if (bounds.Intersects(lightBounds))
						{
							bounds = bounds.ClampTo(lightBounds);
							Color cutoutMask = GetMask(index, 1f - cutout.Alpha, 1f);
							SetCutout(center, cutoutMask, position, bounds.X, bounds.Y, bounds.Width, bounds.Height);
						}
					}
				}
				for (int tx3 = left; tx3 < right; tx3++)
				{
					for (int ty3 = top; ty3 < bottom; ty3++)
					{
						if (level.FgTilesLightMask.Tiles.SafeCheck(tx3, ty3) != null)
						{
							SetCutout(center, mask, position, (mapBounds.X + tx3) * 8, (mapBounds.Y + ty3) * 8, 8f, 8f);
						}
					}
				}
			}
			if (vertexCount <= 0)
			{
				return;
			}
			Engine.Instance.GraphicsDevice.BlendState = OccludeBlendState;
			GFX.FxPrimitive.Parameters["World"].SetValue(matrix);
			foreach (EffectPass pass in GFX.FxPrimitive.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, vertexCount, indices, 0, indexCount / 3);
			}
		}

		private Color GetMask(int index, float maskOn, float maskOff)
		{
			int channel = index / 16;
			return new Color((channel == 0) ? maskOn : maskOff, (channel == 1) ? maskOn : maskOff, (channel == 2) ? maskOn : maskOff, (channel == 3) ? maskOn : maskOff);
		}

		private Vector3 GetCenter(int index)
		{
			int cell = index % 16;
			return new Vector3(128f * ((float)(cell % 4) + 0.5f) * 2f, 128f * ((float)(cell / 4) + 0.5f) * 2f, 0f);
		}

		private void StartDrawingPrimitives()
		{
			vertexCount = 0;
			indexCount = 0;
		}

		private void SetClear(int index)
		{
			Vector3 center = GetCenter(index);
			Color mask = GetMask(index, 0f, 1f);
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 1;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount + 3;
			verts[vertexCount].Position = center + new Vector3(-128f, -128f, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(128f, -128f, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(128f, 128f, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(-128f, 128f, 0f);
			verts[vertexCount++].Color = mask;
		}

		private void SetGradient(int index, float startFade, float endFade)
		{
			Vector3 center = GetCenter(index);
			Color mask = GetMask(index, 1f, 0f);
			int startVertex = vertexCount;
			verts[vertexCount].Position = center;
			verts[vertexCount].Color = mask;
			vertexCount++;
			for (int i = 0; i < 20; i++)
			{
				verts[vertexCount].Position = center + angles[i] * startFade;
				verts[vertexCount].Color = mask;
				vertexCount++;
				verts[vertexCount].Position = center + angles[i] * endFade;
				verts[vertexCount].Color = Color.Transparent;
				vertexCount++;
				int last = i;
				int next = (i + 1) % 20;
				indices[indexCount++] = startVertex;
				indices[indexCount++] = startVertex + 1 + last * 2;
				indices[indexCount++] = startVertex + 1 + next * 2;
				indices[indexCount++] = startVertex + 1 + last * 2;
				indices[indexCount++] = startVertex + 2 + last * 2;
				indices[indexCount++] = startVertex + 2 + next * 2;
				indices[indexCount++] = startVertex + 1 + last * 2;
				indices[indexCount++] = startVertex + 2 + next * 2;
				indices[indexCount++] = startVertex + 1 + next * 2;
			}
		}

		private void SetOccluder(Vector3 center, Color mask, Vector2 light, Vector2 edgeA, Vector2 edgeB)
		{
			Vector2 start = (edgeA - light).Floor();
			Vector2 end = (edgeB - light).Floor();
			float angle = start.Angle();
			float endAngle = end.Angle();
			int startVertex = vertexCount;
			verts[vertexCount].Position = center + new Vector3(start, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(end, 0f);
			verts[vertexCount++].Color = mask;
			while (angle != endAngle)
			{
				verts[vertexCount].Position = center + new Vector3(Calc.AngleToVector(angle, 128f), 0f);
				verts[vertexCount].Color = mask;
				indices[indexCount++] = startVertex;
				indices[indexCount++] = vertexCount;
				indices[indexCount++] = vertexCount + 1;
				vertexCount++;
				angle = Calc.AngleApproach(angle, endAngle, (float)Math.PI / 4f);
			}
			verts[vertexCount].Position = center + new Vector3(Calc.AngleToVector(angle, 128f), 0f);
			verts[vertexCount].Color = mask;
			indices[indexCount++] = startVertex;
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = startVertex + 1;
			vertexCount++;
		}

		private void SetCutout(Vector3 center, Color mask, Vector2 light, float x, float y, float width, float height)
		{
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 1;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount + 3;
			verts[vertexCount].Position = center + new Vector3(x - light.X, y - light.Y, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(x + width - light.X, y - light.Y, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(x + width - light.X, y + height - light.Y, 0f);
			verts[vertexCount++].Color = mask;
			verts[vertexCount].Position = center + new Vector3(x - light.X, y + height - light.Y, 0f);
			verts[vertexCount++].Color = mask;
		}

		private void DrawLight(int index, Vector2 position, Color color, float radius)
		{
			Vector3 center = GetCenter(index);
			Color mask = GetMask(index, 1f, 0f);
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 1;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount;
			indices[indexCount++] = vertexCount + 2;
			indices[indexCount++] = vertexCount + 3;
			resultVerts[vertexCount].Position = new Vector3(position + new Vector2(0f - radius, 0f - radius), 0f);
			resultVerts[vertexCount].Color = color;
			resultVerts[vertexCount].Mask = mask;
			resultVerts[vertexCount++].Texcoord = new Vector2(center.X - radius, center.Y - radius) / 1024f;
			resultVerts[vertexCount].Position = new Vector3(position + new Vector2(radius, 0f - radius), 0f);
			resultVerts[vertexCount].Color = color;
			resultVerts[vertexCount].Mask = mask;
			resultVerts[vertexCount++].Texcoord = new Vector2(center.X + radius, center.Y - radius) / 1024f;
			resultVerts[vertexCount].Position = new Vector3(position + new Vector2(radius, radius), 0f);
			resultVerts[vertexCount].Color = color;
			resultVerts[vertexCount].Mask = mask;
			resultVerts[vertexCount++].Texcoord = new Vector2(center.X + radius, center.Y + radius) / 1024f;
			resultVerts[vertexCount].Position = new Vector3(position + new Vector2(0f - radius, radius), 0f);
			resultVerts[vertexCount].Color = color;
			resultVerts[vertexCount].Mask = mask;
			resultVerts[vertexCount++].Texcoord = new Vector2(center.X - radius, center.Y + radius) / 1024f;
		}

		public override void Render(Scene scene)
		{
			GFX.FxDither.CurrentTechnique = GFX.FxDither.Techniques["InvertDither"];
			GFX.FxDither.Parameters["size"].SetValue(new Vector2(GameplayBuffers.Light.Width, GameplayBuffers.Light.Height));
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, GFX.DestinationTransparencySubtract, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDither, Matrix.Identity);
			Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Light, Vector2.Zero, Color.White * MathHelper.Clamp(Alpha, 0f, 1f));
			Draw.SpriteBatch.End();
		}
	}
}
