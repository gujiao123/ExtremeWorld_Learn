using Models;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestItem : ListView.ListViewItem
{

    //学习父类写法 怎么感觉怪怪的
    public Text title;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;
    //一个新的方法来响应选中状态的变化
    public override void onSelected(bool selected)
    {
        background.sprite = selected ? selectedBg : normalBg;
    }

    public Quest quest;

    void Start()
    {

    }

    bool isEquiped = false;
    /// <summary>
    /// 就是根据任务定义来设置任务Item信息
    /// </summary>
    /// <param name="item"></param>
    public void SetQuestInfo(Quest item)
    {
        this.quest = item;
        if (this.title != null)
        {
            this.title.text = this.quest.Define.Name;
        }
    }
}

