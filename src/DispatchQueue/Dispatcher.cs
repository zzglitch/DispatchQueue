//
// Dispatcher.cs
//
// Author:
//       Chad Barry <zzglitch@hotmail.com>
//
// Copyright (c) 2013 Charles Barry
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Threading;
using System.Collections.Generic;

namespace DispatchQueue
{
	public abstract class Dispatcher : IDispatcher, IDisposable
	{
		#region IDispatcher implementation

		public IQueue CreateQueue(string name = null)
		{
			if (name == null)
			{
				name = Interlocked.Increment(ref queueId).ToString ();
			}
			Queue queue = new Queue(name, this);
			lock (mapLock)
			{
				mapQueues.Add(queue.Name, new WeakReference(queue,false) );
			}
			return queue;
		}

		public IQueue GetQueueByName(string name)
		{
			WeakReference queueRef = null;
			lock (mapLock)
			{
				mapQueues.TryGetValue (name, out queueRef);
			}
			if (queueRef != null)
			{
				if (queueRef.IsAlive)
				{
					return (IQueue)queueRef.Target;
				}
				else
				{
					lock (mapLock)
					{
						mapQueues.Remove (name);
					}
				}
			}
			return null;
		}

		#endregion IDispatcher implementation

		#region IDisposable implementation

		~Dispatcher()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					lock (mapLock)
					{
						foreach (WeakReference queueRef in mapQueues.Values)
						{
							if (queueRef.IsAlive)
							{
								Queue q = (Queue) queueRef.Target;
								q.DisconnectDispatcher();
								q.Dispose();
							}
						}
						mapQueues.Clear();
					}
				}
				mapQueues = null;
				disposed = true;
			}
		}

		#endregion

		#region internals exposed for Queue class

		internal abstract void SubmitQueueForProcessing(Queue queue);
	
		internal void DeleteQueue(string name)
		{
			lock (mapLock)
			{
				mapQueues.Remove(name);
			}
		}

		#endregion internals exposed for Queue class

		#region internal variables

		// unique queue id
		private long queueId = 0;

		// track queues associated with this dispatcher
		private Dictionary<string,WeakReference> mapQueues = new Dictionary<string,WeakReference>();
		private readonly object mapLock = new object();

		// Dispose tracking
		protected bool disposed = false;

		#endregion internal variables
	}
}

