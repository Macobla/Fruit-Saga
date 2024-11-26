using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int levelNumber; 

    void Start()
    {
        
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning("No Button component found on this GameObject.");
        }
    }

    void OnButtonClick()
    {
        string sceneName = "Level " + levelNumber;
        SceneManager.LoadScene(sceneName);
    }
}
