using UnityEngine;
using UnityEngine.UI;
//me 说是UIMainCity实际上每个地图都会用到 改为持久单例最好

public class UIMain : MonoSingleton<UIMain>
{
    public Text textName;
    public Text textLevel;
    //虽然是单例 但是内部组件数据没有更新
    public UITeam TeamWindow;//这个比较特殊

    protected override void OnStart()
    {
        Init();
    }
    void Init()
    {
        if (Models.User.Instance.CurrentCharacterInfo != null)
        {
            textName.text = Models.User.Instance.CurrentCharacterInfo.Name + " - " + Models.User.Instance.CurrentCharacterInfo.Id;
            textLevel.text = "Lv." + Models.User.Instance.CurrentCharacterInfo.Level.ToString();
        }
    }
    void Update()
    {

    }
    /// <summary>
    /// 退出游戏目前只是一个按钮的事件
    /// </summary>
    public void BackToCharSelect()
    {
        Debug.Log("谁返回啦SelectCharacter");
        Debug.LogWarningFormat("BackToCharSelect 被调用! 调用堆栈:\n{0}", System.Environment.StackTrace);
        SceneManager.Instance.LoadScene("SelectCharacter");
        Services.UserService.Instance.SendGameLeave();
    }

    /// <summary>
    /// 测试按钮
    /// </summary>
    public void OnClickTest()
    {
        Debug.Log("点击了测试按钮");
        UIManager.Instance.Show<UITest>();

    }
    public void OnClickBag()
    {
        UIManager.Instance.Show<UIBag>();
    }
    /// <summary>
    /// 打开背包
    /// </summary>
    public void OnClickEquip()
    {
        UIManager.Instance.Show<UICharEquip>();
    }

    public void OnClickQuest()
    {
        UIManager.Instance.Show<UIQuestSystem>();
    }
    public void OnClickFriends()
    {
        UIManager.Instance.Show<UIFriends>();
    }
    public void ShowTeamUI(bool show)
    {
        TeamWindow.ShowTeam(show);
    }

    public void OnClickGuild()
    {//工会管理
        GuildManager.Instance.ShowGuild();
    }

    public void OnClickRide()
    {
        //   UIManager.Instance.Show<UIRide>();
    }

    //public void OnClickSetting()
    //{
    //    UIManager.Instance.Show<UISetting>();
    //}

    //public void OnClickSkill()
    //{
    //    UIManager.Instance.Show<UISkill>();
    //}
}