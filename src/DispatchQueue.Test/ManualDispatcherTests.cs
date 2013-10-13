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
using DispatchQueue;

namespace DispatchQueue.Test
{
	public class ManualDispatcherTests
	{
		[TestFixture ()]
		public class Test
		{
			[Test ()]
			public void CreateMultipleQueue()
			{
				// test CreateQueue - succeeds
				ManualDispatcher dispatcher = new ManualDispatcher();
				IQueue queue1 = dispatcher.CreateQueue();
				Assert.IsNotNull(queue1);
				Assert.IsNotNull(queue1.Name);
				Assert.IsTrue(queue1.Dispatcher == dispatcher);

				// test CreateQueue - succeeds again
				IQueue queue2 = dispatcher.CreateQueue();
				Assert.IsNotNull(queue2);
				Assert.IsNotNull(queue2.Name);
				Assert.IsTrue(queue2.Dispatcher == dispatcher);

				// verify the two queues are different
				Assert.AreNotSame (queue1, queue2);
				Assert.IsTrue(queue1.Name != queue2.Name);

				// test GetQueueByName - succeeds
				IQueue a = dispatcher.GetQueueByName(queue1.Name);
				Assert.AreSame(queue1, a);

				// test GetQueueByName - succeeds
				IQueue b = dispatcher.GetQueueByName(queue2.Name);
				Assert.AreSame(queue2, b);

				// test GetQueueByName - fails
				IQueue c = dispatcher.GetQueueByName("not_found");
				Assert.IsNull(c);
			}
		}
	}
}

