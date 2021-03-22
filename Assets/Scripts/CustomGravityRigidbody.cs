using UnityEngine;

/// <summary>
/// 为自定义重力设定的Rigidbody类
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    Rigidbody m_body;

    Color m_initialColor;

    [SerializeField]
    Color m_sleepColor = new Color(1f, 1f, 1f);

    /// <summary>
    /// 记录静止时长，用于处理物理沉睡
    /// </summary>
    float m_floatDelay;

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
        // 5. 否则设置当前为非休眠颜色并施加重力
        GetComponent<MeshRenderer>().material.SetColor("_Color", m_initialColor);
        m_body.AddForce(CustomGravity.GetGravity(m_body.position), ForceMode.Acceleration);
    }
}
