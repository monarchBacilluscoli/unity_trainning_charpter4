using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// 用于重置当前场景
/// </summary>
public class ResetScene : MonoBehaviour
{
    /// <summary>
    /// 每帧被调用
    /// </summary>
    void Update()
    {
        // 1. 如果R键被按下，则重置场景
        if (Input.GetKeyDown(KeyCode.R))
        {
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
