using Common;
using GameServer.Entities;
using GameServer.Manager;
using GameServer.Services;
using SkillBridge.Message;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Managers
{

    //!!你好友怎么能包含等级这个信息呢,更不就不好更新了 ,除非在等级提升的时候 通知数据库 客户端 好友列表 放在等级后处理里面


    class FriendManager
    {
        //!!我算是知道了 你数据库中的friend信息很少更新

        Character Owner;
        //数据库的好友列表
        /// <summary>
        /// 上来初始化了数据库里面全部的好友信息
        /// </summary>
        List<NFriendInfo> friends = new List<NFriendInfo>();
        bool friendChanged = false;

        public FriendManager(Character owner)
        {
            this.Owner = owner;
            this.InitFriends();
        }
        /// <summary>
        /// 从数据库构造为网络信息
        /// </summary>
        public void InitFriends()
        {
            this.friends.Clear();
            foreach (var friend in this.Owner.Data.Friends)
            {
                this.friends.Add(GetFriendInfo(friend));
            }
        }
        /// <summary>
        /// 拿到网络好友列表
        /// </summary>
        /// <param name="list"></param>
        public void GetFriendInfos(List<NFriendInfo> list)
        {
            foreach (var f in this.friends)
            {
                list.Add(f);
            }
        }

        /// <summary>
        /// 添加到数据库好友列表
        /// </summary>
        /// <param name="friend">从session中拿到的character</param>
        public void AddFriend(Character friend)
        {
            TCharacterFriend tf = new TCharacterFriend()
            {
                FriendID = friend.Id,
                FriendName = friend.Data.Name,
                Class = friend.Data.Class,
                Level = friend.Data.Level,
            };
            this.Owner.Data.Friends.Add(tf);
            friendChanged = true;
        }

        public bool RemoveFriendByFriendId(int friendId)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.FriendID == friendId);
            if (removeItem != null)
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            friendChanged = true;
            return true;
        }

        public bool RemoveFriendByID(int id)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.Id == id);
            if (removeItem != null)
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            friendChanged = true;
            return true;
        }
        /// <summary>
        /// 再次通过是否在线来更新状态
        /// </summary>
        /// <param name="friend"></param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(TCharacterFriend friend)
        {
            NFriendInfo friendInfo = new NFriendInfo();
            //先看是否在线
            var character = CharacterManager.Instance.GetCharacter(friend.FriendID);
            friendInfo.friendInfo = new NCharacterInfo();
            friendInfo.Id = friend.Id;
            //如果不在线
            if (character == null)
            {
                friendInfo.friendInfo.Id = friend.FriendID;
                friendInfo.friendInfo.Name = friend.FriendName;
                friendInfo.friendInfo.Class = (CharacterClass)friend.Class;
                friendInfo.friendInfo.Level = friend.Level;
                friendInfo.Status = 0;//离线
            }
            else
            {//如果在线
                friendInfo.friendInfo = character.GetBasicInfo();
                friendInfo.friendInfo.Name = character.Info.Name;
                friendInfo.friendInfo.Class = character.Info.Class;
                friendInfo.friendInfo.Level = character.Info.Level;
                //更新好友等级
                //这里还要更新一下数据库数据
                //!!既然都放在后处理里面,当角色有friend相关通知的时候才会更新数据库中friend的信息
                //TODO 感觉是Bug
                if (friend.Level != character.Info.Level)
                    friend.Level = character.Info.Level;

                character.FriendManager.UpdateFriendInfo(this.Owner.Info, 1);
                friendInfo.Status = 1;//在线
            }

            Log.InfoFormat("{0}:{1} GetFriendInfo : {2}:{3}", this.Owner.Id, this.Owner.Info.Name, friendInfo.friendInfo.Id, friendInfo.friendInfo.Name);
            return friendInfo;
        }


        /// <summary>
        /// 通过friendId获取好友信息 
        /// </summary>
        /// <param name="friendId">数据库的ID</param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(int friendId)
        {
            foreach (var f in this.friends)
            {
                if (f.friendInfo.Id == friendId)
                    return f;
            }
            return null;
        }

        public void UpdateFriendInfo(NCharacterInfo friendInfo, int status)
        {
            //更改自己所有friend的状态 自己下线了
            foreach (var f in this.friends)
            {
                if (f.friendInfo.Id == friendInfo.Id)
                {
                    f.Status = status;
                    break;
                }
            }
            this.friendChanged = true;
        }
        /// <summary>
        /// 离开时候显示通知
        /// </summary>
        public void OfflineNotify()
        {
            //自己下线 通知所有在线好友
            foreach (var friendInfo in this.friends)
            {
                var friend = CharacterManager.Instance.GetCharacter(friendInfo.friendInfo.Id);
                if (friend != null)
                    friend.FriendManager.UpdateFriendInfo(this.Owner.Info, 0);
            }
        }

        public void PostProcess(NetMessageResponse message)
        {
            if (this.friendChanged)
            {
                Log.InfoFormat("PostProcess > FriendManager : characterID:{0}:{1}", this.Owner.Id, this.Owner.Info.Name);
                this.InitFriends();
                //把内存中 所有角色包含更新的朋友发给客户端
                if (message.friendList == null)
                {
                    message.friendList = new FriendListResponse();
                    message.friendList.Friends.AddRange(this.friends);
                }
                friendChanged = false;
            }
        }
    }
}
