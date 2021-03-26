using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class DetectionZone : MonoBehaviour
{
    /// <summary>
    /// 当物体进入区域时
    /// </summary>
    [SerializeField]
    UnityEvent m_onFirstEnter = default; //? 连public都不需要的喔

    /// <summary>
    /// 当物体离开区域时
    /// </summary>
    [SerializeField]
    UnityEvent m_onLastExit = default;

    /// <summary>
    /// 进入当前区域的物体
    /// </summary>
    /// <typeparam name="Collider"></typeparam>
    /// <returns></returns>
    List<Collider> m_colliders = new List<Collider>();

    void Awake()
    {
        enabled = false; // 避免无必要地调用FixedUpdate
    }

    /// <summary>
    /// 当触发体进入时被调用
    /// </summary>
    /// <param name="other">进入当前触发体的物体</param>
    void OnTriggerEnter(Collider other)
    {
        //todo 调用第一次
        if (m_colliders.Count == 0)
        {
            m_onFirstEnter.Invoke();
            enabled = true;
        }
        m_colliders.Add(other);
    }

    /// <summary>
    /// 当出发体被离开时调用
    /// </summary>
    /// <param name="other">离开当前触发体的物体</param>
    void OnTriggerExit(Collider other)
    {
        if (m_colliders.Remove(other) && m_colliders.Count == 0)
        {
            m_onLastExit.Invoke();
            enabled = false;
        }
    }

    /// <summary>
    /// 以固定游戏中时间间隔调用
    /// </summary>
    /// <remarks>
    /// 用于处理在区域中物体被deactivated的情况，将之视作Exit
    /// </remarks>
    void FixedUpdate()
    {
        //todo 检测所有在colliders中物体是否可用
        for (int i = 0; i < m_colliders.Count; i++)
        {
            Collider collider = m_colliders[i];
            //todo 如果不可用，将其移除 
            //? !collider似乎不可用，似乎应该改为collider.enabled
            if (!collider.enabled) //? 也就是存在即使当前物体active但父物体仍可能deactivate了所有的东西的情况？
            {
                m_colliders.RemoveAt(i--); //! 注意此处需要将i-1，因为整个list此时少了1个
                //todo 如果移除至最后一个则调用Exit（但是为什么在循环内调用？）
                if (m_colliders.Count == 0)
                {
                    m_onLastExit.Invoke();
                    enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// 当当前物体要被disabled的时候调用
    /// </summary>
    void OnDisable()
    {
        //todo 检查是否为hot reload（如果处于editor并且所有层级物体都enable则是），如果是，则直接返回而不调用该函数
#if UNITY_EDITOR
        if (enabled && gameObject.activeInHierarchy)
        {
            return;
        }
#endif
        //todo 当区域中还存在物体时调用Exit
        if (m_colliders.Count > 0)
        {
            m_colliders.Clear();
            m_onLastExit.Invoke();
        }
    }
}
