using UnityEngine;

/// <summary>
/// 基于物理模拟的小球移动控制
/// +自定义重力支持
/// +物体联动
/// +爬墙
/// +游泳
/// </summary>
public class MovingSphereWithConnection : MonoBehaviour
{

    #region 参数设定

    #region 能力及环境要求

    /// <summary>
    /// 最大速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSpeed = 10f;

    /// <summary>
    /// 最大吸附速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSnapSpeed = 100f;

    /// <summary>
    /// 最大攀岩速度
    /// </summary>
    /// <remarks>
    /// 应该足够小
    /// </remarks>
    [SerializeField, Range(0f, 100f)]
    float m_maxClimbSpeed = 2f;

    /// <summary>
    /// 最大攀岩加速度
    /// </summary>
    /// <remarks>
    /// 应该足够大，基于攀岩时更好的控制
    /// </remarks>
    [SerializeField, Range(0f, 100f)]
    float m_maxClimbAcceleration = 20f;

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
    /// 最大跳跃高度
    /// </summary>
    [SerializeField, Range(0f, 10f)]
    float m_jumpHeight = 2f;

    /// <summary>
    /// 最大空中跳跃次数
    /// </summary>
    [SerializeField, Range(0, 5)]
    int m_maxAirJumps = 0;

    /// <summary>
    /// 最大地面坡角
    /// </summary>
    [SerializeField, Range(0f, 90f)]
    float m_maxGroundAngle = 25f;

    /// <summary>
    /// 最大楼梯坡角
    /// </summary>
    float m_maxStairAngle = 50f;

    /// <summary>
    /// 最大攀爬角度
    /// </summary>
    /// <remarks>
    /// Overhand岩壁(>90°)也可以算Climable
    /// </remarks>
    [SerializeField, Range(90, 180)]
    float m_maxClimbAngle = 140f;

    /// <summary>
    /// 飞跃吸附检测高度
    /// </summary>
    [SerializeField, Min(0f)]
    float m_probeDistance = 1f;

    /// <summary>
    /// 视作完全浸没的offset
    /// </summary>
    /// <remarks>
    /// 从球心开始向上移动距离作为完全浸没的位置
    /// 用于处理在没有完全浸没物体的时候视作完全浸没的需求（鼻子位置比头顶低）
    /// </remarks>
    float m_submergenceOffset = 0.5f;

    /// <summary>
    /// 整个浸没的最大距离
    /// </summary>
    /// <remarks>
    /// 从小球的“鼻子”（见submergenceOffset）到开始计算浸没的高度
    /// 用于处理有些情形水太浅以至于可以完全不视作浸没
    /// </remarks>
    float m_submergenceRange = 1f;

    /// <summary>
    /// 浮力率
    /// </summary>
    /// <remarks>
    /// 0表示完全不会受影响，1表示在水中完全浸没受力平衡，2表示在水中会向上和下落一般浮起
    /// </remarks>
    [SerializeField, Min(0f)]
    float m_buoyancy = 1f;

    /// <summary>
    /// 水阻
    /// </summary>
    /// <remarks>
    /// 对速度的减慢作用
    /// </remarks>
    [SerializeField, Range(0f, 10f)]
    float m_waterDrag = 1f;

    /// <summary>
    /// 划水门槛
    /// </summary>
    /// <remarks>
    /// 浸水率达到该比例方可游泳
    /// </remarks>
    [SerializeField, Range(0.01f, 1f)]
    float m_swimThreashold = 0.5f;

    /// <summary>
    /// 最大划水速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSwimSpeed = 5f;

    /// <summary>
    /// 最大划水加速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float m_maxSwimAcceleration = 5f;

    #endregion // 环境和玩家能力

    #region Layers
    /// <summary>
    /// 飞跃吸附检测layer
    /// </summary>
    [SerializeField]
    LayerMask m_probeMask = -1;

    /// <summary>
    /// 将某些layer的物体视作stair
    /// </summary>
    /// <remarks>
    /// 用于进行不同的移动判定
    /// </remakrs>
    [SerializeField]
    LayerMask m_stairMask = -1;

    /// <summary>
    /// 可攀爬物体的Mask
    /// </summary>
    /// <remarks>
    /// 用于跟物体的layer结合判断何物可攀爬
    /// </remarks>
    [SerializeField]
    LayerMask m_climbableMask = -1;

    /// <summary>
    /// 水的layer蒙版
    /// </summary>
    [SerializeField]
    LayerMask m_waterMask = 0; //? 为什么是0

    #endregion //Layers

    #region 外观设定

    /// <summary>
    /// 小球在空中时的旋转因数
    /// </summary>
    [SerializeField]
    float m_ballAirRotation = 0.5f;

    /// <summary>
    /// 小球在水中的旋转因数
    /// </summary>
    [SerializeField]
    float m_ballSwimRotation = 2f;

    /// <summary>
    /// 用于表现滚动的dummy ball
    /// </summary>
    [SerializeField]
    Transform m_ball = default;

    /// <summary>
    /// 用于表现滚动的ball半径
    /// </summary>
    [SerializeField]
    float m_ballRadius = 0.5f;

    /// <summary>
    /// 小球旋转时恢复立正的速度
    /// </summary>
    /// <remarks>
    /// （把它看成轮胎！）
    /// </remarks>
    [SerializeField]
    float m_ballAlignSpeed = 180f;

    /// <summary>
    /// 普通材质
    /// </summary>
    [SerializeField]
    Material m_normalMaterial = default;

    /// <summary>
    /// 攀岩材质
    /// </summary>
    [SerializeField]
    Material m_climbingMaterial = default;

    /// <summary>
    /// 游泳材质
    /// </summary>
    [SerializeField]
    Material m_swimmingMaterial = default;

    #endregion // 材质设定

    #endregion // 参数设定

    #region 游戏中各类数据记录

    #region 环境信息
    /// <summary>
    /// 连接物体的移动速度
    /// </summary>
    Vector3 m_connectionVelocity;

    /// <summary>
    /// 用于记录当前和小球联动的物体
    /// </summary>
    Rigidbody m_connectedBody;

    /// <summary>
    /// 用于记录先前和小球联动的物体
    /// </summary>
    /// <remarks>
    /// 用于处理小球的联动移动
    /// </remarks>
    Rigidbody m_previousConnectionBody;

    /// <summary>
    /// 联动点的世界坐标
    /// </summary>
    /// <remarks>
    /// 用于处理联动平移
    /// </remarks>
    Vector3 m_connectionWorldPosition;

    /// <summary>
    /// 联动物体的局部坐标
    /// </summary>
    /// <remarks>
    /// 用于处理联动旋转
    /// </remarks>
    Vector3 m_connectionLocalPosition;

    /// <summary>
    /// 重力反方向
    /// </summary>
    Vector3 m_upAxis;

    #endregion // 环境信息

    #region 输入状态

    /// <summary>
    /// 输入所在的参考系
    /// </summary>
    /// <remarks>
    /// 用于支持随视角处理输入
    /// </remarks>
    [SerializeField]
    Transform m_playerInputSpace = default;

    /// <summary>
    /// 记录玩家输入
    /// </summary>
    Vector3 m_playerInput;

    /// <summary>
    /// 是否意图跳跃
    /// </summary>
    bool m_desiredJump;

    /// <summary>
    /// 是否意图爬墙
    /// </summary>
    bool m_desiresClimbing;

    #endregion

    #region Agent信息

    /// <summary>
    /// 当前物体的rigidbody
    /// </summary>
    Rigidbody m_body;
    /// <summary>
    /// 用于存储当前物体的renderer
    /// </summary>
    MeshRenderer m_meshRenderer;

    /// <summary>
    /// 当前右方向
    /// </summary>
    Vector3 m_rightAxis;

    /// <summary>
    /// 当前前方向
    /// </summary>
    Vector3 m_forwardAxis;

    /// <summary>
    /// 当前速度
    /// </summary>
    Vector3 m_velocity;

    /// <summary>
    /// 浮空跳的当前阶段
    /// </summary>
    int m_jumpPhase;

    /// <summary>
    /// 自最后一次在地面后的steps（非跳跃）
    /// </summary>
    int m_stepsSinceLastGrounded;

    /// <summary>
    /// 自最后一次跳跃之后的steps
    /// </summary>
    int m_stepsSinceLastJump;

    /// <summary>
    /// 是否在水里
    /// </summary>
    /// <value>是否在水里</value>
    bool InWater => m_submergence > 0f;

    /// <summary>
    /// 淹水比例
    /// </summary>
    /// <remarks>
    /// 0和1分别代表没有和全部淹水
    /// </remarks>
    float m_submergence = 0f;

    /// <summary>
    /// 是否在游泳
    /// </summary>
    bool Swimming => m_submergence >= m_swimThreashold;

    #endregion

    #region 各种点乘结果，方便计算
    /// <summary>
    /// 上方向和地面单位法线的最小点乘结果，小于它意味着碰撞面不被视为地面
    /// </summary>
    float m_minGroundDotProduct;

    /// <summary>
    /// 上方向和楼梯单位法线的最小点乘结果，小于它意味着碰撞面不被视为楼梯，楼梯可移动角度会比地面大一些
    /// </summary>
    float m_minStairDotProduct;

    /// <summary>
    /// 最大爬坡角度法线和地面的点乘结果
    /// </summary>
    /// <remark>
    /// 用于爬坡时方便计算
    /// </remark>
    float m_minClimbDotProduct;
    #endregion //各种点乘结果，方便计算

    #region 各种接触面数量
    /// <summary>
    /// 接触地面的数量
    /// </summary>
    bool OnGround => m_groundContactCount > 0;

    /// <summary>
    /// 接触陡壁的数量
    /// </summary>
    bool OnSteep => m_steepContactCount > 0;

    /// <summary>
    /// 接触可攀爬对象的数量
    /// </summary>
    bool Climbing => m_climbContactCount > 0 && m_stepsSinceLastJump > 2; //? 为什么>2？

    /// <summary>
    /// 和地面接触的数量
    /// </summary>
    int m_groundContactCount;

    /// <summary>
    /// 和陡壁接触的数量
    /// </summary>
    /// <remarks>
    /// 用于不触地时脱困
    /// </remarks>
    int m_steepContactCount;

    /// <summary>
    /// 和可攀爬平面接触的数量
    /// </summary>
    int m_climbContactCount;

    #endregion // 各种接触面数量

    #region 各种Normals
    /// <summary>
    /// 当前地面/攀岩面触点的法线求和的方向
    /// </summary>
    Vector3 m_contactNormal;

    /// <summary>
    /// 接触面法线
    /// </summary>
    /// <remarks>
    /// 用于在Clear状态后仍然可以更新球旋转效果
    /// </remarks>
    Vector3 m_lastContactNormal;

    /// <summary>
    /// 当前陡壁的接触法线
    /// </summary>
    /// <remarks>
    /// 用于当没有和地面接触但和多个陡壁接触时的脱困
    /// </remarks>
    Vector3 m_steepNormal;

    /// <summary>
    /// 当前可攀爬平面的法线
    /// </summary>
    Vector3 m_climbNormal;

    /// <summary>
    /// 记录上次的攀爬法线
    /// </summary>
    /// <remarks>
    /// 做加法处理先前版本被缝隙夹住视作平地的情况
    /// </remarks>
    Vector3 m_lastClimbNormal;

    /// <summary>
    /// 上次陡壁法线
    /// </summary>
    Vector3 m_lastSteepNormal;
    #endregion //各种normal

    #endregion //游戏中各类数据记录

    /// <summary>
    /// 用于hot reload
    /// </summary>
    void OnValidate()
    {
        // 1. 根据各类最大爬坡角度计算法线相乘结果用于简化后面计算
        m_minGroundDotProduct = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad); // 不用真的和Vector3.up进行dot了，可以直接计算咧
        m_minStairDotProduct = Mathf.Cos(m_maxStairAngle * Mathf.Deg2Rad);
        m_minClimbDotProduct = Mathf.Cos(m_maxClimbAngle * Mathf.Deg2Rad);
    }

    /// <summary>
    /// 启用的时候被调用
    /// </summary>
    void Awake()
    {
        // 1. 记录body组件并对参数进行Validate
        m_body = GetComponent<Rigidbody>();
        m_body.useGravity = false;
        m_meshRenderer = m_ball.GetComponent<MeshRenderer>();
        OnValidate();
    }

    /// <summary>
    /// 每帧被调用
    /// </summary>
    void Update()
    {
        // 1. 记录输入
        m_playerInput.x = Input.GetAxis("Horizontal");
        m_playerInput.z = Input.GetAxis("Vertical");
        //todo 仅当游泳状态时记录游泳的上下移动
        m_playerInput.y = Swimming ? Input.GetAxis("UpDown") : 0f;
        m_playerInput = Vector3.ClampMagnitude(m_playerInput, 1f);
        // 2. 如果输入空间被设定
        if (m_playerInputSpace)
        {
            // 3. 将玩家输入映射到以重力反向为normal的平面
            m_rightAxis = ProjectDirectionOnPlane(m_playerInputSpace.right, m_upAxis);
            m_forwardAxis = ProjectDirectionOnPlane(m_playerInputSpace.forward, m_upAxis);
        }
        else
        {
            // 4. 设定为世界xz平面
            m_rightAxis = ProjectDirectionOnPlane(Vector3.right, m_upAxis);
            m_rightAxis = ProjectDirectionOnPlane(Vector3.forward, m_upAxis);
        }

        // 6. 记录跳跃和爬墙
        if (Swimming)
        {
            m_desiresClimbing = false;
        }
        else
        {
            m_desiredJump |= Input.GetButtonDown("Jump"); // 本来如果没有down那就是false，用了|=就是两者只要有一个是true就是true
            m_desiresClimbing = Input.GetButton("Climb");
        }

        // 7. 在小球在各种不同状态的时候更新小球材质
        // m_meshRenderer.material.SetColor(
        //     "_Color", OnGround ? Color.black : Color.white
        // );

        UpdateBall();
    }

    /// <summary>
    /// 按照固定时长被调用，在被推迟之后仍然会调用本应调用的次数
    /// </summary>
    void FixedUpdate()
    {
        // 1. 记录当前重力及反方向
        Vector3 gravity = CustomGravity.GetGravity(m_body.position, out m_upAxis);
        // 2. 更新当前物体物理状态为Update做准备
        UpdateState();

        //todo 如果在水中，则先添加水阻
        if (InWater)
        {
            // 根据浸没率和水阻、时间计算实际水阻对速度的影响
            m_velocity *= 1f - m_waterDrag * m_waterDrag * Time.deltaTime; // 阻力在此添加仍能保证之后允许加速 //? 但是一个绝对数量1和一个时间比例是如何ok的？
        }

        // 3. 更新速度
        AdjustVelocity();
        // 4. 如果之前(Update中)按下了跳跃键
        if (m_desiredJump)
        {
            // 5. 将“妄图跳跃”重置代表已经进行跳跃
            m_desiredJump = false;
            // 6. 进行跳跃
            Jump(gravity);
        }

        // todo 如果攀爬
        if (Climbing)
        {
            //todo 增加一项下压力，用于越过墙壁凸角
            m_velocity -= m_contactNormal * (m_maxClimbAcceleration * 0.9f * Time.deltaTime); // 0.9的原因是免得速度抵消被吸到内叫上
        }
        //todo 如果不攀爬则判断是否在水中（攀爬判定优先）
        else if (InWater)
        {
            //todo 根据浸没率及浮力率计算速度的修正
            m_velocity += gravity * ((1f - m_submergence * m_buoyancy) * Time.deltaTime);
        }
        //todo 如果不在水中则判断是否在地面（水中优先）
        else if (OnGround && m_velocity.sqrMagnitude < 0.01f)
        {
            //todo 增加一项沿斜面法线的下压力，使压力不至于因为这个branch而消失但也不会在很平缓的斜面上缓慢拉动个体
            m_velocity += m_contactNormal * Vector3.Dot(gravity, m_contactNormal) * Time.deltaTime;
        }
        //todo 如果在地面但同时妄图攀爬
        else if (m_desiresClimbing && OnGround)
        {
            //todo 增加一项与墙面凸角相关的压力，用于从Ground进入攀援
            m_velocity += (gravity - m_contactNormal * (m_maxClimbAcceleration * 0.9f)) * Time.deltaTime;
        }
        else
        {
            // 7. 在速度变化上叠加重力影响
            m_velocity += gravity * Time.deltaTime;
        }
        // 8. 更新rigidbody速度
        m_body.velocity = m_velocity;

        // 9. 清空为当前帧其他函数记录的状态
        ClearState();
    }

    /// <summary>
    /// 当两个物体碰撞刚检测到的时候调用
    /// </summary>
    /// <param name="collision">碰撞体</param>
    void OnCollisionEnter(Collision collision)
    {
        // 1. 检测碰撞状态
        EvaluateCollision(collision);
    }

    /// <summary>
    /// 当两个物体碰撞在进行的时候调用
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionStay(Collision collision)
    {
        // 1. 检测碰撞状态
        EvaluateCollision(collision);
    }

    /// <summary>
    /// 更新记录当前物体状态，如接触法线、跳跃计数、联动状态等
    /// </summary>
    /// <remarks>
    /// 之后要根据更新的状态设置速度等物理量
    /// </remarks>
    void UpdateState()
    {
        // 1. 更新计时
        m_stepsSinceLastGrounded += 1;
        m_stepsSinceLastJump += 1;
        m_velocity = m_body.velocity;
        // 2. 如果处于任何一种非浮空状态（order matters）
        if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts() || CheckSwimming())
        {
            // 3. 更新计时器和跳跃计数
            m_stepsSinceLastGrounded = 0;
            if (m_stepsSinceLastJump > 1) // 因为jump后第一帧还是会OnGround，如果这个时候再进行跳跃那就可以无限浮空跳
            {
                m_jumpPhase = 0;
            }
            // 4. 如果多个地面才进行和向量的标准化
            if (m_groundContactCount > 1)
            {
                m_contactNormal.Normalize();
            }
        }
        // 5. 如果处于浮空状态
        else
        {
            // 6. 每次Update状态的时候记得重置接触方向向上（用于浮空跳之类）
            m_contactNormal = m_upAxis;
        }
        //todo 如果存在联动物体，则更新联动物体状态
        if (m_connectedBody)
        {
            //todo 如果联动物体不会受运动学影响、够重才会影响当前物体
            if (m_connectedBody.isKinematic || m_connectedBody.mass >= m_body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    /// <summary>
    /// 清空状态
    /// 用于下次物理更新
    /// </summary>
    void ClearState()
    {
        //todo 在清空信息前记录接触面法线用于更新小球旋转
        m_lastContactNormal = m_contactNormal;
        m_lastSteepNormal = m_steepNormal;
        // 1. 每次fixedUpdate结束时将onGround等接触数据重置于false，在下次物理检测时再行更新
        m_groundContactCount = 0;
        m_steepContactCount = 0;
        m_climbContactCount = 0;
        m_contactNormal = Vector3.zero;
        m_steepNormal = Vector3.zero;
        m_connectionVelocity = Vector3.zero;
        m_climbNormal = Vector3.zero;
        //todo 更新前后连接对象
        m_previousConnectionBody = m_connectedBody;
        m_connectedBody = null;
        m_submergence = 0f;
    }

    /// <summary>
    /// 评估碰撞对象，在OnCollisionXXX时被调用
    /// </summary>
    /// <param name="collision">碰撞体</param>
    void EvaluateCollision(Collision collision)
    {
        //todo 如果物体处于游泳状态，则不进行后期评估，保持当前联动对象
        if (Swimming)
        {
            return;
        }
        //todo 存储物体的layer以备调用
        int layer = collision.gameObject.layer;
        // 1. 得到碰撞对象的layer
        float minDot = GetMinDot(layer);
        // 2. 遍历所有碰撞对象
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 3. 得到碰撞接触面法线
            Vector3 normal = collision.GetContact(i).normal;
            // 4. 如果坡度小于预设坡度，那么仍视作是地面
            // 计算当前接触面法线在重力方向投影
            float upDot = Vector3.Dot(m_upAxis, normal);
            if (upDot >= minDot)
            {
                // 5. 增加接触地面计数
                m_groundContactCount += 1;
                // 6. 更新地面接触法线
                m_contactNormal += normal;
                //todo 更新连接对象
                m_connectedBody = collision.rigidbody;
            }
            // 7. 否则应为陡壁或攀爬对象
            else
            {
                //todo 仍大体处于直立平面以内的都视为陡壁
                if (upDot > -0.01f)
                {
                    // 8. 增加陡壁计数
                    m_steepContactCount += 1;
                    // 9. 更新陡壁法线
                    m_steepNormal += normal;
                    //todo 如果没有和地面接触，则更新陡壁为连接
                    if (m_groundContactCount == 0)
                    {
                        m_connectedBody = collision.rigidbody;
                    }
                }
                //todo 判断是否可攀爬（按键+角度+layer）
                if (m_desiresClimbing && upDot > m_minClimbDotProduct && (m_climbableMask & (1 << layer)) != 0)
                {
                    m_climbContactCount += 1;
                    m_climbNormal += normal;
                    //todo 永远优先和Climb连接
                    m_previousConnectionBody = collision.rigidbody;
                }
            }
        }
    }

    /// <summary>
    /// 跳！
    /// </summary>
    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        // 1. 如果在（等效）地面上，地面法线方向跳跃
        if (OnGround)
        {
            jumpDirection = m_contactNormal;
        }
        // 2. 如果在墙上，墙跳
        else if (OnSteep)
        {
            jumpDirection = m_steepNormal;
            m_jumpPhase = 0;
        }
        // 3. 如果在Air，UpdateState中更新了接触法线是向上
        else if (m_maxAirJumps > 0 && m_jumpPhase <= m_maxAirJumps)
        {
            if (m_jumpPhase == 0) //? 处理从地面掉下的多余一次跳跃？相当于这样将掉落视作第一次浮空跳了，这就和其他的跳跃形式齐整了，否则就多一次
            {
                m_jumpPhase = 1;
            }
            jumpDirection = m_contactNormal;
        }
        // 4. 否则不满足跳跃条件返回
        else
        {
            return;
        }
        // 5. 重置上次跳跃之后的steps
        m_stepsSinceLastJump = 0;
        // 6. 记录已经进行的跳跃阶段
        m_jumpPhase += 1;
        // 7. 根据设定的跳跃高度计算跳跃速度
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * m_jumpHeight);
        //todo 根据陷入水中程度修正跳跃高度
        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0f, 1f - m_submergence / m_swimThreashold); // 因为submergence和swimT的关系会导致负值，所以使用Max
        }
        // 8. 根据坡角修正跳跃方向
        jumpDirection = (jumpDirection + m_upAxis).normalized;
        float alignedSpeed = Vector3.Dot(m_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        // 9. 原始速度叠加沿法线方向跳跃速度
        m_velocity += jumpDirection * jumpSpeed;
    }

    /// <summary>
    /// 根据当前平面调整速度使其沿任意有角度平面正常更新
    /// </summary>
    void AdjustVelocity()
    {
        //todo 设定“等待赋值”变量
        float acceleration, speed;
        Vector3 xAxis, zAxis;
        //todo 如果是攀爬，则构建zAxis（速度改变的世界相应方向）为上方向，左右方向则为沿岩壁横向
        if (Climbing)
        {
            acceleration = m_maxClimbAcceleration;
            speed = m_maxClimbSpeed;
            xAxis = Vector3.Cross(m_contactNormal, m_upAxis);
            zAxis = m_upAxis;
        }
        //todo 如果划水，则设定速度为划水系列速度。方向和陆上移动一致
        else if (InWater)
        {
            float swimFactor = Mathf.Min(1f, m_submergence / m_swimThreashold);
            acceleration = Mathf.LerpUnclamped(OnGround ? m_maxAcceleration : m_maxAirAcceleration, m_maxSwimAcceleration, swimFactor);
            speed = Mathf.LerpUnclamped(m_maxSpeed, m_maxSwimSpeed, swimFactor);
            xAxis = m_rightAxis;
            zAxis = m_forwardAxis;
        }
        else
        {
            acceleration = OnGround ? m_maxAcceleration : m_maxAirAcceleration;
            speed = OnGround && m_desiresClimbing ? m_maxClimbSpeed : m_maxSpeed;
            xAxis = m_rightAxis;
            zAxis = m_forwardAxis;
        }
        //todo 将对应方向投影到岩壁
        xAxis = ProjectDirectionOnPlane(xAxis, m_contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, m_contactNormal);

        // 2. 求出当前速度（已经沿接触面）沿运动轴上两个方向的分量
        //todo 应用当前物体和联动物体的相对速度(即控制速度)
        Vector3 relativeVelocity = m_velocity - m_connectionVelocity; //! 这里的m_velocity是全局的和速度！
        //todo 计算当前输入的预期速度和当前速度的差值并作为其修正值
        Vector3 adjustment;
        adjustment.x = m_playerInput.x * speed - Vector3.Dot(relativeVelocity, xAxis);
        adjustment.z = m_playerInput.z * speed - Vector3.Dot(relativeVelocity, zAxis);
        adjustment.y = Swimming ? m_playerInput.y * speed - Vector3.Dot(relativeVelocity, m_upAxis) : 0f;
        //todo 统一进行Clamp，而不是每个维度单独进行clamp
        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);
        // 5. 将当前速度沿先前计算的沿平面的方向进行分配，而非沿水平x、z进行分配
        m_velocity += xAxis * adjustment.x + zAxis * adjustment.z;

        //todo 最后加上游泳时的上下移动
        if (Swimming)
        {
            m_velocity += m_upAxis * adjustment.y; // 不使用直接修改Y为newY的原因是Vector3是struct
        }
    }

    /// <summary>
    /// 求得任意向量沿任意平面的方向
    /// </summary>
    /// <param name="vector">任意向量</param>
    /// <param name="normal">任意平面法线</param>
    /// <returns>沿当前平面的分量</returns>
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        // 1. 利用三角函数及向量运算求得任意向量沿当前接触平面的分量
        return (direction - normal * Vector3.Dot(direction, normal)).normalized; //! 点乘不满足交换律
    }

    /// <summary>
    /// 在越过地面尖角时令物体压在地面上，用于处理惯性飞出的情况
    /// </summary>
    /// <returns>返回是否需要进行移动吸附</returns>
    bool SnapToGround()
    {
        // 1. 如果已经跳起 || 刚刚预备跳跃而不是跳了好久了则不要snap
        if (m_stepsSinceLastGrounded > 1 || m_stepsSinceLastJump <= 2)
        {
            return false;
        }
        // 2. 如果开车已经超速了那飞出去算了
        float speed = m_velocity.magnitude;
        if (speed > m_maxSnapSpeed)
        {
            return false;
        }
        // 3. 如果下面没地面那么也不需要了
        if (!Physics.Raycast(m_body.position, -m_upAxis, out RaycastHit hit, m_probeDistance, m_probeMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        // 4. 如果下面的地面角度太陡那也不需要了
        // 计算出当前接触面法线在重力方向上的投影
        float upDot = Vector3.Dot(m_upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }
        // 5. 如果需要进行吸附，那么将地面计数设为1，并设置地面法线为当前hit的normal
        m_groundContactCount = 1; //? 此时需要将地面接触数量设定为1提示其他函数进行处理
        m_contactNormal = hit.normal;
        // 6. 速度沿地面法线上的分量
        float dot = Vector3.Dot(m_velocity, m_contactNormal);
        // 7. 如果向下坡方向移动，则求得速度沿斜面的方向的单位向量，并和速度绝对值相乘得到速度方向
        if (dot > 0f)
        {
            m_velocity = (m_velocity - m_contactNormal * dot).normalized * speed;
        }
        //todo 在折弯的时候也更新联动物体
        m_connectedBody = hit.rigidbody;
        return true;
    }

    /// <summary>
    /// 得到和地面的爬坡角度
    /// </summary>
    /// <param name="layer">当前物体所处layer</param>
    /// <returns>最大爬坡角度的点乘值</returns>
    float GetMinDot(int layer)
    {
        // 1. 判断是地面还是阶梯并返回不同爬坡角度
        return (m_stairMask & (1 << layer)) == 0 ?
            m_minGroundDotProduct : m_minStairDotProduct; // 注意==和&的优先级
    }

    /// <summary>
    /// 检查陡壁接触状态
    /// </summary>
    /// <returns>是否陡壁稳定</returns>
    bool CheckSteepContacts()
    {
        // 1. 判断陡壁接触数量，多于一次陡壁接触（夹住时）方可视为稳定地面
        if (m_steepContactCount > 1)
        {
            m_steepNormal.Normalize();
            // 2. 如果等效地面坡度小于最小可用地面坡度
            float upDot = Vector3.Dot(m_upAxis, m_steepNormal);
            if (upDot >= m_minGroundDotProduct)
            {
                // 3. 视作地面接触，并修改地面接触计数（结合Jump的优先判断Ground可以优先以等效ground进行处理）
                m_groundContactCount = 1;
                // 4. 修改地面接触法线
                m_contactNormal = m_steepNormal;
                // 5. 返回接触成功
                return true;
            }
        }
        // 6. 返回接触失败
        return false;
    }

    /// <summary>
    /// 根据是否Swimming更新信息
    /// </summary>
    /// <remarks>
    /// 包含接触信息等
    /// </remarks>
    /// <returns>是否在游泳</returns>
    bool CheckSwimming()
    {
        //todo 如果当前处于游泳状态
        if (Swimming)
        {
            //todo 更新接触信息
            m_groundContactCount = 0;
            m_contactNormal = m_upAxis;
            return true;
        }
        //todo 如果并非游泳则返回false
        return false;

    }

    /// <summary>
    ///? 检查是否处于攀爬状态？不是很懂
    /// </summary>
    /// <returns></returns>
    bool CheckClimbing()
    {
        //todo 如果是攀爬状态则将ground相关设定设为一致？
        if (Climbing)
        {
            if (m_climbContactCount > 1)
            {
                m_climbNormal.Normalize();
                float upDot = Vector3.Dot(m_upAxis, m_climbNormal);
                if (upDot >= m_minGroundDotProduct)
                {
                    m_climbNormal = m_lastClimbNormal;
                }
            }
            //? 不是很懂
            m_groundContactCount = m_climbContactCount;
            m_contactNormal = m_climbNormal; //! 将墙壁视作地面了（移动逻辑变了，因为所有的速度都依赖于全局的contactNormal（沿平面））但是重力没有变（视角没变）
            return true;
        }
        return false;
    }

    /// <summary>
    /// 更新连接物体、连接点状态
    /// </summary>
    void UpdateConnectionState()
    {
        //todo 如果当前联动物体和先前联动物体一致
        if (m_connectedBody == m_previousConnectionBody)
        {
            //todo 更新连接点速度
            Vector3 connectionMovement = m_connectedBody.transform.TransformPoint(m_connectionLocalPosition) - m_connectionWorldPosition;
            m_connectionVelocity = connectionMovement / Time.deltaTime;
        }
        //todo 更新连接点的世界坐标，用于替代connectionBody的坐标（因为已经连上了就直接用本体坐标省的计算了）
        m_connectionWorldPosition = m_body.position;
        //todo 更新连接点的局部坐标
        m_connectionLocalPosition = m_connectedBody.transform.InverseTransformPoint(
            m_connectionWorldPosition
        ); // InverseTransformPoint将点从世界坐标到局部坐标x
    }

    /// <summary>
    /// 在进入触发器的时候调用
    /// </summary>
    /// <param name="other">被进入的触发器</param>
    void OnTriggerEnter(Collider other)
    {
        //todo 如果trigger对象是水
        if ((m_waterMask & (1 << other.gameObject.layer)) != 0)
        {
            //todo 则设置InWater
            EvaluateSubmergence(other);
        }
    }

    /// <summary>
    /// 在触发器中的时候调用
    /// </summary>
    /// <param name="other">被离开的触发器</param>
    void OnTriggerStay(Collider other)
    {
        //todo 如果trigger对象是水
        if ((m_waterMask & (1 << other.gameObject.layer)) != 0)
        {
            //todo 设置InWater
            EvaluateSubmergence(other);
        }
    }

    /// <summary>
    /// 评估是否淹水
    /// </summary>
    void EvaluateSubmergence(Collider collider)
    {
        //todo 如果射线检测到水体
        if (Physics.Raycast(m_body.position + m_submergenceOffset * m_upAxis,
        -m_upAxis,
        out RaycastHit hit,
        m_submergenceRange + 1f,
        m_waterMask,
        QueryTriggerInteraction.Collide
        ))
        {
            //todo 计算浸没率
            m_submergence = 1 - hit.distance / m_submergenceRange;
        }
        else
        {
            //todo 处理浸没hit在极值的时候办法精确反应的情况
            m_submergence = 1;
        }
        //todo 如果游泳的时候，把水体作为联动物体
        if (Swimming)
        {
            m_connectedBody = collider.attachedRigidbody;
        }
    }

    /// <summary>
    /// 避免对地面的吸附
    /// </summary>
    /// <remarks>
    /// 用于JumpPad等对其进行调用
    /// </remarks>
    public void PreventSnapToGround()
    {
        //todo 设定从上次跳跃开始的steps是-1
        m_stepsSinceLastJump = -1; //! 一个trick，使SnapToGround视其为“刚刚”跳跃（数值很小，但又不是正常数值）
    }

    /// <summary>
    /// 更新附属子ball的状态
    /// </summary>
    void UpdateBall()
    {
        //todo 根据状态更新小球材质信息
        Material ballMaterial = m_normalMaterial;
        Vector3 rotationPlaneNormal = m_lastContactNormal;
        //todo 应用小球在不同环境中的旋转因数
        float m_rotationFactor = 1f;
        if (Climbing)
        {
            ballMaterial = m_climbingMaterial;
        }
        else if (Swimming)
        {
            m_rotationFactor = m_ballSwimRotation;
            ballMaterial = m_swimmingMaterial;
        }
        //todo 如果并非在地面，则使用上次的陡壁的法线
        else if (!OnGround)
        {
            if (OnSteep)
            {
                m_lastContactNormal = m_lastSteepNormal;
            }
            else
            {
                m_rotationFactor = m_ballAirRotation;
            }
        }
        m_meshRenderer.material = ballMaterial;

        //todo 根据速度计算移动距离
        Vector3 movement = m_body.velocity * Time.deltaTime;
        //todo 原始movement减掉在平面法线方向上的分量
        movement -= rotationPlaneNormal * Vector3.Dot(movement, rotationPlaneNormal);
        float distance = movement.magnitude;

        Quaternion rotation = m_ball.localRotation;
        //todo 如果当前联动物体仍然是先前连动物体
        if (m_connectedBody && m_connectedBody == m_previousConnectionBody)
        {
            //todo 计算当前物体的旋转
            rotation = Quaternion.Euler(
                m_connectedBody.angularVelocity * (Mathf.Rad2Deg * Time.deltaTime)
            ) * rotation;
            //todo 仅在本体移动距离非常近的时候更新联动旋转
            if (distance < 0.001f)
            {
                m_ball.localRotation = rotation;
                return;
            }
        }
        else if (distance < 0.001f)
        {
            return;
        }
        //todo 根据移动距离计算旋转角度(应用旋转因数)
        float angle = distance * m_rotationFactor * (180 / Mathf.PI) / m_ballRadius;
        //todo 根据接触法线移动方向更新旋转轴
        Vector3 rotationAxis = Vector3.Cross(rotationPlaneNormal, movement).normalized;
        //todo 根据角度更新小球移动旋转
        rotation = Quaternion.Euler(rotationAxis * angle) * rotation;
        //todo 
        if (m_ballAlignSpeed > 0f)
        {
            rotation = AlignBallRotation(rotationAxis, rotation, distance);
        }
        m_ball.localRotation = rotation;
    }

    /// <summary>
    ///? 矫正当前小球的旋转方向
    /// </summary>
    /// <param name="rotationAxis"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    Quaternion AlignBallRotation(Vector3 rotationAxis, Quaternion rotation, float traveledDistance)
    {
        // 1. 得到小球的上方向
        Vector3 ballAxis = m_ball.up; // 由当前重力纵轴确定
        // 2. 得到旋转的角度值
        float dot = Mathf.Clamp(Vector3.Dot(ballAxis, rotationAxis), -1f, 1f); // 精度问题可能会超出有效范围，造成非预期后果所以clamp之
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        // 3. 计算当前帧最大旋转矫正速度
        float maxAngle = m_ballAlignSpeed * traveledDistance;

        // 4。由两个纵轴插值和起始旋转求得目标旋转
        Quaternion newAlignment = Quaternion.FromToRotation(ballAxis, rotationAxis) * rotation;

        // 5. 如果所需旋转角不大于旋转速度，则不进行旋转
        if (angle <= maxAngle)
        {
            return newAlignment;
        }
        // 6. 如果所需旋转角大于旋转速度，则进行旋转
        else
        {
            return Quaternion.SlerpUnclamped(rotation, newAlignment, maxAngle / angle); // 由于插值比例保证合理，所以“Unclamped”
        }
    }
}
