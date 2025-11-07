using Common;
using Common.Utils;
using GameServer.Entities;
using GameServer.Manager;
using GameServer.Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Models
{
    class Guild
    {
        //工会ID
        public int Id { get { return this.Data.Id; } }
        //工会名字
        public string Name { get { return this.Data.Name; } }


        public double timestamp;



        public TGuild Data;//这个是数据库中的公会数据 放一份 在内存里面

        public Guild(TGuild guild)
        {
            this.Data = guild;
        }

        /// <summary>
        /// 加入公会申请
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        internal bool JoinApply(NGuildApplyInfo apply)
        {
            //查找上一次的申请记录
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId);
            if (oldApply != null)
            {
                return false;
            }
            //存放在数据库里面申请信息
            var dbApply = DBService.Instance.Entities.GuildApplies.Create();
            dbApply.GuildId = this.Id;
            dbApply.CharacterId = apply.characterId;
            dbApply.Name = apply.Name;
            dbApply.Class = apply.Class;
            dbApply.Level = apply.Level;
            dbApply.ApplyTime = DateTime.Now;

            DBService.Instance.Entities.GuildApplies.Add(dbApply);
            this.Data.Applies.Add(dbApply);
            DBService.Instance.Save();

            this.timestamp = TimeUtil.timestamp;

            return true;
        }
        /// <summary>
        /// 成功加入工会
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        internal bool JoinAppove(NGuildApplyInfo apply)
        {
            //?反正就必须有一次申请记录
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId && v.Result == 0);
            if (oldApply == null)
            {
                return false;
            }

            oldApply.Result = (int)apply.Result;
            //添加到成员引发状态变更
            if (apply.Result == ApplyResult.Accept)
            {
                this.AddMember(apply.characterId, apply.Name, apply.Class, apply.Level, GuildTitle.None);
            }

            DBService.Instance.Save();

            this.timestamp = TimeUtil.timestamp;
            return true;
        }
        /// <summary>
        /// 工会成员变更
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="name"></param>
        /// <param name="class"></param>
        /// <param name="level"></param>
        /// <param name="title"></param>
        /// 兼容了leader和普通成员的加入
        public void AddMember(int characterId, string name, int @class, int level, GuildTitle title)
        {
            DateTime now = DateTime.Now;
            //数据库中一个工会的成员列表
            TGuildMember dbMember = new TGuildMember()
            {
                CharacterId = characterId,
                Name = name,
                Class = @class,
                Level = level,
                Title = (int)title,
                JoinTime = now,
                LastTime = now,
            };
            this.Data.Members.Add(dbMember);
            //一个工会是一个单独岛屿 
            var character = CharacterManager.Instance.GetCharacter(characterId);
            //对了 修改角色的公会ID 指向对应工会
            //!!角色与工会的唯一关系就是角色数据中的GuildID字段
            if (character != null)
            {//对于在线角色直接修改内存 数据 
                character.Data.GuildId = this.Id;
            }
            else
            {
                //角色不在线还是更新数据库
                //数据库另一种写法
                //DBService.Instance.Entities.Database.ExecuteSqlCommand("UPDATE Characters SET GuildId = @p0 WHERE CharacterId = @p1", this.Id, characterId);
                TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == characterId);
                dbChar.GuildId = this.Id;
            }
            //记录变更时间
            timestamp = TimeUtil.timestamp;
        }

        /// <summary>
        /// 移除公会成员
        /// </summary>
        /// <param name="member"></param>
        public void Leave(Character member)
        {
            Log.InfoFormat("离开公会：{0}:{1}", member.Id, member.Info.Name);

            TGuildMember guildMember = this.Data.Members.FirstOrDefault(m => m.CharacterId == member.Id);

            DBService.Instance.Entities.GuildMembers.Remove(guildMember);

            var character = CharacterManager.Instance.GetCharacter(member.Id);
            // 如果角色在线
            if (character != null)
            {
                character.Data.GuildId = 0;
            }
            // 如果角色不在线
            else
            {
                TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == member.Id);
                dbChar.GuildId = 0;
            }
            DBService.Instance.Save();
        }

        public void PostProcess(Character from, NetMessageResponse message)
        {
            if (message.Guild == null)
            {
                message.Guild = new GuildResponse();
                message.Guild.Result = Result.Success;
                message.Guild.guildInfo = this.GuildInfo(from);
            }
            else
            {
                //获得最新的工会信息
                message.Guild.guildInfo = this.GuildInfo(from);
            }
        }
        /// <summary>
        /// from有值代表是对应工会的成员 可以获取更多信息
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        internal NGuildInfo GuildInfo(Character from)
        {
            NGuildInfo info = new NGuildInfo()
            {
                Id = this.Id,
                GuildName = this.Name,
                Notice = this.Data.Notice,
                leaderId = this.Data.LeaderID,
                leaderName = this.Data.LeaderName,
                createTime = (long)TimeUtil.GetTimestamp(this.Data.CreateTime),
                memberCount = this.Data.Members.Count,
            };
            //有from代表是工会成员 可以获取成员列表
            //其他人看不到成员信息
            if (from != null)
            {
                info.Members.AddRange(GetMemberInfos());
                if (from.Id == this.Data.LeaderID)//队长才有审批权限的信息
                    info.Applies.AddRange(GetApplyInfos());
            }

            return info;
        }
        /// <summary>
        /// 获得工会成员信息从数据库到网络信息
        /// </summary>
        /// <returns></returns>
        List<NGuildMemberInfo> GetMemberInfos()
        {
            List<NGuildMemberInfo> members = new List<NGuildMemberInfo>();

            foreach (var member in this.Data.Members)
            {
                var memberInfo = new NGuildMemberInfo()
                {
                    Id = member.Id,
                    characterId = member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime),
                };

                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);
                //在线的成员 获取更新工会的信息
                if (character != null)
                {
                    memberInfo.Info = character.GetBasicInfo();
                    memberInfo.Status = 1;
                    member.Level = character.Data.Level;
                    member.Name = character.Data.Name;
                    member.LastTime = DateTime.Now;
                }
                else
                {
                    memberInfo.Info = this.GetMemberInfo(member);
                    memberInfo.Status = 0;
                }
                members.Add(memberInfo);
            }
            return members;
        }

        NCharacterInfo GetMemberInfo(TGuildMember member)
        {
            return new NCharacterInfo()
            {
                Id = member.CharacterId,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level,
            };
        }
        /// <summary>
        /// 获得工会申请信息从数据库到网络信息
        /// </summary>
        /// <returns></returns>
        List<NGuildApplyInfo> GetApplyInfos()
        {
            List<NGuildApplyInfo> applies = new List<NGuildApplyInfo>();
            foreach (var apply in this.Data.Applies)
            {
                if (apply.Result != (int)ApplyResult.None) continue;
                applies.Add(new NGuildApplyInfo()
                {
                    characterId = apply.CharacterId,
                    GuildId = apply.GuildId,
                    Class = apply.Class,
                    Level = apply.Level,
                    Name = apply.Name,
                    Result = (ApplyResult)apply.Result,
                });
            }
            return applies;
        }


        //管理部分

        TGuildMember GetDBMember(int characterId)
        {
            foreach (var member in this.Data.Members)
            {
                if (member.CharacterId == characterId)
                    return member;
            }
            return null;
        }

        internal void ExecuteAdmin(GuildAdminCommand command, int targetId, int sourceId)
        {
            var target = GetDBMember(targetId);
            var source = GetDBMember(sourceId);
            switch (command)
            {
                case GuildAdminCommand.Promote:
                    target.Title = (int)GuildTitle.VicePresident;
                    break;
                case GuildAdminCommand.Depost:
                    target.Title = (int)GuildTitle.None;
                    break;
                case GuildAdminCommand.Transfer:
                    target.Title = (int)GuildTitle.President;
                    source.Title = (int)GuildTitle.None;
                    this.Data.LeaderID = targetId;
                    this.Data.LeaderName = target.Name;
                    break;
                case GuildAdminCommand.Kickout:
                    //待完成
                    break;
            }
            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
        }
    }
}
