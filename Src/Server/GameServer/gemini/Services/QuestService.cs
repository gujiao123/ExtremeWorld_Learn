using Common;
using GameServer.Entities;
using Network;
using SkillBridge.Message;

namespace GameServer.Services
{
    class QuestService : Singleton<QuestService>
    {
        public QuestService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestAcceptRequest>(this.OnQuestAccept);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestSubmitRequest>(this.OnQuestSubmit);
        }

        public void Init()
        {

        }

        void OnQuestAccept(NetConnection<NetSession> sender, QuestAcceptRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("QuestAcceptRequest: characterId:{0} questId:{1}", character.Id, request.QuestId);

            sender.Session.Response.questAccept = new QuestAcceptResponse();
            //就这里关键把接到的任务添加到角色任务管理器里了
            Result result = character.QuestManager.AcceptQuest(sender, request.QuestId);
            sender.Session.Response.questAccept.Result = result;
            sender.SendResponse();
        }

        void OnQuestSubmit(NetConnection<NetSession> sender, QuestSubmitRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("QuestSubmitRequest: characterId:{0} questId:{1}", character.Id, request.QuestId);

            sender.Session.Response.questSubmit = new QuestSubmitResponse();

            Result result = character.QuestManager.SubmitQuest(sender, request.QuestId);
            sender.Session.Response.questSubmit.Result = result;
            sender.SendResponse();
        }
    }
}
