using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuChanger : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("Menú");
    }
}
