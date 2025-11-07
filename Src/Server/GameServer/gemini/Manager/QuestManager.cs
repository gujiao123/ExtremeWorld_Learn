using Common.Data;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Managers
{
    class QuestManager
    {
        Character Owner;
        //注意 这个竟然没有在内存中存放一份任务列表 
        //而是转化为了网络 info  放在NQuestInfo
        //为什么 内存不需要存放任务列表  
        public QuestManager(Character owner)
        {
            Owner = owner;
        }

        public void GetQuestInfos(List<NQuestInfo> list)
        {
            foreach (var quest in this.Owner.Data.Quests)
            {
                list.Add(GetQuestInfo(quest));
            }
        }
        /// <summary>
        /// 把数据库任务转化为网络任务结构
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        public NQuestInfo GetQuestInfo(TCharacterQuest quest)
        {
            //把数据库里面接的任务改为网络传输的任务结构
            return new NQuestInfo()
            {
                QuestId = quest.QuestId,
                QuestGuid = quest.Id,
                Status = (QuestStatus)quest.Status,
                Targets = new int[3]
                {
                    quest.Target1,
                    quest.Target2,
                    quest.Target3
                }
            };
        }
        /// <summary>
        /// 接受任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questId"></param>
        /// <returns></returns>
        public Result AcceptQuest(NetConnection<NetSession> sender, int questId)
        {
            Character character = sender.Session.Character;

            QuestDefine quest;
            if (DataManager.Instance.Quests.TryGetValue(questId, out quest))
            {
                var dpquest = DBService.Instance.Entities.CharacterQuests.Create();
                dpquest.QuestId = quest.ID;//数据库任务ID与定义ID相同 这个特殊一点

                //通过questDefine的复杂定义简化了dbQuest的存储 
                //根据任务定义初始化任务目标
                if (quest.Target1 == QuestTarget.None)
                {   //没有目标的直接完成的任务
                    dpquest.Status = (int)QuestStatus.Complated;
                }
                else
                {   //有目标的任务
                    dpquest.Status = (int)QuestStatus.InProgress;
                }
                sender.Session.Response.questAccept.Quest = this.GetQuestInfo(dpquest);
                //保存到数据库里面
                //哎呀看来内存中真的不妨
                character.Data.Quests.Add(dpquest);
                DBService.Instance.Save();
                return Result.Success;
            }
            else
            {
                sender.Session.Response.questAccept.Errormsg = "任务不存在";
                return Result.Failed;
            }
        }
        /// <summary>
        /// 完成任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questId"></param>
        /// <returns></returns>
        public Result SubmitQuest(NetConnection<NetSession> sender, int questId)
        {
            Character character = sender.Session.Character;

            QuestDefine quest;
            if (DataManager.Instance.Quests.TryGetValue(questId, out quest))
            {
                //看你接任务没有
                var dpquest = character.Data.Quests.Where(q => q.QuestId == questId).FirstOrDefault();
                if (dpquest != null)
                {
                    if (dpquest.Status != (int)QuestStatus.Complated)
                    {
                        //还不是完成状态
                        sender.Session.Response.questSubmit.Errormsg = "任务未完成";
                        return Result.Failed;
                    }
                    //更改数据库里面的任务状态
                    dpquest.Status = (int)QuestStatus.Finished;
                    //打包网络任务结构
                    sender.Session.Response.questSubmit.Quest = this.GetQuestInfo(dpquest);
                    DBService.Instance.Save();

                    //处理任务奖励
                    if (quest.RewardGold > 0)
                    {
                        character.Gold += quest.RewardGold;
                    }
                    if (quest.RewardExp > 0)
                    {
                        //character.AddExp(quest.RewardExp);
                    }
                    if (quest.RewardItem1 > 0)
                    {
                        //道具变化放在状态里面真是个好设计
                        character.ItemManager.AddItem(quest.RewardItem1, quest.RewardItem1Count);
                    }
                    if (quest.RewardItem2 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem2, quest.RewardItem2Count);
                    }
                    if (quest.RewardItem3 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem3, quest.RewardItem3Count);
                    }
                    //保存数据库
                    DBService.Instance.Save();
                    return Result.Success;
                }
                sender.Session.Response.questSubmit.Errormsg = "任务不存在[2]";
                return Result.Failed;
            }
            else
            {
                sender.Session.Response.questSubmit.Errormsg = "任务不存在[1]";
                return Result.Failed;
            }
        }
    }
}
