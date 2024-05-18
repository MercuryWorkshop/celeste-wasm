using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class TextMenu : Entity
	{
		public enum InnerContentMode
		{
			OneColumn,
			TwoColumn
		}

		public abstract class Item
		{
			public bool Selectable;

			public bool Visible = true;

			public bool Disabled;

			public bool IncludeWidthInMeasurement = true;

			public bool AboveAll;

			public TextMenu Container;

			public Wiggler SelectWiggler;

			public Wiggler ValueWiggler;

			public Action OnEnter;

			public Action OnLeave;

			public Action OnPressed;

			public Action OnAltPressed;

			public Action OnUpdate;

			public bool Hoverable
			{
				get
				{
					if (Selectable && Visible)
					{
						return !Disabled;
					}
					return false;
				}
			}

			public float Width => LeftWidth() + RightWidth();

			public Item Enter(Action onEnter)
			{
				OnEnter = onEnter;
				return this;
			}

			public Item Leave(Action onLeave)
			{
				OnLeave = onLeave;
				return this;
			}

			public Item Pressed(Action onPressed)
			{
				OnPressed = onPressed;
				return this;
			}

			public Item AltPressed(Action onPressed)
			{
				OnAltPressed = onPressed;
				return this;
			}

			public virtual void ConfirmPressed()
			{
			}

			public virtual void LeftPressed()
			{
			}

			public virtual void RightPressed()
			{
			}

			public virtual void Added()
			{
			}

			public virtual void Update()
			{
			}

			public virtual float LeftWidth()
			{
				return 0f;
			}

			public virtual float RightWidth()
			{
				return 0f;
			}

			public virtual float Height()
			{
				return 0f;
			}

			public virtual void Render(Vector2 position, bool highlighted)
			{
			}
		}

		public class Header : Item
		{
			public const float Scale = 2f;

			public string Title;

			public Header(string title)
			{
				Title = title;
				Selectable = false;
				IncludeWidthInMeasurement = false;
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Title).X * 2f;
			}

			public override float Height()
			{
				return ActiveFont.LineHeight * 2f;
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				float alpha = Container.Alpha;
				Color outline = Color.Black * (alpha * alpha * alpha);
				ActiveFont.DrawEdgeOutline(Title, position + new Vector2(Container.Width * 0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Gray * alpha, 4f, Color.DarkSlateBlue * alpha, 2f, outline);
			}
		}

		public class SubHeader : Item
		{
			public const float Scale = 0.6f;

			public string Title;

			public bool TopPadding = true;

			public SubHeader(string title, bool topPadding = true)
			{
				Title = title;
				Selectable = false;
				TopPadding = topPadding;
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Title).X * 0.6f;
			}

			public override float Height()
			{
				return ((Title.Length > 0) ? (ActiveFont.LineHeight * 0.6f) : 0f) + (float)(TopPadding ? 48 : 0);
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				if (Title.Length > 0)
				{
					float alpha = Container.Alpha;
					Color outline = Color.Black * (alpha * alpha * alpha);
					int top = (TopPadding ? 32 : 0);
					Vector2 pos = position + ((Container.InnerContent == InnerContentMode.TwoColumn) ? new Vector2(0f, top) : new Vector2(Container.Width * 0.5f, top));
					Vector2 justify = new Vector2((Container.InnerContent == InnerContentMode.TwoColumn) ? 0f : 0.5f, 0.5f);
					ActiveFont.DrawOutline(Title, pos, justify, Vector2.One * 0.6f, Color.Gray * alpha, 2f, outline);
				}
			}
		}

		public class Option<T> : Item
		{
			public string Label;

			public int Index;

			public Action<T> OnValueChange;

			public int PreviousIndex;

			public List<Tuple<string, T>> Values = new List<Tuple<string, T>>();

			private float sine;

			private int lastDir;

			public Option(string label)
			{
				Label = label;
				Selectable = true;
			}

			public Option<T> Add(string label, T value, bool selected = false)
			{
				Values.Add(new Tuple<string, T>(label, value));
				if (selected)
				{
					PreviousIndex = (Index = Values.Count - 1);
				}
				return this;
			}

			public Option<T> Change(Action<T> action)
			{
				OnValueChange = action;
				return this;
			}

			public override void Added()
			{
				Container.InnerContent = InnerContentMode.TwoColumn;
			}

			public override void LeftPressed()
			{
				if (Index > 0)
				{
					Audio.Play("event:/ui/main/button_toggle_off");
					PreviousIndex = Index;
					Index--;
					lastDir = -1;
					ValueWiggler.Start();
					if (OnValueChange != null)
					{
						OnValueChange(Values[Index].Item2);
					}
				}
			}

			public override void RightPressed()
			{
				if (Index < Values.Count - 1)
				{
					Audio.Play("event:/ui/main/button_toggle_on");
					PreviousIndex = Index;
					Index++;
					lastDir = 1;
					ValueWiggler.Start();
					if (OnValueChange != null)
					{
						OnValueChange(Values[Index].Item2);
					}
				}
			}

			public override void ConfirmPressed()
			{
				if (Values.Count == 2)
				{
					if (Index == 0)
					{
						Audio.Play("event:/ui/main/button_toggle_on");
					}
					else
					{
						Audio.Play("event:/ui/main/button_toggle_off");
					}
					PreviousIndex = Index;
					Index = 1 - Index;
					lastDir = ((Index == 1) ? 1 : (-1));
					ValueWiggler.Start();
					if (OnValueChange != null)
					{
						OnValueChange(Values[Index].Item2);
					}
				}
			}

			public override void Update()
			{
				sine += Engine.RawDeltaTime;
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Label).X + 32f;
			}

			public override float RightWidth()
			{
				float width = 0f;
				foreach (Tuple<string, T> label in Values)
				{
					width = Math.Max(width, ActiveFont.Measure(label.Item1).X);
				}
				return width + 120f;
			}

			public override float Height()
			{
				return ActiveFont.LineHeight;
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				float alpha = Container.Alpha;
				Color outline = Color.Black * (alpha * alpha * alpha);
				Color c = (Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha));
				ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, c, 2f, outline);
				if (Values.Count > 0)
				{
					float rW = RightWidth();
					ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2(Container.Width - rW * 0.5f + (float)lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, c, 2f, outline);
					Vector2 sin = Vector2.UnitX * (highlighted ? ((float)Math.Sin(sine * 4f) * 4f) : 0f);
					bool las = Index > 0;
					Color lac = (las ? c : (Color.DarkSlateGray * alpha));
					Vector2 lap = position + new Vector2(Container.Width - rW + 40f + ((lastDir < 0) ? ((0f - ValueWiggler.Value) * 8f) : 0f), 0f) - (las ? sin : Vector2.Zero);
					ActiveFont.DrawOutline("<", lap, new Vector2(0.5f, 0.5f), Vector2.One, lac, 2f, outline);
					bool ras = Index < Values.Count - 1;
					Color rac = (ras ? c : (Color.DarkSlateGray * alpha));
					Vector2 rap = position + new Vector2(Container.Width - 40f + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + (ras ? sin : Vector2.Zero);
					ActiveFont.DrawOutline(">", rap, new Vector2(0.5f, 0.5f), Vector2.One, rac, 2f, outline);
				}
			}
		}

		public class Slider : Option<int>
		{
			public Slider(string label, Func<int, string> values, int min, int max, int value = -1)
				: base(label)
			{
				for (int i = min; i <= max; i++)
				{
					Add(values(i), i, value == i);
				}
			}
		}

		public class OnOff : Option<bool>
		{
			public OnOff(string label, bool on)
				: base(label)
			{
				Add(Dialog.Clean("options_off"), value: false, !on);
				Add(Dialog.Clean("options_on"), value: true, on);
			}
		}

		public class Setting : Item
		{
			public string ConfirmSfx = "event:/ui/main/button_select";

			public string Label;

			public List<object> Values = new List<object>();

			public Binding Binding;

			public bool BindingController;

			private int bindingHash;

			public Setting(string label, string value = "")
			{
				Label = label;
				Values.Add(value);
				Selectable = true;
			}

			public Setting(string label, Binding binding, bool controllerMode)
				: this(label)
			{
				Binding = binding;
				BindingController = controllerMode;
				bindingHash = 0;
			}

			public void Set(List<Keys> keys)
			{
				Values.Clear();
				int i = 0;
				for (int count = Math.Min(Input.MaxBindings, keys.Count); i < count; i++)
				{
					if (keys[i] == Keys.None)
					{
						continue;
					}
					MTexture tex = Input.GuiKey(keys[i], null);
					if (tex != null)
					{
						Values.Add(tex);
						continue;
					}
					string str = keys[i].ToString();
					string result = "";
					for (int j = 0; j < str.Length; j++)
					{
						if (j > 0 && char.IsUpper(str[j]))
						{
							result += " ";
						}
						result += str[j];
					}
					Values.Add(result);
				}
			}

			public void Set(List<Buttons> buttons)
			{
				Values.Clear();
				int i = 0;
				for (int count = Math.Min(Input.MaxBindings, buttons.Count); i < count; i++)
				{
					MTexture tex = Input.GuiSingleButton(buttons[i], Input.PrefixMode.Latest, null);
					if (tex != null)
					{
						Values.Add(tex);
						continue;
					}
					string str = buttons[i].ToString();
					string result = "";
					for (int j = 0; j < str.Length; j++)
					{
						if (j > 0 && char.IsUpper(str[j]))
						{
							result += " ";
						}
						result += str[j];
					}
					Values.Add(result);
				}
			}

			public override void Added()
			{
				Container.InnerContent = InnerContentMode.TwoColumn;
			}

			public override void ConfirmPressed()
			{
				Audio.Play(ConfirmSfx);
				base.ConfirmPressed();
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Label).X;
			}

			public override float RightWidth()
			{
				float width = 0f;
				foreach (object val in Values)
				{
					if (val is MTexture)
					{
						width += (float)(val as MTexture).Width;
					}
					else if (val is string)
					{
						width += ActiveFont.Measure(val as string).X * 0.7f + 16f;
					}
				}
				return width;
			}

			public override float Height()
			{
				return ActiveFont.LineHeight * 1.2f;
			}

			public override void Update()
			{
				if (Binding == null)
				{
					return;
				}
				int newHash = 17;
				if (BindingController)
				{
					foreach (Buttons item in Binding.Controller)
					{
						newHash = newHash * 31 + item.GetHashCode();
					}
				}
				else
				{
					foreach (Keys item2 in Binding.Keyboard)
					{
						newHash = newHash * 31 + item2.GetHashCode();
					}
				}
				if (newHash != bindingHash)
				{
					bindingHash = newHash;
					if (BindingController)
					{
						Set(Binding.Controller);
					}
					else
					{
						Set(Binding.Keyboard);
					}
				}
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				float alpha = Container.Alpha;
				Color outline = Color.Black * (alpha * alpha * alpha);
				Color c = (Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha));
				ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, c, 2f, outline);
				float rW = RightWidth();
				foreach (object val in Values)
				{
					if (val is MTexture)
					{
						MTexture tex = val as MTexture;
						tex.DrawJustified(position + new Vector2(Container.Width - rW, 0f), new Vector2(0f, 0.5f), Color.White * alpha);
						rW -= (float)tex.Width;
					}
					else if (val is string)
					{
						string text = val as string;
						float w = ActiveFont.Measure(val as string).X * 0.7f + 16f;
						ActiveFont.DrawOutline(text, position + new Vector2(Container.Width - rW + w * 0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, Color.LightGray * alpha, 2f, outline);
						rW -= w;
					}
				}
			}
		}

		public class Button : Item
		{
			public string ConfirmSfx = "event:/ui/main/button_select";

			public string Label;

			public bool AlwaysCenter;

			public Button(string label)
			{
				Label = label;
				Selectable = true;
			}

			public override void ConfirmPressed()
			{
				if (!string.IsNullOrEmpty(ConfirmSfx))
				{
					Audio.Play(ConfirmSfx);
				}
				base.ConfirmPressed();
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Label).X;
			}

			public override float Height()
			{
				return ActiveFont.LineHeight;
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				float alpha = Container.Alpha;
				Color color = (Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha));
				Color outline = Color.Black * (alpha * alpha * alpha);
				bool leftAlign = Container.InnerContent == InnerContentMode.TwoColumn && !AlwaysCenter;
				Vector2 pos = position + (leftAlign ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0f));
				Vector2 justify = ((leftAlign && !AlwaysCenter) ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f));
				ActiveFont.DrawOutline(Label, pos, justify, Vector2.One, color, 2f, outline);
			}
		}

		public class LanguageButton : Item
		{
			public string ConfirmSfx = "event:/ui/main/button_select";

			public string Label;

			public Language Language;

			public bool AlwaysCenter;

			public LanguageButton(string label, Language language)
			{
				Label = label;
				Language = language;
				Selectable = true;
			}

			public override void ConfirmPressed()
			{
				Audio.Play(ConfirmSfx);
				base.ConfirmPressed();
			}

			public override float LeftWidth()
			{
				return ActiveFont.Measure(Label).X;
			}

			public override float RightWidth()
			{
				return Language.Icon.Width;
			}

			public override float Height()
			{
				return ActiveFont.LineHeight;
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				float alpha = Container.Alpha;
				Color color = (Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha));
				Color outline = Color.Black * (alpha * alpha * alpha);
				ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, outline);
				Language.Icon.DrawJustified(position + new Vector2(Container.Width - RightWidth(), 0f), new Vector2(0f, 0.5f), Color.White, 1f);
			}
		}

		public bool Focused = true;

		public InnerContentMode InnerContent;

		private List<Item> items = new List<Item>();

		public int Selection = -1;

		public Vector2 Justify;

		public float ItemSpacing = 4f;

		public float MinWidth;

		public float Alpha = 1f;

		public Color HighlightColor = Color.White;

		public static readonly Color HighlightColorA = Calc.HexToColor("84FF54");

		public static readonly Color HighlightColorB = Calc.HexToColor("FCFF59");

		public Action OnESC;

		public Action OnCancel;

		public Action OnUpdate;

		public Action OnPause;

		public Action OnClose;

		public bool AutoScroll = true;

		public Item Current
		{
			get
			{
				if (items.Count <= 0 || Selection < 0)
				{
					return null;
				}
				return items[Selection];
			}
			set
			{
				Selection = items.IndexOf(value);
			}
		}

		public new float Width { get; private set; }

		public new float Height { get; private set; }

		public float LeftColumnWidth { get; private set; }

		public float RightColumnWidth { get; private set; }

		public float ScrollableMinSize => Engine.Height - 300;

		public int FirstPossibleSelection
		{
			get
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] != null && items[i].Hoverable)
					{
						return i;
					}
				}
				return 0;
			}
		}

		public int LastPossibleSelection
		{
			get
			{
				for (int i = items.Count - 1; i >= 0; i--)
				{
					if (items[i] != null && items[i].Hoverable)
					{
						return i;
					}
				}
				return 0;
			}
		}

		public float ScrollTargetY
		{
			get
			{
				float min = (float)(Engine.Height - 150) - Height * Justify.Y;
				float max = 150f + Height * Justify.Y;
				return Calc.Clamp((float)(Engine.Height / 2) + Height * Justify.Y - GetYOffsetOf(Current), min, max);
			}
		}

		public TextMenu()
		{
			base.Tag = (int)Tags.PauseUpdate | (int)Tags.HUD;
			Position = new Vector2(Engine.Width, Engine.Height) / 2f;
			Justify = new Vector2(0.5f, 0.5f);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (AutoScroll)
			{
				if (Height > ScrollableMinSize)
				{
					Position.Y = ScrollTargetY;
				}
				else
				{
					Position.Y = 540f;
				}
			}
		}

		public TextMenu Add(Item item)
		{
			items.Add(item);
			item.Container = this;
			Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f));
			Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f));
			item.ValueWiggler.UseRawDeltaTime = (item.SelectWiggler.UseRawDeltaTime = true);
			if (Selection == -1)
			{
				FirstSelection();
			}
			RecalculateSize();
			item.Added();
			return this;
		}

		public void Clear()
		{
			items = new List<Item>();
		}

		public int IndexOf(Item item)
		{
			return items.IndexOf(item);
		}

		public void FirstSelection()
		{
			Selection = -1;
			MoveSelection(1);
		}

		public void MoveSelection(int direction, bool wiggle = false)
		{
			int was = Selection;
			direction = Math.Sign(direction);
			int available = 0;
			foreach (Item item in items)
			{
				if (item.Hoverable)
				{
					available++;
				}
			}
			bool canWrap = available > 2;
			do
			{
				Selection += direction;
				if (canWrap)
				{
					if (Selection < 0)
					{
						Selection = items.Count - 1;
					}
					else if (Selection >= items.Count)
					{
						Selection = 0;
					}
				}
				else if (Selection < 0 || Selection > items.Count - 1)
				{
					Selection = Calc.Clamp(Selection, 0, items.Count - 1);
					break;
				}
			}
			while (!Current.Hoverable);
			if (!Current.Hoverable)
			{
				Selection = was;
			}
			if (Selection != was && Current != null)
			{
				if (was >= 0 && items[was] != null && items[was].OnLeave != null)
				{
					items[was].OnLeave();
				}
				if (Current.OnEnter != null)
				{
					Current.OnEnter();
				}
				if (wiggle)
				{
					Audio.Play((direction > 0) ? "event:/ui/main/rollover_down" : "event:/ui/main/rollover_up");
					Current.SelectWiggler.Start();
				}
			}
		}

		public void RecalculateSize()
		{
			float num2 = (Height = 0f);
			float num5 = (LeftColumnWidth = (RightColumnWidth = num2));
			foreach (Item item3 in items)
			{
				if (item3.IncludeWidthInMeasurement)
				{
					LeftColumnWidth = Math.Max(LeftColumnWidth, item3.LeftWidth());
				}
			}
			foreach (Item item2 in items)
			{
				if (item2.IncludeWidthInMeasurement)
				{
					RightColumnWidth = Math.Max(RightColumnWidth, item2.RightWidth());
				}
			}
			foreach (Item item in items)
			{
				if (item.Visible)
				{
					Height += item.Height() + ItemSpacing;
				}
			}
			Height -= ItemSpacing;
			Width = Math.Max(MinWidth, LeftColumnWidth + RightColumnWidth);
		}

		public float GetYOffsetOf(Item item)
		{
			if (item == null)
			{
				return 0f;
			}
			float y = 0f;
			foreach (Item current in items)
			{
				if (item.Visible)
				{
					y += current.Height() + ItemSpacing;
				}
				if (current == item)
				{
					break;
				}
			}
			return y - item.Height() * 0.5f - ItemSpacing;
		}

		public void Close()
		{
			if (Current != null && Current.OnLeave != null)
			{
				Current.OnLeave();
			}
			if (OnClose != null)
			{
				OnClose();
			}
			RemoveSelf();
		}

		public void CloseAndRun(IEnumerator routine, Action onClose)
		{
			Focused = false;
			Visible = false;
			Add(new Coroutine(CloseAndRunRoutine(routine, onClose)));
		}

		private IEnumerator CloseAndRunRoutine(IEnumerator routine, Action onClose)
		{
			yield return routine;
			onClose?.Invoke();
			Close();
		}

		public override void Update()
		{
			base.Update();
			if (OnUpdate != null)
			{
				OnUpdate();
			}
			if (Focused)
			{
				if (Input.MenuDown.Pressed)
				{
					if (!Input.MenuDown.Repeating || Selection != LastPossibleSelection)
					{
						MoveSelection(1, wiggle: true);
					}
				}
				else if (Input.MenuUp.Pressed && (!Input.MenuUp.Repeating || Selection != FirstPossibleSelection))
				{
					MoveSelection(-1, wiggle: true);
				}
				if (Current != null)
				{
					if (Input.MenuLeft.Pressed)
					{
						Current.LeftPressed();
					}
					if (Input.MenuRight.Pressed)
					{
						Current.RightPressed();
					}
					if (Input.MenuConfirm.Pressed)
					{
						Current.ConfirmPressed();
						if (Current.OnPressed != null)
						{
							Current.OnPressed();
						}
					}
					if (Input.MenuJournal.Pressed && Current.OnAltPressed != null)
					{
						Current.OnAltPressed();
					}
				}
				if (!Input.MenuConfirm.Pressed)
				{
					if (Input.MenuCancel.Pressed && OnCancel != null)
					{
						OnCancel();
					}
					else if (Input.ESC.Pressed && OnESC != null)
					{
						Input.ESC.ConsumeBuffer();
						OnESC();
					}
					else if (Input.Pause.Pressed && OnPause != null)
					{
						Input.Pause.ConsumeBuffer();
						OnPause();
					}
				}
			}
			foreach (Item item in items)
			{
				if (item.OnUpdate != null)
				{
					item.OnUpdate();
				}
				item.Update();
			}
			if (Settings.Instance.DisableFlashes)
			{
				HighlightColor = HighlightColorA;
			}
			else if (Engine.Scene.OnRawInterval(0.1f))
			{
				if (HighlightColor == HighlightColorA)
				{
					HighlightColor = HighlightColorB;
				}
				else
				{
					HighlightColor = HighlightColorA;
				}
			}
			if (AutoScroll)
			{
				if (Height > ScrollableMinSize)
				{
					Position.Y += (ScrollTargetY - Position.Y) * (1f - (float)Math.Pow(0.009999999776482582, Engine.RawDeltaTime));
				}
				else
				{
					Position.Y = 540f;
				}
			}
		}

		public override void Render()
		{
			RecalculateSize();
			Vector2 start = Position - Justify * new Vector2(Width, Height);
			Vector2 position = start;
			bool hasAnyAbove = false;
			foreach (Item item2 in items)
			{
				if (item2.Visible)
				{
					float itemHeight2 = item2.Height();
					if (!item2.AboveAll)
					{
						item2.Render(position + new Vector2(0f, itemHeight2 * 0.5f + item2.SelectWiggler.Value * 8f), Focused && Current == item2);
					}
					else
					{
						hasAnyAbove = true;
					}
					position.Y += itemHeight2 + ItemSpacing;
				}
			}
			if (!hasAnyAbove)
			{
				return;
			}
			position = start;
			foreach (Item item in items)
			{
				if (item.Visible)
				{
					float itemHeight = item.Height();
					if (item.AboveAll)
					{
						item.Render(position + new Vector2(0f, itemHeight * 0.5f + item.SelectWiggler.Value * 8f), Focused && Current == item);
					}
					position.Y += itemHeight + ItemSpacing;
				}
			}
		}
	}
}
