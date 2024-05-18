using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
	public class CoroutineHolder : Component
	{
		private class CoroutineData
		{
			public int ID;

			public Stack<IEnumerator> Data;

			public CoroutineData(int id, IEnumerator functionCall)
			{
				ID = id;
				Data = new Stack<IEnumerator>();
				Data.Push(functionCall);
			}
		}

		private List<CoroutineData> coroutineList;

		private HashSet<CoroutineData> toRemove;

		private int nextID;

		private bool isRunning;

		public CoroutineHolder()
			: base(active: true, visible: false)
		{
			coroutineList = new List<CoroutineData>();
			toRemove = new HashSet<CoroutineData>();
		}

		public override void Update()
		{
			isRunning = true;
			for (int i = 0; i < coroutineList.Count; i++)
			{
				IEnumerator now = coroutineList[i].Data.Peek();
				if (now.MoveNext())
				{
					if (now.Current is IEnumerator)
					{
						coroutineList[i].Data.Push(now.Current as IEnumerator);
					}
					continue;
				}
				coroutineList[i].Data.Pop();
				if (coroutineList[i].Data.Count == 0)
				{
					toRemove.Add(coroutineList[i]);
				}
			}
			isRunning = false;
			if (toRemove.Count <= 0)
			{
				return;
			}
			foreach (CoroutineData r in toRemove)
			{
				coroutineList.Remove(r);
			}
			toRemove.Clear();
		}

		public void EndCoroutine(int id)
		{
			foreach (CoroutineData c in coroutineList)
			{
				if (c.ID == id)
				{
					if (isRunning)
					{
						toRemove.Add(c);
					}
					else
					{
						coroutineList.Remove(c);
					}
					break;
				}
			}
		}

		public int StartCoroutine(IEnumerator functionCall)
		{
			CoroutineData data = new CoroutineData(nextID++, functionCall);
			coroutineList.Add(data);
			return data.ID;
		}

		public static IEnumerator WaitForFrames(int frames)
		{
			for (int i = 0; i < frames; i++)
			{
				yield return 0;
			}
		}
	}
}
