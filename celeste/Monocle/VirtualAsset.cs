namespace Monocle
{
	public abstract class VirtualAsset
	{
		public string Name { get; internal set; }

		public int Width { get; internal set; }

		public int Height { get; internal set; }

		internal virtual void Unload()
		{
		}

		internal virtual void Reload()
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
