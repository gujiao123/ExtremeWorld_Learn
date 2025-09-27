using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System;
using System.IO;

using Common.Data;

using Newtonsoft.Json;//就是把C#转化为Json格式的一个插件
/// <summary>
/// 这个文件很重要 必须最早开始加载
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // 存储配置文件所在的文件夹路径
    public string DataPath;
    // 存储所有地图的定义数据，用地图ID（int）作为key来快速查找
    public Dictionary<int, MapDefine> Maps = null;
    // 存储所有角色定义数据
    public Dictionary<int, CharacterDefine> Characters = null;
    // 存储所有传送点定义数据
    public Dictionary<int, TeleporterDefine> Teleporters = null;
    // 存储所有出生点定义数据。这是一个嵌套字典，第一层key是地图ID，第二层key是出生点ID
    public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;
    //me 注意这里面的数据都是写死在本地文件Data里面的
    public DataManager()
    {
        this.DataPath = "Data/";
        Debug.LogFormat("DataManager > DataManager()");
    }

    public void Load()
    {
        // 1. 读取 MapDefine.txt 文件中的所有文本内容
        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        // 2. ✅ 使用 Json.NET 将 JSON 文本反序列化为 C# 的 Dictionary<int, MapDefine> 对象
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

        json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

        json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
        this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
        this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);
    }

    /// <summary>
    /// 异步加载所有数据。
    /// 这是一个协程（Coroutine），它将加载过程分散到多个游戏帧中执行，避免游戏卡顿。
    /// </summary>
    public IEnumerator LoadData()
    {
        // 读取地图数据
        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);
        // 暂停此协程的执行，等到下一帧再继续，把CPU时间让给其他任务
        yield return null;

        // 读取角色数据
        json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);
        yield return null; // 等待下一帧

        // 读取传送点数据
        json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
        this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);
        yield return null; // 等待下一帧

        // 读取出生点数据
        json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
        this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);
        yield return null; // 等待下一帧
    }

    // 这是一个预处理指令，#if 和 #endif 之间的代码只会在 Unity 编辑器模式下被编译。
    // 这样做可以确保玩家玩的游戏最终版本中不包含这些只给开发者用的功能。
#if UNITY_EDITOR
    /// <summary>
    /// (仅在编辑器中可用) 保存传送点数据到文件。
    /// </summary>
    public void SaveTeleporters()
    {
        // ✅ 使用 Json.NET 将内存中的 Teleporters 字典序列化为 JSON 文本。
        // Formatting.Indented 参数会让生成的 JSON 文本有缩进，格式优美，方便人阅读。
        string json = JsonConvert.SerializeObject(this.Teleporters, Formatting.Indented);
        // 将生成的 JSON 文本字符串写入到文件中，实现保存功能。
        File.WriteAllText(this.DataPath + "TeleporterDefine.txt", json);
    }

    /// <summary>
    /// (仅在编辑器中可用) 保存出生点数据到文件。
    /// </summary>
    public void SaveSpawnPoints()
    {
        // 同样，序列化 SpawnPoints 数据
        string json = JsonConvert.SerializeObject(this.SpawnPoints, Formatting.Indented);
        // 写入文件
        File.WriteAllText(this.DataPath + "SpawnPointDefine.txt", json);
    }

#endif
}
