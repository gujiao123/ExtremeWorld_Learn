using Models;
using UnityEngine;
using UnityEngine.UI;


//一个用于显示任务详细的类 和UI搭配使用
public class UIQuestInfo : MonoBehaviour
{
    public Text title;
    public Text[] targets;//没有用
    public Text description;
    public Text overview;
    public UIIconItem rewardItems;//没有用
    public Text rewardMoney;
    public Text rewardExp;
    //public Button navButton;
    private int npc = 0;
    /// <summary>
    /// 更新任务信息
    /// </summary>
    /// <param name="quest"></param>
    public void SetQuestInfo(Quest quest)
    {
        this.title.text = string.Format("[{0}]{1}", quest.Define.Type, quest.Define.Name);

        if (this.overview != null)
            this.overview.text = quest.Define.Overview;

        if (this.description != null)
        {
            if (quest.Info == null)
            {
                this.description.text = quest.Define.Dialog;
            }
            else
            {
                if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
                {
                    this.description.text = quest.Define.DialogFinish;
                }
            }
        }

        this.rewardMoney.text = quest.Define.RewardGold.ToString();
        this.rewardExp.text = quest.Define.RewardExp.ToString();

        if (quest.Info == null)
            this.npc = quest.Define.AcceptNPC;
        else if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
            this.npc = quest.Define.SubmitNPC;

        //this.navButton.gameObject.SetActive(this.npc > 0);

        foreach (var fitter in this.GetComponentsInChildren<ContentSizeFitter>())
        {
            //强制更新布局
            fitter.SetLayoutVertical();
        }
    }

    public void OnClickAbandon()
    {

    }

    public void OnClickNav()
    {
        //Vector3 pos = NPCManager.Instance.GetNpcPosition(this.npc);
        //User.Instance.CurrentCharacterObject.StartNav(pos);
        //UIManager.Instance.Close<UIQuestSystem>();
    }
}

