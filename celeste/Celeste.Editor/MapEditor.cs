using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Editor
{
	public class MapEditor : Scene
	{
		private enum MouseModes
		{
			Hover,
			Pan,
			Select,
			Move,
			Resize
		}

		private static readonly Color gridColor = new Color(0.1f, 0.1f, 0.1f);

		private static Camera Camera;

		private static AreaKey area = AreaKey.None;

		private static float saveFlash = 0f;

		private MapData mapData;

		private List<LevelTemplate> levels = new List<LevelTemplate>();

		private Vector2 mousePosition;

		private MouseModes mouseMode;

		private Vector2 lastMouseScreenPosition;

		private Vector2 mouseDragStart;

		private HashSet<LevelTemplate> selection = new HashSet<LevelTemplate>();

		private HashSet<LevelTemplate> hovered = new HashSet<LevelTemplate>();

		private float fade;

		private List<Vector2[]> undoStack = new List<Vector2[]>();

		private List<Vector2[]> redoStack = new List<Vector2[]>();

		public MapEditor(AreaKey area, bool reloadMapData = true)
		{
			area.ID = Calc.Clamp(area.ID, 0, AreaData.Areas.Count - 1);
			mapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
			if (reloadMapData)
			{
				mapData.Reload();
			}
			foreach (LevelData level in mapData.Levels)
			{
				levels.Add(new LevelTemplate(level));
			}
			foreach (Rectangle rect in mapData.Filler)
			{
				levels.Add(new LevelTemplate(rect.X, rect.Y, rect.Width, rect.Height));
			}
			if (area != MapEditor.area)
			{
				MapEditor.area = area;
				Camera = new Camera();
				Camera.Zoom = 6f;
				Camera.CenterOrigin();
			}
			if (SaveData.Instance == null)
			{
				SaveData.InitializeDebugMode();
			}
		}

		public override void GainFocus()
		{
			base.GainFocus();
			SaveAndReload();
		}

		private void SelectAll()
		{
			selection.Clear();
			foreach (LevelTemplate level in levels)
			{
				selection.Add(level);
			}
		}

		public void Rename(string oldName, string newName)
		{
			LevelTemplate from = null;
			LevelTemplate to = null;
			foreach (LevelTemplate level in levels)
			{
				if (from == null && level.Name == oldName)
				{
					from = level;
					if (to != null)
					{
						break;
					}
				}
				else if (to == null && level.Name == newName)
				{
					to = level;
					if (from != null)
					{
						break;
					}
				}
			}
			string src = Path.Combine("..", "..", "..", "Content", "Levels", mapData.Filename);
			if (to == null)
			{
				File.Move(Path.Combine(src, from.Name + ".xml"), Path.Combine(src, newName + ".xml"));
				from.Name = newName;
			}
			else
			{
				string temp = Path.Combine(src, "TEMP.xml");
				File.Move(Path.Combine(src, from.Name + ".xml"), temp);
				File.Move(Path.Combine(src, to.Name + ".xml"), Path.Combine(src, oldName + ".xml"));
				File.Move(temp, Path.Combine(src, newName + ".xml"));
				from.Name = newName;
				to.Name = oldName;
			}
			Save();
		}

		private void Save()
		{
		}

		private void SaveAndReload()
		{
		}

		private void UpdateMouse()
		{
			mousePosition = Vector2.Transform(MInput.Mouse.Position, Matrix.Invert(Camera.Matrix));
		}

		public override void Update()
		{
			Vector2 mouseMoved = default(Vector2);
			mouseMoved.X = (lastMouseScreenPosition.X - MInput.Mouse.Position.X) / Camera.Zoom;
			mouseMoved.Y = (lastMouseScreenPosition.Y - MInput.Mouse.Position.Y) / Camera.Zoom;
			if (MInput.Keyboard.Pressed(Keys.Space) && MInput.Keyboard.Check(Keys.LeftControl))
			{
				Camera.Zoom = 6f;
				Camera.Position = Vector2.Zero;
			}
			int dir = Math.Sign(MInput.Mouse.WheelDelta);
			if ((dir > 0 && Camera.Zoom >= 1f) || Camera.Zoom > 1f)
			{
				Camera.Zoom += dir;
			}
			else
			{
				Camera.Zoom += (float)dir * 0.25f;
			}
			Camera.Zoom = Math.Max(0.25f, Math.Min(24f, Camera.Zoom));
			Camera.Position += new Vector2(Input.MoveX.Value, Input.MoveY.Value) * 300f * Engine.DeltaTime;
			UpdateMouse();
			hovered.Clear();
			if (mouseMode == MouseModes.Hover)
			{
				mouseDragStart = mousePosition;
				if (MInput.Mouse.PressedLeftButton)
				{
					bool hit = LevelCheck(mousePosition);
					if (MInput.Keyboard.Check(Keys.Space))
					{
						mouseMode = MouseModes.Pan;
					}
					else if (MInput.Keyboard.Check(Keys.LeftControl))
					{
						if (hit)
						{
							ToggleSelection(mousePosition);
						}
						else
						{
							mouseMode = MouseModes.Select;
						}
					}
					else if (MInput.Keyboard.Check(Keys.F))
					{
						levels.Add(new LevelTemplate((int)mousePosition.X, (int)mousePosition.Y, 32, 32));
					}
					else if (hit)
					{
						if (!SelectionCheck(mousePosition))
						{
							SetSelection(mousePosition);
						}
						bool resizeFiller = false;
						if (selection.Count == 1)
						{
							foreach (LevelTemplate level4 in selection)
							{
								if (level4.ResizePosition(mousePosition) && level4.Type == LevelTemplateType.Filler)
								{
									resizeFiller = true;
								}
							}
						}
						if (resizeFiller)
						{
							foreach (LevelTemplate item in selection)
							{
								item.StartResizing();
							}
							mouseMode = MouseModes.Resize;
						}
						else
						{
							StoreUndo();
							foreach (LevelTemplate item2 in selection)
							{
								item2.StartMoving();
							}
							mouseMode = MouseModes.Move;
						}
					}
					else
					{
						mouseMode = MouseModes.Select;
					}
				}
				else if (MInput.Mouse.PressedRightButton)
				{
					LevelTemplate level3 = TestCheck(mousePosition);
					if (level3 != null)
					{
						if (level3.Type == LevelTemplateType.Filler)
						{
							if (MInput.Keyboard.Check(Keys.F))
							{
								levels.Remove(level3);
							}
						}
						else
						{
							LoadLevel(level3, mousePosition * 8f);
						}
						return;
					}
				}
				else if (MInput.Mouse.PressedMiddleButton)
				{
					mouseMode = MouseModes.Pan;
				}
				else if (!MInput.Keyboard.Check(Keys.Space))
				{
					foreach (LevelTemplate level2 in levels)
					{
						if (level2.Check(mousePosition))
						{
							hovered.Add(level2);
						}
					}
					if (MInput.Keyboard.Check(Keys.LeftControl))
					{
						if (MInput.Keyboard.Pressed(Keys.Z))
						{
							Undo();
						}
						else if (MInput.Keyboard.Pressed(Keys.Y))
						{
							Redo();
						}
						else if (MInput.Keyboard.Pressed(Keys.A))
						{
							SelectAll();
						}
					}
				}
			}
			else if (mouseMode == MouseModes.Pan)
			{
				Camera.Position += mouseMoved;
				if (!MInput.Mouse.CheckLeftButton && !MInput.Mouse.CheckMiddleButton)
				{
					mouseMode = MouseModes.Hover;
				}
			}
			else if (mouseMode == MouseModes.Select)
			{
				Rectangle rect = GetMouseRect(mouseDragStart, mousePosition);
				foreach (LevelTemplate level in levels)
				{
					if (level.Check(rect))
					{
						hovered.Add(level);
					}
				}
				if (!MInput.Mouse.CheckLeftButton)
				{
					if (MInput.Keyboard.Check(Keys.LeftControl))
					{
						ToggleSelection(rect);
					}
					else
					{
						SetSelection(rect);
					}
					mouseMode = MouseModes.Hover;
				}
			}
			else if (mouseMode == MouseModes.Move)
			{
				Vector2 move2 = (mousePosition - mouseDragStart).Round();
				bool snap = selection.Count == 1 && !MInput.Keyboard.Check(Keys.LeftAlt);
				foreach (LevelTemplate item3 in selection)
				{
					item3.Move(move2, levels, snap);
				}
				if (!MInput.Mouse.CheckLeftButton)
				{
					mouseMode = MouseModes.Hover;
				}
			}
			else if (mouseMode == MouseModes.Resize)
			{
				Vector2 move = (mousePosition - mouseDragStart).Round();
				foreach (LevelTemplate item4 in selection)
				{
					item4.Resize(move);
				}
				if (!MInput.Mouse.CheckLeftButton)
				{
					mouseMode = MouseModes.Hover;
				}
			}
			if (MInput.Keyboard.Pressed(Keys.D1))
			{
				SetEditorColor(0);
			}
			else if (MInput.Keyboard.Pressed(Keys.D2))
			{
				SetEditorColor(1);
			}
			else if (MInput.Keyboard.Pressed(Keys.D3))
			{
				SetEditorColor(2);
			}
			else if (MInput.Keyboard.Pressed(Keys.D4))
			{
				SetEditorColor(3);
			}
			else if (MInput.Keyboard.Pressed(Keys.D5))
			{
				SetEditorColor(4);
			}
			else if (MInput.Keyboard.Pressed(Keys.D6))
			{
				SetEditorColor(5);
			}
			else if (MInput.Keyboard.Pressed(Keys.D7))
			{
				SetEditorColor(6);
			}
			if (MInput.Keyboard.Pressed(Keys.F1) || (MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S)))
			{
				SaveAndReload();
				return;
			}
			if (saveFlash > 0f)
			{
				saveFlash -= Engine.DeltaTime * 4f;
			}
			lastMouseScreenPosition = MInput.Mouse.Position;
			base.Update();
		}

		private void SetEditorColor(int index)
		{
			foreach (LevelTemplate item in selection)
			{
				item.EditorColorIndex = index;
			}
		}

		public override void Render()
		{
			UpdateMouse();
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix * Engine.ScreenMatrix);
			float w = 1920f / Camera.Zoom;
			float h = 1080f / Camera.Zoom;
			int gridSize = 5;
			float left2 = (float)Math.Floor(Camera.Left / (float)gridSize - 1f) * (float)gridSize;
			float top = (float)Math.Floor(Camera.Top / (float)gridSize - 1f) * (float)gridSize;
			for (float k = left2; k <= left2 + w + 10f; k += 5f)
			{
				Draw.Line(k, Camera.Top, k, Camera.Top + h, gridColor);
			}
			for (float j = top; j <= top + h + 10f; j += 5f)
			{
				Draw.Line(Camera.Left, j, Camera.Left + w, j, gridColor);
			}
			Draw.Line(0f, Camera.Top, 0f, Camera.Top + h, Color.DarkSlateBlue, 1f / Camera.Zoom);
			Draw.Line(Camera.Left, 0f, Camera.Left + w, 0f, Color.DarkSlateBlue, 1f / Camera.Zoom);
			foreach (LevelTemplate level3 in levels)
			{
				level3.RenderContents(Camera, levels);
			}
			foreach (LevelTemplate level4 in levels)
			{
				level4.RenderOutline(Camera);
			}
			foreach (LevelTemplate level2 in levels)
			{
				level2.RenderHighlight(Camera, selection.Contains(level2), hovered.Contains(level2));
			}
			if (mouseMode == MouseModes.Hover)
			{
				Draw.Line(mousePosition.X - 12f / Camera.Zoom, mousePosition.Y, mousePosition.X + 12f / Camera.Zoom, mousePosition.Y, Color.Yellow, 3f / Camera.Zoom);
				Draw.Line(mousePosition.X, mousePosition.Y - 12f / Camera.Zoom, mousePosition.X, mousePosition.Y + 12f / Camera.Zoom, Color.Yellow, 3f / Camera.Zoom);
			}
			else if (mouseMode == MouseModes.Select)
			{
				Draw.Rect(GetMouseRect(mouseDragStart, mousePosition), Color.Lime * 0.25f);
			}
			if (saveFlash > 0f)
			{
				Draw.Rect(Camera.Left, Camera.Top, w, h, Color.White * Ease.CubeInOut(saveFlash));
			}
			if (fade > 0f)
			{
				Draw.Rect(0f, 0f, 320f, 180f, Color.Black * fade);
			}
			Draw.SpriteBatch.End();
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
			Draw.Rect(0f, 0f, 1920f, 72f, Color.Black);
			Vector2 left = new Vector2(16f, 4f);
			Vector2 right = new Vector2(1904f, 4f);
			if (MInput.Keyboard.Check(Keys.Q))
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.25f);
				foreach (LevelTemplate level in levels)
				{
					int i = 0;
					while (level.Strawberries != null && i < level.Strawberries.Count)
					{
						Vector2 strawb = level.Strawberries[i];
						ActiveFont.DrawOutline(level.StrawberryMetadata[i], (new Vector2((float)level.X + strawb.X, (float)level.Y + strawb.Y) - Camera.Position) * Camera.Zoom + new Vector2(960f, 532f), new Vector2(0.5f, 1f), Vector2.One * 1f, Color.Red, 2f, Color.Black);
						i++;
					}
				}
			}
			if (hovered.Count == 0)
			{
				if (selection.Count > 0)
				{
					ActiveFont.Draw(selection.Count + " levels selected", left, Color.Red);
				}
				else
				{
					ActiveFont.Draw(Dialog.Clean(mapData.Data.Name), left, Color.Aqua);
					ActiveFont.Draw(string.Concat(mapData.Area.Mode, " MODE"), right, Vector2.UnitX, Vector2.One, Color.Red);
				}
			}
			else if (hovered.Count == 1)
			{
				LevelTemplate lvl = null;
				using (HashSet<LevelTemplate>.Enumerator enumerator2 = hovered.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						lvl = enumerator2.Current;
					}
				}
				string text = lvl.ActualWidth.ToString() + "x" + lvl.ActualHeight.ToString() + "   " + lvl.X + "," + lvl.Y + "   " + lvl.X * 8 + "," + lvl.Y * 8;
				ActiveFont.Draw(lvl.Name, left, Color.Yellow);
				ActiveFont.Draw(text, right, Vector2.UnitX, Vector2.One, Color.Green);
			}
			else
			{
				ActiveFont.Draw(hovered.Count + " levels", left, Color.Yellow);
			}
			Draw.SpriteBatch.End();
		}

		private void LoadLevel(LevelTemplate level, Vector2 at)
		{
			Save();
			Engine.Scene = new LevelLoader(new Session(area)
			{
				FirstLevel = false,
				Level = level.Name,
				StartedFromBeginning = false
			}, at);
		}

		private void StoreUndo()
		{
			Vector2[] state = new Vector2[levels.Count];
			for (int i = 0; i < levels.Count; i++)
			{
				state[i] = new Vector2(levels[i].X, levels[i].Y);
			}
			undoStack.Add(state);
			while (undoStack.Count > 30)
			{
				undoStack.RemoveAt(0);
			}
			redoStack.Clear();
		}

		private void Undo()
		{
			if (undoStack.Count > 0)
			{
				Vector2[] redo = new Vector2[levels.Count];
				for (int j = 0; j < levels.Count; j++)
				{
					redo[j] = new Vector2(levels[j].X, levels[j].Y);
				}
				redoStack.Add(redo);
				Vector2[] state = undoStack[undoStack.Count - 1];
				undoStack.RemoveAt(undoStack.Count - 1);
				for (int i = 0; i < state.Length; i++)
				{
					levels[i].X = (int)state[i].X;
					levels[i].Y = (int)state[i].Y;
				}
			}
		}

		private void Redo()
		{
			if (redoStack.Count > 0)
			{
				Vector2[] undo = new Vector2[levels.Count];
				for (int j = 0; j < levels.Count; j++)
				{
					undo[j] = new Vector2(levels[j].X, levels[j].Y);
				}
				undoStack.Add(undo);
				Vector2[] state = redoStack[undoStack.Count - 1];
				redoStack.RemoveAt(undoStack.Count - 1);
				for (int i = 0; i < state.Length; i++)
				{
					levels[i].X = (int)state[i].X;
					levels[i].Y = (int)state[i].Y;
				}
			}
		}

		private Rectangle GetMouseRect(Vector2 a, Vector2 b)
		{
			Vector2 min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
			Vector2 max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
			return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
		}

		private LevelTemplate TestCheck(Vector2 point)
		{
			foreach (LevelTemplate level in levels)
			{
				if (!level.Dummy && level.Check(point))
				{
					return level;
				}
			}
			return null;
		}

		private bool LevelCheck(Vector2 point)
		{
			foreach (LevelTemplate level in levels)
			{
				if (level.Check(point))
				{
					return true;
				}
			}
			return false;
		}

		private bool SelectionCheck(Vector2 point)
		{
			foreach (LevelTemplate item in selection)
			{
				if (item.Check(point))
				{
					return true;
				}
			}
			return false;
		}

		private bool SetSelection(Vector2 point)
		{
			selection.Clear();
			foreach (LevelTemplate level in levels)
			{
				if (level.Check(point))
				{
					selection.Add(level);
				}
			}
			return selection.Count > 0;
		}

		private bool ToggleSelection(Vector2 point)
		{
			bool hit = false;
			foreach (LevelTemplate level in levels)
			{
				if (level.Check(point))
				{
					hit = true;
					if (selection.Contains(level))
					{
						selection.Remove(level);
					}
					else
					{
						selection.Add(level);
					}
				}
			}
			return hit;
		}

		private void SetSelection(Rectangle rect)
		{
			selection.Clear();
			foreach (LevelTemplate level in levels)
			{
				if (level.Check(rect))
				{
					selection.Add(level);
				}
			}
		}

		private void ToggleSelection(Rectangle rect)
		{
			foreach (LevelTemplate level in levels)
			{
				if (level.Check(rect))
				{
					if (selection.Contains(level))
					{
						selection.Remove(level);
					}
					else
					{
						selection.Add(level);
					}
				}
			}
		}
	}
}
