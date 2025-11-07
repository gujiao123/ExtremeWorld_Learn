using Models;
using SkillBridge.Message;

namespace Managers
{
    class TeamManager : Singleton<TeamManager>
    {
        //这里吧team的信息人给user了 没什么管理的
        public void Init()
        {

        }

        public void UpdateTeamInfo(NTeamInfo team)
        {
            User.Instance.TeamInfo = team;
            ShowTeamUI(team != null);
        }

        public void ShowTeamUI(bool show)
        {
            //可能UIMain还没初始化切换地图
            if (UIMain.Instance != null)
                UIMain.Instance.ShowTeamUI(show);
        }
    }
}
