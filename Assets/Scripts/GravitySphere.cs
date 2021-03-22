using UnityEngine;

/// <summary>
/// 用于处理球核和球壳体的重力逻辑（内部向内吸外部向外推）
/// </summary>
public class GravitySphere : GravitySource
{
    #region 设定参数
    /// <summary>
    /// 重力值
    /// </summary>
    [SerializeField]
    float m_gravity = 9.81f;

    /// <summary>
    /// 重力起始范围
    /// </summary>
    [SerializeField, Min(0f)]
    float m_outerRadius = 10f;

    /// <summary>
    /// 向内重力极限范围
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerFalloffRadius = 1f;

    /// <summary>
    /// 向内重力起始范围
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerRadius = 5f;

    /// <summary>
    /// 重力下降范围
    /// </summary>
    [SerializeField, Min(0f)]
    float m_outerFalloffRadius = 15f;
    #endregion

    #region 
    /// <summary>
    /// 衰减空间内的重力衰减率
    /// </summary>
    float m_outerFalloffFactor;

    /// <summary>
    /// 衰减空间内的重力衰减率
    /// </summary>
    float m_innerFalloffFactor;
    #endregion

    /// <summary>
    /// 初始化时调用
    /// </summary>
    void Awake()
    {
        OnValidate();
    }

    /// <summary>
    /// hot reload的时候被调用
    /// </summary>
    void OnValidate()
    {
        // 1. 设定内外距离参数为valid值
        m_innerFalloffRadius = Mathf.Max(m_innerFalloffRadius, 0f);
        m_innerRadius = Mathf.Max(m_innerRadius, m_innerFalloffRadius);
        m_outerRadius = Mathf.Max(m_outerRadius, m_innerRadius);
        m_outerFalloffRadius = Mathf.Max(m_outerFalloffRadius, m_outerRadius);

        // 2. 设定重力衰减率
        m_innerFalloffFactor = 1f / (m_innerRadius - m_innerFalloffRadius);
        m_outerFalloffFactor = 1f / (m_outerFalloffRadius - m_outerRadius);
    }

    /// <summary>
    /// 计算某点在该物体影响下的重力
    /// </summary>
    /// <param name="position">目标点</param>
    /// <returns>重力</returns>
    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 vector = transform.position - position;
        float distance = vector.magnitude;
        // 1. 如果位置不在壳和核之间重力区域则重力为0
        if (distance > m_outerFalloffRadius || distance < m_innerFalloffRadius)
        {
            return Vector3.zero;
        }
        // 2. 在outerRaidus之外重力随distance减弱
        float g = m_gravity / distance;
        if (distance > m_outerRadius)
        {
            g *= 1f - (distance - m_outerRadius) * m_outerFalloffFactor;
        }
        // 3. innerRadius之内重力随distance亦减弱，并且反向
        else if (distance < m_innerRadius)
        {
            g *= 1f - (m_innerRadius - distance) * m_innerFalloffFactor;
            g = -g;
        }
        return g * vector;
    }

    /// <summary>
    /// 绘制外部和内部重力范围的示意
    /// </summary>
    void OnDrawGizmos()
    {
        // 1. 判断距离设定是否valid并绘制外壳和核的重力有效范围
        Vector3 p = transform.position;
        if (m_innerFalloffRadius > 0f && m_innerFalloffRadius < m_innerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, m_innerFalloffRadius);
        }
        Gizmos.color = Color.yellow;
        if (m_innerRadius > 0f && m_innerRadius < m_outerRadius)
        {
            Gizmos.DrawWireSphere(p, m_innerRadius);
        }
        Gizmos.DrawWireSphere(p, m_outerRadius);
        if (m_outerFalloffRadius > m_outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, m_outerFalloffRadius);
        }
    }
}
