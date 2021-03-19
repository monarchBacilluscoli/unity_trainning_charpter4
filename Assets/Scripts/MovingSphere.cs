using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    readonly float m_maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    readonly float m_maxAcceleration = 10f;

    [SerializeField]
    Rect m_allowedArea = new Rect(-5f, -5f, 10f, 10f);

    [SerializeField, Range(0f, 1f)]
    readonly float bounciness = 0.5f;

    Vector3 m_velocity;

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;
        float maxSpeedChange = m_maxAcceleration * Time.deltaTime; // 直接用max了
        m_velocity.x = Mathf.MoveTowards(m_velocity.x, desiredVelocity.x, maxSpeedChange);
        m_velocity.z = Mathf.MoveTowards(m_velocity.z, desiredVelocity.z, maxSpeedChange);

        Vector3 newPosition = transform.position + m_velocity * Time.deltaTime;

        if (newPosition.x < m_allowedArea.xMin)
        {
            newPosition.x = m_allowedArea.xMin;
            m_velocity.x = -m_velocity.x * bounciness;
        }
        else if (newPosition.x > m_allowedArea.xMax)
        {
            newPosition.x = m_allowedArea.xMax;
            m_velocity.x = -m_velocity.x * bounciness;
        }
        else if (newPosition.z < m_allowedArea.yMin)
        {
            newPosition.z = m_allowedArea.yMax;
            // m_velocity.z = -m_velocity.z;
        }
        else if (newPosition.z > m_allowedArea.yMax)
        {
            newPosition.z = m_allowedArea.yMin;
            // m_velocity.z = -m_velocity.z;
        }

        transform.localPosition = newPosition;


    }
}
