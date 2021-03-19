using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // 表明这个实例需要一个camera    
public class OrbitCamera : MonoBehaviour
{

    #region 设定参数
    /// <summary>
    /// 注视的位置
    /// </summary>
    [SerializeField]
    Transform m_focus;

    /// <summary>
    /// 和焦点距离
    /// </summary>
    [SerializeField]
    float m_distatnce = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        Vector3 focusPosition = m_focus.position;
        Vector3 lookDirection = transform.forward; //! 注意forward在transform里...
        transform.localPosition = focusPosition - lookDirection * m_distatnce;
    }
}
