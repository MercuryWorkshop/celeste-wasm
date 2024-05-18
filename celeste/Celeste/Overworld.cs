using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Overworld : Scene, IOverlayHandler
	{
		public enum StartMode
		{
			Titlescreen,
			ReturnFromOptions,
			AreaComplete,
			AreaQuit,
			ReturnFromPico8,
			MainMenu
		}

		private class InputEntity : Entity
		{
			public Overworld Overworld;

			private Wiggler confirmWiggle;

			private Wiggler cancelWiggle;

			private float confirmWiggleDelay;

			private float cancelWiggleDelay;

			public InputEntity(Overworld overworld)
			{
				Overworld = overworld;
				base.Tag = Tags.HUD;
				base.Depth = -100000;
				Add(confirmWiggle = Wiggler.Create(0.4f, 4f));
				Add(cancelWiggle = Wiggler.Create(0.4f, 4f));
			}

			public override void Update()
			{
				if (Input.MenuConfirm.Pressed && confirmWiggleDelay <= 0f)
				{
					confirmWiggle.Start();
					confirmWiggleDelay = 0.5f;
				}
				if (Input.MenuCancel.Pressed && cancelWiggleDelay <= 0f)
				{
					cancelWiggle.Start();
					cancelWiggleDelay = 0.5f;
				}
				confirmWiggleDelay -= Engine.DeltaTime;
				cancelWiggleDelay -= Engine.DeltaTime;
				base.Update();
			}

			public override void Render()
			{
				float ease = Overworld.inputEase;
				if (ease > 0f)
				{
					float scale = 0.5f;
					int spacing = 32;
					string cancelText = Dialog.Clean("ui_cancel");
					string confirmText = Dialog.Clean("ui_confirm");
					float cancelWidth = ButtonUI.Width(cancelText, Input.MenuCancel);
					float confirmWidth = ButtonUI.Width(confirmText, Input.MenuConfirm);
					Vector2 position = new Vector2(1880f, 1024f);
					position.X += (40f + (confirmWidth + cancelWidth) * scale + (float)spacing) * (1f - Ease.CubeOut(ease));
					ButtonUI.Render(position, cancelText, Input.MenuCancel, scale, 1f, cancelWiggle.Value * 0.05f);
					if (Overworld.ShowConfirmUI)
					{
						position.X -= scale * cancelWidth + (float)spacing;
						ButtonUI.Render(position, confirmText, Input.MenuConfirm, scale, 1f, confirmWiggle.Value * 0.05f);
					}
				}
			}
		}

		public List<Oui> UIs = new List<Oui>();

		public Oui Current;

		public Oui Last;

		public Oui Next;

		public bool EnteringPico8;

		public bool ShowInputUI = true;

		public bool ShowConfirmUI = true;

		private float inputEase;

		public MountainRenderer Mountain;

		public HiresSnow Snow;

		private Snow3D Snow3D;

		public Maddy3D Maddy;

		private Entity routineEntity;

		private bool transitioning;

		private int lastArea = -1;

		public Overlay Overlay { get; set; }

		public Overworld(OverworldLoader loader)
		{
			Add(Mountain = new MountainRenderer());
			Add(new HudRenderer());
			Add(routineEntity = new Entity());
			Add(new InputEntity(this));
			Snow = loader.Snow;
			if (Snow == null)
			{
				Snow = new HiresSnow();
			}
			Add(Snow);
			base.RendererList.UpdateLists();
			Add(Snow3D = new Snow3D(Mountain.Model));
			Add(new MoonParticle3D(Mountain.Model, new Vector3(0f, 31f, 0f)));
			Add(Maddy = new Maddy3D(Mountain));
			ReloadMenus(loader.StartMode);
			Mountain.OnEaseEnd = delegate
			{
				if (Mountain.Area >= 0 && (!Maddy.Show || lastArea != Mountain.Area))
				{
					Maddy.Running(Mountain.Area < 7);
					Maddy.Wiggler.Start();
				}
				lastArea = Mountain.Area;
			};
			lastArea = Mountain.Area;
			if (Mountain.Area < 0)
			{
				Maddy.Hide();
			}
			else
			{
				Maddy.Position = AreaData.Areas[Mountain.Area].MountainCursor;
			}
			Settings.Instance.ApplyVolumes();
		}

		public override void Begin()
		{
			base.Begin();
			SetNormalMusic();
			ScreenWipe.WipeColor = Color.Black;
			new FadeWipe(this, wipeIn: true);
			base.RendererList.UpdateLists();
			if (!EnteringPico8)
			{
				base.RendererList.MoveToFront(Snow);
				base.RendererList.UpdateLists();
			}
			EnteringPico8 = false;
			ReloadMountainStuff();
		}

		public override void End()
		{
			if (!EnteringPico8)
			{
				Mountain.Dispose();
			}
			base.End();
		}

		public void ReloadMenus(StartMode startMode = StartMode.Titlescreen)
		{
			foreach (Oui ui2 in UIs)
			{
				Remove(ui2);
			}
			UIs.Clear();
			Type[] types = Assembly.GetEntryAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (typeof(Oui).IsAssignableFrom(type) && !type.IsAbstract)
				{
					Oui ui = (Oui)Activator.CreateInstance(type);
					ui.Visible = false;
					Add(ui);
					UIs.Add(ui);
					if (ui.IsStart(this, startMode))
					{
						ui.Visible = true;
						Last = (Current = ui);
					}
				}
			}
		}

		public void SetNormalMusic()
		{
			Audio.SetMusic("event:/music/menu/level_select");
			Audio.SetAmbience("event:/env/amb/worldmap");
		}

		public void ReloadMountainStuff()
		{
			MTN.MountainBird.ReassignVertices();
			MTN.MountainMoon.ReassignVertices();
			MTN.MountainTerrain.ReassignVertices();
			MTN.MountainBuildings.ReassignVertices();
			MTN.MountainCoreWall.ReassignVertices();
			Mountain.Model.DisposeBillboardBuffers();
			Mountain.Model.ResetBillboardBuffers();
		}

		public override void HandleGraphicsReset()
		{
			ReloadMountainStuff();
			base.HandleGraphicsReset();
		}

		public override void Update()
		{
			if (Mountain.Area >= 0 && !Mountain.Animating)
			{
				Vector3 position = AreaData.Areas[Mountain.Area].MountainCursor;
				if (position != Vector3.Zero)
				{
					Maddy.Position = position + new Vector3(0f, (float)Math.Sin(TimeActive * 2f) * 0.02f, 0f);
				}
			}
			if (Overlay != null)
			{
				if (Overlay.XboxOverlay)
				{
					Mountain.Update(this);
					Snow3D.Update();
				}
				Overlay.Update();
				base.Entities.UpdateLists();
				if (Snow != null)
				{
					Snow.Update(this);
				}
			}
			else
			{
				if (!transitioning || !ShowInputUI)
				{
					inputEase = Calc.Approach(inputEase, (ShowInputUI && !Input.GuiInputController()) ? 1 : 0, Engine.DeltaTime * 4f);
				}
				base.Update();
			}
			if (SaveData.Instance != null && SaveData.Instance.LastArea.ID == 10 && 10 <= SaveData.Instance.UnlockedAreas && !IsCurrent<OuiMainMenu>())
			{
				Audio.SetMusicParam("moon", 1f);
			}
			else
			{
				Audio.SetMusicParam("moon", 0f);
			}
			float duration = 1f;
			bool wipe = false;
			foreach (Renderer renderer in base.RendererList.Renderers)
			{
				if (renderer is ScreenWipe)
				{
					wipe = true;
					duration = (renderer as ScreenWipe).Duration;
				}
			}
			bool title = (Current is OuiTitleScreen && Next == null) || Next is OuiTitleScreen;
			if (Snow != null)
			{
				Snow.ParticleAlpha = Calc.Approach(Snow.ParticleAlpha, (title || wipe || (Overlay != null && !Overlay.XboxOverlay)) ? 1 : 0, Engine.DeltaTime / duration);
			}
		}

		public T Goto<T>() where T : Oui
		{
			T next = GetUI<T>();
			if (next != null)
			{
				routineEntity.Add(new Coroutine(GotoRoutine(next)));
			}
			return next;
		}

		public bool IsCurrent<T>() where T : Oui
		{
			if (Current != null)
			{
				return Current is T;
			}
			return Last is T;
		}

		public T GetUI<T>() where T : Oui
		{
			Oui oui = null;
			foreach (Oui ui in UIs)
			{
				if (ui is T)
				{
					oui = ui;
				}
			}
			return oui as T;
		}

		private IEnumerator GotoRoutine(Oui next)
		{
			while (Current == null)
			{
				yield return null;
			}
			transitioning = true;
			Next = next;
			Last = Current;
			Current = null;
			Last.Focused = false;
			yield return Last.Leave(next);
			if (next.Scene != null)
			{
				yield return next.Enter(Last);
				next.Focused = true;
				Current = next;
				transitioning = false;
			}
			Next = null;
		}
	}
}
