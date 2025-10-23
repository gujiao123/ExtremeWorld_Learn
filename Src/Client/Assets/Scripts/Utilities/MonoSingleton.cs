using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;
    static T instance;
    public static T Instance
    {
        get
        {

            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }
    }






    void Start()
    {
        Debug.LogWarningFormat("{0}  [{1}] Start", typeof(T), this.GetInstanceID());
        //对于全局持久化单例 我们要阻止重复创建scene加载时创建多个实例
        if (global)
        {
            //当前不为空而且与当前对象不相等
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {
                //摧毁自己 保留原来那一个 就不用重复订阅了
                Destroy(this.gameObject);
                return;
            }
            //如果原来是空的就直接赋值
            instance = this.gameObject.GetComponent<T>();
            DontDestroyOnLoad(this.gameObject);
        }
        this.OnStart();
    }

    protected virtual void OnStart()
    {

    }
}