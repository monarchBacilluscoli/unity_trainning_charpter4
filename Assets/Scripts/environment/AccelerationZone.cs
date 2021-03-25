using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    /// <summary>
    /// 对其上物体施加的速度
    /// </summary>
    [SerializeField, Min(0f)]
    float m_speed = 10f;

    /// <summary>
    /// 当其他物体进入当前物体触发器时被调用
    /// </summary>
    /// <param name="other">其他碰撞体</param>
    void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(other.attachedRigidbody);
        }
    }

    /// <summary>
    /// 应用速度
    /// </summary>
    /// <param name="body">接触对象的刚体</param>
    void Accelerate(Rigidbody body)
    {
        Vector3 velocity = body.velocity;
        //todo 如果当前物体速度大于应用速度则不加速
        if (velocity.y > m_speed)
        {
            return;
        }
        //todo 应用速度于上方向
        velocity.y = m_speed;
        body.velocity = velocity;
    }
}
