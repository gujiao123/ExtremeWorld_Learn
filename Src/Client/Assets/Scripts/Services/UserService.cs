using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Network;
using UnityEngine;

using SkillBridge.Message;

namespace Services
{
    class UserService : Singleton<UserService>, IDisposable
    {
        // 定义两个事件，供UI层订阅
        public UnityEngine.Events.UnityAction<Result, string> OnLogin;
        public UnityEngine.Events.UnityAction<Result, string> OnRegister;


        // 用于暂存一个待发送的网络消息。
        // 主要用在当用户点击登录/注册时网络还未连接的情况下，先把消息存起来，等连接成功后再发送。
        NetMessage pendingMessage = null;
        // 标记当前客户端是否已经成功连接到服务器
        bool connected = false;

        public UserService()
        {
            //me这些订阅全部都是关于服务器之间的 与客户端的设计没有关系
            NetClient.Instance.OnConnect += OnGameServerConnect;
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(this.OnUserRegister);

        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(this.OnUserRegister);
            NetClient.Instance.OnConnect -= OnGameServerConnect;
            NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;
        }

        public void Init()
        {

        }

        public void ConnectToServer()
        {
            Debug.Log("ConnectToServer() Start ");
            //NetClient.Instance.CryptKey = this.SessionId;
            NetClient.Instance.Init("127.0.0.1", 8000);
            NetClient.Instance.Connect();
        }

        /// <summary>
        /// 【事件回调】当网络连接结果返回时被调用
        /// </summary>
        /// <param name="result"></param>
        /// <param name="reason"></param>
        void OnGameServerConnect(int result, string reason)
        {
            Log.InfoFormat("LoadingMesager::OnGameServerConnect :{0} reason:{1}", result, reason);
            // 如果连接成功
            if (NetClient.Instance.Connected)
            {
                this.connected = true;
                // 检查是否有待处理的消息（比如用户在连接前就点击了登录）
                if (this.pendingMessage != null)
                {
                    // 如果有，立刻发送这个消息
                    NetClient.Instance.SendMessage(this.pendingMessage);
                    // 发送后清空，防止重复发送
                    this.pendingMessage = null;
                }
            }
            else
            {
                if (!this.DisconnectNotify(result, reason))
                {
                    MessageBox.Show(string.Format("网络错误，无法连接到服务器！\n RESULT:{0} ERROR:{1}", result, reason), "错误", MessageBoxType.Error);
                }
            }
        }
        /// <summary>
        /// 【事件回调】当与服务器的连接断开时被调用
        /// </summary>
        /// <param name="result"></param>
        /// <param name="reason"></param>
        public void OnGameServerDisconnect(int result, string reason)
        {
            this.DisconnectNotify(result, reason);
            return;
        }
        /// <summary>
        /// 一个辅助方法，用于在断开连接时，通知UI层具体的失败信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool DisconnectNotify(int result, string reason)
        {
            // 检查断线时，是否有一个登录或注册请求正在等待发送
            if (this.pendingMessage != null)
            {
                // 如果是登录请求
                if (this.pendingMessage.Request.userLogin != null)
                {
                    // 触发 OnLogin 事件，并告知UI登录失败
                    if (this.OnLogin != null)
                    {
                        this.OnLogin(Result.Failed, string.Format("服务器断开！\n 结果:{0} 错误:{1}", result, reason));
                    }
                }
                // 如果是注册请求
                else if (this.pendingMessage.Request.userRegister != null)
                {
                    // 触发 OnRegister 事件，并告知UI注册失败
                    if (this.OnRegister != null)
                    {
                        this.OnRegister(Result.Failed, string.Format("服务器断开！\n 结果:{0} 错误:{1}", result, reason));
                    }
                }
                return true; // 表示已经处理了通知
            }
            return false; // 表示没有待处理的消息，无法进行特定通知

        }
        /// <summary>
        /// 【公开API】发送登录请求，由UI层的登录按钮调用
        /// </summary>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        public void SendLogin(string user, string psw)
        {
            Debug.LogFormat("UserLoginRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.userLogin = new UserLoginRequest();
            message.Request.userLogin.User = user;
            message.Request.userLogin.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }
        /// <summary>
        /// 【消息回调】当收到服务器的登录响应时被调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        void OnUserLogin(object sender, UserLoginResponse response)
        {
            Debug.LogFormat("OnLogin:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success)
            {
                //登陆成功逻辑
                //me 把服务器返回的用户信息存到本地
                Models.User.Instance.SetupUserInfo(response.Userinfo);
            }
            //如果有订阅OnLogin事件的函数，调用它
            //me 这里就是调用UI层订阅的UILogin.OnLogin函数
            //me 实现UI层也能根据响应更新
            if (this.OnLogin != null)
            {
                this.OnLogin(response.Result, response.Errormsg);

            }
        }

        /// <summary>
        /// 【公开API】发送注册请求，由UI层的注册按钮调用
        /// </summary>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        public void SendRegister(string user, string psw)
        {
            Debug.LogFormat("UserRegisterRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.userRegister = new UserRegisterRequest();
            message.Request.userRegister.User = user;
            message.Request.userRegister.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }
        /// <summary>
        /// 【消息回调】当收到服务器的注册响应时被调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        void OnUserRegister(object sender, UserRegisterResponse response)
        {
            Debug.LogFormat("OnUserRegister:{0} [{1}]", response.Result, response.Errormsg);
            //me 同理调用UI层订阅的注册相关函数
            if (this.OnRegister != null)
            {
                this.OnRegister(response.Result, response.Errormsg);

            }

        }
    }
}
