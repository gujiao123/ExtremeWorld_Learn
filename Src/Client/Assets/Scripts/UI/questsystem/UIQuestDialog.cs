using Models;
using UnityEngine;


//用于接任务 //和交任务的对话框 与服务器通信

public class UIQuestDialog : UIWindow
{
    public UIQuestInfo questInfo;
    public Quest quest;

    //用于切换按钮显示
    public GameObject openButtons;
    public GameObject submitButtons;

    /// <summary>
    /// 设置任务更新任务信息
    /// </summary>
    /// <param name="quest"></param>
    public void SetQuest(Quest quest)
    {
        this.quest = quest;
        this.UpdateQuest();

        // 根据任务状态显示不同的按钮
        //服务器没有返回证明是新任务
        if (this.quest.Info == null)
        {
            openButtons.SetActive(true);
            submitButtons.SetActive(false);
        }
        else
        {
            if (this.quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
            {
                openButtons.SetActive(false);
                submitButtons.SetActive(true);
            }
            else
            {
                openButtons.SetActive(false);
                submitButtons.SetActive(false);
            }
        }
    }

    public void UpdateQuest()
    {
        if (this.quest != null)
        {
            if (this.questInfo != null)
            {
                this.questInfo.SetQuestInfo(quest);
            }
        }
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close(typeof(UIQuestDialog));
    }
}