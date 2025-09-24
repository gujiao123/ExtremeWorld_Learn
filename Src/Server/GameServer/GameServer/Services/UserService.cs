using Common;
using Network;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    internal class UserService : Singleton<UserService>
    {
        /// <summary>
        /// 构造函数：在服务实例被创建时调用。
        /// 主要作用是向消息分发器“订阅”或“注册”它所关心的消息类型。
        /// </summary>
        public UserService()
        {
            // 订阅“用户登录请求”消息。
            // 当网络层收到一个 UserLoginRequest 类型的消息时，
            // 消息分发器会自动调用本类的 OnLogin 方法来处理。
            //!!就是把自己的OnLogin方法注册到消息分发器上 让消息分发器调用invoke
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);



            // 订阅“用户注册请求”消息。
            // 当网络层收到一个 UserRegisterRequest 类型的消息时，
            // 消息分发器会自动调用本类的 OnRegister 方法来处理。
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
        }
        public void Init()
        {
        }
        /// <summary>
        /// 处理用户登录请求的回调方法。
        /// </summary>
        /// <param name="sender">发送此请求的客户端网络连接对象，用于向其回发消息。</param>
        /// <param name="request">客户端发来的登录请求数据包，包含了用户名和密码。</param>
        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {
            // 在服务器控制台打印日志，方便调试。
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            // 1. 准备一个空的网络消息，用于作为给客户端的响应。
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userLogin = new UserLoginResponse();
            // 2. 访问数据库，查询是否存在该用户。
            // 使用 LINQ 的 Where 和 FirstOrDefault 方法在数据库的 Users 表中查找用户名匹配的记录。
            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            // 3. 根据查询结果进行逻辑判断。
            if (user == null)
            {
                // 用户不存在
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "用户不存在";
            }
            else if (user.Password != request.Passward)
            {
                // 密码错误
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "密码错误";
            }
            else
            {
                // 登录成功
                // 关键步骤：将数据库中的用户信息关联到当前的网络会话(Session)中。
                // 这样服务器就知道这个网络连接现在代表的是哪个已登录的用户。
                sender.Session.User = user;

                message.Response.userLogin.Result = Result.Success;
                message.Response.userLogin.Errormsg = "None";
                // 填充用户的详细信息，以便客户端加载玩家数据。
                message.Response.userLogin.Userinfo = new NUserInfo();
                message.Response.userLogin.Userinfo.Id = 1;// 用户ID
                message.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                message.Response.userLogin.Userinfo.Player.Id = user.Player.ID;// 玩家ID
                // 遍历该用户拥有的所有角色，并将其信息添加到响应消息中。
                foreach (var c in user.Player.Characters)
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = c.ID;
                    info.Name = c.Name;
                    info.Class = (CharacterClass)c.Class;
                    message.Response.userLogin.Userinfo.Player.Characters.Add(info);
                }
            }
            // 4. 将构建好的响应消息打包成字节数组。
            byte[] data = PackageHandler.PackMessage(message);
            // 5. 通过发送者的网络连接，将数据发回给客户端。
            sender.SendData(data, 0, data.Length);
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

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                message.Response.userRegister.Result = Result.Failed;
                message.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                TPlayer player = new TPlayer();
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
    }
}
