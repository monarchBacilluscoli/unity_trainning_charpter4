using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 用于计算当前位置重力（以(0,0,0)为重心）的逻辑
/// </summary>
static public class CustomGravity
{

    /// <summary>
    /// 重力源列表
    /// </summary>
    /// <typeparam name="GravitySource">重力源</typeparam>
    static List<GravitySource> m_sources = new List<GravitySource>();

    /// <summary>
    /// 注册重力源
    /// </summary>
    /// <param name="source">重力源</param>
    public static void Register(GravitySource source)
    {
        // 1. 检查是否已存在
        Debug.Assert(!m_sources.Contains(source),
            "Duplicate registration of gravity source!", source
        );
        // 2. 添加
        m_sources.Add(source);
    }

    /// <summary>
    /// 删除重力源
    /// </summary>
    /// <param name="source">重力源</param>
    public static void Unregister(GravitySource source)
    {
        // 1. 检查重力源是否不存在，是则报错
        Debug.Assert(m_sources.Contains(source),
            "Unrestration of unkonwn gravity source", source
        );
        // 2. 移除
        m_sources.Remove(source);
    }

    /// <summary>
    /// 得到当前重力向量
    /// </summary>
    /// <param name="position">当前位置</param>
    /// <returns>重力向量</returns>
    public static Vector3 GetGravity(Vector3 position)
    {
        // 1. 将当前位置所有重力向量求和并返回结果
        Vector3 g = Vector3.zero;
        for (int i = 0; i < m_sources.Count; i++)
        {
            g += m_sources[i].GetGravity(position);
        }
        return g;
    }

    /// <summary>
    /// 得到当前重力向量的反方向
    /// </summary>
    /// <param name="position">当前位置</param>
    /// <returns>重力向量的反方向的单位向量</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        // 1. 根据当前重力向量返回其反方向
        Vector3 g = Vector3.zero;
        for (int i = 0; i < m_sources.Count; i++)
        {
            g += m_sources[i].GetGravity(position);
        }
        return -g.normalized;
    }

    /// <summary>
    /// 得到当前位置的重力向量和上方向
    /// </summary>
    /// <param name="position">当前位置</param>
    /// <param name="upAxis">重力反向的单位向量</param>
    /// <returns>重力向量</returns>
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        // 1. 得到所有重力源合向量并返回 and 根据合向量设置上方向
        Vector3 g = Vector3.zero;
        for (int i = 0; i < m_sources.Count; i++)
        {
            g += m_sources[i].GetGravity(position);
        }
        upAxis = -g.normalized;
        return g;
    }

}
