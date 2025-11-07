// RayMix Libs - RayMix's .Net Libs
// Copyright 2018 Ray@raymix.net.  All rights reserved.
// https://www.raymix.net
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of RayMix.net. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Network
{
    /// <summary>
    /// MessageDistributer
    /// 消息分发器
    /// </summary>
    public class MessageDistributer : MessageDistributer<object>
    {

    }

    /// <summary>
    /// 消息分发器
    /// MessageDistributer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageDistributer<T> : Singleton<MessageDistributer<T>>
    {
        class MessageArgs
        {
            public T sender;
            public SkillBridge.Message.NetMessage message;
        }
        private Queue<MessageArgs> messageQueue = new Queue<MessageArgs>();


        public delegate void MessageHandler<Tm>(T sender, Tm message);
        private Dictionary<string, System.Delegate> messageHandlers = new Dictionary<string, System.Delegate>();

        private bool Running = false;
        private AutoResetEvent threadEvent = new AutoResetEvent(true);

        public int ThreadCount = 0;
        public int ActiveThreadCount = 0;

        public bool ThrowException = false;

        public MessageDistributer()
        {
        }

        public void Subscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] + messageHandler;
        }
        public void Unsubscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] - messageHandler;
        }

        public void RaiseEvent<Tm>(T sender, Tm msg)
        {
            string key = msg.GetType().Name;
            if (messageHandlers.ContainsKey(key))
            {
                MessageHandler<Tm> handler = (MessageHandler<Tm>)messageHandlers[key];
                if (handler != null)
                {
                    try
                    {
                        handler(sender, msg);
                    }
                    catch (System.Exception ex)
                    {
                        Log.ErrorFormat("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
                        if (ThrowException)
                            throw ex;
                    }
                }
                else
                {
                    Log.Warning("No handler subscribed for {0}" + msg.ToString());
                }
            }
        }

        public void ReceiveMessage(T sender, SkillBridge.Message.NetMessage message)
        {
            this.messageQueue.Enqueue(new MessageArgs() { sender = sender, message = message });
            threadEvent.Set();
        }

        public void Clear()
        {
            this.messageQueue.Clear();
        }

        /// <summary>
        /// 一次性分发队列中的所有消息
        /// </summary>
        public void Distribute()
        {
            if (this.messageQueue.Count == 0)
            {
                return;
            }

            while (this.messageQueue.Count > 0)
            {
                MessageArgs package = this.messageQueue.Dequeue();
                if (package.message.Request != null)
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);
                if (package.message.Response != null)
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);
            }
        }


        /// <summary>
        /// 启动消息处理器
        /// [多线程模式]
        /// </summary>
        /// <param name="ThreadNum">工作线程数</param>
        //!! 还是比较特殊的
        public void Start(int ThreadNum)
        {
            // 1. 设置并规范化线程数量
            // 将传入的线程数赋值给成员变量 this.ThreadCount
            this.ThreadCount = ThreadNum;
            // 安全检查：确保线程数至少为1
            if (this.ThreadCount < 1) this.ThreadCount = 1;
            // 安全检查：限制最大线程数，防止资源过度消耗
            if (this.ThreadCount > 1000) this.ThreadCount = 1000;
            // 2. 启动服务
            // 设置运行状态标志为 true，表示服务已启动
            Running = true;
            // 3. 创建并启动工作线程
            // 循环指定次数，创建相应数量的工作线程
            for (int i = 0; i < this.ThreadCount; i++)
            {
                // 使用 .NET 的线程池来执行任务。
                // ThreadPool.QueueUserWorkItem 是一个高效的方式，它会从池中取一个可用的线程来执行指定的任务。
                // 这样做比手动创建 new Thread() 更节省系统资源。
                // new WaitCallback(MessageDistribute) 创建一个委托，指向 MessageDistribute 方法。
                // 这意味着每个工作线程启动后都会去执行 MessageDistribute 这个方法。
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageDistribute));
            }
            // 4. 等待所有线程启动完成
            // ActiveThreadCount 应该是一个在 MessageDistribute 方法内部会自增的原子计数器。
            // 这个 while 循环会阻塞 Start 方法的执行，直到所有请求的线程都已经成功启动并开始运行。
            while (ActiveThreadCount < this.ThreadCount)
            {
                // 短暂休眠，避免 CPU 空转，让出执行权给其他线程。
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 停止消息处理器
        /// [多线程模式]
        /// </summary>
        public void Stop()
        {
            Running = false;
            this.messageQueue.Clear();
            while (ActiveThreadCount > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        /// <summary>
        /// 消息处理线程
        /// [多线程模式]
        /// </summary>
        /// <param name="stateInfo"></param>
        private void MessageDistribute(Object stateInfo)
        {
            Log.Warning("MessageDistribute thread start");
            try
            {
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);
                while (Running)
                {
                    if (this.messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne();
                        //Log.WarningFormat("[{0}]MessageDistribute Thread[{1}] Continue:", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                        continue;
                    }
                    MessageArgs package = this.messageQueue.Dequeue();
                    if (package.message.Request != null)
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);
                    if (package.message.Response != null)
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);
                }
            }
            catch
            {
            }
            finally
            {
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
                Log.Warning("MessageDistribute thread end");
            }
        }
    }
}