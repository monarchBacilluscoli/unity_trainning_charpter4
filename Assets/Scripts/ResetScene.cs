using UnityEngine.SceneManagement;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.UnloadScene(SceneManager.GetActiveScene());
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
