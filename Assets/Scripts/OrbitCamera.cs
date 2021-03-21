using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // 表明这个实例需要一个camera    
public class OrbitCamera : MonoBehaviour
{

    #region 设定参数
    /// <summary>
    /// 目标位置
    /// </summary>
    [SerializeField]
    Transform m_focus;

    /// <summary>
    /// 和焦点距离
    /// </summary>
    [SerializeField]
    float m_distatnce = 5f;

    [SerializeField]
    float m_focusRadius = 3f;

    [SerializeField]
    float m_focusCentering = 0.5f;

    [SerializeField, Range(1f, 360f)]
    float m_rotationSpeed = 90f;

    /// <summary>
    /// 纵向限定旋转角度
    /// </summary>
    [SerializeField, Range(-89f, 89f)]
    float m_minVerticalAngle = -30f, m_maxVerticalAngle = 60f;

    /// <summary>
    /// 镜头自动跟随延迟
    /// 在设定时间没有手动移动镜头后自动跟踪
    /// </summary>
    [SerializeField, Min(0f)]
    float m_alignDelay = 5f;

    /// <summary>
    /// 比例增减的朝向变换范围(<)
    /// </summary>
    [SerializeField, Range(0, 90f)]
    float m_alignSmoothRange = 45f;

    /// <summary>
    /// 不被摄像机绕过的物体
    /// </summary>
    [SerializeField]
    LayerMask obstructionMask = -1;

    #endregion // 设定参数

    #region 中间数据

    Camera m_regularCamera;

    /// <summary>
    /// 相机焦点位置
    /// </summary>
    Vector3 m_focusPoint;

    /// <summary>
    /// 上一帧的焦点位置
    /// </summary>
    Vector3 m_previousFocusPoint;

    /// <summary>
    /// 环绕角度
    /// 只需要？难道不是只需要一个方向？哦，两个方向，摄像机一般不进行roll
    /// </summary>
    Vector2 m_orbitAngles = new Vector2(45f, 0f);

    /// <summary>
    /// 从上次手动镜头操作之后的时间
    /// 用于自动设定镜头
    /// </summary>
    float m_lastManualRotationTime;

    #endregion // 中间数据

    #region properties

    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = m_regularCamera.nearClipPlane * Mathf.Tan(m_regularCamera.fieldOfView * 0.5f * Mathf.Deg2Rad); // nearClipPlane是到这个plane的distance, point of view是这个plane的y轴角度
            halfExtends.x = halfExtends.y * m_regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    #endregion //properties

    void Awake()
    {
        m_regularCamera = GetComponent<Camera>();
        m_focusPoint = m_focus.position;
        // 初始化当前相机旋转
        // 否则LateUpdate在有输入的情况下才会更新，会造成旋转突变
        transform.localRotation = Quaternion.Euler(m_orbitAngles);
    }

    void OnValidate()
    {
        if (m_maxVerticalAngle < m_minVerticalAngle)
        {
            m_maxVerticalAngle = m_minVerticalAngle;
        }
    }

    void LateUpdate()
    {
        // 更新当前跟随目标
        UpdateFocusPoint();
        // 得到输入旋转
        Quaternion lookRotation;
        // 只有在输入/自动调整有效的情况下
        if (ManualRotation() || AutomaticRotation())
        {
            // 限定输入旋转角度
            ConstrainAngles();
            // 进行旋转计算
            lookRotation = Quaternion.Euler(m_orbitAngles);
        }
        else
        {
            // 将旋转仍置于当前状态
            lookRotation = transform.localRotation; // 虽然多了一次赋值但是结构更为简明
        }
        // 得到当前旋转quaternion并更新相机位置和旋转
        Vector3 lookDirection = lookRotation * Vector3.forward; //! 注意forward在transform里...
        Vector3 lookPosition = m_focusPoint - lookDirection * m_distatnce;

        Vector3 rectOffset = lookDirection * m_regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = m_focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        // 处理相机和focus之间的遮挡
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask)) // box本身不包含方向
        {
            lookPosition = m_focusPoint - lookDirection * (hit.distance + m_regularCamera.nearClipPlane);
        }
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        m_previousFocusPoint = m_focusPoint;
        Vector3 targetPoint = m_focus.position;
        // 如果设定了焦点半径
        if (m_focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, m_focusPoint);
            // 如果目标跑出当前焦点半径外
            if (distance > m_focusRadius)
            {
                // 更新摄像机焦点至目标边缘
                m_focusPoint = Vector3.Lerp(targetPoint, m_focusPoint, m_focusRadius / distance);
            }
            // 如果目标并没有到半径外，但半径内处于拉动区域
            if (distance > 0.01f && m_focusCentering > 0f) // 设定平滑拉动阈值（0.01f），否则会接近无限插值
            {
                m_focusPoint = Vector3.Lerp(
                    targetPoint, m_focusPoint,
                    Mathf.Pow(1f - m_focusCentering, Time.unscaledDeltaTime)
                );
            }
        }
        // 否则设置紧随
        else
        {
            m_focusPoint = targetPoint;
        }
    }

    bool ManualRotation()
    {
        // 获得输入
        Vector2 input = new Vector2(
            Input.GetAxis("Vertical Camera"),
            Input.GetAxis("Horizontal Camera"));
        // 根据输入设定rotation角度，仅考虑大于某个阈值的输入
        const float e = 0.001f;
        if (input.x > e || input.x < -e || input.y > e || input.y < -e)
        {
            m_orbitAngles += m_rotationSpeed * Time.unscaledDeltaTime * input;
            m_lastManualRotationTime = Time.unscaledTime; // 这次没有delta
            return true;
        }
        return false;
    }

    void ConstrainAngles()
    {
        // 限定旋转角度到上下界
        m_orbitAngles.x = Mathf.Clamp(m_orbitAngles.x, m_minVerticalAngle, m_maxVerticalAngle);
        // 保证限定后的角度在0-360范围之内，否则可能会发生正转一度最后反转359度的情况估计
        if (m_orbitAngles.y < 0f)
        {
            m_orbitAngles.y += 360f;
        }
        if (m_orbitAngles.y >= 360f)
        {
            m_orbitAngles.y -= 360f;
        }
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - m_lastManualRotationTime < m_alignDelay)
        {
            return false;
        }
        // 更新上次位移
        Vector2 movement = new Vector2(
            m_focusPoint.x - m_previousFocusPoint.x,
            m_focusPoint.z - m_previousFocusPoint.z
        );
        Debug.Log(movement);
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }
        // 得到移动的前方向角度
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        // 计算视野前方和移动角度之间的最小差异
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(m_orbitAngles.y, headingAngle));
        // 将小的移动差异以更小的速度来处理旋转
        float rotationChange = m_rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        // 将小的角度变动以更小的速度处理掉
        if (deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= deltaAbs / m_alignSmoothRange;
        }
        // 在回转180度刚开始的时候以一个较小小的旋转速度处理（因为这其实也算是一个小的移动有时）
        else if (180f - deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / m_alignSmoothRange;
        }
        // 设置旋转角度
        m_orbitAngles.y = Mathf.MoveTowardsAngle(m_orbitAngles.y, headingAngle, rotationChange); //MoveTowardsAngle指从1到2以不超过3的方式移动
        return true;
    }

    /// <summary>
    /// 计算二维移动的单位方向和y轴正方向的夹角
    /// </summary>
    /// <param name="direction">方向</param>
    /// <returns></returns>
    static float GetAngle(Vector2 direction)
    {
        // 根据反三角函数计算夹角的度数
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
}
