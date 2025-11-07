using GameServer;
using GameServer.Entities;
using GameServer.Services;
using SkillBridge.Message;

namespace Network
{
    class NetSession : INetSession
    {
        public TUser User { get; set; }
        public Character Character { get; set; }
        public IPostResponser PostResponser { get; set; }
        public NEntity Entity { get; set; }
        public void Disconnected()
        {
            this.PostResponser = null;

            if (Character != null)
                UserService.Instance.CharacterLeave(this.Character);
        }

        NetMessage response;

        public NetMessageResponse Response
        {
            get
            {
                if (response == null)
                    response = new NetMessage();

                if (response.Response == null)
                    response.Response = new NetMessageResponse();

                return response.Response;
            }
        }

        public byte[] GetResponse()
        {
            if (response != null)
            {
                //应为每个系统都有可能进行一些后处理打包发送所以需要解耦 引入接口实现
                if (PostResponser != null)
                {
                    this.PostResponser.PostProcess(Response);
                }
                byte[] data = PackageHandler.PackMessage(response);
                response = null;
                return data;
            }
            return null;
        }
    }
}
