using UnityEngine;

public class MainMenuSettingsButton : MonoBehaviour
{
    // Called by the Option button
    public void OpenSettings()
    {
        // Find the SettingsOpener that lives in the DontDestroyOnLoad UI
        var opener = FindObjectOfType<SettingsOpener>();

        if (opener != null)
        {
            opener.OpenFromUI();   // will lazy-find the SettingsUIRoot + pause game
        }
        else
        {
            Debug.LogWarning("MainMenuSettingsButton: SettingsOpener not found in scene.");
        }
    }
}
