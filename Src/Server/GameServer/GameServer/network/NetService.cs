using Common;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    class NetService
    {
        static TcpSocketListener ServerListener;
        public bool Init(int port)
        {
            //这里创建监听器
            ServerListener = new TcpSocketListener("127.0.0.1", GameServer.Properties.Settings.Default.ServerPort, 10);
            ServerListener.SocketConnected += OnSocketConnected;
            return true;
        }


        public void Start()
        {
            //启动监听
            Log.Warning("Starting Listener...");
            //在init设置好对应的回调函数后 启动监听
            ServerListener.Start();
            //服务器使用多线程处理消息分发
            MessageDistributer<NetConnection<NetSession>>.Instance.Start(8);
            Log.Warning("NetService Started");
        }


        public void Stop()
        {
            Log.Warning("Stop NetService...");

            ServerListener.Stop();

            Log.Warning("Stoping Message Handler...");
            MessageDistributer<NetConnection<NetSession>>.Instance.Stop();
        }
        /// <summary>
        /// 如果服务器链接上了走这里
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSocketConnected(object sender, Socket e)
        {
            //拿到客户端IP
            IPEndPoint clientIP = (IPEndPoint)e.RemoteEndPoint;
            //可以在这里对IP做一级验证,比如黑名单

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            NetSession session = new NetSession();
            //注册两个回调函数
            //新建一个链接 
            NetConnection<NetSession> connection = new NetConnection<NetSession>(e, args,
                new NetConnection<NetSession>.DataReceivedCallback(DataReceived),
                new NetConnection<NetSession>.DisconnectedCallback(Disconnected), session);


            Log.WarningFormat("Client[{0}]] Connected", clientIP);
        }


        /// <summary>
        /// 连接断开回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Disconnected(NetConnection<NetSession> sender, SocketAsyncEventArgs e)
        {
            //Performance.ServerConnect = Interlocked.Decrement(ref Performance.ServerConnect);
            Log.WarningFormat("Client[{0}] Disconnected", e.RemoteEndPoint);
        }


        /// <summary>
        /// 接受数据回调处理粘包等等
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void DataReceived(NetConnection<NetSession> sender, DataEventArgs e)
        {
            Log.WarningFormat("Client[{0}] DataReceived Len:{1}", e.RemoteEndPoint, e.Length);
            //由包处理器处理封包
            //建立链接后处理收到的数据包
            lock (sender.packageHandler)
            {
                sender.packageHandler.ReceiveData(e.Data, 0, e.Data.Length);
            }
            //PacketsPerSec = Interlocked.Increment(ref PacketsPerSec);
            //RecvBytesPerSec = Interlocked.Add(ref RecvBytesPerSec, e.Data.Length);
        }
    }
}
