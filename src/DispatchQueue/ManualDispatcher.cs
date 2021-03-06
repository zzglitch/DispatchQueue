//
// ManualDispatcher.cs
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
using System.Collections.Generic;

namespace DispatchQueue
{
	/// <summary>
	/// A manual dispatcher that requires an explicit call to ProcessQueues
	/// to process queues with pending actions.  An example use case is calling
	/// ManualDispatcher.ProcessQueue on a Unity3D GameObject update function
	/// to force the queued actions to execute in the main thread and within
	/// Unity's standard tick order.
	/// </summary>
	public class ManualDispatcher : Dispatcher
	{
		#region External API

		public ManualDispatcher()
		{
			pendingList = pendingListA;
		}

		public void ProcessQueues()
		{
			// swap lists so we don't lock during processing
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
			for (int i = 0; i < count; i++)
				processList[i].ProcessQueue();
			processList.Clear();
		}

		#endregion External API

		#region internals exposed for Queue class

		internal override void SubmitQueueForProcessing(ActionQueue queue)
		{
			lock (listLock)
			{
				pendingList.Add(queue);
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
					lock (listLock)
					{
						pendingListA.Clear();
						pendingListB.Clear();
					}
				}
				pendingList = null;
				pendingListA = null;
				pendingListB = null;
			}
			base.Dispose(disposing);
		}

		#endregion IDispose implementation
			
		#region internal variables

		// double buffer lists to reduce locking
		private List<ActionQueue> pendingList = null;
		private List<ActionQueue> pendingListA = new List<ActionQueue>();
		private List<ActionQueue> pendingListB = new List<ActionQueue>();
		private readonly object listLock = new object();

		#endregion internal variables
	}
}
