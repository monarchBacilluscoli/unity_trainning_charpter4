using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;

    /// <summary>
    /// 起始offset，在移动中固定
    /// </summary>
    Vector3 m_offset;
    // Start is called before the first frame update
    void Start()
    {
        m_offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + m_offset;
    }
}
