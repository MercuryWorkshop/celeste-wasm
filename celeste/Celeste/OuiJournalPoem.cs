using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalPoem : OuiJournalPage
	{
		private class PoemLine
		{
			public float Index;

			public string Text;

			public float HoveringEase;

			public float HoldingEase;

			public bool Remix;

			public void Render()
			{
				float left = 100f + Ease.CubeInOut(HoveringEase) * 20f;
				float centerY = GetY(Index);
				Draw.Rect(left, centerY - 22f, 810f, 44f, Color.White * 0.25f);
				Vector2 iconScale = Vector2.One * (0.6f + HoldingEase * 0.4f);
				MTN.Journal[Remix ? "heartgem1" : "heartgem0"].DrawCentered(new Vector2(left + 20f, centerY), Color.White, iconScale);
				Color tColor = Color.Black * (0.7f + HoveringEase * 0.3f);
				Vector2 tScale = Vector2.One * (0.5f + HoldingEase * 0.1f);
				ActiveFont.Draw(Text, new Vector2(left + 60f, centerY), new Vector2(0f, 0.5f), tScale, tColor);
			}
		}

		private const float textScale = 0.5f;

		private const float holdingScaleAdd = 0.1f;

		private const float poemHeight = 44f;

		private const float poemSpacing = 4f;

		private const float poemStanzaSpacing = 16f;

		private List<PoemLine> lines = new List<PoemLine>();

		private int index;

		private float slider;

		private bool dragging;

		private bool swapping;

		private Coroutine swapRoutine = new Coroutine();

		private Wiggler wiggler = Wiggler.Create(0.4f, 4f);

		private Tween tween;

		public OuiJournalPoem(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "page";
			swapRoutine.RemoveOnComplete = false;
			float y = 0f;
			foreach (string id in SaveData.Instance.Poem)
			{
				string text = Dialog.Clean("poem_" + id);
				text = text.Replace("\n", " - ");
				lines.Add(new PoemLine
				{
					Text = text,
					Index = y,
					Remix = AreaData.IsPoemRemix(id)
				});
				y += 1f;
			}
		}

		public static float GetY(float index)
		{
			return 120f + 44f * (index + 0.5f) + 4f * index + (float)((int)index / 4) * 16f;
		}

		public override void Redraw(VirtualRenderTarget buffer)
		{
			base.Redraw(buffer);
			Draw.SpriteBatch.Begin();
			ActiveFont.Draw(Dialog.Clean("journal_poem"), new Vector2(60f, 60f), new Vector2(0f, 0.5f), Vector2.One, Color.Black * 0.6f);
			foreach (PoemLine line in lines)
			{
				line.Render();
			}
			if (lines.Count > 0)
			{
				MTexture mTexture = MTN.Journal[dragging ? "poemSlider" : "poemArrow"];
				Vector2 pos = new Vector2(50f, GetY(slider));
				mTexture.DrawCentered(pos, Color.White, 1f + 0.25f * wiggler.Value);
			}
			Draw.SpriteBatch.End();
		}

		private IEnumerator Swap(int a, int b)
		{
			string temp2 = SaveData.Instance.Poem[a];
			SaveData.Instance.Poem[a] = SaveData.Instance.Poem[b];
			SaveData.Instance.Poem[b] = temp2;
			PoemLine poemA = lines[a];
			PoemLine poemB = lines[b];
			PoemLine temp = lines[a];
			lines[a] = lines[b];
			lines[b] = temp;
			tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.125f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				poemA.Index = MathHelper.Lerp(a, b, t.Eased);
				poemB.Index = MathHelper.Lerp(b, a, t.Eased);
			};
			tween.OnComplete = delegate
			{
				tween = null;
			};
			yield return tween.Wait();
			swapping = false;
		}

		public override void Update()
		{
			if (lines.Count <= 0)
			{
				return;
			}
			if (tween != null && tween.Active)
			{
				tween.Update();
			}
			if (Input.MenuDown.Pressed && index + 1 < lines.Count && !swapping)
			{
				if (dragging)
				{
					Audio.Play("event:/ui/world_map/journal/heart_shift_down");
					swapRoutine.Replace(Swap(index, index + 1));
					swapping = true;
				}
				else
				{
					Audio.Play("event:/ui/world_map/journal/heart_roll");
				}
				index++;
			}
			else if (Input.MenuUp.Pressed && index > 0 && !swapping)
			{
				if (dragging)
				{
					Audio.Play("event:/ui/world_map/journal/heart_shift_up");
					swapRoutine.Replace(Swap(index, index - 1));
					swapping = true;
				}
				else
				{
					Audio.Play("event:/ui/world_map/journal/heart_roll");
				}
				index--;
			}
			else if (Input.MenuConfirm.Pressed)
			{
				Journal.PageTurningLocked = true;
				Audio.Play("event:/ui/world_map/journal/heart_grab");
				dragging = true;
				wiggler.Start();
			}
			else if (!Input.MenuConfirm.Check && dragging)
			{
				Journal.PageTurningLocked = false;
				Audio.Play("event:/ui/world_map/journal/heart_release");
				dragging = false;
				wiggler.Start();
			}
			for (int i = 0; i < lines.Count; i++)
			{
				PoemLine poemLine = lines[i];
				poemLine.HoveringEase = Calc.Approach(poemLine.HoveringEase, (index == i) ? 1f : 0f, 8f * Engine.DeltaTime);
				poemLine.HoldingEase = Calc.Approach(poemLine.HoldingEase, (index == i && dragging) ? 1f : 0f, 8f * Engine.DeltaTime);
			}
			slider = Calc.Approach(slider, index, 16f * Engine.DeltaTime);
			if (swapRoutine != null && swapRoutine.Active)
			{
				swapRoutine.Update();
			}
			wiggler.Update();
			Redraw(Journal.CurrentPageBuffer);
		}
	}
}
