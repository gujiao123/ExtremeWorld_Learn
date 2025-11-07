using Common;
using GameServer.Entities;
using GameServer.Manager;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    class FriendService : Singleton<FriendService>
    {
        public FriendService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendRemoveRequest>(this.OnFriendRemove);
        }


        public void Init()
        {

        }
        //TODO 完善 对方不在线
        /// <summary>
        /// 收到加好友请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void OnFriendAddRequest(NetConnection<NetSession> sender, FriendAddRequest request)
        {
            //朋友A率先发难
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendAddRequest:朋友A向朋友B发出第一次请求 FromId:{0} FromName:{1} ToID:{2} ToName:{3}", request.FromId, request.FromName, request.ToId, request.ToName);
            NetConnection<NetSession> friend = null;


            if (request.ToId == 0) // 未传入ID使用名字查找
            {
                //找在线的角色
                foreach (var cha in CharacterManager.Instance.characters)
                {
                    if (cha.Value.Data.Name == request.ToName)
                    {
                        request.ToId = cha.Key;
                        break;
                    }
                }
                //TODO 找不在线角色方案
            }
            else if (request.ToId > 0)//传入了ID
            {
                //先看自己有没有
                if (character.FriendManager.GetFriendInfo(request.ToId) != null)
                {
                    sender.Session.Response.friendAddRes = new FriendAddResponse();
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "已经是好友";
                    sender.SendResponse();
                    return;
                }
                //拿到对方session
                friend = SessionManager.Instance.GetSession(request.ToId);
            }
            //可能对方不在线 突然掉线
            if (friend == null)
            {
                sender.Session.Response.friendAddRes = new FriendAddResponse();
                sender.Session.Response.friendAddRes.Result = Result.Failed;
                sender.Session.Response.friendAddRes.Errormsg = "好友不存在或不在线";
                sender.SendResponse();
                return;
            }

            Log.InfoFormat("ForeardRequest : : FromId:{0} FromName:{1} ToID:{2} ToName:{3}", request.FromId, request.FromName, request.ToId, request.ToName);
            //向好友发送一个请求 让他选择是否接受
            //消息的转发
            friend.Session.Response.friendAddReq = request;
            friend.SendResponse();
        }
        /// <summary>
        /// 收到加好友响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnFriendAddResponse(NetConnection<NetSession> sender, FriendAddResponse response)
        {
            //朋友B
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendAddResponse : : character:{0} Result:{1} FromId:{2} ToId:{3} ", character.Id, response.Result, response.Request.FromId, response.Request.ToId);
            sender.Session.Response.friendAddRes = response;
            // 朋友B接受了好友请求
            if (response.Result == Result.Success)
            {
                //拿到请求者A
                var requester = SessionManager.Instance.GetSession(response.Request.FromId);
                if (requester == null)
                {
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "请求者不在线";
                }
                else
                {
                    // 互相加好友
                    character.FriendManager.AddFriend(requester.Session.Character);
                    requester.Session.Character.FriendManager.AddFriend(character);
                    DBService.Instance.Save();
                    //马上对请求者A发送响应
                    requester.Session.Response.friendAddRes = response;
                    requester.Session.Response.friendAddRes.Result = Result.Success;
                    requester.Session.Response.friendAddRes.Errormsg = "添加好友成功";
                    requester.SendResponse();
                }
            }
            //TODO 这里对朋友B拒绝没有A就响应 补充

            //这里返回给B结果 目前只有不在线的情况 如果什么都没有就表示成功了
            sender.SendResponse();
        }

        void OnFriendRemove(NetConnection<NetSession> sender, FriendRemoveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendRemove : : character:{0} FriendReletionID:{1} ", character.Id, request.Id);
            sender.Session.Response.friendRemove = new FriendRemoveResponse();
            sender.Session.Response.friendRemove.Id = request.Id;

            // 删除自己的好友
            if (character.FriendManager.RemoveFriendByID(request.Id))
            {
                sender.Session.Response.friendRemove.Result = Result.Success;
                // 移除他人好友中的自己
                var friend = SessionManager.Instance.GetSession(request.friendId);
                // 好友在线
                if (friend != null)
                {
                    friend.Session.Character.FriendManager.RemoveFriendByFriendId(character.Id);
                }
                // 好友不在线
                else
                {
                    this.RemoveFriend(request.friendId, character.Id);
                }
            }
            else
            {
                sender.Session.Response.friendRemove.Result = Result.Failed;
            }
            DBService.Instance.Save();
            sender.SendResponse();
        }
        /// <summary>
        /// 移除好友关系
        /// </summary>
        /// <param name="charId"></param>
        /// <param name="friendId"></param>
        void RemoveFriend(int charId, int friendId)
        {
            var removeItem = DBService.Instance.Entities.CharacterFriends.FirstOrDefault(v => v.CharacterID == charId && v.FriendID == friendId);
            if (removeItem != null)
            {
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            }
        }
    }
}
