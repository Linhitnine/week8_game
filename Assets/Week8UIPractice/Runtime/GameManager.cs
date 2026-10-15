using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Week 8 UI practice popup. The popup is hidden when the scene starts,
/// and becomes visible when EnterGamePopupTrigger reports a collision with the wall.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Popup UI References")]
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;

    [Header("Dynamic Popup Content")]
    [SerializeField] private string popupTitle = "FLIGHT COMPLETE";
    [SerializeField, TextArea(2, 4)] private string popupMessage = "You reached the Enter Wall. Continue to the next scene or restart your flight.";
    [SerializeField] private string continueButtonLabel = "Continue";
    [SerializeField] private string restartButtonLabel = "Restart";

    [Header("Scene Navigation")]
    [Tooltip("Optional scene name or scene path for the Continue button. Leave blank to load the next scene in Build Settings.")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private bool pauseGameWhenPopupOpens = true;

    private bool popupHasBeenShown;

    private void Awake()
    {
        ApplyDynamicText();

        // The practice requires the menu to be hidden when the game starts.
        if (popupCanvas != null)
        {
            popupCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("GameManager: Popup Canvas is not assigned. Use Tools > Week 8 UI Practice > Build Popup in Current Scene.", this);
        }
    }

    /// <summary>Shows the popup once after the airplane reaches the Enter Wall.</summary>
    public void ShowPopup()
    {
        if (popupHasBeenShown || popupCanvas == null)
        {
            return;
        }

        popupHasBeenShown = true;
        ApplyDynamicText();
        popupCanvas.gameObject.SetActive(true);

        if (pauseGameWhenPopupOpens)
        {
            Time.timeScale = 0f;
        }
    }

    /// <summary>Loads the configured next scene, or the next Build Settings scene.</summary>
    public void NextScene()
    {
        int targetBuildIndex = -1;

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            targetBuildIndex = ResolveSceneBuildIndex(nextSceneName.Trim());
        }
        else
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex >= 0)
            {
                targetBuildIndex = currentScene.buildIndex + 1;
            }
        }

        if (targetBuildIndex < 0 || targetBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            if (messageText != null)
            {
                messageText.text = "No next scene is configured. Add the next scene in File > Build Settings (or Build Profiles), or set Next Scene Name on GameManager.";
            }

            Debug.LogWarning("GameManager: NextScene could not find a valid destination. Add the destination scene to Build Settings / Build Profiles.", this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetBuildIndex);
    }

    /// <summary>Reloads the currently active scene.</summary>
    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex < 0)
        {
            if (messageText != null)
            {
                messageText.text = "This scene is not in Build Settings yet. Add the current scene before using Restart.";
            }

            Debug.LogWarning("GameManager: The current scene is not in Build Settings / Build Profiles, so it cannot be reloaded by build index.", this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void ApplyDynamicText()
    {
        if (titleText != null)
        {
            titleText.text = popupTitle;
        }

        if (messageText != null)
        {
            messageText.text = popupMessage;
        }

        SetButtonLabel(continueButton, continueButtonLabel);
        SetButtonLabel(restartButton, restartButtonLabel);
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text labelText = button.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null)
        {
            labelText.text = label;
        }
    }

    private static int ResolveSceneBuildIndex(string sceneNameOrPath)
    {
        int index = SceneUtility.GetBuildIndexByScenePath(sceneNameOrPath);
        if (index >= 0)
        {
            return index;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.Equals(Path.GetFileNameWithoutExtension(path), sceneNameOrPath, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
