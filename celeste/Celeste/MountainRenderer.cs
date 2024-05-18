using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class MountainRenderer : Renderer
	{
		public bool ForceNearFog;

		public Action OnEaseEnd;

		public static readonly Vector3 RotateLookAt = new Vector3(0f, 7f, 0f);

		private const float rotateDistance = 15f;

		private const float rotateYPosition = 3f;

		private bool rotateAroundCenter;

		private bool rotateAroundTarget;

		private float rotateAroundTargetDistance;

		private float rotateTimer = (float)Math.PI / 2f;

		private const float DurationDivisor = 3f;

		public MountainCamera UntiltedCamera;

		public MountainModel Model;

		public bool AllowUserRotation = true;

		private Vector2 userOffset;

		private bool inFreeCameraDebugMode;

		private float percent = 1f;

		private float duration = 1f;

		private MountainCamera easeCameraFrom;

		private MountainCamera easeCameraTo;

		private float easeCameraRotationAngleTo;

		private float timer;

		private float door;

		public MountainCamera Camera => Model.Camera;

		public bool Animating { get; private set; }

		public int Area { get; private set; }

		public MountainRenderer()
		{
			Model = new MountainModel();
			GotoRotationMode();
		}

		public void Dispose()
		{
			Model.Dispose();
		}

		public override void Update(Scene scene)
		{
			timer += Engine.DeltaTime;
			Model.Update();
			Vector2 userOffsetTarget = (AllowUserRotation ? (-Input.MountainAim.Value * 0.8f) : Vector2.Zero);
			userOffset += (userOffsetTarget - userOffset) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
			if (!rotateAroundCenter)
			{
				if (Area == 8)
				{
					userOffset.Y = Math.Max(0f, userOffset.Y);
				}
				if (Area == 7)
				{
					userOffset.X = Calc.Clamp(userOffset.X, -0.4f, 0.4f);
				}
			}
			if (!inFreeCameraDebugMode)
			{
				if (rotateAroundCenter)
				{
					rotateTimer -= Engine.DeltaTime * 0.1f;
					Vector3 posTarget = new Vector3((float)Math.Cos(rotateTimer) * 15f, 3f, (float)Math.Sin(rotateTimer) * 15f);
					Model.Camera.Position += (posTarget - Model.Camera.Position) * (1f - (float)Math.Pow(0.10000000149011612, Engine.DeltaTime));
					Model.Camera.Target = RotateLookAt + Vector3.Up * userOffset.Y;
					Quaternion rotTarget2 = default(Quaternion).LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up);
					Model.Camera.Rotation = Quaternion.Slerp(Model.Camera.Rotation, rotTarget2, Engine.DeltaTime * 4f);
					UntiltedCamera = Camera;
				}
				else
				{
					if (Animating)
					{
						percent = Calc.Approach(percent, 1f, Engine.DeltaTime / duration);
						float ease = Ease.SineOut(percent);
						Model.Camera.Position = GetBetween(easeCameraFrom.Position, easeCameraTo.Position, ease);
						Model.Camera.Target = GetBetween(easeCameraFrom.Target, easeCameraTo.Target, ease);
						Vector3 vector = easeCameraFrom.Rotation.Forward();
						Vector3 forwardsTo = easeCameraTo.Rotation.Forward();
						Vector2 rot = Calc.AngleToVector(length: Calc.LerpClamp(vector.XZ().Length(), forwardsTo.XZ().Length(), ease), angleRadians: MathHelper.Lerp(vector.XZ().Angle(), easeCameraRotationAngleTo, ease));
						float y = Calc.LerpClamp(vector.Y, forwardsTo.Y, ease);
						Model.Camera.Rotation = default(Quaternion).LookAt(new Vector3(rot.X, y, rot.Y), Vector3.Up);
						if (percent >= 1f)
						{
							rotateTimer = new Vector2(Model.Camera.Position.X, Model.Camera.Position.Z).Angle();
							Animating = false;
							if (OnEaseEnd != null)
							{
								OnEaseEnd();
							}
						}
					}
					else if (rotateAroundTarget)
					{
						rotateTimer -= Engine.DeltaTime * 0.1f;
						float dist = (new Vector2(easeCameraTo.Target.X, easeCameraTo.Target.Z) - new Vector2(easeCameraTo.Position.X, easeCameraTo.Position.Z)).Length();
						Vector3 posTarget2 = new Vector3(easeCameraTo.Target.X + (float)Math.Cos(rotateTimer) * dist, easeCameraTo.Position.Y, easeCameraTo.Target.Z + (float)Math.Sin(rotateTimer) * dist);
						Model.Camera.Position += (posTarget2 - Model.Camera.Position) * (1f - (float)Math.Pow(0.10000000149011612, Engine.DeltaTime));
						Model.Camera.Target = easeCameraTo.Target + Vector3.Up * userOffset.Y * 2f + Vector3.Left * userOffset.X * 2f;
						Quaternion rotTarget = default(Quaternion).LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up);
						Model.Camera.Rotation = Quaternion.Slerp(Model.Camera.Rotation, rotTarget, Engine.DeltaTime * 4f);
						UntiltedCamera = Camera;
					}
					else
					{
						Model.Camera.Rotation = easeCameraTo.Rotation;
						Model.Camera.Target = easeCameraTo.Target;
					}
					UntiltedCamera = Camera;
					if (userOffset != Vector2.Zero && !rotateAroundTarget)
					{
						Vector3 userOffsetLeftRight = Model.Camera.Rotation.Left() * userOffset.X * 0.25f;
						Vector3 userOffsetUpDown = Model.Camera.Rotation.Up() * userOffset.Y * 0.25f;
						Vector3 target = Model.Camera.Position + Model.Camera.Rotation.Forward() + userOffsetLeftRight + userOffsetUpDown;
						Model.Camera.LookAt(target);
					}
				}
			}
			else
			{
				Vector3 forward = Vector3.Transform(Vector3.Forward, Model.Camera.Rotation.Conjugated());
				Model.Camera.Rotation = Model.Camera.Rotation.LookAt(Vector3.Zero, forward, Vector3.Up);
				Vector3 left = Vector3.Transform(Vector3.Left, Model.Camera.Rotation.Conjugated());
				Vector3 move = new Vector3(0f, 0f, 0f);
				if (MInput.Keyboard.Check(Keys.W))
				{
					move += forward;
				}
				if (MInput.Keyboard.Check(Keys.S))
				{
					move -= forward;
				}
				if (MInput.Keyboard.Check(Keys.D))
				{
					move -= left;
				}
				if (MInput.Keyboard.Check(Keys.A))
				{
					move += left;
				}
				if (MInput.Keyboard.Check(Keys.Q))
				{
					move += Vector3.Up;
				}
				if (MInput.Keyboard.Check(Keys.Z))
				{
					move += Vector3.Down;
				}
				Model.Camera.Position += move * (MInput.Keyboard.Check(Keys.LeftShift) ? 0.5f : 5f) * Engine.DeltaTime;
				if (MInput.Mouse.CheckLeftButton)
				{
					MouseState state = Mouse.GetState();
					int originX = Engine.Graphics.GraphicsDevice.Viewport.Width / 2;
					int originY = Engine.Graphics.GraphicsDevice.Viewport.Height / 2;
					int xDifference = state.X - originX;
					int yDifference = state.Y - originY;
					Model.Camera.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, (float)xDifference * 0.1f * Engine.DeltaTime);
					Model.Camera.Rotation *= Quaternion.CreateFromAxisAngle(left, (float)(-yDifference) * 0.1f * Engine.DeltaTime);
					Mouse.SetPosition(originX, originY);
				}
				if (Area >= 0)
				{
					Vector3 at = AreaData.Areas[Area].MountainIdle.Target;
					Vector3 i = left * 0.05f;
					Vector3 u = Vector3.Up * 0.05f;
					Model.DebugPoints.Clear();
					Model.DebugPoints.Add(new VertexPositionColor(at - i + u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i + u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at - i + u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at - i - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at - i * 0.25f - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i * 0.25f - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i * 0.25f + Vector3.Down * 100f, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at - i * 0.25f - u, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at + i * 0.25f + Vector3.Down * 100f, Color.Red));
					Model.DebugPoints.Add(new VertexPositionColor(at - i * 0.25f + Vector3.Down * 100f, Color.Red));
				}
			}
			door = Calc.Approach(door, (Area == 9 && !rotateAroundCenter) ? 1 : 0, Engine.DeltaTime * 1f);
			Model.CoreWallPosition = Vector3.Lerp(Vector3.Zero, -new Vector3(-1.5f, 1.5f, 1f), Ease.CubeInOut(door));
			Model.NearFogAlpha = Calc.Approach(Model.NearFogAlpha, (ForceNearFog || rotateAroundCenter) ? 1 : 0, (float)(rotateAroundCenter ? 1 : 4) * Engine.DeltaTime);
			if (Celeste.PlayMode == Celeste.PlayModes.Debug)
			{
				if (MInput.Keyboard.Pressed(Keys.P))
				{
					Console.WriteLine(GetCameraString());
				}
				if (MInput.Keyboard.Pressed(Keys.F2))
				{
					Engine.Scene = new OverworldLoader(Overworld.StartMode.ReturnFromOptions);
				}
				if (MInput.Keyboard.Pressed(Keys.Space))
				{
					inFreeCameraDebugMode = !inFreeCameraDebugMode;
				}
				Model.DrawDebugPoints = inFreeCameraDebugMode;
				if (MInput.Keyboard.Pressed(Keys.F1))
				{
					AreaData.ReloadMountainViews();
				}
			}
		}

		private Vector3 GetBetween(Vector3 from, Vector3 to, float ease)
		{
			Vector2 from2d = new Vector2(from.X, from.Z);
			Vector2 to2d = new Vector2(to.X, to.Z);
			float startAngle = Calc.Angle(from2d, Vector2.Zero);
			float angleTo = Calc.Angle(to2d, Vector2.Zero);
			float angleRadians = Calc.AngleLerp(startAngle, angleTo, ease);
			float distFrom = from2d.Length();
			float distTo = to2d.Length();
			float distance = distFrom + (distTo - distFrom) * ease;
			float y = from.Y + (to.Y - from.Y) * ease;
			Vector2 position2d = -Calc.AngleToVector(angleRadians, distance);
			return new Vector3(position2d.X, y, position2d.Y);
		}

		public override void BeforeRender(Scene scene)
		{
			Model.BeforeRender(scene);
		}

		public override void Render(Scene scene)
		{
			Model.Render();
			Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
			OVR.Atlas["vignette"].Draw(Vector2.Zero, Vector2.Zero, Color.White * 0.2f);
			Draw.SpriteBatch.End();
			if (inFreeCameraDebugMode)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
				ActiveFont.DrawOutline(GetCameraString(), new Vector2(8f, 8f), Vector2.Zero, Vector2.One * 0.75f, Color.White, 2f, Color.Black);
				Draw.SpriteBatch.End();
			}
		}

		public void SnapCamera(int area, MountainCamera transform, bool targetRotate = false)
		{
			Area = area;
			Animating = false;
			rotateAroundCenter = false;
			rotateAroundTarget = targetRotate;
			Model.Camera = transform;
			percent = 1f;
		}

		public void SnapState(int state)
		{
			Model.SnapState(state);
		}

		public float EaseCamera(int area, MountainCamera transform, float? duration = null, bool nearTarget = true, bool targetRotate = false)
		{
			if (Area != area && area >= 0)
			{
				PlayWhoosh(Area, area);
			}
			Area = area;
			percent = 0f;
			Animating = true;
			rotateAroundCenter = false;
			rotateAroundTarget = targetRotate;
			userOffset = Vector2.Zero;
			easeCameraFrom = Model.Camera;
			if (nearTarget)
			{
				easeCameraFrom.Target = easeCameraFrom.Position + (easeCameraFrom.Target - easeCameraFrom.Position).SafeNormalize() * 0.5f;
			}
			easeCameraTo = transform;
			float angleFrom = easeCameraFrom.Rotation.Forward().XZ().Angle();
			float angleTo = easeCameraTo.Rotation.Forward().XZ().Angle();
			float shortDiff = Calc.AngleDiff(angleFrom, angleTo);
			float longDiff = (float)(-Math.Sign(shortDiff)) * ((float)Math.PI * 2f - Math.Abs(shortDiff));
			Vector3 mid = GetBetween(easeCameraFrom.Position, easeCameraTo.Position, 0.5f);
			Vector2 shortRot = Calc.AngleToVector(MathHelper.Lerp(angleFrom, angleFrom + shortDiff, 0.5f), 1f);
			Vector2 longRot = Calc.AngleToVector(MathHelper.Lerp(angleFrom, angleFrom + longDiff, 0.5f), 1f);
			if ((mid + new Vector3(shortRot.X, 0f, shortRot.Y)).Length() < (mid + new Vector3(longRot.X, 0f, longRot.Y)).Length())
			{
				easeCameraRotationAngleTo = angleFrom + shortDiff;
			}
			else
			{
				easeCameraRotationAngleTo = angleFrom + longDiff;
			}
			if (!duration.HasValue)
			{
				this.duration = GetDuration(easeCameraFrom, easeCameraTo);
			}
			else
			{
				this.duration = duration.Value;
			}
			return this.duration;
		}

		public void EaseState(int state)
		{
			Model.EaseState(state);
		}

		public void GotoRotationMode()
		{
			if (!rotateAroundCenter)
			{
				rotateAroundCenter = true;
				rotateTimer = new Vector2(Model.Camera.Position.X, Model.Camera.Position.Z).Angle();
				Model.EaseState(0);
			}
		}

		private float GetDuration(MountainCamera from, MountainCamera to)
		{
			float value = Calc.AngleDiff(Calc.Angle(new Vector2(from.Position.X, from.Position.Z), new Vector2(from.Target.X, from.Target.Z)), Calc.Angle(new Vector2(to.Position.X, to.Position.Z), new Vector2(to.Target.X, to.Target.Z)));
			return Calc.Clamp((float)(Math.Max(val2: Math.Sqrt((from.Position - to.Position).Length()) / 3.0, val1: Math.Abs(value) * 0.5f) * 0.699999988079071), 0.3f, 1.1f);
		}

		private void PlayWhoosh(int from, int to)
		{
			string sfx = "";
			if (from == 0 && to == 1)
			{
				sfx = "event:/ui/world_map/whoosh/400ms_forward";
			}
			else if (from == 1 && to == 0)
			{
				sfx = "event:/ui/world_map/whoosh/400ms_back";
			}
			else if (from == 1 && to == 2)
			{
				sfx = "event:/ui/world_map/whoosh/600ms_forward";
			}
			else if (from == 2 && to == 1)
			{
				sfx = "event:/ui/world_map/whoosh/600ms_back";
			}
			else if (from < to && from > 1 && from < 7)
			{
				sfx = "event:/ui/world_map/whoosh/700ms_forward";
			}
			else if (from > to && from > 2 && from < 8)
			{
				sfx = "event:/ui/world_map/whoosh/700ms_back";
			}
			else if (from == 7 && to == 8)
			{
				sfx = "event:/ui/world_map/whoosh/1000ms_forward";
			}
			else if (from == 8 && to == 7)
			{
				sfx = "event:/ui/world_map/whoosh/1000ms_back";
			}
			else if (from == 8 && to == 9)
			{
				sfx = "event:/ui/world_map/whoosh/600ms_forward";
			}
			else if (from == 9 && to == 8)
			{
				sfx = "event:/ui/world_map/whoosh/600ms_back";
			}
			else if (from == 9 && to == 10)
			{
				sfx = "event:/ui/world_map/whoosh/1000ms_forward";
			}
			else if (from == 10 && to == 9)
			{
				sfx = "event:/ui/world_map/whoosh/1000ms_back";
			}
			if (!string.IsNullOrEmpty(sfx))
			{
				Audio.Play(sfx);
			}
		}

		private string GetCameraString()
		{
			Vector3 pos = Model.Camera.Position;
			Vector3 look = pos + Vector3.Transform(Vector3.Forward, Model.Camera.Rotation.Conjugated()) * 2f;
			return "position=\"" + pos.X.ToString("0.000") + ", " + pos.Y.ToString("0.000") + ", " + pos.Z.ToString("0.000") + "\" \ntarget=\"" + look.X.ToString("0.000") + ", " + look.Y.ToString("0.000") + ", " + look.Z.ToString("0.000") + "\"";
		}
	}
}
