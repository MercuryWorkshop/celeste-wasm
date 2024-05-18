using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiChapterSelectIcon : Entity
	{
		public const float IdleSize = 100f;

		public const float HoverSize = 144f;

		public const float HoverSpacing = 80f;

		public const float IdleY = 130f;

		public const float HoverY = 140f;

		public const float Spacing = 32f;

		public int Area;

		public bool New;

		public Vector2 Scale = Vector2.One;

		public float Rotation;

		public float sizeEase = 1f;

		public bool AssistModeUnlockable;

		public bool HideIcon;

		private Wiggler newWiggle;

		private bool hidden = true;

		private bool selected;

		private Tween tween;

		private Wiggler wiggler;

		private bool wiggleLeft;

		private int rotateDir = -1;

		private Vector2 shake;

		private float spotlightAlpha;

		private float spotlightRadius;

		private MTexture front;

		private MTexture back;

		public Vector2 IdlePosition
		{
			get
			{
				float targetX = 960f + (float)(Area - SaveData.Instance.LastArea.ID) * 132f;
				if (Area < SaveData.Instance.LastArea.ID)
				{
					targetX -= 80f;
				}
				else if (Area > SaveData.Instance.LastArea.ID)
				{
					targetX += 80f;
				}
				float targetY = 130f;
				if (Area == SaveData.Instance.LastArea.ID)
				{
					targetY = 140f;
				}
				return new Vector2(targetX, targetY);
			}
		}

		public Vector2 HiddenPosition => new Vector2(IdlePosition.X, -100f);

		public OuiChapterSelectIcon(int area, MTexture front, MTexture back)
		{
			base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
			Position = new Vector2(0f, -100f);
			Area = area;
			this.front = front;
			this.back = back;
			Add(wiggler = Wiggler.Create(0.35f, 2f, delegate(float f)
			{
				Rotation = (wiggleLeft ? (0f - f) : f) * 0.4f;
				Scale = Vector2.One * (1f + f * 0.5f);
			}));
			Add(newWiggle = Wiggler.Create(0.8f, 2f));
			newWiggle.StartZero = true;
		}

		public void Hovered(int dir)
		{
			wiggleLeft = dir < 0;
			wiggler.Start();
		}

		public void Select()
		{
			Audio.Play("event:/ui/world_map/icon/flip_right");
			selected = true;
			hidden = false;
			Vector2 from = Position;
			StartTween(0.6f, delegate(Tween t)
			{
				SetSelectedPercent(from, t.Percent);
			});
		}

		public void SnapToSelected()
		{
			selected = true;
			hidden = false;
			StopTween();
		}

		public void Unselect()
		{
			Audio.Play("event:/ui/world_map/icon/flip_left");
			hidden = false;
			selected = false;
			Vector2 to = IdlePosition;
			StartTween(0.6f, delegate(Tween t)
			{
				SetSelectedPercent(to, 1f - t.Percent);
			});
		}

		public void Hide()
		{
			Scale = Vector2.One;
			hidden = true;
			selected = false;
			Vector2 from = Position;
			StartTween(0.25f, delegate
			{
				Position = Vector2.Lerp(from, HiddenPosition, tween.Eased);
			});
		}

		public void Show()
		{
			if (SaveData.Instance != null)
			{
				New = SaveData.Instance.Areas[Area].Modes[0].TimePlayed <= 0;
			}
			Scale = Vector2.One;
			hidden = false;
			selected = false;
			Vector2 from = Position;
			StartTween(0.25f, delegate
			{
				Position = Vector2.Lerp(from, IdlePosition, tween.Eased);
			});
		}

		public void AssistModeUnlock(Action onComplete)
		{
			Add(new Coroutine(AssistModeUnlockRoutine(onComplete)));
		}

		private IEnumerator AssistModeUnlockRoutine(Action onComplete)
		{
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime * 4f)
			{
				spotlightRadius = Ease.CubeOut(p3) * 128f;
				spotlightAlpha = Ease.CubeOut(p3) * 0.8f;
				yield return null;
			}
			shake.X = 6f;
			for (int i = 0; i < 10; i++)
			{
				shake.X = 0f - shake.X;
				yield return 0.01f;
			}
			shake = Vector2.Zero;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime * 4f)
			{
				float ease = Ease.CubeIn(p3);
				shake = new Vector2(0f, -160f * ease);
				Scale = new Vector2(1f - p3, 1f + p3 * 0.25f);
				yield return null;
			}
			shake = Vector2.Zero;
			Scale = Vector2.One;
			AssistModeUnlockable = false;
			SaveData.Instance.UnlockedAreas++;
			wiggler.Start();
			yield return 1f;
			for (float p3 = 1f; p3 > 0f; p3 -= Engine.DeltaTime * 4f)
			{
				spotlightRadius = 128f + (1f - Ease.CubeOut(p3)) * 128f;
				spotlightAlpha = Ease.CubeOut(p3) * 0.8f;
				yield return null;
			}
			spotlightAlpha = 0f;
			onComplete?.Invoke();
		}

		public void HighlightUnlock(Action onComplete)
		{
			HideIcon = true;
			Add(new Coroutine(HighlightUnlockRoutine(onComplete)));
		}

		private IEnumerator HighlightUnlockRoutine(Action onComplete)
		{
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 2f)
			{
				spotlightRadius = 128f + (1f - Ease.CubeOut(p2)) * 128f;
				spotlightAlpha = Ease.CubeOut(p2) * 0.8f;
				yield return null;
			}
			Audio.Play("event:/ui/postgame/unlock_newchapter_icon");
			HideIcon = false;
			wiggler.Start();
			yield return 2f;
			for (float p2 = 1f; p2 > 0f; p2 -= Engine.DeltaTime * 2f)
			{
				spotlightRadius = 128f + (1f - Ease.CubeOut(p2)) * 128f;
				spotlightAlpha = Ease.CubeOut(p2) * 0.8f;
				yield return null;
			}
			spotlightAlpha = 0f;
			onComplete?.Invoke();
		}

		private void StartTween(float duration, Action<Tween> callback)
		{
			StopTween();
			Add(tween = Tween.Create(Tween.TweenMode.Oneshot, null, duration, start: true));
			tween.OnUpdate = callback;
			tween.OnComplete = delegate
			{
				tween = null;
			};
		}

		private void StopTween()
		{
			if (tween != null)
			{
				Remove(tween);
			}
			tween = null;
		}

		private void SetSelectedPercent(Vector2 from, float p)
		{
			OuiChapterPanel inspector = (base.Scene as Overworld).GetUI<OuiChapterPanel>();
			Vector2 target = inspector.OpenPosition + inspector.IconOffset;
			SimpleCurve curve = new SimpleCurve(from, target, (from + target) / 2f + new Vector2(0f, 30f));
			float scale = 1f + ((p < 0.5f) ? (p * 2f) : ((1f - p) * 2f));
			Scale.X = (float)Math.Cos(Ease.SineInOut(p) * ((float)Math.PI * 2f)) * scale;
			Scale.Y = scale;
			Position = curve.GetPoint(Ease.Invert(Ease.CubeInOut)(p));
			Rotation = Ease.UpDown(Ease.SineInOut(p)) * ((float)Math.PI / 180f) * 15f * (float)rotateDir;
			if (p <= 0f)
			{
				rotateDir = -1;
			}
			else if (p >= 1f)
			{
				rotateDir = 1;
			}
		}

		public override void Update()
		{
			if (SaveData.Instance == null)
			{
				return;
			}
			sizeEase = Calc.Approach(sizeEase, (SaveData.Instance.LastArea.ID == Area) ? 1f : 0f, Engine.DeltaTime * 4f);
			if (SaveData.Instance.LastArea.ID == Area)
			{
				base.Depth = -50;
			}
			else
			{
				base.Depth = -45;
			}
			if (tween == null)
			{
				if (selected)
				{
					OuiChapterPanel inspector = (base.Scene as Overworld).GetUI<OuiChapterPanel>();
					Position = ((!inspector.EnteringChapter) ? inspector.OpenPosition : inspector.Position) + inspector.IconOffset;
				}
				else if (!hidden)
				{
					Position = Calc.Approach(Position, IdlePosition, 2400f * Engine.DeltaTime);
				}
			}
			if (New && base.Scene.OnInterval(1.5f))
			{
				newWiggle.Start();
			}
			base.Update();
		}

		public override void Render()
		{
			MTexture icon = front;
			Vector2 scale = Scale;
			int width = icon.Width;
			if (scale.X < 0f)
			{
				icon = back;
			}
			if (AssistModeUnlockable)
			{
				icon = GFX.Gui["areas/lock"];
				width -= 32;
			}
			if (!HideIcon)
			{
				scale *= (100f + 44f * Ease.CubeInOut(sizeEase)) / (float)width;
				if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
				{
					scale.X = 0f - scale.X;
				}
				icon.DrawCentered(Position + shake, Color.White, scale, Rotation);
				if (New && SaveData.Instance != null && !SaveData.Instance.CheatMode && Area == SaveData.Instance.UnlockedAreas && !selected && tween == null && !AssistModeUnlockable && Celeste.PlayMode != Celeste.PlayModes.Event)
				{
					Vector2 at = Position + new Vector2((float)width * 0.25f, (float)(-icon.Height) * 0.25f);
					at += Vector2.UnitY * (0f - Math.Abs(newWiggle.Value * 30f));
					GFX.Gui["areas/new"].DrawCentered(at);
				}
			}
			if (spotlightAlpha > 0f)
			{
				HiresRenderer.EndRender();
				SpotlightWipe.DrawSpotlight(new Vector2(Position.X, IdlePosition.Y), spotlightRadius, Color.Black * spotlightAlpha);
				HiresRenderer.BeginRender();
			}
			else if (AssistModeUnlockable && SaveData.Instance.LastArea.ID == Area && !hidden)
			{
				ActiveFont.DrawOutline(Dialog.Clean("ASSIST_SKIP"), Position + new Vector2(0f, 100f), new Vector2(0.5f, 0f), Vector2.One * 0.7f, Color.White, 2f, Color.Black);
			}
		}
	}
}
