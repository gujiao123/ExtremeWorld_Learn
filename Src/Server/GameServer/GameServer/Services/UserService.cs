using Common;
using GameServer.Entities;
using GameServer.Manager;
using GameServer.Managers;
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


            // 订阅“用户注册请求”消息。
            // 当网络层收到一个 UserRegisterRequest 类型的消息时，
            // 消息分发器会自动调用本类的 OnRegister 方法来处理。
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnUserCreateCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnUserGameEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnUserGameLeave);



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
                //!! 这里的USer里面的ID 是SQL自动设置 不管
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

        /// <summary>
        /// 处理登录请求的回调方法。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
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


                    //把用户的所有角色信息也发过去   从服务器发送到客户端
                    //!! 这里的users 已经从数据库里面查出来了
                    //客户端才能根据信息进行角色选择界面的编辑
                    foreach (var c in user.Player.Characters)
                    {
                        //!!注意 这里的所有角色的Id都是0 但是最终生成角色还是内存中生成由服务器发过去
                        NCharacterInfo info = new NCharacterInfo();
                        info.Id = c.ID;//角色选择阶段还是用数据库ID
                        info.Tid = c.ID;
                        info.Type = CharacterType.Player;
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
            //这是一开始创建的逻辑 如果已经创建了就只能去数据库中修改
            //!!这里的xyz 真的是xyz 不是unity坐标系  传递过去自然会处理 全部除以100
            //!! 注意这里创建角色的ID和plyaerID也是SQL自动创建
            TCharacter character = new TCharacter()
            {
                Name = request.Name,
                Class = (int)request.Class,
                TID = (int)request.Class,
                MapID = 1,
                MapPosX = 5000,
                MapPosY = 3000,
                MapPosZ = 850,

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
                info.Id = 0;
                info.Name = c.Name;
                info.Type = CharacterType.Player;
                info.Class = (CharacterClass)c.Class;
                info.Tid = c.ID;//Tid当作数据库中的ID
                //me把所有角色信息添加到里面
                message.Response.createChar.Characters.Add(info);
            }

            //打包为字节流
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        /// <summary>
        /// 当客户端发送进入游戏请求时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        /// !! request里面是客户端游戏人物列表的序号 卧槽
        private void OnUserGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest request)
        {

            //拿出内存中的角色数据
            //根据角色索引而不是uuid 这个是可以改的 因为你客户端没有排序角色所以索引就类似于uuid
            //TCharacter dbchar = DBService.Instance.Entities.Characters.Where(c => c.ID == request.characterIdx).FirstOrDefault();
            //这个是逐渐继承缩小范围的
            //!!尼玛的就是把User.Player.Characters 就是从数据库中元素获取后形成的列表 这下就可以和客户端对应上了
            TCharacter dbchar = sender.Session.User.Player.Characters.ElementAt(request.characterIdx);
            //打印信息
            Log.InfoFormat("OnUserGameEnter: characterIdx:{0} ", request.characterIdx);

            //放在在线角色列表里面
            //!!实体对象只会存放在进入游戏后的过程中 

            Character character = CharacterManager.Instance.AddCharacter(dbchar);
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.gameEnter = new UserGameEnterResponse();

            message.Response.gameEnter.Result = Result.Success;
            message.Response.gameEnter.Errormsg = "None";

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);

            sender.Session.Character = character;

            //告诉地图管理器 有一个角色进入了
            MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);




            //TODO

        }

        private void OnUserGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest message)
        {
            //拿出内存中的角色数据
            //!! 服务器和多个客户端之间用session隔开
            //!!不要忘了角色 会保存到每个session里面 所以sender才会拥有
            Character character = sender.Session.Character;

            Log.InfoFormat("");
            //从服务器的内存中的在线角色列表移除
            CharacterManager.Instance.RemoveCharacter(character.Id);
            //通知地图管理器 有一个角色离开了
            MapManager.Instance[character.Info.mapId].CharacterLeave(character);

            NetMessage netMessage = new NetMessage();
            netMessage.Response = new NetMessageResponse();
            netMessage.Response.gameLeave = new UserGameLeaveResponse();
            netMessage.Response.gameLeave.Result = Result.Success;
            netMessage.Response.gameLeave.Errormsg = "None";
            byte[] data = PackageHandler.PackMessage(netMessage);
            sender.SendData(data, 0, data.Length);

        }
    }
}
