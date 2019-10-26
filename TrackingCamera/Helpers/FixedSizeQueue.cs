using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TrackingCamera.Helpers
{
	/// <summary>
	/// Implements a concurrentQueue FIFO with max number of elements, older elements are 
	/// dequeued automajically by Enqueue.
	/// </summary>
	/// <typeparam name="T">The Type of object being queued.</typeparam>
	public class FixedSizedQueue<T> : ConcurrentQueue<T>
	{
		private readonly object syncObject = new object();

		/// <summary>
		/// The maximum size of the Queue.
		/// </summary>
		public int MaxSize { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size"></param>
		public FixedSizedQueue(int size)
		{
			MaxSize = size;
		}

		/// <summary>
		/// Adds an object of type T to the Queue.
		/// </summary>
		/// <param name="obj">The object being added to the Queue.</param>
		public new void Enqueue(T obj)
		{
			base.Enqueue(obj);
			lock (syncObject)
			{
				while (base.Count > MaxSize)
				{
					T outObj;
					base.TryDequeue(out outObj);
				}
			}
		}
	}
}
