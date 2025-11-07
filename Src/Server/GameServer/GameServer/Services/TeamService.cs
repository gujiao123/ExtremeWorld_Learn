using Common;
using GameServer.Entities;
using GameServer.Manager;
using GameServer.Managers;
using Network;
using SkillBridge.Message;

namespace GameServer.Services
{
    class TeamService : Singleton<TeamService>
    {
        public TeamService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteRequest>(this.OnTeamInviteRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamLeaveRequest>(this.OnTeamLeave);
        }

        public void Init()
        {
            TeamManager.Instance.Init();
        }

        /// <summary>
        /// 收到组队请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void OnTeamInviteRequest(NetConnection<NetSession> sender, TeamInviteRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnTeamInviteRequest: FromId:{0} FromName:{1} ToID:{2} ToName:{3}", request.FromId, request.FromName, request.ToId, request.ToName);

            NetConnection<NetSession> target = SessionManager.Instance.GetSession(request.ToId);

            if (target == null)
            {
                sender.Session.Response.teamInviteRes = new TeamInviteResponse();
                sender.Session.Response.teamInviteRes.Result = Result.Failed;
                sender.Session.Response.teamInviteRes.Errormsg = "对方不在线";
                sender.SendResponse();
                return;
            }
            if (target.Session.Character.Team != null)
            {
                sender.Session.Response.teamInviteRes = new TeamInviteResponse();
                sender.Session.Response.teamInviteRes.Result = Result.Failed;
                sender.Session.Response.teamInviteRes.Errormsg = "对方已经在队伍中";
                sender.SendResponse();
                return;
            }


            Log.InfoFormat("ForwardTeamInviteRequest: FromId:{0} FromName:{1} ToID:{2} ToName:{3}", request.FromId, request.FromName, request.ToId, request.ToName);
            target.Session.Response.teamInviteReq = request;
            //告诉被邀请者，有人邀请他组队
            target.SendResponse();
        }

        /// <summary>
        /// 收到组队相应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        /// //被邀请者得服务器
        void OnTeamInviteResponse(NetConnection<NetSession> sender, TeamInviteResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnTeamInviteResponse: : character:{0} Result:{1} FromId:{2} ToID:{3}", character.Id, response.Result, response.Request.FromId, response.Request.ToId);
            sender.Session.Response.teamInviteRes = response;
            // 接受了组队邀请
            if (response.Result == Result.Success)
            {
                var requster = SessionManager.Instance.GetSession(response.Request.FromId);
                if (requster == null)
                {
                    sender.Session.Response.teamInviteRes.Result = Result.Failed;
                    sender.Session.Response.teamInviteRes.Errormsg = "请求者已下线";
                }
                else
                {
                    //!!重要开始创建Team
                    TeamManager.Instance.AddTeamMember(requster.Session.Character, character);
                    //告诉请求者，组队邀请结果
                    requster.Session.Response.teamInviteRes = response;
                    requster.SendResponse();
                }
            }
            //TODO 邀请者的被拒绝没有得到回应
            //告诉被邀请者，组队邀请结果
            sender.SendResponse();
        }
        /// <summary>
        /// request请求离开队伍
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void OnTeamLeave(NetConnection<NetSession> sender, TeamLeaveRequest request)
        {
            Character character = sender.Session.Character;

            Log.InfoFormat("OnTeamLeave: : character:{0} TeamID:{1} : {2}", character.Id, request.TeamId, request.characterId);

            Models.Team team = character.Team;

            if (team != null)
                team.Leave(character);
            //其他成员收到通知
            foreach (Character member in team.Members)
            {
                //通知其他成员 有人离开了
                NetConnection<NetSession> memberSession = SessionManager.Instance.GetSession(member.Id);
                memberSession.Session.Response.teamInfo = new TeamInfoResponse();
                memberSession.Session.Response.teamInfo.Result = Result.Success;
                memberSession.Session.Response.teamInfo.Team = new NTeamInfo();
                memberSession.Session.Response.teamInfo.Team.Id = team.Id;
                memberSession.Session.Response.teamInfo.Team.Leader = team.Leader.Id;
                Log.InfoFormat("OnTeamLeave:目前正在通知{0},{1}已经离开", member.Info.Name, CharacterManager.Instance.characters[request.characterId].Name);
                //告诉其他人剩下的队员列表
                foreach (Character cha in team.Members)
                {
                    memberSession.Session.Response.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
                //团队变更通知
                memberSession.SendResponse();
            }

            sender.Session.Response.teamLeave = new TeamLeaveResponse();
            sender.Session.Response.teamLeave.Result = Result.Success;
            sender.Session.Response.teamLeave.characterId = request.characterId;

            //character.Team.Leave(character);

            sender.SendResponse();
        }
    }
}