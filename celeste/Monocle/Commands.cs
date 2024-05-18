using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Monocle
{
	public class Commands
	{
		private struct CommandInfo
		{
			public Action<string[]> Action;

			public string Help;

			public string Usage;
		}

		private struct Line
		{
			public string Text;

			public Color Color;

			public Line(string text)
			{
				Text = text;
				Color = Color.White;
			}

			public Line(string text, Color color)
			{
				Text = text;
				Color = color;
			}
		}

		private const float UNDERSCORE_TIME = 0.5f;

		private const float REPEAT_DELAY = 0.5f;

		private const float REPEAT_EVERY = 1f / 30f;

		private const float OPACITY = 0.8f;

		public bool Enabled = true;

		public bool Open;

		private Dictionary<string, CommandInfo> commands;

		private List<string> sorted;

		private KeyboardState oldState;

		private KeyboardState currentState;

		private string currentText = "";

		private List<Line> drawCommands;

		private bool underscore;

		private float underscoreCounter;

		private List<string> commandHistory;

		private int seekIndex = -1;

		private int tabIndex = -1;

		private string tabSearch;

		private float repeatCounter;

		private Keys? repeatKey;

		private bool canOpen;

		public Action[] FunctionKeyActions { get; private set; }

		public Commands()
		{
			commandHistory = new List<string>();
			drawCommands = new List<Line>();
			commands = new Dictionary<string, CommandInfo>();
			sorted = new List<string>();
			FunctionKeyActions = new Action[12];
			BuildCommandsList();
		}

		public void Log(object obj, Color color)
		{
			string str = obj.ToString();
			if (str.Contains("\n"))
			{
				string[] array = str.Split('\n');
				foreach (string line in array)
				{
					Log(line, color);
				}
				return;
			}
			int maxWidth = Engine.Instance.Window.ClientBounds.Width - 40;
			while (Draw.DefaultFont.MeasureString(str).X > (float)maxWidth)
			{
				int split = -1;
				for (int i = 0; i < str.Length; i++)
				{
					if (str[i] == ' ')
					{
						if (!(Draw.DefaultFont.MeasureString(str.Substring(0, i)).X <= (float)maxWidth))
						{
							break;
						}
						split = i;
					}
				}
				if (split == -1)
				{
					break;
				}
				drawCommands.Insert(0, new Line(str.Substring(0, split), color));
				str = str.Substring(split + 1);
			}
			drawCommands.Insert(0, new Line(str, color));
			int maxCommands = (Engine.Instance.Window.ClientBounds.Height - 100) / 30;
			while (drawCommands.Count > maxCommands)
			{
				drawCommands.RemoveAt(drawCommands.Count - 1);
			}
		}

		public void Log(object obj)
		{
			Log(obj, Color.White);
		}

		internal void UpdateClosed()
		{
			if (!canOpen)
			{
				canOpen = true;
			}
			else if (MInput.Keyboard.Pressed(Keys.OemTilde, Keys.Oem8))
			{
				Open = true;
				currentState = Keyboard.GetState();
			}
			for (int i = 0; i < FunctionKeyActions.Length; i++)
			{
				if (MInput.Keyboard.Pressed((Keys)(112 + i)))
				{
					ExecuteFunctionKeyAction(i);
				}
			}
		}

		internal void UpdateOpen()
		{
			oldState = currentState;
			currentState = Keyboard.GetState();
			underscoreCounter += Engine.DeltaTime;
			while (underscoreCounter >= 0.5f)
			{
				underscoreCounter -= 0.5f;
				underscore = !underscore;
			}
			if (repeatKey.HasValue)
			{
				if (currentState[repeatKey.Value] == KeyState.Down)
				{
					repeatCounter += Engine.DeltaTime;
					while (repeatCounter >= 0.5f)
					{
						HandleKey(repeatKey.Value);
						repeatCounter -= 1f / 30f;
					}
				}
				else
				{
					repeatKey = null;
				}
			}
			Keys[] pressedKeys = currentState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				if (oldState[key] == KeyState.Up)
				{
					HandleKey(key);
					break;
				}
			}
		}

		private void HandleKey(Keys key)
		{
			if (key != Keys.Tab && key != Keys.LeftShift && key != Keys.RightShift && key != Keys.RightAlt && key != Keys.LeftAlt && key != Keys.RightControl && key != Keys.LeftControl)
			{
				tabIndex = -1;
			}
			if (key != Keys.OemTilde && key != Keys.Oem8 && key != Keys.Enter && repeatKey != key)
			{
				repeatKey = key;
				repeatCounter = 0f;
			}
			switch (key)
			{
			case Keys.D1:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "!";
				}
				else
				{
					currentText += "1";
				}
				return;
			case Keys.D2:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "@";
				}
				else
				{
					currentText += "2";
				}
				return;
			case Keys.D3:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "#";
				}
				else
				{
					currentText += "3";
				}
				return;
			case Keys.D4:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "$";
				}
				else
				{
					currentText += "4";
				}
				return;
			case Keys.D5:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "%";
				}
				else
				{
					currentText += "5";
				}
				return;
			case Keys.D6:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "^";
				}
				else
				{
					currentText += "6";
				}
				return;
			case Keys.D7:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "&";
				}
				else
				{
					currentText += "7";
				}
				return;
			case Keys.D8:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "*";
				}
				else
				{
					currentText += "8";
				}
				return;
			case Keys.D9:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "(";
				}
				else
				{
					currentText += "9";
				}
				return;
			case Keys.D0:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += ")";
				}
				else
				{
					currentText += "0";
				}
				return;
			case Keys.OemComma:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "<";
				}
				else
				{
					currentText += ",";
				}
				return;
			case Keys.OemPeriod:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += ">";
				}
				else
				{
					currentText += ".";
				}
				return;
			case Keys.OemQuestion:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "?";
				}
				else
				{
					currentText += "/";
				}
				return;
			case Keys.OemSemicolon:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += ":";
				}
				else
				{
					currentText += ";";
				}
				return;
			case Keys.OemQuotes:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "\"";
				}
				else
				{
					currentText += "'";
				}
				return;
			case Keys.OemBackslash:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "|";
				}
				else
				{
					currentText += "\\";
				}
				return;
			case Keys.OemOpenBrackets:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "{";
				}
				else
				{
					currentText += "[";
				}
				return;
			case Keys.OemCloseBrackets:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "}";
				}
				else
				{
					currentText += "]";
				}
				return;
			case Keys.OemMinus:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "_";
				}
				else
				{
					currentText += "-";
				}
				return;
			case Keys.OemPlus:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += "+";
				}
				else
				{
					currentText += "=";
				}
				return;
			case Keys.Space:
				currentText += " ";
				return;
			case Keys.Back:
				if (currentText.Length > 0)
				{
					currentText = currentText.Substring(0, currentText.Length - 1);
				}
				return;
			case Keys.Delete:
				currentText = "";
				return;
			case Keys.Up:
				if (seekIndex < commandHistory.Count - 1)
				{
					seekIndex++;
					currentText = string.Join(" ", commandHistory[seekIndex]);
				}
				return;
			case Keys.Down:
				if (seekIndex > -1)
				{
					seekIndex--;
					if (seekIndex == -1)
					{
						currentText = "";
						return;
					}
					currentText = string.Join(" ", commandHistory[seekIndex]);
				}
				return;
			case Keys.Tab:
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					if (tabIndex == -1)
					{
						tabSearch = currentText;
						FindLastTab();
					}
					else
					{
						tabIndex--;
						if (tabIndex < 0 || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
						{
							FindLastTab();
						}
					}
				}
				else if (tabIndex == -1)
				{
					tabSearch = currentText;
					FindFirstTab();
				}
				else
				{
					tabIndex++;
					if (tabIndex >= sorted.Count || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
					{
						FindFirstTab();
					}
				}
				if (tabIndex != -1)
				{
					currentText = sorted[tabIndex];
				}
				return;
			case Keys.F1:
			case Keys.F2:
			case Keys.F3:
			case Keys.F4:
			case Keys.F5:
			case Keys.F6:
			case Keys.F7:
			case Keys.F8:
			case Keys.F9:
			case Keys.F10:
			case Keys.F11:
			case Keys.F12:
				ExecuteFunctionKeyAction((int)(key - 112));
				return;
			case Keys.Enter:
				if (currentText.Length > 0)
				{
					EnterCommand();
				}
				return;
			case Keys.OemTilde:
			case Keys.Oem8:
				Open = (canOpen = false);
				return;
			}
			if (key.ToString().Length == 1)
			{
				if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
				{
					currentText += key;
				}
				else
				{
					currentText += key.ToString().ToLower();
				}
			}
		}

		private void EnterCommand()
		{
			string[] data = currentText.Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (commandHistory.Count == 0 || commandHistory[0] != currentText)
			{
				commandHistory.Insert(0, currentText);
			}
			drawCommands.Insert(0, new Line(currentText, Color.Aqua));
			currentText = "";
			seekIndex = -1;
			string[] args = new string[data.Length - 1];
			for (int i = 1; i < data.Length; i++)
			{
				args[i - 1] = data[i];
			}
			ExecuteCommand(data[0].ToLower(), args);
		}

		private void FindFirstTab()
		{
			for (int i = 0; i < sorted.Count; i++)
			{
				if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
				{
					tabIndex = i;
					break;
				}
			}
		}

		private void FindLastTab()
		{
			for (int i = 0; i < sorted.Count; i++)
			{
				if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
				{
					tabIndex = i;
				}
			}
		}

		internal void Render()
		{
			int screenWidth = Engine.ViewWidth;
			int screenHeight = Engine.ViewHeight;
			Draw.SpriteBatch.Begin();
			Draw.Rect(10f, screenHeight - 50, screenWidth - 20, 40f, Color.Black * 0.8f);
			if (underscore)
			{
				Draw.SpriteBatch.DrawString(Draw.DefaultFont, ">" + currentText + "_", new Vector2(20f, screenHeight - 42), Color.White);
			}
			else
			{
				Draw.SpriteBatch.DrawString(Draw.DefaultFont, ">" + currentText, new Vector2(20f, screenHeight - 42), Color.White);
			}
			if (drawCommands.Count > 0)
			{
				int height = 10 + 30 * drawCommands.Count;
				Draw.Rect(10f, screenHeight - height - 60, screenWidth - 20, height, Color.Black * 0.8f);
				for (int i = 0; i < drawCommands.Count; i++)
				{
					Draw.SpriteBatch.DrawString(Draw.DefaultFont, drawCommands[i].Text, new Vector2(20f, screenHeight - 92 - 30 * i), drawCommands[i].Color);
				}
			}
			Draw.SpriteBatch.End();
		}

		public void ExecuteCommand(string command, string[] args)
		{
			if (commands.ContainsKey(command))
			{
				commands[command].Action(args);
			}
			else
			{
				Log("Command '" + command + "' not found! Type 'help' for list of commands", Color.Yellow);
			}
		}

		public void ExecuteFunctionKeyAction(int num)
		{
			if (FunctionKeyActions[num] != null)
			{
				FunctionKeyActions[num]();
			}
		}

		private void BuildCommandsList()
		{
			Type[] types = Assembly.GetCallingAssembly().GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo method in methods)
				{
					ProcessMethod(method);
				}
			}
			types = Assembly.GetEntryAssembly().GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo method2 in methods)
				{
					ProcessMethod(method2);
				}
			}
			foreach (KeyValuePair<string, CommandInfo> command in commands)
			{
				sorted.Add(command.Key);
			}
			sorted.Sort();
		}

		private void ProcessMethod(MethodInfo method)
		{
			Command attr = null;
			object[] attrs = method.GetCustomAttributes(typeof(Command), inherit: false);
			if (attrs.Length != 0)
			{
				attr = attrs[0] as Command;
			}
			if (attr == null)
			{
				return;
			}
			if (!method.IsStatic)
			{
				throw new Exception(method.DeclaringType.Name + "." + method.Name + " is marked as a command, but is not static");
			}
			CommandInfo info = default(CommandInfo);
			info.Help = attr.Help;
			ParameterInfo[] parameters = method.GetParameters();
			object[] defaults = new object[parameters.Length];
			string[] usage = new string[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo p = parameters[i];
				usage[i] = p.Name + ":";
				if (p.ParameterType == typeof(string))
				{
					usage[i] += "string";
				}
				else if (p.ParameterType == typeof(int))
				{
					usage[i] += "int";
				}
				else if (p.ParameterType == typeof(float))
				{
					usage[i] += "float";
				}
				else
				{
					if (!(p.ParameterType == typeof(bool)))
					{
						throw new Exception(method.DeclaringType.Name + "." + method.Name + " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool");
					}
					usage[i] += "bool";
				}
				if (p.DefaultValue == DBNull.Value)
				{
					defaults[i] = null;
				}
				else if (p.DefaultValue != null)
				{
					defaults[i] = p.DefaultValue;
					if (p.ParameterType == typeof(string))
					{
						ref string reference = ref usage[i];
						reference = string.Concat(reference, "=\"", p.DefaultValue, "\"");
					}
					else
					{
						ref string reference2 = ref usage[i];
						reference2 = reference2 + "=" + p.DefaultValue;
					}
				}
				else
				{
					defaults[i] = null;
				}
			}
			if (usage.Length == 0)
			{
				info.Usage = "";
			}
			else
			{
				info.Usage = "[" + string.Join(" ", usage) + "]";
			}
			info.Action = delegate(string[] args)
			{
				if (parameters.Length == 0)
				{
					InvokeMethod(method);
				}
				else
				{
					object[] array = (object[])defaults.Clone();
					for (int j = 0; j < array.Length && j < args.Length; j++)
					{
						if (parameters[j].ParameterType == typeof(string))
						{
							array[j] = ArgString(args[j]);
						}
						else if (parameters[j].ParameterType == typeof(int))
						{
							array[j] = ArgInt(args[j]);
						}
						else if (parameters[j].ParameterType == typeof(float))
						{
							array[j] = ArgFloat(args[j]);
						}
						else if (parameters[j].ParameterType == typeof(bool))
						{
							array[j] = ArgBool(args[j]);
						}
					}
					InvokeMethod(method, array);
				}
			};
			commands[attr.Name] = info;
		}

		private void InvokeMethod(MethodInfo method, object[] param = null)
		{
			try
			{
				method.Invoke(null, param);
			}
			catch (Exception e)
			{
				Engine.Commands.Log(e.InnerException.Message, Color.Yellow);
				LogStackTrace(e.InnerException.StackTrace);
			}
		}

		private void LogStackTrace(string stackTrace)
		{
			string[] array = stackTrace.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				string log = array[i];
				int from2 = log.LastIndexOf(" in ") + 4;
				int to2 = log.LastIndexOf('\\') + 1;
				if (from2 != -1 && to2 != -1)
				{
					log = log.Substring(0, from2) + log.Substring(to2);
				}
				int from = log.IndexOf('(') + 1;
				int to = log.IndexOf(')');
				if (from != -1 && to != -1)
				{
					log = log.Substring(0, from) + log.Substring(to);
				}
				int colon = log.LastIndexOf(':');
				if (colon != -1)
				{
					log = log.Insert(colon + 1, " ").Insert(colon, " ");
				}
				log = log.TrimStart();
				log = "-> " + log;
				Engine.Commands.Log(log, Color.White);
			}
		}

		private static string ArgString(string arg)
		{
			if (arg == null)
			{
				return "";
			}
			return arg;
		}

		private static bool ArgBool(string arg)
		{
			if (arg != null)
			{
				if (!(arg == "0") && !(arg.ToLower() == "false"))
				{
					return !(arg.ToLower() == "f");
				}
				return false;
			}
			return false;
		}

		private static int ArgInt(string arg)
		{
			try
			{
				return Convert.ToInt32(arg);
			}
			catch
			{
				return 0;
			}
		}

		private static float ArgFloat(string arg)
		{
			try
			{
				return Convert.ToSingle(arg, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0f;
			}
		}

		[Command("clear", "Clears the terminal")]
		public static void Clear()
		{
			Engine.Commands.drawCommands.Clear();
		}

		[Command("exit", "Exits the game")]
		private static void Exit()
		{
			Engine.Instance.Exit();
		}

		[Command("vsync", "Enables or disables vertical sync")]
		private static void Vsync(bool enabled = true)
		{
			Engine.Graphics.SynchronizeWithVerticalRetrace = enabled;
			Engine.Graphics.ApplyChanges();
			Engine.Commands.Log("Vertical Sync " + (enabled ? "Enabled" : "Disabled"));
		}

		[Command("count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag")]
		private static void Count(int tagIndex = -1)
		{
			if (Engine.Scene == null)
			{
				Engine.Commands.Log("Current Scene is null!");
			}
			else if (tagIndex < 0)
			{
				Engine.Commands.Log(Engine.Scene.Entities.Count.ToString());
			}
			else
			{
				Engine.Commands.Log(Engine.Scene.TagLists[tagIndex].Count.ToString());
			}
		}

		[Command("tracker", "Logs all tracked objects in the scene. Set mode to 'e' for just entities, or 'c' for just components")]
		private static void Tracker(string mode)
		{
			if (Engine.Scene == null)
			{
				Engine.Commands.Log("Current Scene is null!");
			}
			else if (!(mode == "e"))
			{
				if (!(mode == "c"))
				{
					Engine.Commands.Log("-- Entities --");
					Engine.Scene.Tracker.LogEntities();
					Engine.Commands.Log("-- Components --");
					Engine.Scene.Tracker.LogComponents();
				}
				else
				{
					Engine.Scene.Tracker.LogComponents();
				}
			}
			else
			{
				Engine.Scene.Tracker.LogEntities();
			}
		}

		[Command("pooler", "Logs the pooled Entity counts")]
		private static void Pooler()
		{
			Engine.Pooler.Log();
		}

		[Command("fullscreen", "Switches to fullscreen mode")]
		private static void Fullscreen()
		{
			Engine.SetFullscreen();
		}

		[Command("window", "Switches to window mode")]
		private static void Window(int scale = 1)
		{
			Engine.SetWindowed(320 * scale, 180 * scale);
		}

		[Command("help", "Shows usage help for a given command")]
		private static void Help(string command)
		{
			if (Engine.Commands.sorted.Contains(command))
			{
				CommandInfo c = Engine.Commands.commands[command];
				StringBuilder str2 = new StringBuilder();
				str2.Append(":: ");
				str2.Append(command);
				if (!string.IsNullOrEmpty(c.Usage))
				{
					str2.Append(" ");
					str2.Append(c.Usage);
				}
				Engine.Commands.Log(str2.ToString());
				if (string.IsNullOrEmpty(c.Help))
				{
					Engine.Commands.Log("No help info set");
				}
				else
				{
					Engine.Commands.Log(c.Help);
				}
			}
			else
			{
				StringBuilder str = new StringBuilder();
				str.Append("Commands list: ");
				str.Append(string.Join(", ", Engine.Commands.sorted));
				Engine.Commands.Log(str.ToString());
				Engine.Commands.Log("Type 'help command' for more info on that command!");
			}
		}
	}
}
