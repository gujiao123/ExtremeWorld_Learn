using Common;
using GameServer.Entities;
using GameServer.Models;
using System.Collections.Generic;

namespace GameServer.Managers
{
    //管理Team的修改
    class TeamManager : Singleton<TeamManager>
    {
        //都是管里所有的Teams
        //存储的都是引用 一个指针罢了Team 不占很多内存
        public List<Team> Teams = new List<Team>();//用来遍历
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>();//用于查询

        public void Init()
        {

        }

        public Team GetTeamByCharacter(int characterId)
        {
            Team team = null;
            this.CharacterTeams.TryGetValue(characterId, out team);
            return team;
        }

        public void AddTeamMember(Character leader, Character member)
        {
            //!!重要先对 leader进行判断 为空就创建队伍
            if (leader.Team == null)
                leader.Team = CreateTeam(leader);
            leader.Team.AddMember(member);
        }
        //Team 一旦创建就不销毁 等到为空了就继续使用 防止频繁创建销毁内存
        Team CreateTeam(Character leader)
        {
            Team team = null;
            //有空的team就把leader加进去作为leader
            for (int i = 0; i < Teams.Count; i++)
            {
                team = this.Teams[i];
                if (team.Members.Count == 0)
                {
                    team.AddMember(leader);
                    return team;
                }
            }
            team = new Team(leader);
            this.Teams.Add(team);
            team.Id = this.Teams.Count;//队伍数量当ID 反正只增不见
            return team;
        }
    }
}
