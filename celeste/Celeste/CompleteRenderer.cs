using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class CompleteRenderer : HiresRenderer, IDisposable
	{
		public abstract class Layer
		{
			public Vector2 Position;

			public Vector2 ScrollFactor;

			public Layer(XmlElement xml)
			{
				Position = xml.Position(Vector2.Zero);
				if (xml.HasAttr("scroll"))
				{
					ScrollFactor.X = (ScrollFactor.Y = xml.AttrFloat("scroll"));
					return;
				}
				ScrollFactor.X = xml.AttrFloat("scrollX", 0f);
				ScrollFactor.Y = xml.AttrFloat("scrollY", 0f);
			}

			public virtual void Update(Scene scene)
			{
			}

			public abstract void Render(Vector2 scroll);

			public Vector2 GetScrollPosition(Vector2 scroll)
			{
				Vector2 at = Position;
				if (ScrollFactor != Vector2.Zero)
				{
					at.X = MathHelper.Lerp(Position.X, Position.X + scroll.X, ScrollFactor.X);
					at.Y = MathHelper.Lerp(Position.Y, Position.Y + scroll.Y, ScrollFactor.Y);
				}
				return at;
			}
		}

		public class UILayer : Layer
		{
			private CompleteRenderer renderer;

			public UILayer(CompleteRenderer renderer, XmlElement xml)
				: base(xml)
			{
				this.renderer = renderer;
			}

			public override void Render(Vector2 scroll)
			{
				if (renderer.RenderUI != null)
				{
					renderer.RenderUI(scroll);
				}
			}
		}

		public class ImageLayer : Layer
		{
			public List<MTexture> Images = new List<MTexture>();

			public float Frame;

			public float FrameRate;

			public float Alpha;

			public Vector2 Offset;

			public Vector2 Speed;

			public float Scale;

			public ImageLayer(Vector2 offset, Atlas atlas, XmlElement xml)
				: base(xml)
			{
				Position += offset;
				string[] array = xml.Attr("img").Split(',');
				foreach (string img in array)
				{
					if (atlas.Has(img))
					{
						Images.Add(atlas[img]);
					}
					else
					{
						Images.Add(null);
					}
				}
				FrameRate = xml.AttrFloat("fps", 6f);
				Alpha = xml.AttrFloat("alpha", 1f);
				Speed = new Vector2(xml.AttrFloat("speedx", 0f), xml.AttrFloat("speedy", 0f));
				Scale = xml.AttrFloat("scale", 1f);
			}

			public override void Update(Scene scene)
			{
				Frame += Engine.DeltaTime * FrameRate;
				Offset += Speed * Engine.DeltaTime;
			}

			public override void Render(Vector2 scroll)
			{
				Vector2 at = GetScrollPosition(scroll).Floor();
				MTexture img = Images[(int)(Frame % (float)Images.Count)];
				if (img == null)
				{
					return;
				}
				bool flip = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
				if (flip)
				{
					at.X = 1920f - at.X - img.DrawOffset.X * Scale - (float)img.Texture.Texture.Width * Scale;
					at.Y += img.DrawOffset.Y * Scale;
				}
				else
				{
					at += img.DrawOffset * Scale;
				}
				Rectangle source = img.ClipRect;
				int num;
				if (Offset.X == 0f)
				{
					num = ((Offset.Y != 0f) ? 1 : 0);
					if (num == 0)
					{
						goto IL_015b;
					}
				}
				else
				{
					num = 1;
				}
				source = new Rectangle((int)((0f - Offset.X) / Scale) + 1, (int)((0f - Offset.Y) / Scale) + 1, img.ClipRect.Width - 2, img.ClipRect.Height - 2);
				HiresRenderer.EndRender();
				HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearWrap);
				goto IL_015b;
				IL_015b:
				Draw.SpriteBatch.Draw(img.Texture.Texture, at, source, Color.White * Alpha, 0f, Vector2.Zero, Scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
				if (num != 0)
				{
					HiresRenderer.EndRender();
					HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
				}
			}
		}

		private const float ScrollRange = 200f;

		private const float ScrollSpeed = 600f;

		private Atlas atlas;

		private XmlElement xml;

		private float fadeAlpha = 1f;

		private Coroutine routine;

		private Vector2 controlScroll;

		private float controlMult;

		public float SlideDuration = 1.5f;

		public List<Layer> Layers = new List<Layer>();

		public Vector2 Scroll;

		public Vector2 StartScroll;

		public Vector2 CenterScroll;

		public Vector2 Offset;

		public float Scale;

		public Action<Vector2> RenderUI;

		public Action RenderPostUI;

		public bool HasUI { get; private set; }

		public CompleteRenderer(XmlElement xml, Atlas atlas, float delay, Action onDoneSlide = null)
		{
			this.atlas = atlas;
			this.xml = xml;
			if (xml != null)
			{
				if (xml["start"] != null)
				{
					StartScroll = xml["start"].Position();
				}
				if (xml["center"] != null)
				{
					CenterScroll = xml["center"].Position();
				}
				if (xml["offset"] != null)
				{
					Offset = xml["offset"].Position();
				}
				foreach (object layer in xml["layers"])
				{
					if (layer is XmlElement)
					{
						XmlElement e = layer as XmlElement;
						if (e.Name == "layer")
						{
							Layers.Add(new ImageLayer(Offset, atlas, e));
						}
						else if (e.Name == "ui")
						{
							HasUI = true;
							Layers.Add(new UILayer(this, e));
						}
					}
				}
			}
			Scroll = StartScroll;
			routine = new Coroutine(SlideRoutine(delay, onDoneSlide));
		}

		public void Dispose()
		{
			if (atlas != null)
			{
				atlas.Dispose();
			}
		}

		private IEnumerator SlideRoutine(float delay, Action onDoneSlide)
		{
			yield return delay;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / SlideDuration)
			{
				yield return null;
				Scroll = Vector2.Lerp(StartScroll, CenterScroll, Ease.SineOut(p));
				fadeAlpha = Calc.LerpClamp(1f, 0f, p * 2f);
			}
			Scroll = CenterScroll;
			fadeAlpha = 0f;
			yield return 0.2f;
			onDoneSlide?.Invoke();
			while (true)
			{
				controlMult = Calc.Approach(controlMult, 1f, 5f * Engine.DeltaTime);
				yield return null;
			}
		}

		public override void Update(Scene scene)
		{
			Vector2 aim = Input.Aim.Value;
			aim += Input.MountainAim.Value;
			if (aim.Length() > 1f)
			{
				aim.Normalize();
			}
			aim *= 200f;
			controlScroll = Calc.Approach(controlScroll, aim, 600f * Engine.DeltaTime);
			foreach (Layer layer in Layers)
			{
				layer.Update(scene);
			}
			routine.Update();
		}

		public override void RenderContent(Scene scene)
		{
			HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
			foreach (Layer layer in Layers)
			{
				layer.Render(-Scroll - controlScroll * controlMult);
			}
			if (RenderPostUI != null)
			{
				RenderPostUI();
			}
			if (fadeAlpha > 0f)
			{
				Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, Color.Black * fadeAlpha);
			}
			HiresRenderer.EndRender();
		}
	}
}
