
using Models;
using Services;
using SkillBridge.Message;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Managers
{
    public enum NpcQuestStatus
    {
        None = 0, // 无任务
        Complete, // 拥有已完成可提交的任务
        Available, // 拥有可接受的任务
        Incomplete, // 拥有未完成的任务
    }

    class QuestManager : Singleton<QuestManager>
    {
        /// <summary>
        /// 所有有效任务
        ///就是服务器发过来的任务 ,代表你在客户端接到的任务
        /// </summary>
        public List<NQuestInfo> questInfos;
        /// <summary>
        /// 已经接到的任务  有网络信息
        /// 另外加上 可接的任务  没有网络信息 
        /// </summary>
        public Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();


        //questInfos 除了初始化一下就没有用了
        //me 等一下  questInfos和 allQuests都是为了 npcQuests存在 一旦 npcQuests建立起来了 这两个数据结构就没什么意义了
        //后续的任务操作 都是先更改 allQuests 然后再刷新 npcQuests

        /// <summary>
        /// NPC对应的任务列表
        /// </summary>
        public Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>> npcQuests = new Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>>();

        //管理器负责分发通知
        public UnityAction<Quest> onQuestStatusChanged;

        public void Init(List<NQuestInfo> quests)
        {
            this.questInfos = quests;
            allQuests.Clear();
            this.npcQuests.Clear();
            InitQuests();
        }

        void InitQuests()
        {
            //me 不仅要网络信息 还要对应的任务定义
            // 初始化已有任务
            foreach (var info in this.questInfos)
            {
                Quest quest = new Quest(info);
                this.allQuests[quest.Define.ID] = quest;
            }

            this.CheckAvailableQuests();

            foreach (var kv in this.allQuests)
            {
                //根据任务定义 把任务添加到对应NPC身上
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value);
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value);
            }
        }

        /// <summary>
        /// 初始化可用任务
        /// </summary>
        void CheckAvailableQuests()
        {
            foreach (var kv in DataManager.Instance.Quests)
            {
                if (kv.Value.LimitClass != CharacterClass.None && kv.Value.LimitClass != User.Instance.CurrentCharacter.Class)
                    continue; // 职业不符合

                if (kv.Value.LimitLevel > User.Instance.CurrentCharacter.Level)
                    continue; // 等级不符合

                if (this.allQuests.ContainsKey(kv.Key))
                    continue; // 已有任务 第一次是会跳过 ,后面刷新就会查重

                if (kv.Value.PreQuest > 0)
                {
                    Quest preQuest;
                    if (this.allQuests.TryGetValue(kv.Value.PreQuest, out preQuest)) // 获取前置任务
                    {
                        if (preQuest.Info == null)
                            continue; // 前置任务未接取
                        if (preQuest.Info.Status != QuestStatus.Finished)
                            continue; // 前置任务为完成
                    }
                    else
                    {
                        continue; // 前置任务还没接
                    }
                }

                Quest quest = new Quest(kv.Value);
                this.allQuests[quest.Define.ID] = quest;
            }
        }
        /// <summary>
        /// 向NPC添加任务 存放在npcQuests字典里面 等待npc调用
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="quest"></param>
        void AddNpcQuest(int npcId, Quest quest)
        {
            if (!this.npcQuests.ContainsKey(npcId))
                this.npcQuests[npcId] = new Dictionary<NpcQuestStatus, List<Quest>>();

            List<Quest> availables;
            List<Quest> completes;
            List<Quest> incompletes;

            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Available, out availables))
            {
                availables = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Available] = availables;
            }
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Complete, out availables))
            {
                completes = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Complete] = completes;
            }
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Incomplete, out incompletes))
            {
                incompletes = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Incomplete] = incompletes;
            }

            if (quest.Info == null)
            {
                if (npcId == quest.Define.AcceptNPC && !this.npcQuests[npcId][NpcQuestStatus.Available].Contains(quest))
                {
                    this.npcQuests[npcId][NpcQuestStatus.Available].Add(quest);
                }
            }
            else
            {
                if (quest.Define.SubmitNPC == npcId && quest.Info.Status == QuestStatus.Complated)
                {
                    if (!this.npcQuests[npcId][NpcQuestStatus.Complete].Contains(quest))
                    {
                        this.npcQuests[npcId][NpcQuestStatus.Complete].Add(quest);
                    }
                }
                if (quest.Define.SubmitNPC == npcId && quest.Info.Status == QuestStatus.InProgress)
                {
                    if (!this.npcQuests[npcId][NpcQuestStatus.Incomplete].Contains(quest))
                    {
                        this.npcQuests[npcId][NpcQuestStatus.Incomplete].Add(quest);
                    }
                }
            }
        }

        /// <summary>
        /// 读取NPC任务状态
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public NpcQuestStatus GetQuestStatusByNpc(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();
            if (this.npcQuests.TryGetValue(npcId, out status))//获取NPC任务
            {
                if (status[NpcQuestStatus.Complete].Count > 0)
                    return NpcQuestStatus.Complete;
                if (status[NpcQuestStatus.Available].Count > 0)
                    return NpcQuestStatus.Available;
                if (status[NpcQuestStatus.Incomplete].Count > 0)
                    return NpcQuestStatus.Incomplete;
            }
            return NpcQuestStatus.None;
        }
        /// <summary>
        /// 打开NPC对话框
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public bool OpenNpcQuest(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();
            if (this.npcQuests.TryGetValue(npcId, out status))//获取NPC任务
            {
                if (status[NpcQuestStatus.Complete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Complete].First());
                if (status[NpcQuestStatus.Incomplete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Incomplete].First());
                if (status[NpcQuestStatus.Available].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Available].First());
            }
            return false;
        }
        /// <summary>
        /// 任务面版的所有信息都在npcStatus里面直接传送不就好了
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        bool ShowQuestDialog(Quest quest)
        {
            //如果是新任务或者是完成任务
            if (quest.Info == null || quest.Info.Status == QuestStatus.Complated)
            {
                //打开面板
                UIQuestDialog dlg = UIManager.Instance.Show<UIQuestDialog>();
                //更新信息
                dlg.SetQuest(quest);
                //一个对话框绑定一次 所以不会多重绑定
                dlg.OnClose += OnQuestDialogClose;
                return true;
            }
            if (quest.Info != null || quest.Info.Status == QuestStatus.Complated)
            {
                if (!string.IsNullOrEmpty(quest.Define.DialogIncomplete))
                    MessageBox.Show(quest.Define.DialogIncomplete);
            }
            return true;
        }
        /// <summary>
        /// 对于按钮来进行判断响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        /// 相当于是对UIWindow里面的Close事件的响应 
        void OnQuestDialogClose(UIWindow sender, UIWindow.WindowResult result)
        {
            UIQuestDialog dlg = (UIQuestDialog)sender;
            if (result == UIWindow.WindowResult.Yes)
            {
                //提交和接任务
                if (dlg.quest.Info == null)
                    QuestService.Instance.SendQuestAccept(dlg.quest);
                else if (dlg.quest.Info.Status == QuestStatus.Complated)
                    QuestService.Instance.SendQuestSubmit(dlg.quest);


            }
            else if (result == UIWindow.WindowResult.No)
            {
                //拒绝任务
                MessageBox.Show(dlg.quest.Define.DialogDeny);
            }
        }
        /// <summary>
        /// 更新npcStatus 清理再创建
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        Quest RefreshQuestStatus(NQuestInfo quest)
        {
            this.npcQuests.Clear();
            //同步到任务列表里面
            Quest result;
            if (this.allQuests.ContainsKey(quest.QuestId))
            {
                // 更新任务状态
                this.allQuests[quest.QuestId].Info = quest;
                result = this.allQuests[quest.QuestId];
            }
            else
            {
                // 新任务
                result = new Quest(quest);
                this.allQuests[quest.QuestId] = result;
            }

            CheckAvailableQuests();

            foreach (var kv in this.allQuests)
            {
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value);
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value);
            }

            if (onQuestStatusChanged != null)
                onQuestStatusChanged(result);

            return result;
        }

        public void OnQuestAccepted(NQuestInfo info)
        {
            var quest = this.RefreshQuestStatus(info);
            MessageBox.Show(quest.Define.DialogAccept);
        }

        public void OnQuestSubmited(NQuestInfo info)
        {
            var quest = this.RefreshQuestStatus(info);
            MessageBox.Show(quest.Define.DialogFinish);
        }
    }
}
