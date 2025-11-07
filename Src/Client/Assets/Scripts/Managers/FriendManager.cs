using SkillBridge.Message;
using System.Collections.Generic;
//manager的任务 增删改查 通知替他系统 和加载资源等等大众化的事情
namespace Managers
{
    class FriendManager : Singleton<FriendManager>
    {
        // 所有好友
        public List<NFriendInfo> allFriends;

        public void Init(List<NFriendInfo> friends)
        {
            this.allFriends = friends;
        }
    }
}
