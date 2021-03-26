using UnityEngine;

public class PositionInterpolator : MonoBehaviour
{
    #region 设置参数

    /// <summary>
    /// 移动物体的rigidbody
    /// </summary>
    [SerializeField]
    Rigidbody m_body = default;

    /// <summary>
    /// 移动开始位置
    /// </summary>
    [SerializeField]
    Vector3 from = default;

    /// <summary>
    /// 移动目标位置
    /// </summary>
    [SerializeField]
    Vector3 to = default;

    /// <summary>
    /// 是否相对于某处进行移动
    /// </summary>
    /// <remarks>
    /// 用于物体重用
    /// </remarks>
    [SerializeField]
    Transform m_relativeTo = default;

    #endregion // 设置参数

    /// <summary>
    /// 向from和to间比例为t的位置进行移动
    /// </summary>
    /// <param name="t">距离起点比例为t的位置</param>
    public void Interpolate(float t)
    {
        Vector3 p;
        //todo 如果使用了相对位置
        if (m_relativeTo)
        {
            //todo 对以相对位置作为坐标原点的From和To位置进行插值移动
            p = Vector3.LerpUnclamped(m_relativeTo.TransformPoint(from), m_relativeTo.TransformPoint(to), t);
        }
        else
        {
            //todo 如果并非使用相对位置
            //todo 以世界位置作为FromTo位置
            p = Vector3.LerpUnclamped(from, to, t);
        }
        m_body.MovePosition(p);
    }
}
