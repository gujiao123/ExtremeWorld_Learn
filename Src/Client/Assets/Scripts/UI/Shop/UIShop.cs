using Common.Data;
using Managers;
using Models;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : MonoBehaviour
{

    public Text title;
    public Text money;
    public GameObject shopItem;
    ShopDefine shop;
    public Transform[] itemRoot;

    void Start()
    {
        StartCoroutine(InitItems());
    }

    IEnumerator InitItems()
    {
        //考虑分页
        int count = 0;
        int page = 0;
        foreach (var kv in DataManager.Instance.ShopItems[shop.ID])
        {
            if (kv.Value.Status > 0)
            {
                //商店里出售的物品 是直接实例化在content里面的自动排序不需要格子绑定
                //所以10个就换页是可以的
                GameObject go = Instantiate(shopItem, itemRoot[page]);
                UIShopItem ui = go.GetComponent<UIShopItem>();
                ui.SetShopItem(kv.Key, kv.Value, this);
                count++;
                if (count >= 10)
                {
                    count = 0;
                    page++;
                    itemRoot[page].gameObject.SetActive(true);
                }
            }
        }
        yield return null;
    }

    public void SetShop(ShopDefine shop)
    {
        this.shop = shop;
        this.title.text = shop.Name;
        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();
    }

    private UIShopItem selectedItem;

    public void SelectShopItem(UIShopItem item)
    {
        if (selectedItem != null)
        {
            selectedItem.Selected = false;
        }
        selectedItem = item;
    }

    public void OnClickBuy()
    {
        if ((this.selectedItem == null))
        {
            MessageBox.Show("请选择要购买的道具", "购买提示");
            return;
        }

        if (!ShopManager.Instance.BuyItem(this.shop.ID, this.selectedItem.ShopItemID))
        {

        }
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close(typeof(UIShop));
    }
}
