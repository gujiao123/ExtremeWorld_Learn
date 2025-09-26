using Common;
using Network;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    //这里处理客户端传过来的注册请求
    internal class UserService : Singleton<UserService>
    {
        /// <summary>
        /// 构造函数：在服务实例被创建时调用。
        /// 主要作用是向消息分发器“订阅”或“注册”它所关心的消息类型。
        /// </summary>
        public UserService()
        {
            //TODO 自己在这里写注册消息


            // 订阅“用户注册请求”消息。
            // 当网络层收到一个 UserRegisterRequest 类型的消息时，
            // 消息分发器会自动调用本类的 OnRegister 方法来处理。
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);

        }
        public void Init()
        {
        }

        /// <summary>
        /// 处理用户注册请求的回调方法。
        /// </summary>
        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            // 显示一下用户名
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            // 准备好要发给客户端的消息
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userRegister = new UserRegisterResponse();


            //这里可以对注册的消息进行检查校验处理
            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();

            if (user != null)
            {
                message.Response.userRegister.Result = Result.Failed;
                message.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                TPlayer player = new TPlayer();
                //这里向数据库添加数据
                DBService.Instance.Entities.Users.Add(new TUser()
                {
                    Username = request.User,
                    Password = request.Passward,
                    Player = player
                });
                DBService.Instance.Entities.SaveChanges();
                message.Response.userRegister.Result = Result.Success;
                message.Response.userRegister.Errormsg = "None";
            }

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }


        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {

            // 准备好要发给客户端的消息
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            //!!消息的响应要对 这个种类对应着 客户端那边的响应
            //我们封装的是 UserLoginResponse 这个子消息 到message里面
            message.Response.userLogin = new UserLoginResponse();


            //找了一个用户名一样的

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {


                if (request.Passward == user.Password)
                {

                    message.Response.userLogin.Result = Result.Success;
                    message.Response.userLogin.Errormsg = "None";
                }
                else
                {
                    //密码错误
                    message.Response.userLogin.Result = Result.Failed;
                    message.Response.userLogin.Errormsg = "密码错误";
                }


            }
            else
            {
                //用户没有注册
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "用户不存在";
            }

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }
    }

}
