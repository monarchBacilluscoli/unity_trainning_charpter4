using UnityEngine;

/// <summary>
/// todo 处理box内部重力的逻辑
/// </summary>
public class GravityBox : GravitySource
{
    #region 设定参数
    /// <summary>
    /// 重力值
    /// </summary>
    [SerializeField]
    float m_gravity = 9.81f;

    /// <summary>
    /// 墙壁间距离
    /// </summary>
    [SerializeField]
    Vector3 m_boundaryDistance = Vector3.one;

    /// <summary>
    /// 内部稳定重力距离
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerDistance = 0f;

    /// <summary>
    /// 内部重力衰减距离
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerFalloffDistance = 0f;

    #endregion // 设定参数

    #region 中间变量

    /// <summary>
    /// 重力衰减率
    /// </summary>
    float m_innerFalloffFactor;

    #endregion

    void Awake()
    {
        OnValidate();
    }
    void OnValidate()
    {
        m_boundaryDistance = Vector3.Max(m_boundaryDistance, Vector3.zero);
        float maxInner = Mathf.Min(
            Mathf.Min(m_boundaryDistance.x, m_boundaryDistance.y), m_boundaryDistance.z
        );
        m_innerDistance = Mathf.Min(m_innerDistance, maxInner);
        m_innerFalloffDistance = Mathf.Max(Mathf.Min(m_innerFalloffDistance, maxInner), m_innerDistance);
        m_innerFalloffFactor = 1f / (m_innerFalloffDistance - m_innerDistance);
    }
    void OnDrawGizmos()
    {
        Gizmos.matrix =
        Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Vector3 size;
        if (m_innerFalloffDistance > m_innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (m_boundaryDistance.x - m_innerFalloffDistance);
            size.y = 2f * (m_boundaryDistance.y - m_innerFalloffDistance);
            size.z = 2f * (m_boundaryDistance.z - m_innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        if (m_innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (m_boundaryDistance.x - m_innerDistance);
            size.y = 2f * (m_boundaryDistance.y - m_innerDistance);
            size.z = 2f * (m_boundaryDistance.z - m_innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * m_boundaryDistance);
    }

    float GetGravityComponent(float coordinate, float distance)
    {
        if (distance > m_innerFalloffDistance)
        {
            return 0f;
        }
        float g = m_gravity;
        if (distance > m_innerDistance)
        {
            g *= 1f - (distance - m_innerDistance) * m_innerFalloffFactor;
        }
        return coordinate > 0f ? -g : g;
    }
}