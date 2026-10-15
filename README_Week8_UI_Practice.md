# Week 8 - Unity UI Practice (Buttons, Text, Images)

This patch implements the practice at the end of the supplied PDF. It builds a popup with a title, message, Continue and Restart buttons, displays it when the airplane collides with the Enter Wall, and provides scene-navigation callbacks.

## Included files

- `Assets/Week8UIPractice/Runtime/GameManager.cs` - dynamic title/message/button labels, popup visibility, pause, Continue and Restart behavior.
- `Assets/Week8UIPractice/Runtime/EnterGamePopupTrigger.cs` - opens the popup when the Enter Wall is triggered or collided with.
- `Assets/Week8UIPractice/Editor/Week8UIPracticeBuilder.cs` - editor command that builds the Canvas, styled Panel, TextMeshPro text, buttons, references and Inspector button callbacks in the currently open scene.

## Setup

1. Copy the `Assets/Week8UIPractice` folder into the Unity project under `week8_game`.
2. Open the existing airplane scene (the scene containing the airplane and Enter Wall).
3. Ensure TextMeshPro Essential Resources are imported: `Window > TextMeshPro > Import TMP Essential Resources`.
4. In Unity, run `Tools > Week 8 UI Practice > Build Popup in Current Scene`.
5. Select the Enter Wall object, add `EnterGamePopupTrigger`, and assign the `GameManager` component from the Main Camera. If the wall can collide with other objects, drag the airplane into the optional `Airplane` field to filter collisions.
6. The wall needs a Collider. `EnterGamePopupTrigger` works with either `Is Trigger` enabled or a normal collision (at least one object must have a Rigidbody). Avoid keeping the previous scene-loading trigger active at the same time, or it may load another scene before the popup can be seen.
7. Save the scene. Add the current airplane scene and the destination scene to `File > Build Settings` (or `Build Profiles` in newer Unity versions). Put the destination after the current scene, or specify its name/path in `GameManager > Next Scene Name`.
8. Press Play. The popup starts hidden; collide with Enter Wall to reveal it. Continue loads the configured next scene; Restart reloads the current scene.

## Expected UI hierarchy

```
Week8_GameMenuCanvas (Canvas)
└── PopupPanel (full-screen dim layer)
    └── Frame (popup panel)
        ├── Title (TextMeshProUGUI)
        ├── Message (TextMeshProUGUI)
        ├── ContinueButton (Button + TextMeshProUGUI)
        └── RestartButton (Button + TextMeshProUGUI)
```

The builder creates persistent `Button.onClick` Inspector events for `GameManager.NextScene()` and `GameManager.RestartScene()`, matching the PDF's instructions. Popup title, message and button labels are assigned dynamically by `GameManager.cs`.

## Notes

- The builder modifies the currently open scene. The Canvas stays visible in Edit Mode for layout preview, but `GameManager.Awake()` hides it during Play Mode until the collision. Run it in the airplane practice scene, not an unrelated scene.
- The code cannot infer the correct destination scene from repository files alone. Configure the scene list in Build Settings / Build Profiles.
- `EnterGamePopupTrigger` intentionally does not replace or delete your existing script. Disable/remove the previous transition component on the Enter Wall if it immediately changes scenes.
