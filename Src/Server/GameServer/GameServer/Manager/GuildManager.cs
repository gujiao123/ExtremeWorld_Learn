using Common;
using Common.Utils;
using GameServer.Entities;
using GameServer.Models;
using GameServer.Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;

namespace GameServer.Managers
{
    //管理所有人的工会
    class GuildManager : Singleton<GuildManager>
    {
        //工会ID+对应工会
        public Dictionary<int, Guild> Guilds = new Dictionary<int, Guild>();
        //hash表存放工会名字 用于检测重名
        private HashSet<string> GuildNames = new HashSet<string>();

        //加载数据库里面的所有工会
        public void Init()
        {
            this.Guilds.Clear();
            foreach (var guild in DBService.Instance.Entities.Guilds)
            {
                this.AddGuild(new Guild(guild));
            }
        }




        void AddGuild(Guild guild)
        {
            this.Guilds.Add(guild.Id, guild);
            this.GuildNames.Add(guild.Name);
            guild.timestamp = TimeUtil.timestamp;
        }

        public bool CheckNameExisted(string name)
        {
            return this.GuildNames.Contains(name);
        }

        public bool CreateGuild(string name, string notice, Character leader)
        {
            DateTime now = DateTime.Now;
            //数据库中创建一个工会
            TGuild dbGuild = DBService.Instance.Entities.Guilds.Create();
            dbGuild.Name = name;
            dbGuild.LeaderID = leader.Id;
            dbGuild.LeaderName = leader.Data.Name;
            dbGuild.Notice = notice;
            dbGuild.CreateTime = now;
            DBService.Instance.Entities.Guilds.Add(dbGuild);
            //内存中创建对应的一个工会对象
            Guild guild = new Guild(dbGuild);
            //内存中把会长加入工会
            guild.AddMember(leader.Id, leader.Name, leader.Data.Class, leader.Data.Level, GuildTitle.President);
            leader.Guild = guild;
            DBService.Instance.Save();
            leader.Data.GuildId = guild.Id;
            DBService.Instance.Save();
            this.AddGuild(guild);

            return true;
        }
        /// <summary>
        /// 获取工会
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        internal Guild GetGuild(int guildId)
        {
            if (guildId == 0)
                return null;
            Guild guild = null;
            this.Guilds.TryGetValue(guildId, out guild);
            return guild;
        }

        internal List<NGuildInfo> GetGuildsInfo()
        {
            List<NGuildInfo> result = new List<NGuildInfo>();
            foreach (var kv in this.Guilds)
            {
                result.Add(kv.Value.GuildInfo(null));
            }
            return result;
        }
    }
}
