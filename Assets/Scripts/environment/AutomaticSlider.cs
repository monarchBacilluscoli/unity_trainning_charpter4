using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于控制滑动的物体
/// </summary>
/// <remarks>
/// 包含来回滑动物体的插值比例的更新及其更新事件
/// </remarks>
public class AutomaticSlider : MonoBehaviour
{

    /// <summary>
    /// 简单将泛型类型变为具体类型用于Unity的序列化
    /// </summary>
    [System.Serializable]
    public class OnValueChangedEvent : UnityEvent<float> { }

    #region 设置参数

    /// <summary>
    /// 移动一次所需的时间
    /// </summary>
    [SerializeField, Min(0.01f)]
    float m_duration = 1f;

    /// <summary>
    /// 当插值比例更新时触发事件
    /// </summary>
    [SerializeField]
    OnValueChangedEvent m_onValueChanged = default;

    [SerializeField]
    bool m_autoReverse = false;

    /// <summary>
    /// 设定是否开启自动反向
    /// </summary>
    /// <value>是否开启自动反向</value>
    public bool AutoReverse
    {
        get => m_autoReverse;
        set => m_autoReverse = value;
    }

    /// <summary>
    /// 是否开启平滑移动
    /// </summary>
    [SerializeField]
    bool m_smoothStep = false;

    #endregion //设置参数

    #region 中间变量

    /// <summary>
    /// 平滑移动插值比例
    /// </summary>
    float SmoothedValue => 3f * m_value * m_value - 2f * m_value * m_value * m_value;

    /// <summary>
    /// 表征两点移动的插值位置
    /// </summary>
    /// <remarks>
    /// 0-1
    /// </remarks>
    float m_value;

    /// <summary>
    /// 当前是否在进行反向移动
    /// </summary>
    public bool m_reversed
    {
        get;
        set;
    }

    #endregion // 中间变量

    /// <summary>
    /// 固定时长调用
    /// </summary>
    void FixedUpdate()
    {
        //todo 更新流失时间到插值比例
        float delta = Time.deltaTime / m_duration;
        //todo 如果当前为reversed则反向移动
        if (m_reversed)
        {
            m_value -= delta;
            //todo 判断值是否有效，否则重设
            if (m_value <= 0f)
            {
                if (m_autoReverse)
                {
                    m_value = Mathf.Min(1f, -m_value); // 反向时位置仍有可能一下大于1，所以要取最小值
                    m_reversed = false;
                }
                else
                {
                    //todo 如果不进行自动反向，则插值比例定在0位并无效化该脚本
                    m_value = 0f;
                    enabled = false;
                }
            }
        }
        //todo 如果当前还在正向移动
        else
        {
            //todo 更新插值比例
            m_value += delta;
            //todo 如果插值比例超过1，则置于1
            if (m_value >= 1f)
            {
                //todo 如果反向移动
                if (m_autoReverse)
                {
                    m_value = Mathf.Max(0f, 2f - m_value);
                    m_reversed = true;
                }
                else
                {
                    m_value = 1f;
                    //todo 将当前脚本设为false
                    enabled = false;
                }
            }
        }
        //todo 如果设置了平滑移动则进行平滑插值
        m_onValueChanged.Invoke(m_smoothStep ? SmoothedValue : m_value);
    }
}
