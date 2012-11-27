/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nerdcave.Common
{
    public class PriorityBlockingQueue<T>
    {

        private object addItemLock = new object();
        private object takeItemLock = new object();
        private object countLock = new object();

        private EventWaitHandle itemWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private BlockingCollection<T> lowPriorityItems = new BlockingCollection<T>(new ConcurrentQueue<T>());
        private BlockingCollection<T> normalPriorityItems = new BlockingCollection<T>(new ConcurrentQueue<T>());
        private BlockingCollection<T> highPriorityItems = new BlockingCollection<T>(new ConcurrentQueue<T>());

        /// <summary>
        /// Adds a new item to the queue
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        /// <param name="priority">The item's priority</param>
        public void Add(T item, Priority priority)
        {
            lock (addItemLock)
            {
                switch (priority)
                {
                    case Priority.Low:
                        lock (countLock)
                            lowPriorityItems.Add(item);
                        break;
                    case Priority.Normal:
                        lock (countLock)
                            normalPriorityItems.Add(item);
                        break;
                    case Priority.High:
                        lock (countLock)
                            highPriorityItems.Add(item);
                        break;
                    default:
                        throw new Exception("Unexpected Priority value in switch-statement");
                }
                itemWaitHandle.Set();
            }
        }


        /// <summary>
        /// Takes the next item from the queue. If no item is queued, the Thread blocks until a item is available
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            lock (takeItemLock)
            {
                while (true)                
                {
                    if (highPriorityItems.Any())
                    {
                        lock(countLock)
                            return highPriorityItems.Take();
                    }
                    else if (normalPriorityItems.Any())
                    {
                        lock (countLock)
                            return normalPriorityItems.Take();
                    }
                    else if (lowPriorityItems.Any())
                    {
                        lock (countLock)
                            return lowPriorityItems.Take();
                    }
                    else
                    {
                        itemWaitHandle.WaitOne();
                    }
                }
            }
        }



        public int Count
        {
            get 
            {
                lock (countLock)
                {
                    return lowPriorityItems.Count + normalPriorityItems.Count + highPriorityItems.Count;
                }
            }
        }


    }


    
    public enum Priority
    {
        Low = 0, 
        Normal = 1,
        High = 2
    }
}
