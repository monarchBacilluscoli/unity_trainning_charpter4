using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere2 : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float m_maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float m_maxAcceleration = 10f;

    Vector3 m_velocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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

        transform.localPosition = newPosition;


    }
}
