using Common.Data;
using Managers;
using UnityEngine;
using UnityEngine.UI;


public class UIQuestSystem : MonoBehaviour
{
    public Text title;
    public GameObject itemPrefab;
    public TabView Tabs;//切换标签页
    public ListView listMain;//主线任务列表 就是content
    public ListView listBranch;//支线任务列表
    public UIQuestInfo questInfo;
    private bool showAvailableList = false;//显示可接任务列表还是已接任务列表

    void Start()
    {
        //各个按钮注册事件
        //这个onItemSelected 会被按钮触发
        //绑定了按钮点击事件
        this.listMain.onItemSelected += this.OnQuestSelected;
        this.listBranch.onItemSelected += this.OnQuestSelected;
        this.Tabs.OnTabSelect += OnSelectTab;
        RefreshUI();
        //QuestManager.Instance.OnQuestChanged += RefreshUI;
    }
    //代表了可接任务与已经接到的任务

    //这里的Tabs 完全只需要是一个按键即可
    //?奇怪 那你绑定按键传一个idx就行了啊还用tabButton干什么
    void OnSelectTab(int idx)
    {
        //idx 代表可解任务 还是已经任务 区分
        showAvailableList = idx == 1;//1代表可接任务
        RefreshUI();
    }

    private void OnDestroy()
    {
        ///QuestManager.Instance.OnQuestChanged -= RefreshUI;
    }

    void RefreshUI()
    {
        ClearAllQuestList();
        InitAllQuestItems();
    }

    /// <summary>
    /// 初始化所有任务列表
    /// </summary>
    void InitAllQuestItems()
    {
        foreach (var kv in QuestManager.Instance.allQuests)
        {
            //第一次打开就默认显示已经接到的列表
            if (showAvailableList)
            {//可用的任务就是服务器里面没有记录接过的任务
                if (kv.Value.Info != null)
                    continue;
            }
            else
            {
                if (kv.Value.Info == null)
                    continue;
            }
            //实例化任务项预制体 根据任务的类型 放到不同的列表里

            //这里添加ListViewItem 并添加到对应的Listview里面 逻辑上
            GameObject go = Instantiate(itemPrefab, kv.Value.Define.Type == QuestType.Main ? this.listMain.transform : this.listBranch.transform);
            UIQuestItem ui = go.GetComponent<UIQuestItem>();
            ui.SetQuestInfo(kv.Value);

            if (kv.Value.Define.Type == QuestType.Main)
            {
                this.listMain.AddItem(ui as ListView.ListViewItem);
            }
            else
            {
                this.listBranch.AddItem(ui as ListView.ListViewItem);
            }
        }
    }

    void ClearAllQuestList()
    {
        this.listMain.RemoveAll();
        this.listBranch.RemoveAll();
    }

    public void OnQuestSelected(ListView.ListViewItem item)
    {
        //观察者需要对广播的内容进行防御
        if (item == null) return;
        //UIQuestItem 是子类可以转
        UIQuestItem questItem = item as UIQuestItem;
        //把任务信息显示在右侧面板
        this.questInfo.SetQuestInfo(questItem.quest);

        //好好管理自己下面得ListView 这下取消就好了另一个按钮就应该取消选中
        if (item.owner == this.listMain)
        {
            this.listBranch.ClearSelection();
        }
        else if (item.owner == this.listBranch)
        {
            this.listMain.ClearSelection();
        }
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close(typeof(UIQuestSystem));
    }
}

