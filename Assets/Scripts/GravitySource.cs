using UnityEngine;

/// <summary>
/// 重力源的基类
/// </summary>
public class GravitySource : MonoBehaviour
{
    /// <summary>
    /// 获得当前重力源影响下目标点的重力
    /// </summary>
    /// <param name="position">目标点</param>
    /// <returns>重力</returns>
    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }

    /// <summary>
    /// 当该物体可用时调用
    /// </summary>
    void OnEnable()
    {
        // 将该重力源注册至Gravity的处理类
        CustomGravity.Register(this);
    }

    /// <summary>
    /// 当该物体不可用时调用
    /// </summary>
    void OnDisabled()
    {
        // 去掉重力源的注册
        CustomGravity.Unregister(this);
    }
}
