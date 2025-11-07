using Services;
using UnityEngine;
//用于工会申请列表的UI 刷新
public class UIGuildApplyList : UIWindow
{
    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;

    void Start()
    {
        GuildService.Instance.OnGuildUpdate += UpdateList;
        GuildService.Instance.SendGuildListRequest();
        this.UpdateList();
    }

    private void OnDestroy()
    {
        GuildService.Instance.OnGuildUpdate -= UpdateList;
    }

    void UpdateList()
    {
        ClearList();
        InitItems();
    }

    /// <summary>
    /// 初始化所有列表
    /// </summary>
    void InitItems()
    {
        foreach (var item in GuildManager.Instance.guildInfo.Applies)
        {
            //TODO融合下面的代码避免重复 用泛型
            GameObject go = GameObject.Instantiate(itemPrefab, this.listMain.transform);
            UIGuildApplyItem ui = go.GetComponent<UIGuildApplyItem>();
            ui.SetItemInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    void ClearList()
    {
        this.listMain.RemoveAll();
    }
}

