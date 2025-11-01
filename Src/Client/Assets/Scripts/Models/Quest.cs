using Common.Data;
using SkillBridge.Message;

namespace Models
{
    /// <summary>
    /// 这个是把任务定义和任务网络信息结合在一起的一个类
    /// </summary>
    public class Quest
    {
        /// <summary>
        /// 这个Define 加载了都有
        /// </summary>
        public QuestDefine Define;//任务定义 配置表读取
        /// <summary>
        /// 这个网络信息只有你接了任务才有
        /// </summary>
        public NQuestInfo Info;//网络交互的信息

        public Quest()
        {

        }

        public Quest(NQuestInfo info)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Quests[info.QuestId];
        }

        public Quest(QuestDefine define)
        {
            this.Define = define;
            this.Info = null;
        }
    }
}
