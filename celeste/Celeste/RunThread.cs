using System;
using System.Collections.Generic;
using System.Threading;
using Monocle;

namespace Celeste
{
	public static class RunThread
	{
		private static List<Thread> threads = new List<Thread>();

		public static void Start(Action method, string name, bool highPriority = false)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				RunThreadWithLogging(method);
			});
			lock (threads)
			{
				threads.Add(thread);
			}
			thread.Name = name;
			thread.IsBackground = true;
            // Not supported in WASM.
            /*
			if (highPriority)
			{
				thread.Priority = ThreadPriority.Highest;
			}
            */
			thread.Start();
		}

		private static void RunThreadWithLogging(Action method)
		{
			try
			{
				method();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				ErrorLog.Write(ex);
				ErrorLog.Open();
				Engine.Instance.Exit();
			}
			finally
			{
				lock (threads)
				{
					threads.Remove(Thread.CurrentThread);
				}
			}
		}

		public static void WaitAll()
		{
			while (true)
			{
				Thread t;
				lock (threads)
				{
					if (threads.Count == 0)
					{
						break;
					}
					t = threads[0];
				}
				while (t.IsAlive)
				{
					Engine.Instance.GraphicsDevice.Present();
				}
			}
		}
	}
}
