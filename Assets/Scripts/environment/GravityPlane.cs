using UnityEngine;

/// <summary>
/// 处理面状重力逻辑
/// </summary>
public class GravityPlane : GravitySource
{
    /// <summary>
    /// 重力值大小
    /// </summary>
    [SerializeField]
    float m_gravity = 9.81f;

    /// <summary>
    /// 影响范围
    /// </summary>
    [SerializeField, Min(0f)]
    float m_range = 1f;

    /// <summary>
    /// 获得该物体对影响点位置的重力
    /// </summary>
    /// <param name="position">影响点位置</param>
    /// <returns>重力大小和方向</returns>
    public override Vector3 GetGravity(Vector3 position)
    {
        // 1. 计算目标是否在影响距离内
        Vector3 up = transform.up;
        float distance = Vector3.Dot(up, position - transform.position); //? 不太对劲，按说应该是判断是否在box里
        // 2. 不在影响范围内则返回0重力
        if (distance > m_range)
        {
            return Vector3.zero;
        }
        // 2. 否则用距离将边角的重力变化缓和并返回重力值
        float g = -m_gravity;
        if (distance > 0f)
        {
            g *= 1f - distance / m_range;
        }
        // 3. 返回重力值
        return g * up;
    }

    /// <summary>
    /// 绘制Gizmos
    /// 用于绘制plane gravity source的距离和范围
    /// </summary>
    void OnDrawGizmos()
    {
        // 1. 初始化绘制变量
        Vector3 scale = transform.localScale;
        scale.y = m_range;
        // 2. Gizmos变换和物体transform保持一致
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            scale
        );
        Vector3 size = new Vector3(1f, 0f, 01);
        // 3. 绘制plane位置，gravity起点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, size); //? 我的Gizmos只有一点点大了
        // 4. 绘制gravity终点
        if (m_range > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up, size);
        }
    }
}
