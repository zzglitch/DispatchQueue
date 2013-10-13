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
	internal class ActionQueue : IActionQueue, IDisposable
	{
		public ActionQueue( string name, Dispatcher dispatcher )
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
			if (!submitPending)
			{
				submitPending = true;
				dispatcher.SubmitQueueForProcessing(this);
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

		~ActionQueue()
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
				actions = null;
				dispatcher = null;
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
				for (int i = 0; (actions != null) && (i < count); i++)
				{
					Action action = null;
					lock (queueLock)
					{
						action = actions.Dequeue();
					}
					if (action == null)
						break;
					action();
				}
			}
			finally
			{
				// release lock
				Monitor.Exit(processLock);
				submitPending = false;

				// if action was added during processing, re-submit for processing
				lock (queueLock)
				{
					if ((actions != null) && (actions.Count > 0))
					{
						submitPending = true;
						dispatcher.SubmitQueueForProcessing(this);
					}
				}
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

		// Has queue been submited to dispatcher?
		private volatile bool submitPending = false;

		// Dispose tracking
		private bool disposed = false;

		#endregion internal variables
	}
}
