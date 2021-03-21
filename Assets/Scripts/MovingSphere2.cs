using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于物理模拟的小球移动控制
/// </summary>
public class MovingSphere2 : MonoBehaviour
{

    #region 参数设定
    /// <summary>
    /// 最大速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSpeed = 10f;

    /// <summary>
    /// 最大吸附角度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSnapSpeed = 100f;

    /// <summary>
    /// 最大加速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxAcceleration = 10f;

    /// <summary>
    /// 最大空中加速度
    /// </summary>
    [SerializeField, Range(0, 100f)]
    float m_maxAirAcceleration = 1f;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField, Range(0f, 10f)]
    float m_jumpHeight = 2f;

    [SerializeField, Range(0f, 90f)]
    float m_maxGroundAngle = 25f;

    /// <summary>
    /// 最大空中跳跃次数
    /// </summary>
    [SerializeField, Range(0, 5)]
    int m_maxAirJumps = 0;

    /// <summary>
    /// 飞跃吸附检测高度
    /// </summary>
    [SerializeField, Min(0f)]
    float m_probeDistance = 1f;

    /// <summary>
    /// 飞跃吸附检测layer
    /// </summary>
    [SerializeField]
    LayerMask m_probeMask = -1;

    /// <summary>
    /// 将某些layer的物体视作stair，用于进行不同的移动判定
    /// </summary>
    [SerializeField]
    LayerMask m_stairMask = -1;
    #endregion // 参数设定

    #region 游戏中各类数据记录
    Vector3 m_velocity;

    Vector3 m_desiredVelocity;

    bool m_desiredJump;

    /// <summary>
    /// 和地面接触的数量
    /// </summary>
    int m_groundContactCount;

    /// <summary>
    /// 和陡壁接触的数量
    /// 用于不触地时的脱困
    /// </summary>
    int m_steepContactCount;

    /// <summary>
    /// 上方向和地面单位法线的最小点乘结果，小于它意味着碰撞面不被视为地面
    /// </summary>
    float m_minGroundDotProduct;

    /// <summary>
    /// 上方向和楼梯单位法线的最小点乘结果，小于它意味着碰撞面不被视为楼梯，楼梯可移动角度会比地面大一些
    /// </summary>
    float m_minStairDotProduct;

    /// <summary>
    /// 是否接触地面
    /// </summary>
    bool OnGround => m_groundContactCount > 0;

    /// <summary>
    /// 是否接触陡壁
    /// </summary>
    bool OnSteep => m_steepContactCount > 0;

    /// <summary>
    /// 浮空跳的阶段
    /// </summary>
    int m_jumpPhase;

    /// <summary>
    /// 当前地面接触的法线求和的方向
    /// </summary>
    Vector3 m_contactNormal;

    /// <summary>
    /// 当前陡壁的接触法线
    /// 用于当没有和地面接触但和多个陡壁接触时的脱困
    /// </summary>
    Vector3 m_steepNormal;

    /// <summary>
    /// 当前物体的rigidbody
    /// </summary>
    Rigidbody m_body;

    /// <summary>
    /// 自最后一次在地面后的steps（非跳跃）
    /// </summary>
    int m_stepsSinceLastGrounded;

    /// <summary>
    /// 自最后一次跳跃之后的steps
    /// </summary>
    int m_stepsSinceLastJump;

    #endregion //游戏中各类数据记录

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

        // 在小球不和地面接触的时候更改小球颜色
        GetComponent<MeshRenderer>().material.SetColor(
            "_Color", OnGround ? Color.black : Color.white
        );
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
        m_stepsSinceLastGrounded += 1;
        m_stepsSinceLastJump += 1;
        m_velocity = m_body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            m_stepsSinceLastGrounded = 0;
            if (m_stepsSinceLastJump > 1) // 因为jump后第一帧还是会OnGround，如果这个时候再进行跳跃那就可以无限浮空跳
            {
                m_jumpPhase = 0;
            }
            // 如果多个地面才进行和向量的标准化
            if (m_groundContactCount > 1)
            {
                m_contactNormal.Normalize();
            }
        }
        // 如果不在地面
        else
        {
            // 每次Update状态的时候记得重置接触方向向上（用于浮空跳之类）
            m_contactNormal = Vector3.up;
        }
    }

    void ClearState()
    {
        // 每次fixedUpdate结束时将onGround等接触数据重置于false，在下次物理检测时再行更新
        m_groundContactCount = 0;
        m_steepContactCount = 0;
        m_contactNormal = Vector3.zero;
        m_steepNormal = Vector3.zero;
    }

    /// <summary>
    /// 评估碰撞对象，在OnCollisionXXX时被调用
    /// </summary>
    /// <param name="collision">碰撞体</param>
    void EvaluateCollision(Collision collision)
    {
        // 得到碰撞对象的layer
        float minDot = GetMinDot(collision.gameObject.layer);
        // 1. 遍历所有碰撞对象
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 2. 得到碰撞接触面法线
            Vector3 normal = collision.GetContact(i).normal;
            // 3. 如果坡度小于预设坡度，那么仍视作是地面
            if (normal.y >= minDot)
            {
                // 4. 增加接触地面计数
                m_groundContactCount += 1;
                // 5. 更新地面接触法线
                m_contactNormal += normal;
            }
            // 否则应为陡壁
            else if (normal.y > -0.01f)
            {
                // 增加陡壁计数
                m_steepContactCount += 1;
                // 更新陡壁法线
                m_steepNormal += normal;
            }
        }
    }

    /// <summary>
    /// 跳！
    /// </summary>
    void Jump()
    {
        Vector3 jumpDirection;
        // 如果在（等效）地面上，地面法线方向跳跃
        if (OnGround)
        {
            jumpDirection = m_contactNormal;
        }
        // 如果在墙上，墙跳
        else if (OnSteep)
        {
            jumpDirection = m_steepNormal;
            m_jumpPhase = 0;
        }
        // 如果在Air，UpdateState中更新了接触法线是向上
        else if (m_maxAirJumps > 0 && m_jumpPhase <= m_maxAirJumps)
        {
            if (m_jumpPhase == 0) //? 处理从地面掉下的多余一次跳跃？相当于这样将掉落视作第一次浮空跳了，这就和其他的跳跃形式齐整了，否则就多一次
            {
                m_jumpPhase = 1;
            }
            jumpDirection = m_contactNormal;
        }
        // 否则不满足跳跃条件返回
        else
        {
            return;
        }
        // 重置上次跳跃之后的steps
        m_stepsSinceLastJump = 0;
        // 记录已经进行的跳跃阶段
        m_jumpPhase += 1;
        // 根据设定的跳跃高度计算跳跃速度
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
        //? 可能用于浮空跳
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(m_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        // 原始速度叠加沿法线方向跳跃速度
        m_velocity += jumpDirection * jumpSpeed;
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

    /// <summary>
    /// 在越过地面尖角时令物体压在地面上，用于处理惯性飞出的情况
    /// </summary>
    /// <returns>返回是否需要进行移动吸附</returns>
    bool SnapToGround()
    {
        // 如果已经不是跳起的第一帧那么没必要再Sanp了因为已经正常起飞了 + 如果是跳跃的前两帧也不要snap了
        if (m_stepsSinceLastGrounded > 1 || m_stepsSinceLastJump <= 2) // 为什么考虑跳跃的前2帧呢？1.跳跃的第一帧仍然是OnGrounded判断可能true 2.跳跃2帧开始必不为OnGrounded不用它来判断了
        {
            return false;
        }
        // 如果开车已经超速了那飞出去算了
        float speed = m_velocity.magnitude;
        if (speed > m_maxSnapSpeed)
        {
            return false;
        }
        // 如果下面没地面那么也不需要了
        if (!Physics.Raycast(m_body.position, Vector3.down, out RaycastHit hit, m_probeDistance, m_probeMask))
        {
            return false;
        }
        // 如果下面的地面角度太陡那也不需要了
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }
        // 如果需要进行吸附，那么将地面技术设为1，并设置地面法线为当前hit的normal
        m_groundContactCount = 1; //? 此时需要将地面接触数量设定为1提示其他函数进行处理
        m_contactNormal = hit.normal;
        // 速度沿地面法线上的分量
        float dot = Vector3.Dot(m_velocity, m_contactNormal);
        // 如果向下坡方向移动，则求得速度沿斜面的方向的单位向量，并和速度绝对值相乘得到速度方向
        if (dot > 0f)
        {
            m_velocity = (m_velocity - m_contactNormal * dot).normalized * speed;
        }
        return false;
    }

    float GetMinDot(int layer)
    {
        // 判断是否
        return (m_stairMask & (1 << layer)) == 0 ?
            m_minGroundDotProduct : m_minStairDotProduct; // 注意==和&的优先级
    }

    bool CheckSteepContacts()
    {
        // 判断陡壁接触数量，多于一次陡壁接触（夹住时）方可视为稳定地面
        if (m_steepContactCount > 1)
        {
            m_steepNormal.Normalize();
            // 如果等效地面坡度小于最小可用地面坡度
            if (m_steepNormal.y >= m_minGroundDotProduct)
            {
                // 视作地面接触，并修改地面接触计数（结合Jump的优先判断Ground可以优先以等效ground进行处理）
                m_groundContactCount = 1;
                // 修改地面接触法线
                m_contactNormal = m_steepNormal;
                // 返回接触成功
                return true;
            }
        }
        // 返回接触失败
        return false;
    }
}
