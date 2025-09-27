using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;
using SkillBridge.Message;
public class UICharacterSelect : MonoBehaviour
{

    public GameObject panelCreate;
    public GameObject panelSelect;

    public GameObject btnCreateCancel;

    public InputField charName;
    CharacterClass charClass;

    public Transform uiCharList;
    public GameObject uiCharInfo;

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

    private int selectCharacterIdx = -1;

    public UICharacterView characterView;

    // Use this for initialization
    void Start()
    {
        InitCharacterSelect(true);
    }



    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 点击创建角色的按钮
    /// </summary>
    public void OnClickCreate()
    {
        if (string.IsNullOrEmpty(this.charName.text))
        {
            MessageBox.Show("请输入角色名称");
            return;
        }
    }


    void OnCharacterCreate(Result result, string message)
    {
        if (result == Result.Success)
        {
            InitCharacterSelect(true);

        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }


    public void InitCharacterSelect(bool init)
    {
        panelCreate.SetActive(false);
        panelSelect.SetActive(true);

        if (init)
        {
            foreach (var old in uiChars)
            {
                Destroy(old);
            }
            uiChars.Clear();
            int count = User.Instance.Info.Player.Characters.Count;



            for (int i = 0; i < count; i++)
            {

                GameObject go = Instantiate(uiCharInfo, this.uiCharList);
                UICharInfo chrInfo = go.GetComponent<UICharInfo>();

                Button button = go.GetComponent<Button>();
                int idx = i;
                button.onClick.AddListener(() =>
                {
                    OnSelectCharacter(idx);
                });

                uiChars.Add(go);
                go.SetActive(true);
            }




        }
    }
    public void InitCharacterCreate()
    {
        panelCreate.SetActive(true);
        panelSelect.SetActive(false);
        OnSelectClass(1);
    }

    ///
    public void OnSelectCharacter(int idx)
    {
        this.selectCharacterIdx = idx;
        var cha = User.Instance.Info.Player.Characters[idx];
        Debug.LogFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);
        //User.Instance.CurrentCharacterInfo = cha;
        characterView.CurrentCharacter = ((int)cha.Class - 1);
    }



    /// <summary>
    /// 选择职业 这个有用 在点击职业按钮的时候调用
    /// 切换一切相关
    /// </summary>
    /// <param name="charClass"></param>
    public void OnSelectClass(int charClass)
    {
        this.charClass = (CharacterClass)charClass;
        //这个是因为 我们在点击职业按钮的时候传递的是从1 开始 ,而服务器的协议是从0开始
        characterView.CurrentCharacter = charClass - 1;

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