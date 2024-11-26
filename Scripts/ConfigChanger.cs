using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigChanger : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("Config");
    }
}
