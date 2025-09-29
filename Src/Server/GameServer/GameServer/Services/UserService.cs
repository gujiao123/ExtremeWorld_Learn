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
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnUserCreateCharacter);


        }

        /// <summary>
        /// 当客户端发送创建角色请求时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnUserCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest request)
        {

            //角色名称和职业
            Log.InfoFormat("UserRegisterRequest: Name:{0}  Class:{1}", request.Name, request.Class);


            //创建一个角色数据 等会儿存到数据库
            TCharacter character = new TCharacter()
            {
                Name = request.Name,
                Class = (int)request.Class,
                TID = (int)request.Class,
                MapID = 1,
                MapPosX = 5000,
                MapPosY = 4000,
                MapPosZ = 520,

            };



            //存一份到内存 就可以持久化存储
            //me 这里是存储到服务器的session 里面 因为 一个用户只在session里面进行通信
            //!! 这里的User 是登录成功后存储的
            sender.Session.User.Player.Characters.Add(character);
            //存档到数据库 然后保存
            DBService.Instance.Entities.Characters.Add(character);
            DBService.Instance.Entities.SaveChanges();

            // 准备好要发给客户端的消息
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.createChar = new UserCreateCharacterResponse();

            message.Response.createChar.Result = Result.Success;
            message.Response.createChar.Errormsg = "None";

            //该死啊 这里完善一下 服务器是把所有的角色信息都发过去
            foreach (var c in sender.Session.User.Player.Characters)
            {
                NCharacterInfo info = new NCharacterInfo();
                info.Id = c.ID;
                info.Name = c.Name;
                info.Type = CharacterType.Player;
                info.Class = (CharacterClass)c.Class;
                //me把所有角色信息添加到里面
                message.Response.createChar.Characters.Add(info);
            }

            //打包为字节流
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
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

                    //!!开始保存信息到服务器内存中 方便使用
                    sender.Session.User = user;

                    message.Response.userLogin.Userinfo = new NUserInfo();
                    message.Response.userLogin.Userinfo.Id = 1;
                    message.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                    message.Response.userLogin.Userinfo.Player.Id = user.Player.ID;


                    //把用户的角色信息也发过去   从服务器发送到客户端
                    //!! 这里的users 已经从数据库里面查出来了
                    //客户端才能根据信息进行角色选择界面的编辑
                    foreach (var c in user.Player.Characters)
                    {


                        NCharacterInfo info = new NCharacterInfo();

                        info.Id = c.ID;
                        info.Name = c.Name;
                        info.Class = (CharacterClass)c.Class;
                        //message.Response.userLogin.Userinfo.Player.Characters.Add(new NCharacterInfo());
                        //SB啊 这里发了一个空包 卧槽呜呜呜呜
                        message.Response.userLogin.Userinfo.Player.Characters.Add(info);
                    }
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
