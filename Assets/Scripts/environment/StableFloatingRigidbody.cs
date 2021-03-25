using UnityEngine;

/// <summary>
/// 为自定义重力设定的Rigidbody类
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class StableFloatingRigidbody : MonoBehaviour
{
    #region 设置参数
    /// <summary>
    /// 完全淹没位置
    /// </summary>
    /// <remarks>
    /// 相对于物体中心向上
    /// </remarks>
    float m_submergenceOffset = 0.5f;

    /// <summary>
    /// 淹没范围
    /// </summary>
    [SerializeField]
    float m_submergenceRange = 1f;

    /// <summary>
    /// 浮力率（相对于重力的倍数）
    /// </summary>
    [SerializeField, Min(0.1f)]
    float m_buoyancy = 1f;

    /// <summary>
    /// 浮力作用点
    /// </summary>
    /// <remarks>
    /// 并非是比例..比例确实不合适因为物体形状朝向多种多样可能会..那也无非是呃 确实不好弄
    /// </remarks>
    [SerializeField]
    Vector3[] m_buoyancyOffsets = default;

    /// <summary>
    /// 水阻
    /// </summary>
    [SerializeField, Range(0f, 10f)]
    float m_waterDrag = 1f;

    /// <summary>
    /// 这一开关保证几个点的浮力会在这些浮力点淹没在水中的时候才进行计算
    /// </summary>
    /// <remarks>
    /// 但由于需要更多的CheckSphere，对性能会有所损耗，故当仅有大块物体处于浮动状态时才进行
    /// </remarks>
    [SerializeField]
    bool safeFloating = false;

    /// <summary>
    /// 水layer
    /// </summary>
    [SerializeField]
    LayerMask m_waterMask = 0;

    [SerializeField]
    Color m_sleepColor = new Color(0.1f, 0.1f, 0.1f);


    #endregion // 设置参数

    #region 中间数据

    /// <summary>
    /// 当前物体刚体
    /// </summary>
    Rigidbody m_body;

    /// <summary>
    /// 初始颜色记录
    /// </summary>
    Color m_initialColor;

    /// <summary>
    /// 记录静止时长，用于处理物理沉睡
    /// </summary>
    float m_floatDelay;

    /// <summary>
    /// 不同浮力点的浸入比例
    /// </summary>
    float[] m_submergence;

    /// <summary>
    /// 所受重力
    /// </summary>
    Vector3 m_gravity;

    #endregion

    /// <summary>
    /// 初始化时调用
    /// </summary>
    void Awake()
    {
        // 1. 记录起始颜色
        m_initialColor = GetComponent<MeshRenderer>().material.GetColor("_Color");
        // 2. 取消应用原生重力
        m_body = GetComponent<Rigidbody>();
        m_body.useGravity = false;
        m_submergence = new float[m_buoyancyOffsets.Length];
    }

    /// <summary>
    /// 定时更新
    /// </summary>
    void FixedUpdate()
    {
        // 1. 检查如果物体受力平衡，则直接返回不要打扰
        if (m_body.IsSleeping())
        {
            m_floatDelay = 0f;
            return;
        }
        // 2. 但是如果前一帧AddForce，则不会进入Sleep()状态，所以首先通过检查速度来决定是否AddForce
        if (m_body.velocity.sqrMagnitude < 0.0001f)
        {
            m_floatDelay += Time.deltaTime;
            // 3. 当物体已经静止超过一段时间后，视为静止
            if (m_floatDelay >= 1f)
            {
                //4. 设定休眠颜色并直接返回
                GetComponent<MeshRenderer>().material.SetColor("_Color", m_sleepColor);
                return;
            }
        }
        // 5. 否则设置当前为非休眠颜色
        GetComponent<MeshRenderer>().material.SetColor("_Color", m_initialColor);
        //todo 得到当前位置的重力以及浸入度，并处理受力
        m_gravity = CustomGravity.GetGravity(m_body.position);
        //todo 计算每个受力点的分力(这里所有的受力都需要被不同的受力点分散)
        float dragFactor = m_waterDrag * Time.deltaTime / m_buoyancyOffsets.Length;
        float buoyancyFactor = -m_buoyancy / m_buoyancyOffsets.Length;
        for (int i = 0; i < m_buoyancyOffsets.Length; i++)
        {
            if (m_submergence[i] > 0f)
            {
                //todo 从水阻入手计算速度的比例系数（水阻对速度的阻碍程度）
                float drag = Mathf.Max(0f, 1f - m_waterDrag * m_submergence[i] * Time.deltaTime);
                //todo 直接*速度
                m_body.velocity *= drag;
                m_body.angularVelocity *= drag;
                //todo 在浮力应用点（是全局位置）上应用浮力
                m_body.AddForceAtPosition(
                    m_gravity * (buoyancyFactor * m_submergence[i]),
                    transform.TransformPoint(m_buoyancyOffsets[i]),
                    ForceMode.Acceleration
                );
                m_submergence[i] = 0f; //? 这个直接设置为0不妥了？
            }
        }
        m_body.AddForce(m_gravity, ForceMode.Acceleration);
    }

    /// <summary>
    /// 进入触发器则执行
    /// </summary>
    /// <param name="other">被碰撞的碰撞体</param>
    void OnTriggerEnter(Collider other)
    {
        //todo 如果进入水中，计算浸润率 
        if ((m_waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    /// <summary>
    /// 在触发器中时执行
    /// </summary>
    /// <param name="other">被碰撞的碰撞体</param>
    void OnTriggerStay(Collider other)
    {
        //todo 如果待在水中，更新浸润率
        if (!m_body.IsSleeping() && (m_waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    /// <summary>
    /// 评估所有浮力点的浸润率
    /// </summary>
    /// <remarks>
    ///? 或许这个浸润检测起点可以从头顶以上检测，长度长一点保证上下均可穿过这样就不会错失hit？
    /// </remarks>
    void EvaluateSubmergence()
    {
        //todo 找到重力方向的反方向
        Vector3 down = m_gravity.normalized;
        Vector3 offset = down * -m_submergenceOffset;
        //todo 沿着重力反方向的射线检测water，并进行浸润率更新
        for (int i = 0; i < m_buoyancyOffsets.Length; i++)
        {
            //todo 计算浮力射线检测的起点
            Vector3 p = offset + transform.TransformPoint(m_buoyancyOffsets[i]);
            if (Physics.Raycast(p, down,
                        out RaycastHit hit,
                        m_submergenceRange + 1f,
                        m_waterMask,
                        QueryTriggerInteraction.Collide))
            {
                m_submergence[i] = 1f - hit.distance / m_submergenceRange;
            }
            //todo 如果当前无法hit到（因为从起点已全部进入），则直接更新为1（函数调用之前需要保证与水碰撞保证了这一设定的正确性）
            else
            {
                //todo 由于多个受力点时存在部分受力点被举起的情况，所以需要在设定完全浸润的时候进行球检测，确保真正完全浸润
                if (!safeFloating || Physics.CheckSphere(p, 0.01f, m_waterMask, QueryTriggerInteraction.Collide))
                {
                    m_submergence[i] = 1f;
                }
            }
        }
    }
}
