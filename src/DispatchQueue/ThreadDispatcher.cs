//
// ThreadDispatcher.cs
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
	/// <summary>
	/// A dispatcher that uses a single worker thread for processing queued actions.
	/// </summary>
	public class ThreadDispatcher : Dispatcher
	{
		#region External API

		public ThreadDispatcher()
		{
			pendingList = pendingListA;
			workerThread = new Thread(new ThreadStart(ProcessQueues));
			workerThread.IsBackground = true;
			workerThread.Start();
		}

		#endregion External API

		#region internals exposed for Queue class

		internal override void SubmitQueueForProcessing(ActionQueue queue)
		{
			lock (listLock)
			{
				pendingList.Add(queue);
				listWait.Set();
			}
		}

		#endregion internals exposed for Queue class

		#region IDispose implementation

		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// signal thread to stop and clean up
					stop = true;
					listWait.Set();

					// wait for thread to exit
					workerThread.Join();
				}
			}
			base.Dispose(disposing);
		}

		#endregion IDispose implementation

		#region internal functions

		private void ProcessQueues()
		{
			for (; !stop; )
			{
				// wait for work
				listWait.WaitOne();

				// are we shutting down?
				if (stop)
					break;

				// swap lists
				List<ActionQueue> processList = null;
				lock (listLock)
				{
					processList = pendingList;
					if (pendingList == pendingListA)
						pendingList = pendingListB;
					else
						pendingList = pendingListA;
				}

				// process the queues
				int count = processList.Count;
				for (int i = 0; (!stop) && (i < count); i++)
					processList[i].ProcessQueue();
				processList.Clear();
			}

			// clean up internals
			lock (listLock)
			{
				pendingListA.Clear();
				pendingListB.Clear();
				pendingList = null;
				pendingListA = null;
				pendingListB = null;
			}
		}

		#endregion internal functions

		#region internal variables

		// double buffer lists to reduce locking
		private List<ActionQueue> pendingList = null;
		private List<ActionQueue> pendingListA = new List<ActionQueue>();
		private List<ActionQueue> pendingListB = new List<ActionQueue>();
		private readonly object listLock = new object();
		private AutoResetEvent listWait = new AutoResetEvent(false);

		// worker thread
		private Thread workerThread;
		private volatile bool stop = false;

		#endregion internal variables
	}
}

