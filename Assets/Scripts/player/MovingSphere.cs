using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    /// <summary>
    /// 最大速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    readonly float m_maxSpeed = 10f;

    /// <summary>
    /// 最大加速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    readonly float m_maxAcceleration = 10f;

    /// <summary>
    /// 可移动区域
    /// </summary>
    [SerializeField]
    Rect m_allowedArea = new Rect(-5f, -5f, 10f, 10f); // 以角点坐标和尺寸表示

    /// <summary>
    /// 弹跳存速率
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    readonly float m_bounciness = 0.5f;

    /// <summary>
    /// 当前速度
    /// </summary>
    Vector3 m_velocity;

    /// <summary>
    /// 每帧调用
    /// </summary>
    void Update()
    {
        // 1. 得到玩家输入
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        // 2. 根据玩家输入计算速度
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;
        // 3. 根据加速度计算速度更新值
        float maxSpeedChange = m_maxAcceleration * Time.deltaTime; // 直接用max了
        m_velocity.x = Mathf.MoveTowards(m_velocity.x, desiredVelocity.x, maxSpeedChange);
        m_velocity.z = Mathf.MoveTowards(m_velocity.z, desiredVelocity.z, maxSpeedChange);

        // 4. 根据速度计算目标坐标
        Vector3 newPosition = transform.position + m_velocity * Time.deltaTime;

        // 5. 将目标坐标固定在可行区域内。
        if (newPosition.x < m_allowedArea.xMin)
        {
            newPosition.x = m_allowedArea.xMin;
            m_velocity.x = -m_velocity.x * m_bounciness; // 使用弹跳
        }
        else if (newPosition.x > m_allowedArea.xMax)
        {
            newPosition.x = m_allowedArea.xMax;
            m_velocity.x = -m_velocity.x * m_bounciness;
        }
        else if (newPosition.z < m_allowedArea.yMin)
        {
            newPosition.z = m_allowedArea.yMax; // 使用teleport
            // m_velocity.z = -m_velocity.z;
        }
        else if (newPosition.z > m_allowedArea.yMax)
        {
            newPosition.z = m_allowedArea.yMin;
            // m_velocity.z = -m_velocity.z;
        }
        // 6. 更新当前坐标
        transform.localPosition = newPosition;
    }
}
