using GameServer;
using GameServer.Entities;
using GameServer.Services;
using SkillBridge.Message;

namespace Network
{
    class NetSession
    {
        public TUser User { get; set; }
        public Character Character { get; set; }
        public NEntity Entity { get; set; }
        /// <summary>
        /// session断开连接时调用
        /// </summary>
        internal void Disconneted()
        {
            if (Character != null)
            {
                UserService.Instance.CharacterLeave(Character);
            }
        }
    }
}
