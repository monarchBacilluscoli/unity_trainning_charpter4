using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    /// <summary>
    /// 对其上物体施加的速度
    /// </summary>
    [SerializeField, Min(0f)]
    float m_speed = 10f;

    /// <summary>
    /// 对其上物体施加的加速度
    /// </summary>
    [SerializeField, Min(0f)]
    float m_acceleration = 10f;

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

    void OnTriggerStay(Collider other)
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
        //todo 将速度转换为物体的局部速度，从而应用速度方向为物体的局部速度方向，可以控制最终加速的方向与物体朝向相关
        Vector3 velocity = transform.InverseTransformDirection(body.velocity); // 注意是Direction，这样与物体尺寸就没有关系了（否则Point就会按比例缩放）
        //todo 如果当前物体速度大于应用速度则不加速
        if (velocity.y > m_speed)
        {
            return;
        }
        //todo 判断是否有加速度从而应用立即加速或逐渐加速
        if (m_acceleration > 0f)
        {
            //todo 进行逐步加速
            velocity.y = Mathf.MoveTowards(velocity.y, m_speed, m_acceleration * Time.deltaTime);
        }
        else
        {
            //todo 进行立即加速
            velocity.y = m_speed;
        }
        body.velocity = transform.TransformDirection(velocity);
        if (body.TryGetComponent(out MovingSphereWithConnection sphere))
        {
            sphere.PreventSnapToGround();
        }
    }
}
