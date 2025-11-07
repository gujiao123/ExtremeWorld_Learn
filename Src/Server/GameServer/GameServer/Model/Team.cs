using Common;
using Common.Utils;
using GameServer.Entities;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Models
{
    class Team
    {
        //管理一个队伍的信息修改
        public int Id;
        public Character Leader;//队长
        public List<Character> Members = new List<Character>();//成员
        /// <summary>
        /// 记录队伍信息最后更新时间
        /// </summary>
        public double timestamp;//成员变更时间戳

        public Team(Character leader)
        {
            this.AddMember(leader);
        }

        public void AddMember(Character member)
        {
            //感觉就是为了Team创建的时候设定队长的
            if (this.Members.Count == 0)
            {
                this.Leader = member;
            }
            //把如果是leader自己加到队伍里
            this.Members.Add(member);
            member.Team = this;
            timestamp = TimeUtil.timestamp;
        }
        /// <summary>
        /// 一个成员离开队伍
        /// </summary>
        /// <param name="member"></param>
        public void Leave(Character member)
        {
            Log.InfoFormat("Leave Team: memberID:{0} memberName:{1}", member.Id, member.Info.Name);
            this.Members.Remove(member);
            Log.InfoFormat("现在队伍还剩{0}", this.Members.Count);

            //如果离开的成员是队长 那么就指定新的队长
            if (member == this.Leader)
            {
                if (this.Members.Count > 0)
                    this.Leader = this.Members[0];
                else
                    this.Leader = null;
            }
            //离开人的队伍设为空
            member.Team = null;
            timestamp = TimeUtil.timestamp;
        }

        public void PostProcess(NetMessageResponse message)
        {
            if (message.teamInfo == null)
            {
                message.teamInfo = new TeamInfoResponse();
                message.teamInfo.Result = Result.Success;
                message.teamInfo.Team = new NTeamInfo();
                message.teamInfo.Team.Id = this.Id;
                message.teamInfo.Team.Leader = this.Leader.Id;
                foreach (var member in this.Members)
                {
                    message.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
            }
        }
    }
}
