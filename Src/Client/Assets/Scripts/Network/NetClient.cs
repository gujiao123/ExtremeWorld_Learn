using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using SkillBridge.Message;

namespace Network
{
    /// <summary>
    /// 继承单例类
    /// </summary>
    class NetClient : MonoSingleton<NetClient>
    {

        // =================================== 常量定义 ===================================
        const int DEF_POLL_INTERVAL_MILLISECONDS = 100; // 默认的网络线程轮询间隔（此处代码并未实际使用）
        /// <summary>
        /// 默认的连接服务器重试次数
        /// </summary>
        const int DEF_TRY_CONNECT_TIMES = 3;
        const int DEF_RECV_BUFFER_SIZE = 64 * 1024;     // 默认的接收缓冲区大小 (64KB)
        const int DEF_PACKAGE_HEADER_LENGTH = 4;        // 默认的数据包头长度（通常用于存放整个包的长度信息）
        const int DEF_SEND_PING_INTERVAL = 30;          // 默认的发送心跳包的时间间隔（秒）
        const int NetConnectTimeout = 10000;            // 连接服务器的超时时间 (10秒)
        const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   // 默认的等待毫秒数，之后显示加载圈（UI相关）
        const int NetReconnectPeriod = 10;              // 默认的重连间隔时间（秒）

        // =================================== 网络错误码定义 ===================================
        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;           //协议错误
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       //发送异常
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      //接受到错误数据包
        public const int NET_ERROR_ZERO_BYTE = 1002;            //收发0字节
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      //收包超时
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        //proxy超时
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      //3次连接不上
        public const int NET_ERROR_PROXY_ERROR = 1006;          //proxy重启
        public const int NET_ERROR_ON_DESTROY = 1007;           //结束的时候，关闭网络连接
        public const int NET_ERROR_ON_KICKOUT = 25;           //被踢了

        // =================================== 事件和委托定义 ===================================

        // 定义一个委托，用于处理连接相关的事件
        public delegate void ConnectEventHandler(int result, string reason);
        // 定义一个委托，用于处理期望数据包的事件
        public delegate void ExpectPackageEventHandler();

        // OnConnect: 当连接结果（成功或失败）返回时触发的事件
        public event ConnectEventHandler OnConnect;
        // OnDisconnect: 当连接断开时触发的事件
        public event ConnectEventHandler OnDisconnect;
        // OnExpectPackageTimeout: 当等待某个特定数据包超时时触发的事件
        public event ExpectPackageEventHandler OnExpectPackageTimeout;
        // OnExpectPackageResume: 当从超时状态恢复时触发的事件
        public event ExpectPackageEventHandler OnExpectPackageResume;


        // =================================== 成员变量定义 ===================================
        /// <summary>
        /// 服务器的IP地址和端口号
        /// </summary>
        private IPEndPoint address;
        /// <summary>
        /// 客户端的 Socket 实例，用于与服务器通信
        /// </summary>
        private Socket clientSocket;
        private MemoryStream sendBuffer = new MemoryStream(); // 发送数据的缓冲区
        private MemoryStream receiveBuffer = new MemoryStream(DEF_RECV_BUFFER_SIZE); // 接收数据的缓冲区，初始大小为64KB
        private Queue<NetMessage> sendQueue = new Queue<NetMessage>(); // 待发送消息的队列
        /// <summary>
        /// 标记当前是否正在尝试连接服务器
        /// </summary>
        private bool connecting = false;

        private int retryTimes = 0; // 当前已经重试的次数
        private int retryTimesTotal = DEF_TRY_CONNECT_TIMES; // 总共允许的重试次数
        private float lastSendTime = 0; // 上次发送数据的时间，用于检测超时
        private int sendOffset = 0; // 发送缓冲区中已经发送的数据的偏移量

        public bool running { get; set; } // running 属性：控制网络客户端是否运行
        public PackageHandler packageHandler = new PackageHandler(null);  // packageHandler: 数据包处理器，负责将字节流解析为消息包

        // =================================== Unity 生命周期方法 ===================================
        void Awake()
        {
            // 在对象创建时立即执行，这里将 running 状态设为 true，表示网络客户端开始运行
            running = true;
        }

        protected override void OnStart()
        {
            // 在 MonoSingleton 的 Start 方法中调用，设置消息分发器在遇到问题时抛出异常
            MessageDistributer.Instance.ThrowException = true;
        }
        // =================================== 事件触发方法 ===================================
        // === 事件触发辅助方法 ===
        // 这些 protected virtual 方法用于安全地触发事件，避免因没有订阅者而产生空指针异常。
        protected virtual void RaiseConnected(int result, string reason)
        {
            ConnectEventHandler handler = OnConnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        public virtual void RaiseDisonnected(int result, string reason = "")
        {
            ConnectEventHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        protected virtual void RaiseExpectPackageTimeout()
        {
            ExpectPackageEventHandler handler = OnExpectPackageTimeout;
            if (handler != null)
            {
                handler();
            }
        }
        protected virtual void RaiseExpectPackageResume()
        {
            ExpectPackageEventHandler handler = OnExpectPackageResume;
            if (handler != null)
            {
                handler();
            }
        }
        // =================================== 事件触发方法 ===================================
        // 这些方法用于安全地触发对应的事件
        public bool Connected
        {
            get
            {
                return (clientSocket != default(Socket)) ? clientSocket.Connected : false;
            }
        }

        public NetClient()
        {
        }
        // Reset: 重置客户端状态，清空所有数据和事件订阅
        public void Reset()
        {
            MessageDistributer.Instance.Clear(); // 清空消息分发器
            this.sendQueue.Clear(); // 清空发送队列

            this.sendOffset = 0; // 重置发送偏移

            this.connecting = false; // 重置连接状态

            this.retryTimes = 0; // 重置重试次数
            this.lastSendTime = 0; // 重置上次发送时间

            // 取消所有事件订阅

            this.OnConnect = null;
            this.OnDisconnect = null;
            this.OnExpectPackageTimeout = null;
            this.OnExpectPackageResume = null;
        }
        /// <summary>
        /// Init: 初始化服务器地址
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="port"></param>

        public void Init(string serverIP, int port)
        {
            //链接地址在这里初始化
            //!! 目前没有初始化IP注意
            this.address = new IPEndPoint(IPAddress.Parse(serverIP), port);
        }

        /// <summary>
        /// 异步连接到服务器
        /// 真正的连接结果通过 OnConnect 事件返回
        /// </summary>
        /// <param name="times">重试次数</param>
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)
        {
            if (this.connecting)// Connect: 公开的连接方法，启动异步连接过程
            {
                return;
            }

            if (this.clientSocket != null)// 如果已存在旧的 socket，先关闭它
            {
                this.clientSocket.Close();
            }
            if (this.address == default(IPEndPoint))// 检查是否已通过 Init 初始化服务器地址
            {
                throw new Exception("Please Init first.");
            }
            Debug.Log("DoConnect");
            // 设置状态为正在连接
            this.connecting = true;
            this.lastSendTime = 0;

            // 调用内部方法执行连接操作
            this.DoConnect();
        }

        public void OnDestroy()
        {
            Debug.Log("OnDestroy NetworkManager.");
            this.CloseConnection(NET_ERROR_ON_DESTROY);
        }

        public void CloseConnection(int errCode)
        {
            Debug.LogWarning("CloseConnection(), errorCode: " + errCode.ToString());
            this.connecting = false;
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }

            //清空缓冲区
            MessageDistributer.Instance.Clear();
            this.sendQueue.Clear();

            this.receiveBuffer.Position = 0;
            this.sendBuffer.Position = sendOffset = 0;

            switch (errCode)
            {
                case NET_ERROR_UNKNOW_PROTOCOL:
                    {
                        //致命错误，停止网络服务
                        this.running = false;
                    }
                    break;
                case NET_ERROR_FAIL_TO_CONNECT:
                case NET_ERROR_PROXY_TIMEOUT:
                case NET_ERROR_PROXY_ERROR:
                    //NetworkManager.Instance.dropCurMessage();
                    //NetworkManager.Instance.Connect();
                    break;
                //离线处理
                case NET_ERROR_ON_KICKOUT:
                case NET_ERROR_ZERO_BYTE:
                case NET_ERROR_ILLEGAL_PACKAGE:
                case NET_ERROR_SEND_EXCEPTION:
                case NET_ERROR_PACKAGE_TIMEOUT:
                default:
                    this.lastSendTime = 0;
                    this.RaiseDisonnected(errCode);
                    break;
            }

        }

        //send a Protobuf message

        //me NetMessage 这个就是根据协议自动生成的传输数据格式
        public void SendMessage(NetMessage message)
        {
            if (!running)
            {
                return;
            }

            if (!this.Connected)
            {
                this.receiveBuffer.Position = 0;
                this.sendBuffer.Position = sendOffset = 0;

                this.Connect();
                Debug.Log("Connect Server before Send Message!");
                return;
            }

            sendQueue.Enqueue(message);

            if (this.lastSendTime == 0)
            {
                this.lastSendTime = Time.time;
            }
        }
        // === 内部核心逻辑方法 ===

        /// <summary>
        /// 执行实际的连接操作
        /// </summary>
        void DoConnect()
        {
            Debug.Log("NetClient.DoConnect on " + this.address.ToString());
            try
            {
                // 如果有旧的 Socket，先关闭
                if (this.clientSocket != null)
                {
                    this.clientSocket.Close();
                }

                // 创建新的 TCP Socket
                //!!这里创建一个socket
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.clientSocket.Blocking = true;// 暂时设置为阻塞模式
                // 开始异步连接，但使用 WaitOne 进行带超时的同步等待
                Debug.Log(string.Format("Connect[{0}] to server {1}", this.retryTimes, this.address) + "\n");
                //me 异步方式和服务器获得链接
                IAsyncResult result = this.clientSocket.BeginConnect(this.address, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout); // 等待最多10秒
                if (success)
                {
                    // 连接成功，结束异步操作
                    this.clientSocket.EndConnect(result);
                }
            }
            catch (SocketException ex)
            {
                // 处理 Socket 异常，例如服务器拒绝连接
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    this.CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                }
                Debug.LogErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("DoConnect Exception:" + e.ToString() + "\n");
            }
            // 检查连接最终是否成功
            if (this.clientSocket.Connected)
            {
                // 成功后，将 Socket 设置为非阻塞模式，这是游戏客户端的关键
                this.clientSocket.Blocking = false;
                // 触发连接成功事件
                this.RaiseConnected(0, "Success");
            }
            else
            {
                this.retryTimes++;
                if (this.retryTimes >= this.retryTimesTotal)
                {
                    this.RaiseConnected(1, "Cannot connect to server");
                }
            }
            this.connecting = false;
        }
        /// <summary>
        /// 保持连接
        /// 用于每帧调用
        /// </summary>
        /// <returns></returns>
        bool KeepConnect()
        {
            if (this.connecting)
            {
                return false;
            }
            if (this.address == null)
                return false;

            if (this.Connected)
            {
                return true;
            }

            if (this.retryTimes < this.retryTimesTotal)
            {
                this.Connect();
            }
            return false;
        }
        /// <summary>
        /// 处理数据接收
        /// </summary>
        /// <returns>操作是否成功</returns>
        bool ProcessRecv()
        {
            bool ret = false;
            try
            {

                if (this.clientSocket.Blocking)
                {
                    Debug.Log("this.clientSocket.Blocking = true\n");
                }
                // 使用 Poll 方法以非阻塞方式检查 Socket 状态
                // 1. 检查是否有错误
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Debug.Log("ProcessRecv Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                // 2. 检查是否有可读数据
                ret = this.clientSocket.Poll(0, SelectMode.SelectRead);
                if (ret)
                {
                    // 从 Socket 接收数据到接收缓冲区
                    int n = this.clientSocket.Receive(this.receiveBuffer.GetBuffer(), 0, this.receiveBuffer.Capacity, SocketFlags.None);
                    if (n <= 0)
                    { // 接收到0字节，通常表示对方已关闭连接
                        this.CloseConnection(NET_ERROR_ZERO_BYTE);
                        return false;
                    }
                    // 将收到的原始字节数据交给 PackageHandler 处理（粘包、分包）
                    this.packageHandler.ReceiveData(this.receiveBuffer.GetBuffer(), 0, n);

                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessReceive exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_ILLEGAL_PACKAGE);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 处理数据发送
        /// </summary>
        /// <returns>操作是否成功</returns>
        bool ProcessSend()
        {
            bool ret = false;
            try
            {

                if (this.clientSocket.Blocking)
                {
                    Debug.Log("this.clientSocket.Blocking = true\n");
                }
                // 1. 检查 Socket 是否有错误
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Debug.Log("ProcessSend Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                // 2. 检查 Socket 是否可写

                ret = this.clientSocket.Poll(0, SelectMode.SelectWrite);
                if (ret)
                {
                    //sendStream exist data
                    // 如果发送缓冲区中有未发送完的数据（上次没发完）

                    if (this.sendBuffer.Position > this.sendOffset)
                    {
                        // 继续发送剩余的数据
                        int bufsize = (int)(this.sendBuffer.Position - this.sendOffset);
                        int n = this.clientSocket.Send(this.sendBuffer.GetBuffer(), this.sendOffset, bufsize, SocketFlags.None);
                        if (n <= 0)
                        {
                            this.CloseConnection(NET_ERROR_ZERO_BYTE);
                            return false;
                        }
                        this.sendOffset += n; // 更新已发送的偏移量
                        if (this.sendOffset >= this.sendBuffer.Position)
                        {
                            // 当前包已完整发送，重置发送缓冲区和偏移
                            this.sendOffset = 0;
                            this.sendBuffer.Position = 0;
                            // 从队列中移除已发送的消息
                            this.sendQueue.Dequeue();
                        }
                    }
                    else
                    {
                        // 发送缓冲区为空，从发送队列中取出一个新消息进行处理
                        if (this.sendQueue.Count > 0)
                        {
                            NetMessage message = this.sendQueue.Peek(); // 只看不取，发送成功再取
                            // 使用 PackageHandler 将消息对象打包成字节数组
                            byte[] package = PackageHandler.PackMessage(message);
                            // 写入发送缓冲区，等待下一次循环发送
                            this.sendBuffer.Write(package, 0, package.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessSend exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理已解析完成的消息
        /// </summary>
        void ProceeMessage()
        {
            // 将 PackageHandler 解析出的完整消息分发给对应的逻辑处理器
            MessageDistributer.Instance.Distribute();
        }

        //Update need called once per frame
        public void Update()
        {
            if (!running)
            {
                return;
            }

            if (this.KeepConnect())
            {
                if (this.ProcessRecv())
                {
                    if (this.Connected)
                    {
                        this.ProcessSend();
                        this.ProceeMessage();
                    }
                }
            }
        }
    }
}
