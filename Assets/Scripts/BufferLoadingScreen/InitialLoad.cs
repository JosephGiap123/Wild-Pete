using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
public class InitialLoad : MonoBehaviour
{
    [SerializeField] TMP_Text loadingText;
    private float timerToChange = 0f;
    private int textIndex = 0;
    private string[] texts = new string[] {
        "Now Loading...",
        "Please wait...",
        "Loading.",
        "Loading..",
        "Loading...",
    };

    public void Update()
    {
        timerToChange += Time.deltaTime;
        if (timerToChange >= 0.2f)
        {
            timerToChange = 0f;
            textIndex++;
            if (textIndex >= texts.Length)
            {
                textIndex = 0;
            }
            loadingText.text = texts[textIndex];
        }
    }
    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(LoadScene());
    }

    public IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1); //load main menu
    }
}
