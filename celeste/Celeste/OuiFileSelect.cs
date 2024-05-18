using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiFileSelect : Oui
	{
		public OuiFileSelectSlot[] Slots = new OuiFileSelectSlot[3];

		public int SlotIndex;

		public bool SlotSelected;

		public static bool Loaded;

		private bool loadedSuccess;

		public bool HasSlots;

		public OuiFileSelect()
		{
			Loaded = false;
		}

		public override IEnumerator Enter(Oui from)
		{
			SlotSelected = false;
			if (!Loaded)
			{
				for (int l = 0; l < Slots.Length; l++)
				{
					if (Slots[l] != null)
					{
						base.Scene.Remove(Slots[l]);
					}
				}
                LoadThread();
				//RunThread.Start(LoadThread, "FILE_LOADING");
				float elapsed = 0f;
				while (!Loaded || elapsed < 0.5f)
				{
					elapsed += Engine.DeltaTime;
					yield return null;
				}
				for (int k = 0; k < Slots.Length; k++)
				{
					if (Slots[k] != null)
					{
						base.Scene.Add(Slots[k]);
					}
				}
				if (!loadedSuccess)
				{
					FileErrorOverlay error = new FileErrorOverlay(FileErrorOverlay.Error.Load);
					while (error.Open)
					{
						yield return null;
					}
					if (!error.Ignore)
					{
						base.Overworld.Goto<OuiMainMenu>();
						yield break;
					}
				}
			}
			else if (!(from is OuiFileNaming) && !(from is OuiAssistMode))
			{
				yield return 0.2f;
			}
			HasSlots = false;
			for (int j = 0; j < Slots.Length; j++)
			{
				if (Slots[j].Exists)
				{
					HasSlots = true;
				}
			}
			Audio.Play("event:/ui/main/whoosh_savefile_in");
			if (from is OuiFileNaming || from is OuiAssistMode)
			{
				if (!SlotSelected)
				{
					SelectSlot(reset: false);
				}
			}
			else if (!HasSlots)
			{
				SlotIndex = 0;
				Slots[SlotIndex].Position = new Vector2(Slots[SlotIndex].HiddenPosition(1, 0).X, Slots[SlotIndex].SelectedPosition.Y);
				SelectSlot(reset: true);
			}
			else if (!SlotSelected)
			{
				Alarm.Set(this, 0.4f, delegate
				{
					Audio.Play("event:/ui/main/savefile_rollover_first");
				});
				for (int i = 0; i < Slots.Length; i++)
				{
					Slots[i].Position = new Vector2(Slots[i].HiddenPosition(1, 0).X, Slots[i].IdlePosition.Y);
					Slots[i].Show();
					yield return 0.02f;
				}
			}
		}

		private void LoadThread()
		{
			if (UserIO.Open(UserIO.Mode.Read))
			{
				for (int i = 0; i < Slots.Length; i++)
				{
					OuiFileSelectSlot slot;
					if (!UserIO.Exists(SaveData.GetFilename(i)))
					{
						slot = new OuiFileSelectSlot(i, this, corrupted: false);
					}
					else
					{
						SaveData savedata = UserIO.Load<SaveData>(SaveData.GetFilename(i));
						if (savedata != null)
						{
							savedata.AfterInitialize();
							slot = new OuiFileSelectSlot(i, this, savedata);
						}
						else
						{
							slot = new OuiFileSelectSlot(i, this, corrupted: true);
						}
					}
					Slots[i] = slot;
				}
				UserIO.Close();
				loadedSuccess = true;
			}
			Loaded = true;
		}

		public override IEnumerator Leave(Oui next)
		{
			Audio.Play("event:/ui/main/whoosh_savefile_out");
			int slideTo = 1;
			if (next == null || next is OuiChapterSelect || next is OuiFileNaming || next is OuiAssistMode)
			{
				slideTo = -1;
			}
			for (int i = 0; i < Slots.Length; i++)
			{
				if (next is OuiFileNaming && SlotIndex == i)
				{
					Slots[i].MoveTo(Slots[i].IdlePosition.X, Slots[0].IdlePosition.Y);
				}
				else if (next is OuiAssistMode && SlotIndex == i)
				{
					Slots[i].MoveTo(Slots[i].IdlePosition.X, -400f);
				}
				else
				{
					Slots[i].Hide(slideTo, 0);
				}
				yield return 0.02f;
			}
		}

		public void UnselectHighlighted()
		{
			SlotSelected = false;
			Slots[SlotIndex].Unselect();
			for (int i = 0; i < Slots.Length; i++)
			{
				if (SlotIndex != i)
				{
					Slots[i].Show();
				}
			}
		}

		public void SelectSlot(bool reset)
		{
			if (!Slots[SlotIndex].Exists && reset)
			{
				if (Settings.Instance != null && !string.IsNullOrWhiteSpace(Settings.Instance.DefaultFileName))
				{
					Slots[SlotIndex].Name = Settings.Instance.DefaultFileName;
				}
				else
				{
					Slots[SlotIndex].Name = Dialog.Clean("FILE_DEFAULT");
				}
				Slots[SlotIndex].AssistModeEnabled = false;
				Slots[SlotIndex].VariantModeEnabled = false;
			}
			SlotSelected = true;
			Slots[SlotIndex].Select(reset);
			for (int i = 0; i < Slots.Length; i++)
			{
				if (SlotIndex != i)
				{
					Slots[i].Hide(0, (i >= SlotIndex) ? 1 : (-1));
				}
			}
		}

		public override void Update()
		{
			base.Update();
			if (!Focused)
			{
				return;
			}
			if (!SlotSelected)
			{
				if (Input.MenuUp.Pressed && SlotIndex > 0)
				{
					Audio.Play("event:/ui/main/savefile_rollover_up");
					SlotIndex--;
				}
				else if (Input.MenuDown.Pressed && SlotIndex < Slots.Length - 1)
				{
					Audio.Play("event:/ui/main/savefile_rollover_down");
					SlotIndex++;
				}
				else if (Input.MenuConfirm.Pressed)
				{
					Audio.Play("event:/ui/main/button_select");
					Audio.Play("event:/ui/main/whoosh_savefile_out");
					SelectSlot(reset: true);
				}
				else if (Input.MenuCancel.Pressed)
				{
					Audio.Play("event:/ui/main/button_back");
					base.Overworld.Goto<OuiMainMenu>();
				}
			}
			else if (Input.MenuCancel.Pressed && !HasSlots && !Slots[SlotIndex].StartingGame)
			{
				Audio.Play("event:/ui/main/button_back");
				base.Overworld.Goto<OuiMainMenu>();
			}
		}
	}
}
