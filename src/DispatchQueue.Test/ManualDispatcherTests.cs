//
// ManualDispatcherTests.cs
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
	public class ManualDispatcherTests
	{
		[TestFixture ()]
		public class Test
		{
			private int counter = 0;

			[Test ()]
			public void CreateMultipleQueues()
			{
				// test CreateQueue - succeeds
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue1 = dispatcher.CreateQueue();
				Assert.IsNotNull(queue1);
				Assert.IsNotNull(queue1.Name);
				Assert.IsTrue(queue1.Dispatcher == dispatcher);

				// test CreateQueue - succeeds again
				IActionQueue queue2 = dispatcher.CreateQueue();
				Assert.IsNotNull(queue2);
				Assert.IsNotNull(queue2.Name);
				Assert.IsTrue(queue2.Dispatcher == dispatcher);

				// verify the two queues are different
				Assert.AreNotSame (queue1, queue2);
				Assert.IsTrue(queue1.Name != queue2.Name);

				// test GetQueueByName - succeeds
				IActionQueue a = dispatcher.GetQueueByName(queue1.Name);
				Assert.AreSame(queue1, a);

				// test GetQueueByName - succeeds
				IActionQueue b = dispatcher.GetQueueByName(queue2.Name);
				Assert.AreSame(queue2, b);

				// test GetQueueByName - fails
				IActionQueue c = dispatcher.GetQueueByName("not_found");
				Assert.IsNull(c);
			}

			[Test()]
			public void SimpleLambda()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				int localCounter = 0;
				for (int i = 0; i < 10; i++)
				{
					queue.Enqueue( () => { localCounter += 1; } );
				}
				dispatcher.ProcessQueues();

				Assert.IsTrue(localCounter == 10);
			}

			[Test()]
			public void SimpleFunction()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				counter = 0;
				for (int i = 0; i < 10; i++)
				{
					queue.Enqueue( IncrementCounter );
				}

				// all the actions should be processed
				dispatcher.ProcessQueues();
				Assert.IsTrue(counter == 10);
			}

			[Test()]
			public void NestedEnqueue()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				counter = 0;
				for (int i = 0; i < 10; i++)
				{
					queue.Enqueue( () => {
						counter += 1;
						queue.Enqueue( IncrementCounter );
					});
				}

				// the first 10 actions should be processed
				dispatcher.ProcessQueues();
				Assert.IsTrue(counter == 10);

				// the secondary actions should be processed
				dispatcher.ProcessQueues();
				Assert.IsTrue(counter == 20);
			}

			[Test()]
			public void MultipleThreads()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				Thread thread1 = new Thread (new ThreadStart (() =>
				{
					ThreadWorkerA(queue);
				}));

				Thread thread2 = new Thread (new ThreadStart (() =>
				{
					ThreadWorkerB(queue);
				}));

				// start the threads
				counter = 0;
				thread1.Start();
				thread2.Start();

				// spin waiting for threads to end
				while (thread1.IsAlive || thread2.IsAlive )
				{
					dispatcher.ProcessQueues();
				}
				dispatcher.ProcessQueues();

				Assert.IsTrue(counter == 30);
			}

			[Test()]
			public void DisposeQueueFirst()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();
				string name = queue.Name;

				((IDisposable)queue).Dispose();
				Assert.IsNull( queue.Dispatcher );
				Assert.IsNull( dispatcher.GetQueueByName(name) );

				dispatcher.Dispose();
			}

			[Test()]
			public void DisposeDispatcherFirst()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				dispatcher.Dispose();
				Assert.IsNull( queue.Dispatcher );

				((IDisposable)queue).Dispose();
			}

			[Test()]
			public void DisposeQueueInsideAction()
			{
				ManualDispatcher dispatcher = new ManualDispatcher();
				IActionQueue queue = dispatcher.CreateQueue();

				queue.Enqueue( () => {} );
				queue.Enqueue( () => { ((IDisposable)queue).Dispose(); } );
				queue.Enqueue( () => {} );

				dispatcher.ProcessQueues();
				dispatcher.Dispose();
			}


			#region helper functions for test

			private void IncrementCounter()
			{
				counter += 1;
			}

			/// <summary>
			/// Add 10 to object counter
			/// </summary>
			/// <param name="queue">Queue.</param>
			private void ThreadWorkerA( IActionQueue queue )
			{
				for (int i = 0; i < 10; i++)
				{
					counter += 1;
					Thread.Sleep(0);
				}
			}

			/// <summary>
			/// Add 20 to object counter
			/// </summary>
			/// <param name="queue">Queue.</param>
			private void ThreadWorkerB( IActionQueue queue )
			{
				for (int i = 0; i < 10; i++)
				{
					counter += 2;
					Thread.Sleep(0);
				}
			}

			#endregion helper functions for test
		}
	}
}

