using UnityEngine;

/// <summary>
/// 处理box内部重力的逻辑
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
    /// 中心点到各个墙壁间距离
    /// </summary>
    [SerializeField]
    Vector3 m_boundaryDistance = Vector3.one;

    /// <summary>
    /// 内部稳定重力距离
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerDistance = 0f;

    /// <summary>
    /// 内部重力衰减极限
    /// </summary>
    [SerializeField, Min(0f)]
    float m_innerFalloffDistance = 0f;

    /// <summary>
    /// 外部重力稳定距离
    /// </summary>
    [SerializeField, Min(0f)]
    float m_outerDistance = 0f;

    /// <summary>
    /// 外部重力衰减极限
    /// </summary>
    [SerializeField, Min(0f)]
    float m_outerFalloffDistance = 0f;

    #endregion // 设定参数

    #region 中间变量

    /// <summary>
    /// 重力衰减率
    /// </summary>
    float m_innerFalloffFactor;

    /// <summary>
    /// 外部重力衰减率
    /// </summary>
    float m_outerFalloffFactor;

    #endregion

    /// <summary>
    /// 初始化时调用
    /// </summary>
    void Awake()
    {
        // 1. 验证或修正几何设定参数
        OnValidate();
    }

    /// <summary>
    /// 验证或修正设定值
    /// </summary>
    void OnValidate()
    {
        // 1. 计算最近的几何边界距离并设定内部重力稳定距离
        m_boundaryDistance = Vector3.Max(m_boundaryDistance, Vector3.zero);
        float maxInner = Mathf.Min(
            Mathf.Min(m_boundaryDistance.x, m_boundaryDistance.y), m_boundaryDistance.z
        );
        m_innerDistance = Mathf.Min(m_innerDistance, maxInner);
        // 2. 根据重力稳定距离来设定有效的重力衰减距离
        m_innerFalloffDistance = Mathf.Max(Mathf.Min(m_innerFalloffDistance, maxInner), m_innerDistance);
        m_outerFalloffDistance = Mathf.Max(m_outerFalloffDistance, m_outerDistance);
        // 3. 设定重力衰减率
        m_innerFalloffFactor = 1f / (m_innerFalloffDistance - m_innerDistance);
        m_outerFalloffFactor = 1f / (m_outerFalloffDistance - m_outerDistance);
    }

    /// <summary>
    /// 用于绘制重力边缘
    /// </summary>
    void OnDrawGizmos()
    {
        // 1. 设定Gizmos的变换矩阵和Box一致
        Gizmos.matrix =
        Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Vector3 size;
        // 2. 验证各个内部distance的绘制必要性并绘制
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
        // 3. 验证各个外部distance的绘制必要性并绘制
        if (m_outerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            DrawGizmosOuterCube(m_outerDistance);
        }
        if (m_outerFalloffDistance > m_outerDistance)
        {
            Gizmos.color = Color.cyan;
            DrawGizmosOuterCube(m_outerFalloffDistance);
        }
    }

    /// <summary>
    /// 画一个矩形（可能画不出来矩形hhh）
    /// </summary>
    /// <param name="a">点1</param>
    /// <param name="b">点2</param>
    /// <param name="c">点3</param>
    /// <param name="d">点4</param>
    void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector4 d)
    {
        // 1. 绘制矩形的四条边
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    /// <summary>
    /// 绘制外部的重力立方
    /// </summary>
    /// <param name="distance">重力立方距几何边界距离</param>
    void DrawGizmosOuterCube(float distance)
    {
        // 1. 画x轴上的两个面
        Vector3 a, b, c, d;
        a.y = b.y = m_boundaryDistance.y;
        d.y = c.y = -m_boundaryDistance.y;
        b.z = c.z = m_boundaryDistance.z;
        d.z = a.z = -m_boundaryDistance.z;
        a.x = b.x = c.x = d.x = m_boundaryDistance.x + distance;
        DrawGizmosRect(a, b, c, d);
        a.x = b.x = c.x = d.x = -a.x;
        DrawGizmosRect(a, b, c, d);

        // 2. 画y轴上的两个面
        a.x = d.x = m_boundaryDistance.x;
        b.x = c.x = -m_boundaryDistance.x;
        a.z = b.z = m_boundaryDistance.z;
        c.z = d.z = -m_boundaryDistance.z;
        a.y = b.y = c.y = d.y = m_boundaryDistance.y + distance;
        DrawGizmosRect(a, b, c, d);
        a.y = b.y = c.y = d.y = -a.y;
        DrawGizmosRect(a, b, c, d);

        // 3. 画z轴上的两个面
        a.x = d.x = m_boundaryDistance.x;
        b.x = c.x = -m_boundaryDistance.x;
        a.y = b.y = m_boundaryDistance.y;
        c.y = d.y = -m_boundaryDistance.y;
        a.z = b.z = c.z = d.z = m_boundaryDistance.z + distance;
        DrawGizmosRect(a, b, c, d);
        a.z = b.z = c.z = d.z = -a.z;
        DrawGizmosRect(a, b, c, d);

        distance *= 0.5773502692f;
        Vector3 size = m_boundaryDistance;
        size.x = 2f * (size.x + distance);
        size.y = 2f * (size.y + distance);
        size.z = 2f * (size.z + distance);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }


    /// <summary>
    /// 计算某个点受Box的引力值
    /// </summary>
    /// <param name="position">点的位置</param>
    /// <returns>引力值</returns>
    public override Vector3 GetGravity(Vector3 position)
    {
        // 1. 计算相对于box中心的位移，在Box当前局部坐标下
        position = transform.InverseTransformDirection(position - transform.position);
        // 2. 将vector初始化为0（仅考虑单个轴上的重力）
        Vector3 vector = Vector3.zero;

        // 3. 判断是否在box外部，如果是设定vector的对应轴的值（位移）为相对box对应面的距离，并令outside计数增加
        int outside = 0;
        if (position.x > m_boundaryDistance.x)
        {
            vector.x = m_boundaryDistance.x - position.x;
            outside = 1;
        }
        else if (position.x < -m_boundaryDistance.x)
        {
            vector.x = -m_boundaryDistance.x - position.x;
            outside = 1;
        }
        if (position.y > m_boundaryDistance.y)
        {
            vector.y = m_boundaryDistance.y - position.y;
            outside += 1;
        }
        else if (position.y < -m_boundaryDistance.y)
        {
            vector.y = -m_boundaryDistance.y - position.y;
            outside += 1;
        }
        if (position.z > m_boundaryDistance.z)
        {
            vector.z = m_boundaryDistance.z - position.z;
            outside += 1;
        }
        else if (position.z < -m_boundaryDistance.z)
        {
            vector.z = -m_boundaryDistance.z - position.z;
            outside += 1;
        }

        // 4. 检查是否真正在外部
        if (outside > 0)
        {
            // 5. 如果是，距离相当于前述相对距离vector的模
            float distance = outside == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude; // 在outside计数为1的情况下不使用模计算更快
            // 6. 根据距离判断应用的重力大小
            if (distance > m_outerFalloffDistance)
            {
                return Vector3.zero;
            }
            float g = m_gravity / distance;
            if (distance > m_outerDistance)
            {
                g *= 1f - (distance - m_outerDistance) * m_outerFalloffFactor;
            }
            return transform.TransformDirection(-g * vector);
        }
        // 7. 计算在不同坐标轴上的到各个对面上的最近距离
        Vector3 distances;
        distances.x = m_boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = m_boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = m_boundaryDistance.z - Mathf.Abs(position.z);
        // 8. 判断三个轴的距离相对大小，并取最小的距离计算相应轴的重力值
        if (distances.x < distances.y)
        {
            if (distances.x < distances.z)
            {
                vector.x = GetGravityComponent(position.x, distances.x);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
        }
        else if (distances.y < distances.z)
        {
            vector.y = GetGravityComponent(position.y, distances.y);
        }
        else
        {
            vector.z = GetGravityComponent(position.z, distances.z);
        }
        // 9. 返回计算的重力值（由于是局部坐标，需要转回全局坐标）
        return transform.TransformDirection(vector);
    }

    /// <summary>
    /// 计算Box对象中某个坐标轴的相对墙壁对某点的引力值
    /// </summary>
    /// <param name="coordinate">和box中心的位移</param>
    /// <param name="distance">到最近墙壁的距离（只含有大于0的值）</param>
    /// <returns>某个墙壁对该点的重力值</returns>
    float GetGravityComponent(float coordinate, float distance)
    {
        // 1. 如果距离已经超过衰减距离则没有引力
        if (distance > m_innerFalloffDistance)
        {
            return 0f;
        }
        float g = m_gravity;
        // 2. 如果距离超过稳定距离则线性降低引力
        if (distance > m_innerDistance)
        {
            g *= 1f - (distance - m_innerDistance) * m_innerFalloffFactor;
        }
        // 3. 考虑每个轴，coordinate处于正半轴和负半轴时重力方向相反
        return coordinate > 0f ? -g : g;
    }
}