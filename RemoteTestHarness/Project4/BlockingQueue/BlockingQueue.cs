/////////////////////////////////////////////////////////////////////
// BlockingQueue.cs - It is blocking queue with Threads            //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
// Insturctor: Jim Fawcett                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It is blocking queue which is used to enqueue and dequeue the object
 * with which you defined it.   
 * 
 * Public Interface 
 * ================
 * public BlockingQueue()                       // Constructor
 * public void enQ(T msg) //enqueue the T
 *  public T deQ() //deqeueue the T
 *  public int size() //return the size of the queue
 *
 * Build Process
 * =============
 * - Required Files: BlockingQueue.cs 
 * - Compiler Command: csc BlockingQueue.cs 
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
//
using System;
using System.Collections;
using System.Threading;

namespace SWTools
{
    public class BlockingQueue<T>
    {
        private Queue blockingQ;
        object locker_ = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public BlockingQueue()
        {
            blockingQ = new Queue();
        }

        /// <summary>
        /// enqueue the T
        /// </summary>
        /// <param name="msg"></param>
        public void enQ(T msg)
        {
            lock (locker_)  // uses Monitor
            {
                blockingQ.Enqueue(msg);
                Monitor.Pulse(locker_);
            }
        }

        /// <summary>
        /// deqeueue the T
        /// </summary>
        /// <returns></returns>
        public T deQ()
        {
            T msg = default(T);
            lock (locker_)
            {
                while (this.size() == 0)
                {
                    Monitor.Wait(locker_);
                }
                msg = (T)blockingQ.Dequeue();
                return msg;
            }
        }

        /// <summary>
        /// return the size of the queue
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            int count;
            lock (locker_) { count = blockingQ.Count; }
            return count;
        }
    }
        class Program
        {
#if(TEST_BLOCKINGQUEUE)
        static void Main(string[] args)
            {
                try
                {
                    BlockingQueue<int> q = new BlockingQueue<int>();
                    q.enQ(3);
                    Console.Write("\n Dequeued object:{0}", q.deQ());
                }
                catch (Exception ex)
                {
                    Console.Write("\n Exception caught:{0}", ex.Message);
                }
                Console.ReadLine();
            }
#endif
    }
    
}
