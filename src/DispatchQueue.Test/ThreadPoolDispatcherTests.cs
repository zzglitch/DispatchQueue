//
// ThreadPoolDispatcherTests.cs
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
using NUnit.Framework;
using System;
using System.Threading;
using DispatchQueue;

namespace DispatchQueue.Test
{
	[TestFixture()]
	public class ThreadPoolDispatcherTests
	{
		private volatile int counter = 0;
		private volatile bool failed = false;

		[Test ()]
		public void BasicThreadPoolQueue()
		{
			ThreadPoolDispatcher dispatcher = new ThreadPoolDispatcher();

			// test CreateQueue - succeeds
			IActionQueue queue = dispatcher.CreateQueue();
			Assert.IsNotNull(queue);

			// test queuing an action
			counter = 0;
			queue.Enqueue(() => { IncrementCounter(Thread.CurrentThread.ManagedThreadId); });
			queue.Enqueue(() => { IncrementCounter(Thread.CurrentThread.ManagedThreadId); });

			// spin until counter updates
			while (counter < 2)
				Thread.Sleep(0);

			// check if a thread failed
			Assert.IsFalse(failed);

			// close things out
			dispatcher.Dispose();
		}

		private void IncrementCounter( int mainThreadId )
		{
			counter += 1;
			if (mainThreadId != Thread.CurrentThread.ManagedThreadId)
				failed = true;
		}
	}
}
