using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public abstract class MenuButton : Entity
	{
		public Vector2 TargetPosition;

		public Vector2 TweenFrom;

		public MenuButton LeftButton;

		public MenuButton RightButton;

		public MenuButton UpButton;

		public MenuButton DownButton;

		public Action OnConfirm;

		private bool canAcceptInput;

		private Oui oui;

		private bool selected;

		private Tween tween;

		public Color SelectionColor
		{
			get
			{
				if (selected)
				{
					if (!Settings.Instance.DisableFlashes && !base.Scene.BetweenInterval(0.1f))
					{
						return TextMenu.HighlightColorB;
					}
					return TextMenu.HighlightColorA;
				}
				return Color.White;
			}
		}

		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				if (base.Scene == null)
				{
					throw new Exception("Cannot set Selected while MenuButton is not in a Scene.");
				}
				if (!selected && value)
				{
					MenuButton old = GetSelection(base.Scene);
					if (old != null)
					{
						old.Selected = false;
					}
					selected = true;
					canAcceptInput = false;
					OnSelect();
				}
				else if (selected && !value)
				{
					selected = false;
					OnDeselect();
				}
			}
		}

		public abstract float ButtonHeight { get; }

		public static MenuButton GetSelection(Scene scene)
		{
			foreach (MenuButton button in scene.Tracker.GetEntities<MenuButton>())
			{
				if (button.Selected)
				{
					return button;
				}
			}
			return null;
		}

		public static void ClearSelection(Scene scene)
		{
			MenuButton sel = GetSelection(scene);
			if (sel != null)
			{
				sel.Selected = false;
			}
		}

		public MenuButton(Oui oui, Vector2 targetPosition, Vector2 tweenFrom, Action onConfirm)
			: base(tweenFrom)
		{
			TargetPosition = targetPosition;
			TweenFrom = tweenFrom;
			OnConfirm = onConfirm;
			this.oui = oui;
		}

		public override void Update()
		{
			base.Update();
			if (!canAcceptInput)
			{
				canAcceptInput = true;
			}
			else if (oui.Selected && oui.Focused && selected)
			{
				if (Input.MenuConfirm.Pressed)
				{
					Confirm();
				}
				else if (Input.MenuLeft.Pressed && LeftButton != null)
				{
					Audio.Play("event:/ui/main/rollover_up");
					LeftButton.Selected = true;
				}
				else if (Input.MenuRight.Pressed && RightButton != null)
				{
					Audio.Play("event:/ui/main/rollover_down");
					RightButton.Selected = true;
				}
				else if (Input.MenuUp.Pressed && UpButton != null)
				{
					Audio.Play("event:/ui/main/rollover_up");
					UpButton.Selected = true;
				}
				else if (Input.MenuDown.Pressed && DownButton != null)
				{
					Audio.Play("event:/ui/main/rollover_down");
					DownButton.Selected = true;
				}
			}
		}

		public void TweenIn(float time)
		{
			if (tween != null && tween.Entity == this)
			{
				tween.RemoveSelf();
			}
			Vector2 from = Position;
			Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, time, start: true));
			tween.OnUpdate = delegate(Tween t)
			{
				Position = Vector2.Lerp(from, TargetPosition, t.Eased);
			};
		}

		public void TweenOut(float time)
		{
			if (tween != null && tween.Entity == this)
			{
				tween.RemoveSelf();
			}
			Vector2 from = Position;
			Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, time, start: true));
			tween.OnUpdate = delegate(Tween t)
			{
				Position = Vector2.Lerp(from, TweenFrom, t.Eased);
			};
		}

		public virtual void OnSelect()
		{
		}

		public virtual void OnDeselect()
		{
		}

		public virtual void Confirm()
		{
			OnConfirm();
		}

		public virtual void StartSelected()
		{
			selected = true;
		}
	}
}
