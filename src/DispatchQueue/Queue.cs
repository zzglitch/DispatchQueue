//
// Queue.cs
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
	internal class Queue : IQueue, IDisposable
	{
		public Queue( string name, Dispatcher dispatcher )
		{
			this.name = name;
			this.dispatcher = dispatcher;
		}

		#region IQueue implementation

		public void Enqueue(Action action)
		{
			lock (queueLock)
			{
				actions.Enqueue(action);
			}
			if (!submitPending == false)
			{
				submitPending = true;
				dispatcher.SubmitQueueForProcessing (this);
			}
		}

		public string Name
		{
			get { return name; }
		}

		public IDispatcher Dispatcher
		{
			get { return dispatcher; }
		}


		#endregion IQueue implementation

		#region IDisposable implementation

		~Queue()
		{
			Dispose (false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (dispatcher != null)
						dispatcher.DeleteQueue(name);
				}
				dispatcher = null;
				actions = null;
				disposed = true;
			}
		}

		#endregion

		#region internals exposed for Dispatcher class

		internal void ProcessQueue()
		{
			// skip if we've been disposed
			if (disposed)
				return;

			// skip if we're already being processed
			if (!Monitor.TryEnter(processLock))
				return;

			// execute pending actions but not new ones added after we start
			try
			{
				int count = 0;
				lock (queueLock)
				{
					count = actions.Count;
				}
				for (int i = 0; i < count; i++)
				{
					Action action = null;
					lock (queueLock)
					{
						action = actions.Dequeue();
					}

					if (action != null)
						action();
					else
						break;
				}
			}
			finally
			{
				Monitor.Exit(processLock);
				submitPending = false;
			}
		}

		internal void DisconnectDispatcher()
		{
			dispatcher = null;
		}

		#endregion internals exposed for dispatcher

		#region internal variables

		// name of the queue
		private string name;

		// Dispatcher associated with this queue
		private Dispatcher dispatcher;

		// Queue of actions and lock protecting it.
		private Queue<Action> actions = new Queue<Action>();
		private readonly object queueLock = new object();

		// lock to prevent accidental simultanous processing
		private readonly object processLock = new object();

		// Has queue been submited to dispatcher?  Not thread safe
		// but failure just results in an extra submit.
		private volatile bool submitPending = false;

		// Dispose tracking
		private bool disposed = false;

		#endregion internal variables
	}
}
