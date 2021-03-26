using UnityEngine;

public class MaterialSelector : MonoBehaviour
{
    /// <summary>
    /// 供更换材质
    /// </summary>
    [SerializeField]
    Material[] m_materials = default;

    /// <summary>
    /// 当前物体的renderer
    /// </summary>
    [SerializeField]
    MeshRenderer m_meshRenderer = default;

    /// <summary>
    /// 选择并更换材质
    /// </summary>
    /// <param name="index">待更换材质的index</param>
    public void Select(int index)
    {
        if (m_meshRenderer && m_materials != null &&
            index >= 0 && index < m_materials.Length)
        {
            m_meshRenderer.material = m_materials[index];
        }
    }
}
