using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TabView : MonoBehaviour
{

    public TabButton[] tabButtons;//管理按钮
    public GameObject[] tabPages;

    /// <summary>
    /// 这个也把button的效果那里出来,button连自己的回调函数都是TabView进行管理的
    /// </summary>
    public UnityAction<int> OnTabSelect;

    public int index = -1;

    IEnumerator Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].tabView = this;
            tabButtons[i].tabIndex = i;
        }
        yield return new WaitForEndOfFrame();
        SelectTab(0);
    }

    public void SelectTab(int index)
    {
        if (this.index != index)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].Select(i == index);
                //页面切换逻辑没有就算了 就类似附加的OnTabSelect 看作
                if (i < tabPages.Length)
                    tabPages[i].SetActive(i == index);
            }
            //对于切换任务其需要这个事件来更新显示 上面的tabPage没有用
            if (OnTabSelect != null)
                OnTabSelect(index);
        }
    }
}
//🧩 1.中介者模式(Mediator Pattern)定义： “中介者”对象（TabView）封装了“同事”对象（TabButton）之间的所有交互。同事们不再互相通信，只和中介者通信。


//🧩 2.关注点分离(Separation of Concerns, SoC)
//TabView(总控) 的关注点是：状态管理（index 是多少）和逻辑调度（谁该显示/隐藏）。

//TabButton (按钮) 的关注点是：用户输入（OnClick）和视觉表现（Select 方法里的 overrideSprite）。


//🧩 3. 依赖注入 (Dependency Injection, DI)
//这体现在 TabView 的 Start 方法中，是“中介者模式”得以实现的基础。

//定义： 一个对象（TabButton）不应该“主动”去查找它所依赖的对象（TabView），而应该由“外部”（TabView）在创建时（Start）将其依赖“注入”给它。

//在这个代码中：

//TabButton 有一个 public TabView tabView; 字段，这是它的“依赖”。

//TabButton 没有在自己的 Start 里写 tabView = GetComponentInParent<TabView>();。

//相反，TabView 在自己的 Start 循环中，主动将自己“注入”给了每一个它管理的 TabButton：tabButtons[i].tabView = this;。

//好处： 解耦 和 清晰的层级。TabButton 变得更像一个“数据结构”，它的依赖关系非常明确（在 Inspector 面板中被 TabView 统一管理），而不是在代码运行时“满世界去找”它的上级。🧩 3. 依赖注入 (Dependency Injection, DI)
//这体现在 TabView 的 Start 方法中，是“中介者模式”得以实现的基础。

//定义： 一个对象（TabButton）不应该“主动”去查找它所依赖的对象（TabView），而应该由“外部”（TabView）在创建时（Start）将其依赖“注入”给它。

//在这个代码中：

//TabButton 有一个 public TabView tabView; 字段，这是它的“依赖”。

//TabButton 没有在自己的 Start 里写 tabView = GetComponentInParent<TabView>();。

//相反，TabView 在自己的 Start 循环中，主动将自己“注入”给了每一个它管理的 TabButton：tabButtons[i].tabView = this;。

//好处： 解耦 和 清晰的层级。TabButton 变得更像一个“数据结构”，它的依赖关系非常明确（在 Inspector 面板中被 TabView 统一管理），而不是在代码运行时“满世界去找”它的上级。