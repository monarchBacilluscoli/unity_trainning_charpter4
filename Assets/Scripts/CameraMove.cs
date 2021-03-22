using UnityEngine;

/// <summary>
/// 简单处理镜头跟随的逻辑
/// </summary>
public class CameraMove : MonoBehaviour
{
    /// <summary>
    /// 跟随目标
    /// </summary>
    public Transform m_target;

    /// <summary>
    /// 起始offset，在移动中固定
    /// </summary>
    Vector3 m_offset;

    /// <summary>
    /// Update前调用
    /// 记录和目标的offset
    /// </summary>
    void Start()
    {
        m_offset = transform.position - m_target.position;
    }

    /// <summary>
    /// 逐帧调用
    /// 更新相机位置
    /// </summary>
    void Update()
    {
        transform.position = m_target.position + m_offset;
    }
}
