using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
//它的核心功能是管理一组列表项(ListViewItem)，并确保在任何时候只有一个列表项能被选中。
//管理content里面的内容
[System.Serializable]
public class ItemEvent : UnityEvent<ListView.ListViewItem>
{

}


//脚本定义了两个核心类：

//ListView: 这是主组件，作为列表的“容器”和“管理器”。

//ListViewItem: 这是一个嵌套类，代表列表中的“每一个可点击项”。

//还有一个事件类：

//ItemEvent: 一个可序列化的 UnityEvent，用于在 Inspector 面板中暴露事件（尽管在当前代码中并未使用，而是使用了 UnityAction）。
public class ListView : MonoBehaviour
{
    // 这是一个 C# 委托 (delegate)，用于在"选中项"发生变化时广播事件。
    // 外部脚本可以通过 "myListView.onItemSelected += ..." 来订阅这个事件。
    public UnityAction<ListViewItem> onItemSelected;



    public class ListViewItem : MonoBehaviour, IPointerClickHandler
    {
        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                onSelected(selected);
            }
        }
        public virtual void onSelected(bool selected)
        {

        }

        public ListView owner;
        /// <summary>
        /// 一切点击的入口更改选中状态
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!this.selected)
            {
                this.Selected = true;
            }
            if (owner != null && owner.SelectedItem != this)
            {
                owner.SelectedItem = this;
            }
        }
    }



    // 内部列表，用于追踪所有已添加的 ListViewItem
    List<ListViewItem> items = new List<ListViewItem>();

    // 私有字段，存储当前被选中的那一项
    private ListViewItem selectedItem = null;

    // 管理自己得ListViewItem
    public ListViewItem SelectedItem
    {
        get { return selectedItem; }
        private set
        {
            //有一个旧的选中项 并且和新的不一样
            if (selectedItem != null && selectedItem != value)
            {
                //原来的取消选中
                selectedItem.Selected = false;
            }
            //设置新的选中项
            selectedItem = value;
            //广播者只管广播 null也要广播
            if (onItemSelected != null)
                onItemSelected.Invoke((ListViewItem)value);
        }
    }

    public void AddItem(ListViewItem item)
    {
        item.owner = this;
        this.items.Add(item);
    }
    /// <summary>
    /// 很方便啊  清除所有Item
    /// </summary>
    public void RemoveAll()
    {
        if (items != null)
        {
            foreach (var it in items)
            {
                if (it != null)
                    Destroy(it.gameObject);
            }
            items.Clear();
        }
    }


    /// <summary>
    /// 公开的API，用于从外部清除当前选中项。
    /// 这将正确地触发 onSelected(false) 和 onItemSelected(null) 事件。
    /// </summary>
    public void ClearSelection()
    {
        // 我们不需要执行任何复杂的逻辑，
        // 只需要调用我们自己的 'SelectedItem' 属性的 'set' 访问器
        // 传入 null 即可。
        // 'private set' 允许*类自己内部*调用 'set'。
        this.SelectedItem = null;
    }
}


//@ 🧩 1. 中介者模式 (Mediator Pattern)ListViewItem 之间相互不知情 只知道自己的owner是谁 由owner来管理选中状态


//🧩 2.观察者模式(Observer Pattern) ListView 通过 onItemSelected 事件通知外部订阅者选中项的变化

//🧩 3.模板方法模式(Template Method Pattern) 抽象类 ListViewItem 定义了 onSelected 方法 允许子类重写以实现自定义行为

//🧩 5.封装(Encapsulation)定义： 隐藏一个对象的内部状态和实现细节，只暴露有限的公共接口。