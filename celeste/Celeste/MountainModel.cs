using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class MountainModel : IDisposable
	{
		public MountainCamera Camera;

		public Vector3 Forward;

		public float SkyboxOffset;

		public bool LockBufferResizing;

		private VirtualRenderTarget buffer;

		private VirtualRenderTarget blurA;

		private VirtualRenderTarget blurB;

		public static RasterizerState MountainRasterizer = new RasterizerState
		{
			CullMode = CullMode.CullClockwiseFace,
			MultiSampleAntiAlias = true
		};

		public static RasterizerState CullNoneRasterizer = new RasterizerState
		{
			CullMode = CullMode.None,
			MultiSampleAntiAlias = false
		};

		public static RasterizerState CullCCRasterizer = new RasterizerState
		{
			CullMode = CullMode.CullCounterClockwiseFace,
			MultiSampleAntiAlias = false
		};

		public static RasterizerState CullCRasterizer = new RasterizerState
		{
			CullMode = CullMode.CullClockwiseFace,
			MultiSampleAntiAlias = false
		};

		private int currState;

		private int nextState;

		private int targetState;

		private float easeState = 1f;

		private MountainState[] mountainStates = new MountainState[4];

		public Vector3 CoreWallPosition = Vector3.Zero;

		private VertexBuffer billboardVertices;

		private IndexBuffer billboardIndices;

		private VertexPositionColorTexture[] billboardInfo = new VertexPositionColorTexture[2048];

		private Texture2D[] billboardTextures = new Texture2D[512];

		private Ring fog;

		private Ring fog2;

		public float NearFogAlpha;

		public float StarEase;

		public float SnowStretch;

		public float SnowSpeedAddition = 1f;

		public float SnowForceFloat;

		private Ring starsky;

		private Ring starfog;

		private Ring stardots0;

		private Ring starstream0;

		private Ring starstream1;

		private Ring starstream2;

		private bool ignoreCameraRotation;

		private Quaternion lastCameraRotation;

		private Vector3 starCenter = new Vector3(0f, 32f, 0f);

		private float birdTimer;

		public List<VertexPositionColor> DebugPoints = new List<VertexPositionColor>();

		public bool DrawDebugPoints;

		public MountainModel()
		{
			mountainStates[0] = new MountainState(MTN.MountainTerrainTextures[0], MTN.MountainBuildingTextures[0], MTN.MountainSkyboxTextures[0], Calc.HexToColor("010817"));
			mountainStates[1] = new MountainState(MTN.MountainTerrainTextures[1], MTN.MountainBuildingTextures[1], MTN.MountainSkyboxTextures[1], Calc.HexToColor("13203E"));
			mountainStates[2] = new MountainState(MTN.MountainTerrainTextures[2], MTN.MountainBuildingTextures[2], MTN.MountainSkyboxTextures[2], Calc.HexToColor("281A35"));
			mountainStates[3] = new MountainState(MTN.MountainTerrainTextures[0], MTN.MountainBuildingTextures[0], MTN.MountainSkyboxTextures[0], Calc.HexToColor("010817"));
			fog = new Ring(6f, -1f, 20f, 0f, 24, Color.White, MTN.MountainFogTexture);
			fog2 = new Ring(6f, -4f, 10f, 0f, 24, Color.White, MTN.MountainFogTexture);
			starsky = new Ring(18f, -18f, 20f, 0f, 24, Color.White, Color.Transparent, MTN.MountainStarSky);
			starfog = new Ring(10f, -18f, 19.5f, 0f, 24, Calc.HexToColor("020915"), Color.Transparent, MTN.MountainFogTexture);
			stardots0 = new Ring(16f, -18f, 19f, 0f, 24, Color.White, Color.Transparent, MTN.MountainStars, 4f);
			starstream0 = new Ring(5f, -8f, 18.5f, 0.2f, 80, Color.Black, MTN.MountainStarStream);
			starstream1 = new Ring(4f, -6f, 18f, 1f, 80, Calc.HexToColor("9228e2") * 0.5f, MTN.MountainStarStream);
			starstream2 = new Ring(3f, -4f, 17.9f, 1.4f, 80, Calc.HexToColor("30ffff") * 0.5f, MTN.MountainStarStream);
			ResetRenderTargets();
			ResetBillboardBuffers();
		}

		public void SnapState(int state)
		{
			currState = (nextState = (targetState = state % mountainStates.Length));
			easeState = 1f;
			if (state == 3)
			{
				StarEase = 1f;
			}
		}

		public void EaseState(int state)
		{
			targetState = state % mountainStates.Length;
			lastCameraRotation = Camera.Rotation;
		}

		public void Update()
		{
			if (currState != nextState)
			{
				easeState = Calc.Approach(easeState, 1f, (float)((nextState == targetState) ? 1 : 4) * Engine.DeltaTime);
				if (easeState >= 1f)
				{
					currState = nextState;
				}
			}
			else if (nextState != targetState)
			{
				nextState = targetState;
				easeState = 0f;
			}
			StarEase = Calc.Approach(StarEase, (nextState == 3) ? 1f : 0f, ((nextState == 3) ? 1.5f : 1f) * Engine.DeltaTime);
			SnowForceFloat = Calc.ClampedMap(StarEase, 0.95f, 1f);
			ignoreCameraRotation = (nextState == 3 && currState != 3 && StarEase < 0.5f) || (nextState != 3 && currState == 3 && StarEase > 0.5f);
			if (nextState == 3)
			{
				SnowStretch = Calc.ClampedMap(StarEase, 0f, 0.25f) * 50f;
				SnowSpeedAddition = SnowStretch * 4f;
			}
			else
			{
				SnowStretch = Calc.ClampedMap(StarEase, 0.25f, 1f) * 50f;
				SnowSpeedAddition = (0f - SnowStretch) * 4f;
			}
			starfog.Rotate((0f - Engine.DeltaTime) * 0.01f);
			fog.Rotate((0f - Engine.DeltaTime) * 0.01f);
			fog.TopColor = (fog.BotColor = Color.Lerp(mountainStates[currState].FogColor, mountainStates[nextState].FogColor, easeState));
			fog2.Rotate((0f - Engine.DeltaTime) * 0.01f);
			fog2.TopColor = (fog2.BotColor = Color.White * 0.3f * NearFogAlpha);
			starstream1.Rotate(Engine.DeltaTime * 0.01f);
			starstream2.Rotate(Engine.DeltaTime * 0.02f);
			birdTimer += Engine.DeltaTime;
		}

		public void ResetRenderTargets()
		{
			int width = Math.Min(1920, Engine.ViewWidth);
			int height = Math.Min(1080, Engine.ViewHeight);
			if (buffer == null || buffer.IsDisposed || (buffer.Width != width && !LockBufferResizing))
			{
				DisposeTargets();
				buffer = VirtualContent.CreateRenderTarget("mountain-a", width, height, depth: true, preserve: false);
				blurA = VirtualContent.CreateRenderTarget("mountain-blur-a", width / 2, height / 2);
				blurB = VirtualContent.CreateRenderTarget("mountain-blur-b", width / 2, height / 2);
			}
		}

		public void ResetBillboardBuffers()
		{
			if (billboardVertices == null || billboardIndices.IsDisposed || billboardIndices.GraphicsDevice.IsDisposed || billboardVertices.IsDisposed || billboardVertices.GraphicsDevice.IsDisposed || billboardInfo.Length > billboardVertices.VertexCount)
			{
				DisposeBillboardBuffers();
				billboardVertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionColorTexture), billboardInfo.Length, BufferUsage.None);
				billboardIndices = new IndexBuffer(Engine.Graphics.GraphicsDevice, typeof(short), billboardInfo.Length / 4 * 6, BufferUsage.None);
				short[] indices = new short[billboardIndices.IndexCount];
				int i = 0;
				int v = 0;
				while (i < indices.Length)
				{
					indices[i] = (short)v;
					indices[i + 1] = (short)(v + 1);
					indices[i + 2] = (short)(v + 2);
					indices[i + 3] = (short)v;
					indices[i + 4] = (short)(v + 2);
					indices[i + 5] = (short)(v + 3);
					i += 6;
					v += 4;
				}
				billboardIndices.SetData(indices);
			}
		}

		public void Dispose()
		{
			DisposeTargets();
			DisposeBillboardBuffers();
		}

		public void DisposeTargets()
		{
			if (buffer != null && !buffer.IsDisposed)
			{
				buffer.Dispose();
				blurA.Dispose();
				blurB.Dispose();
			}
		}

		public void DisposeBillboardBuffers()
		{
			if (billboardVertices != null && !billboardVertices.IsDisposed)
			{
				billboardVertices.Dispose();
			}
			if (billboardIndices != null && !billboardIndices.IsDisposed)
			{
				billboardIndices.Dispose();
			}
		}

		public void BeforeRender(Scene scene)
		{
			ResetRenderTargets();
			Quaternion camRotation = Camera.Rotation;
			if (ignoreCameraRotation)
			{
				camRotation = lastCameraRotation;
			}
			Matrix cameraProjection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, (float)Engine.Width / (float)Engine.Height, 0.25f, 50f);
			Matrix cameraView = Matrix.CreateTranslation(-Camera.Position) * Matrix.CreateFromQuaternion(camRotation);
			Matrix mountainMatrix = cameraView * cameraProjection;
			Forward = Vector3.Transform(Vector3.Forward, Camera.Rotation.Conjugated());
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			if (StarEase < 1f)
			{
				Matrix skyboxMatrix2 = Matrix.CreateTranslation(0f, 5f - Camera.Position.Y * 1.1f, 0f) * Matrix.CreateFromQuaternion(camRotation) * cameraProjection;
				if (currState == nextState)
				{
					mountainStates[currState].Skybox.Draw(skyboxMatrix2, Color.White);
				}
				else
				{
					mountainStates[currState].Skybox.Draw(skyboxMatrix2, Color.White);
					mountainStates[nextState].Skybox.Draw(skyboxMatrix2, Color.White * easeState);
				}
				if (currState != nextState)
				{
					GFX.FxMountain.Parameters["ease"].SetValue(easeState);
					GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Easing"];
				}
				else
				{
					GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];
				}
				Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
				Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				Engine.Graphics.GraphicsDevice.RasterizerState = MountainRasterizer;
				GFX.FxMountain.Parameters["WorldViewProj"].SetValue(mountainMatrix);
				GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
				Engine.Graphics.GraphicsDevice.Textures[0] = mountainStates[currState].TerrainTexture.Texture;
				Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
				if (currState != nextState)
				{
					Engine.Graphics.GraphicsDevice.Textures[1] = mountainStates[nextState].TerrainTexture.Texture;
					Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
				}
				MTN.MountainTerrain.Draw(GFX.FxMountain);
				GFX.FxMountain.Parameters["WorldViewProj"].SetValue(Matrix.CreateTranslation(CoreWallPosition) * mountainMatrix);
				MTN.MountainCoreWall.Draw(GFX.FxMountain);
				GFX.FxMountain.Parameters["WorldViewProj"].SetValue(mountainMatrix);
				Engine.Graphics.GraphicsDevice.Textures[0] = mountainStates[currState].BuildingsTexture.Texture;
				Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
				if (currState != nextState)
				{
					Engine.Graphics.GraphicsDevice.Textures[1] = mountainStates[nextState].BuildingsTexture.Texture;
					Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
				}
				MTN.MountainBuildings.Draw(GFX.FxMountain);
				fog.Draw(mountainMatrix);
			}
			if (StarEase > 0f)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
				Draw.Rect(0f, 0f, buffer.Width, buffer.Height, Color.Black * Ease.CubeInOut(Calc.ClampedMap(StarEase, 0f, 0.6f)));
				Draw.SpriteBatch.End();
				Matrix skyboxMatrix = Matrix.CreateTranslation(starCenter - Camera.Position) * Matrix.CreateFromQuaternion(camRotation) * cameraProjection;
				float alpha = Calc.ClampedMap(StarEase, 0.8f, 1f);
				starsky.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				starfog.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				stardots0.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				starstream0.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				starstream1.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				starstream2.Draw(skyboxMatrix, CullCCRasterizer, alpha);
				Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
				Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				Engine.Graphics.GraphicsDevice.RasterizerState = CullCRasterizer;
				Engine.Graphics.GraphicsDevice.Textures[0] = MTN.MountainMoonTexture.Texture;
				Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
				GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];
				GFX.FxMountain.Parameters["WorldViewProj"].SetValue(mountainMatrix);
				GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
				MTN.MountainMoon.Draw(GFX.FxMountain);
				float rot = birdTimer * 0.2f;
				Matrix birdMatrix = Matrix.CreateScale(0.25f) * Matrix.CreateRotationZ((float)Math.Cos(rot * 2f) * 0.5f) * Matrix.CreateRotationX(0.4f + (float)Math.Sin(rot) * 0.05f) * Matrix.CreateRotationY(0f - rot - (float)Math.PI / 2f) * Matrix.CreateTranslation((float)Math.Cos(rot) * 2.2f, 31f + (float)Math.Sin(rot * 2f) * 0.8f, (float)Math.Sin(rot) * 2.2f);
				GFX.FxMountain.Parameters["WorldViewProj"].SetValue(birdMatrix * mountainMatrix);
				GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
				MTN.MountainBird.Draw(GFX.FxMountain);
			}
			DrawBillboards(mountainMatrix, scene.Tracker.GetComponents<Billboard>());
			if (StarEase < 1f)
			{
				fog2.Draw(mountainMatrix, CullCRasterizer);
			}
			if (DrawDebugPoints && DebugPoints.Count > 0)
			{
				GFX.FxDebug.World = Matrix.Identity;
				GFX.FxDebug.View = cameraView;
				GFX.FxDebug.Projection = cameraProjection;
				GFX.FxDebug.TextureEnabled = false;
				GFX.FxDebug.VertexColorEnabled = true;
				VertexPositionColor[] p = DebugPoints.ToArray();
				foreach (EffectPass pass in GFX.FxDebug.CurrentTechnique.Passes)
				{
					pass.Apply();
					Engine.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, p, 0, p.Length / 3);
				}
			}
			GaussianBlur.Blur((RenderTarget2D)buffer, blurA, blurB, 0.75f, clear: true, GaussianBlur.Samples.Five);
		}

		private void DrawBillboards(Matrix matrix, List<Component> billboards)
		{
			int sprites = 0;
			int maxSprites = billboardInfo.Length / 4;
			Vector3 left = Vector3.Transform(Vector3.Left, Camera.Rotation.LookAt(Vector3.Zero, Forward, Vector3.Up).Conjugated());
			Vector3 up = Vector3.Transform(Vector3.Up, Camera.Rotation.LookAt(Vector3.Zero, Forward, Vector3.Up).Conjugated());
			foreach (Billboard board in billboards)
			{
				if (!board.Entity.Visible || !board.Visible)
				{
					continue;
				}
				if (board.BeforeRender != null)
				{
					board.BeforeRender();
				}
				if (board.Color.A >= 0 && board.Size.X != 0f && board.Size.Y != 0f && board.Scale.X != 0f && board.Scale.Y != 0f && board.Texture != null)
				{
					if (sprites < maxSprites)
					{
						Vector3 pos = board.Position;
						Vector3 j = left * board.Size.X * board.Scale.X;
						Vector3 u = up * board.Size.Y * board.Scale.Y;
						Vector3 r = -j;
						Vector3 d = -u;
						int s0 = sprites * 4;
						int s1 = sprites * 4 + 1;
						int s2 = sprites * 4 + 2;
						int s3 = sprites * 4 + 3;
						billboardInfo[s0].Color = board.Color;
						billboardInfo[s0].TextureCoordinate.X = board.Texture.LeftUV;
						billboardInfo[s0].TextureCoordinate.Y = board.Texture.BottomUV;
						billboardInfo[s0].Position = pos + j + d;
						billboardInfo[s1].Color = board.Color;
						billboardInfo[s1].TextureCoordinate.X = board.Texture.LeftUV;
						billboardInfo[s1].TextureCoordinate.Y = board.Texture.TopUV;
						billboardInfo[s1].Position = pos + j + u;
						billboardInfo[s2].Color = board.Color;
						billboardInfo[s2].TextureCoordinate.X = board.Texture.RightUV;
						billboardInfo[s2].TextureCoordinate.Y = board.Texture.TopUV;
						billboardInfo[s2].Position = pos + r + u;
						billboardInfo[s3].Color = board.Color;
						billboardInfo[s3].TextureCoordinate.X = board.Texture.RightUV;
						billboardInfo[s3].TextureCoordinate.Y = board.Texture.BottomUV;
						billboardInfo[s3].Position = pos + r + d;
						billboardTextures[sprites] = board.Texture.Texture.Texture;
					}
					sprites++;
				}
			}
			ResetBillboardBuffers();
			if (sprites <= 0)
			{
				return;
			}
			billboardVertices.SetData(billboardInfo);
			Engine.Graphics.GraphicsDevice.SetVertexBuffer(billboardVertices);
			Engine.Graphics.GraphicsDevice.Indices = billboardIndices;
			Engine.Graphics.GraphicsDevice.RasterizerState = CullNoneRasterizer;
			Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
			Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			GFX.FxTexture.Parameters["World"].SetValue(matrix);
			int spriteCount = Math.Min(sprites, billboardInfo.Length / 4);
			Texture2D tex = billboardTextures[0];
			int offset = 0;
			for (int i = 1; i < spriteCount; i++)
			{
				if (billboardTextures[i] != tex)
				{
					DrawBillboardBatch(tex, offset, i - offset);
					tex = billboardTextures[i];
					offset = i;
				}
			}
			DrawBillboardBatch(tex, offset, spriteCount - offset);
			if (sprites * 4 > billboardInfo.Length)
			{
				billboardInfo = new VertexPositionColorTexture[billboardInfo.Length * 2];
				billboardTextures = new Texture2D[billboardInfo.Length / 4];
			}
		}

		private void DrawBillboardBatch(Texture2D texture, int offset, int sprites)
		{
			Engine.Graphics.GraphicsDevice.Textures[0] = texture;
			foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, offset * 4, 0, sprites * 4, 0, sprites * 2);
			}
		}

		public void Render()
		{
			float scale = (float)Engine.ViewWidth / (float)buffer.Width;
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
			Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, buffer.Bounds, Color.White * 1f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			Draw.SpriteBatch.Draw((RenderTarget2D)blurB, Vector2.Zero, blurB.Bounds, Color.White, 0f, Vector2.Zero, scale * 2f, SpriteEffects.None, 0f);
			Draw.SpriteBatch.End();
		}
	}
}
