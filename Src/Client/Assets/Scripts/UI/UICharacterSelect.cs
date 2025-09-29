using Models;
using Services;
using SkillBridge.Message;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UICharacterSelect : MonoBehaviour
{

    public GameObject panelCreate;
    public GameObject panelSelect;

    public GameObject btnCreateCancel;

    public InputField charName;
    CharacterClass charClass;

    public Transform uiCharList;
    /// <summary>
    /// 角色信息预制体 一般先创建一个隐藏起来
    /// </summary>
    public GameObject uiCharInfo;
    /// <summary>
    /// 角色信息列表 这个是动态生成的
    /// 用于选择界面角色UI展示
    /// </summary>
    public List<GameObject> uiChars = new List<GameObject>();

    public Image[] titles;//职业的标题图片

    /// <summary>
    /// 职业的描述
    /// 也是从DataManager 读取的
    /// </summary>
    public Text descs;

    /// <summary>
    /// 职业描述 名称
    /// 比如  战士 法师 刺客 这个是从DataManager 读取的
    /// </summary>
    /// me 就是职业按钮object下面的text物体
    public Text[] names;
    /// <summary>
    /// 从0开始角色列表索引
    /// </summary>
    private int selectCharacterIdx = 0;

    public UICharacterView characterView;

    // Use this for initialization
    void Start()
    {
        InitCharacterSelect(true);
        //me 订阅角色创建事件
        UserService.Instance.OnCharacterCreate += OnCharacterCreate;
    }



    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 点击开始游戏的按钮
    /// </summary>
    /// 一个用户可以有多个角色
    public void OnClickCreate()
    {
        if (string.IsNullOrEmpty(charName.text))
        {
            MessageBox.Show("角色名称不能为空", "错误", MessageBoxType.Error);
            return;
        }
        //me当你点击"开启冒险时"服务器就会再你的账号下面创建一个角色
        UserService.Instance.SendCharacterCreate(charName.text, charClass);

    }

    /// <summary>
    /// 订阅服务器创建完角色的函数
    /// </summary>
    /// <param name="result"></param>
    /// <param name="message"></param>
    void OnCharacterCreate(Result result, string message)
    {
        if (result == Result.Success)
        {
            //就是根据内存中新增的角色 又来一次初始化
            InitCharacterSelect(true);
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    /// <summary>
    /// 初始化角色选择界面
    /// </summary>
    /// <param name="init"></param>
    public void InitCharacterSelect(bool init)
    {
        //me 这里的逻辑是创建成功了就返回角色选择界面
        panelCreate.SetActive(false);
        panelSelect.SetActive(true);

        if (init)
        {
            //第一步永远先删除一遍之前的GameObject
            foreach (var old in uiChars)
            {
                Destroy(old);
            }
            uiChars.Clear();
            //me 这里是从User单例中获取角色列表
            int count = User.Instance.Info.Player.Characters.Count;
            Debug.LogFormat("Character Count:{0}", count);


            for (int i = 0; i < count; i++)
            {
                //me 这里是实例化一个角色信息的预制体
                GameObject go = Instantiate(uiCharInfo, this.uiCharList);

                UICharInfo chrInfo = go.GetComponent<UICharInfo>();
                //这里设置角色信息 显示 就和预制体不一样了
                //!! 关于执行顺序 只能再Start中执行初始化函数 ,然后你生成的预制体的Start函数会马上执行 这才是更新的机制
                //me 所以说这个初始化只能在Start中执行
                chrInfo.info = User.Instance.Info.Player.Characters[i];

                Button button = go.GetComponent<Button>();
                int idx = i;
                Debug.Log(idx);
                //这里只是绑定以一个时事件啊
                //!! 一般化讲创建预制体的时候要添加事件监听 就是这个样子的
                button.onClick.AddListener(() =>
                {
                    OnSelectCharacter(idx);
                });
                //me 放入内存中保存
                uiChars.Add(go);
                //把创建好的预制体激活
                go.SetActive(true);
            }
            //有多个就默认显示第一个
            if (count >= 1)
            {
                OnSelectCharacter(this.selectCharacterIdx);
            }

        }
    }


    public void InitCharacterCreate()
    {
        panelCreate.SetActive(true);
        panelSelect.SetActive(false);
        OnSelectClass(1);
    }

    /// <summary>
    /// 选择角色 这个在点击角色列表的按钮时调用
    /// </summary>
    /// <param name="idx"></param>
    public void OnSelectCharacter(int idx)
    {

        this.selectCharacterIdx = idx;
        var cha = User.Instance.Info.Player.Characters[idx];
        Debug.LogFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);
        //User.Instance.CurrentCharacterInfo = cha;
        //更换角色展示
        //注意Class 是从1开始的 而UICharacterView 是从0开始的
        characterView.CurrentCharacter = ((int)cha.Class - 1);

        for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++)
        {
            //获取每个角色UI的UICharInfo脚本 然后设置选中状态 
            UICharInfo ci = this.uiChars[i].GetComponent<UICharInfo>();
            ci.Selected = idx == i;
        }



    }



    /// <summary>
    /// 选择职业 这个有用 在点击职业按钮的时候调用
    /// 切换一切相关
    /// </summary>
    /// <param name="charClass"></param>
    /// 
    //!!缺少内部排序 必须靠外部才能维持顺序,就是说外部要存储顺序
    public void OnSelectClass(int charClass)
    {
        this.charClass = (CharacterClass)charClass;
        //这个是因为 我们在点击职业按钮的时候传递的是从1 开始 ,而服务器的协议是从0开始
        characterView.CurrentCharacter = charClass - 1;
        //目前只有三个职业哈
        for (int i = 0; i < 3; i++)
        {
            //更换职业标题
            titles[i].gameObject.SetActive(i == charClass - 1);
            //更换职业描述 这个是从本地文件读取
            //!!注意DataManager一般在游戏初加载的时候加载 如果只是测试这一个场景的话 会抱错没有加载
            names[i].text = DataManager.Instance.Characters[i + 1].Name;
        }
        //这里Description报错是因为服务器与客户端的Common类定义缺少或者没有同步
        descs.text = DataManager.Instance.Characters[charClass].Description;
    }

    /// <summary>
    /// 点击进入游戏的按钮
    /// </summary>
    public void OnClickPlay()
    {

        if (selectCharacterIdx >= 0)
        {

        }
    }
}