using System;


class EnumUtil
{
    /// <summary>
    /// 获取枚举值的 Description 特性描述文本，如果没有则返回枚举名称。
    /// </summary>
    /// <param name="enumValue">要获取描述的枚举值。</param>
    /// <returns>枚举的 Description 特性描述文本，若无则返回枚举名称。</returns>
    public static string GetEnumDescription(Enum enumValue)
    {
        // 获取枚举值的名称
        string str = enumValue.ToString();
        // 通过反射获取该名称对应的字段信息
        System.Reflection.FieldInfo field = enumValue.GetType().GetField(str);
        // 获取该字段上所有 DescriptionAttribute 特性
        object[] objs = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
        // 如果没有 DescriptionAttribute，则返回枚举名称
        if (objs == null || objs.Length == 0) return str;
        // 获取第一个 DescriptionAttribute
        System.ComponentModel.DescriptionAttribute da = (System.ComponentModel.DescriptionAttribute)objs[0];
        // 返回 Description 的内容
        return da.Description;
    }
}