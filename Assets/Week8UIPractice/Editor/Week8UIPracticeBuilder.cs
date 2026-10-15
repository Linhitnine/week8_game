#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the popup UI described in the Week 8 UI practice PDF in the currently open scene.
/// Open the airplane scene before using this command.
/// </summary>
public static class Week8UIPracticeBuilder
{
    private const string RootName = "Week8_GameMenuCanvas";

    [MenuItem("Tools/Week 8 UI Practice/Build Popup in Current Scene")]
    public static void BuildPopupInCurrentScene()
    {
        if (SceneManager.GetActiveScene().rootCount == 0)
        {
            EditorUtility.DisplayDialog("Week 8 UI Practice", "Open the airplane practice scene first, then run this command.", "OK");
            return;
        }

        if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
        {
            bool import = EditorUtility.DisplayDialog(
                "TextMesh Pro resources needed",
                "Import TextMesh Pro Essential Resources first, then run the builder again.\n\nUnity menu: Window > TextMeshPro > Import TMP Essential Resources.",
                "Open TMP menu", "Cancel");
            if (import)
            {
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            }
            return;
        }

        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Popup already exists",
                "A Week 8 popup canvas already exists in this scene. Rebuild it? The existing generated canvas will be replaced.",
                "Rebuild", "Cancel");
            if (!replace) return;
            Undo.DestroyObjectImmediate(existing);
        }

        Canvas canvas = CreateCanvas();
        GameObject panelObject = CreatePanel(canvas.transform);
        GameObject frameObject = CreateFrame(panelObject.transform);
        TMP_Text title = CreateText(frameObject.transform, "Title", "FLIGHT COMPLETE", 42, FontStyles.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(560f, 70f));
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.95f, 0.97f, 1f);

        TMP_Text message = CreateText(frameObject.transform, "Message",
            "You reached the Enter Wall. Continue to the next scene or restart your flight.", 25, FontStyles.Normal,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 5f), new Vector2(550f, 110f));
        message.alignment = TextAlignmentOptions.Center;
        message.color = new Color(0.82f, 0.86f, 0.93f);

        Button continueButton = CreateButton(frameObject.transform, "ContinueButton", "Continue", new Vector2(-145f, -110f), new Color(0.15f, 0.52f, 0.95f));
        Button restartButton = CreateButton(frameObject.transform, "RestartButton", "Restart", new Vector2(145f, -110f), new Color(0.28f, 0.33f, 0.42f));

        GameManager manager = FindOrCreateGameManager();
        SerializedObject serializedManager = new SerializedObject(manager);
        SetObject(serializedManager, "popupCanvas", canvas);
        SetObject(serializedManager, "titleText", title);
        SetObject(serializedManager, "messageText", message);
        SetObject(serializedManager, "continueButton", continueButton);
        SetObject(serializedManager, "restartButton", restartButton);
        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);

        // Assign persistent Unity Button events to match the Inspector-based instructions in the PDF.
        continueButton.onClick = new Button.ButtonClickedEvent();
        restartButton.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(continueButton.onClick, manager.NextScene);
        UnityEventTools.AddPersistentListener(restartButton.onClick, manager.RestartScene);
        EditorUtility.SetDirty(continueButton);
        EditorUtility.SetDirty(restartButton);

        // Leave the Canvas visible in Edit Mode so the layout can be previewed.
        // GameManager.Awake hides it when Play Mode begins.

        EnsureEventSystem();
        Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Create Week 8 popup UI");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = manager.gameObject;

        EditorUtility.DisplayDialog(
            "Week 8 popup created",
            "Popup Canvas, Panel, TextMeshPro title/message, Continue/Restart buttons, and GameManager references were created.\n\nNext: attach EnterGamePopupTrigger to the Enter Wall, assign GameManager, save the scene, and add the required scenes to Build Settings / Build Profiles.",
            "OK");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create popup canvas");
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        // Full-screen dim layer, matching the Canvas > Panel > Frame hierarchy in the practice.
        GameObject panel = new GameObject("PopupPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panel, "Create popup panel");
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.015f, 0.025f, 0.05f, 0.48f);
        return panel;
    }

    private static GameObject CreateFrame(Transform parent)
    {
        GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(frame, "Create popup frame");
        frame.transform.SetParent(parent, false);
        RectTransform rect = frame.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(720f, 430f);
        Image image = frame.GetComponent<Image>();
        image.color = new Color(0.08f, 0.12f, 0.2f, 0.97f);
        return frame;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textObject, "Create popup text");
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(buttonObject, "Create popup button");
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240f, 66f);
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 25f, FontStyles.Bold,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        RectTransform labelRect = labelText.GetComponent<RectTransform>();
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        return button;
    }

    private static GameManager FindOrCreateGameManager()
    {
        GameObject mainCamera = null;
        try { mainCamera = GameObject.FindGameObjectWithTag("MainCamera"); }
        catch (UnityException) { /* MainCamera tag may not exist in a custom project. */ }

        if (mainCamera == null)
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length > 0) mainCamera = cameras[0].gameObject;
        }

        if (mainCamera == null)
        {
            mainCamera = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(mainCamera, "Create GameManager object");
        }

        GameManager manager = mainCamera.GetComponent<GameManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<GameManager>(mainCamera);
        }
        return manager;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create UI EventSystem");

        // Use the new Input System module when installed; otherwise use the legacy module.
        System.Type inputSystemModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputSystemModule != null)
        {
            eventSystem.AddComponent(inputSystemModule);
        }
        else
        {
            System.Type legacyModule = System.Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
            if (legacyModule != null) eventSystem.AddComponent(legacyModule);
        }
    }

    private static void SetObject(SerializedObject target, string propertyName, Object value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null) property.objectReferenceValue = value;
    }
}
#endif
