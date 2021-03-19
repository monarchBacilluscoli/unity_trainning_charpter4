using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于物理模拟的小球移动控制
/// </summary>
public class MovingSphere2 : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float m_maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float m_maxAcceleration = 10f;

    [SerializeField, Range(0, 100f)]
    float m_maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float m_jumpHeight = 2f;

    [SerializeField, Range(0f, 90f)]
    float m_maxGroundAngle = 25f;

    float m_minGroundDotProduct;


    /// <summary>
    /// 最大空中跳跃次数
    /// </summary>
    [SerializeField, Range(0, 5)]
    int m_maxAirJumps = 0;


    Vector3 m_velocity;

    Vector3 m_desiredVelocity;

    bool m_desiredJump;

    int m_groundContactCount;

    bool OnGround => m_groundContactCount > 0;

    int m_jumpPhase;

    Vector3 m_contactNormal;

    Rigidbody m_body;

    /// <summary>
    /// 用于hot reload
    /// </summary>
    void OnValidate()
    {
        m_minGroundDotProduct = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);
    }

    /// <summary>
    /// 启用的时候被调用
    /// </summary>
    void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        OnValidate();
    }

    /// <summary>
    /// 每帧被调用
    /// </summary>
    void Update()
    {
        // 更新并记录速度
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        m_desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;

        // 记录跳跃
        m_desiredJump |= Input.GetButtonDown("Jump"); // 本来如果没有down那就是false，用了|=就是两者只要有一个是true就是true
    }

    /// <summary>
    /// 按照固定时长被调用，在被推迟之后仍然会调用本应调用的次数
    /// </summary>
    void FixedUpdate()
    {
        // 更新当前物体物理状态为Update做准备？
        UpdateState();
        // 更新速度
        AdjustVelocity();
        // 如果之前(Update中)按下了跳跃键
        if (m_desiredJump)
        {
            // 将“妄图跳跃”重置代表已经进行跳跃
            m_desiredJump = false;
            // 进行跳跃
            Jump();
        }

        // 更新rigidbody速度
        m_body.velocity = m_velocity;

        // 清空为当前帧其他函数记录的状态
        ClearState();
    }

    /// <summary>
    /// 当两个物体碰撞刚检测到的时候调用
    /// </summary>
    /// <param name="collision">碰撞体</param>
    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision); //? 为什么需要两个？
    }

    /// <summary>
    /// 当两个物体碰撞在进行的时候调用
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    /// <summary>
    /// 更新记录当前物体状态
    /// </summary>
    void UpdateState()
    {
        m_velocity = m_body.velocity;
        if (OnGround)
        {
            m_jumpPhase = 0;
            if (m_groundContactCount > 1)
            {
                m_contactNormal.Normalize();
            }
        }
        else
        {
            // 如果不在地面，每次Update状态的时候记得重置接触方向向上
            m_contactNormal = Vector3.up;
        }
    }

    void ClearState()
    {
        // 每次fixedUpdate结束时将onGround重置于false，在下次物理检测时再行更新
        m_groundContactCount = 0;
        m_contactNormal = Vector3.zero;
    }

    /// <summary>
    /// 评估碰撞对象，在OnCollisionXXX时被调用
    /// </summary>
    /// <param name="collision">碰撞体</param>
    void EvaluateCollision(Collision collision)
    {
        // 1. 遍历所有碰撞对象
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 2. 得到碰撞接触面法线
            Vector3 normal = collision.GetContact(i).normal;
            // 3. 如果坡度小于预设坡度，那么仍视作是地面
            if (normal.y >= Mathf.Cos(m_maxGroundAngle))
            {
                // 4. 增加接触地面计数
                m_groundContactCount += 1;
                // 5. 更新地面接触法线
                m_contactNormal += normal;
            }
        }
    }

    /// <summary>
    /// 跳！
    /// </summary>
    void Jump()
    {
        if (OnGround || m_jumpPhase < m_maxAirJumps)
        {
            m_jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
            float alignedSpeed = Vector3.Dot(m_velocity, m_contactNormal);
            if (m_velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            m_velocity += jumpSpeed * m_contactNormal;
        }
    }

    /// <summary>
    /// 根据当前平面调整速度使其沿任意有角度平面正常更新
    /// </summary>
    void AdjustVelocity()
    {
        // 求得当前x、z轴在接触面上的分量
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        // 求出当前速度（已经沿接触面）沿接触面上两个方向的分量
        float currentX = Vector3.Dot(m_velocity, xAxis);
        float currentZ = Vector3.Dot(m_velocity, zAxis);
        // 拿到加速度
        float acceleration = OnGround ? m_maxAcceleration : m_maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        // 计算当前应到速度
        float newX = Mathf.MoveTowards(currentX, m_desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, m_desiredVelocity.z, maxSpeedChange);

        // 将当前速度沿平面的方向进行分配，而非沿水平x、z进行分配
        m_velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /// <summary>
    /// 求得任意向量沿当前平面的分量
    /// </summary>
    /// <param name="vector">任意向量</param>
    /// <returns>沿当前平面的分量</returns>
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        // 利用三角函数及向量运算求得任意向量沿当前接触平面的分量
        return vector - m_contactNormal * Vector3.Dot(m_contactNormal, vector); //! 点乘不满足交换律
    }
}
