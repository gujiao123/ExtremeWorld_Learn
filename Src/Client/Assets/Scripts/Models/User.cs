using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using Entities;
using UnityEngine;

namespace Models
{
    class User : Singleton<User>
    {
        //me 用户信息 还是通过协议定义的类型来保存
        SkillBridge.Message.NUserInfo userInfo;

        public SkillBridge.Message.NUserInfo Info
        {
            get { return userInfo; }
        }

        /// <summary>
        /// 设置保存用户信息
        /// </summary>
        /// <param name="info"></param>
        public void SetupUserInfo(SkillBridge.Message.NUserInfo info)
        {
            this.userInfo = info;
        }
        //me 对于一个user来说 管理一个当前角色和地图 是显得合理的
        public MapDefine CurrentMapData { get; set; }
        //这个是角色实例化出来的游戏对象
        public GameObject CurrentCharacterObject { get; set; }
        //这个只是当前角色的信息  这个是从服务器返回的角色信息 一个通信协议定义的类型
        /// <summary>
        /// 我去这个还没有初始化的地方,服务器把角色信息发过来了 你才能知道啊
        /// </summary>
        public SkillBridge.Message.NCharacterInfo CurrentCharacter { get; set; }

    }
}
