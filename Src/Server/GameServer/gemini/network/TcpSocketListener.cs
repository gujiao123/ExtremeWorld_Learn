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

using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    /// <summary>
    /// 在给定的地址和端口上监听 Socket 连接。
    /// 实现了 IDisposable 接口，意味着它管理着需要手动释放的资源 (Socket)。
    /// </summary>
    public class TcpSocketListener : IDisposable
    {
        #region Fields
        // 等待处理的连接队列的最大长度。
        private Int32 connectionBacklog;
        // 服务器监听的IP地址和端口号。
        private IPEndPoint endPoint;
        // !!核心的监听 Socket 对象。
        private Socket listenerSocket;
        // 可复用的异步 Socket 操作参数对象。这是高性能异步模型的关键。
        private SocketAsyncEventArgs args;
        #endregion

        #region Properties
        /// <summary>
        /// 连接积压队列的长度。
        /// </summary>
        public Int32 ConnectionBacklog
        {
            get { return connectionBacklog; }
            set
            { // 使用 lock 确保线程安全
                lock (this)
                {
                    // 关键配置，只允许在服务器未运行时修改
                    if (IsRunning)
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        connectionBacklog = value;
                }
            }
        }
        /// <summary>
        /// 监听器 Socket 绑定的 IPEndPoint。
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return endPoint; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        endPoint = value;
                }
            }
        }
        /// <summary>
        /// 指示当前是否正在监听。
        /// </summary>
        public Boolean IsRunning
        {
            // 通过判断 listenerSocket 是否为 null 来确定运行状态，非常简洁。
            get { return listenerSocket != null; }
        }
        #endregion

        #region 

        // 提供多种构造函数重载，方便使用者通过不同参数（字符串IP、IPAddress对象、IPEndPoint对象）来初始化。
        // 这是一种常见且良好的设计模式，称为构造函数链。
        public TcpSocketListener(String address, Int32 port, Int32 connectionBacklog)
            : this(IPAddress.Parse(address), port, connectionBacklog)
        { }
        public TcpSocketListener(IPAddress address, Int32 port, Int32 connectionBacklog)
            : this(new IPEndPoint(address, port), connectionBacklog)
        { }


        /// <summary>
        /// 最终的、最核心的构造函数。
        /// </summary>
        public TcpSocketListener(IPEndPoint endPoint, Int32 connectionBacklog)
        {
            this.endPoint = endPoint;

            args = new SocketAsyncEventArgs();
            args.Completed += OnSocketAccepted;
        }
        #endregion

        #region Public Methods (公开方法)
        /// <summary>
        /// 开始监听 Socket 连接。
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                // 防止重复启动
                if (!IsRunning)
                {
                    // 1. 创建监听 Socket
                    listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    // 2. 将 Socket 绑定到指定的 IP 和端口
                    listenerSocket.Bind(endPoint);
                    // 3. 开始监听，并设置等待队列长度
                    listenerSocket.Listen(connectionBacklog);
                    // 4. 启动第一个异步接受连接的操作。此方法不会阻塞，会立即返回。
                    BeginAccept(args);
                }
                else
                    throw new InvalidOperationException("The Server is already running.");
            }

        }

        /// <summary>
        /// 停止监听 Socket 连接。
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (listenerSocket == null)
                    return; // 如果已经停止，则直接返回

                // 关闭 Socket 会释放端口，并使任何挂起的异步操作（如 AcceptAsync）中止。
                listenerSocket.Close();
                listenerSocket = null; // 将其设为 null，以更新 IsRunning 状态
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 启动一个异步的接受连接操作。
        /// </summary>
        private void BeginAccept(SocketAsyncEventArgs args)
        {
            // 每次接受前，清空 AcceptSocket 属性，以便填充新的客户端 Socket。
            args.AcceptSocket = null;
            // 调用 AcceptAsync 开始异步等待客户端连接。
            // 这个调用会立即返回，操作系统会在后台处理连接请求。
            listenerSocket.AcceptAsync(args);
        }
        /// <summary>
        /// 当一个异步接受操作完成时被调用（事件回调）。
        /// </summary>
        private void OnSocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            //!!当服务器接受一个socket的时候调用
            // 检查异步操作的结果
            if (e.SocketError == SocketError.OperationAborted)
                return; // 如果操作被中止（通常是调用了 Stop()），则正常退出

            if (e.SocketError == SocketError.Success)
            {
                // 操作成功，e.AcceptSocket 属性现在包含了新连接的客户端 Socket
                Socket handler = e.AcceptSocket;
                // 触发外部事件，将新的客户端 Socket 传递给上层业务逻辑处理
                OnSocketConnected(handler);
            }

            // *** 这是实现持续监听的关键 ***
            // 无论本次接受成功与否，立即启动下一次的接受操作，形成一个循环。
            // 这样服务器就能不间断地接受新连接，而不会阻塞任何线程。
            lock (this)
            {
                if (IsRunning) // 确保服务器仍在运行状态
                    BeginAccept(e);
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// 当接收到一个新的连接时触发。
        /// </summary>
        public event EventHandler<Socket> SocketConnected;

        /// <summary>
        /// 安全地触发 SocketConnected 事件。
        /// </summary>
        private void OnSocketConnected(Socket client)
        {
            // 检查是否有订阅者，避免空指针异常
            if (SocketConnected != null)
                SocketConnected(this, client);
        }
        #endregion

        #region IDisposable Members (资源释放)
        // 这是标准的 IDisposable 实现模式，用于确保 Socket 等非托管资源被正确释放。
        private Boolean disposed = false;
        // 终结器（Finalizer），作为最后的保险，防止忘记调用 Dispose() 导致资源泄漏。
        ~TcpSocketListener()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // 通知垃圾回收器(GC)不要再调用此对象的终结器。
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    Stop(); // 停止服务器并关闭 Socket
                    if (args != null)
                    {
                        // 移除事件订阅，并释放 args 对象
                        args.Completed -= OnSocketAccepted;
                        args.Dispose();
                    }
                }
                // 标记为已释放
                disposed = true;
            }
        }
        #endregion
    }
}
