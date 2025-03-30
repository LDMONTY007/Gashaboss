// Base class for UI panels that need to handle input when active
// Attach this to any UI manager or panel controller script (CollectionManager, ObjectViewer, etc.)

using UnityEngine;

public class UIInputHandler : MonoBehaviour
{
    protected virtual bool IsUIActive() { return false; } // Must be overridden by child classes

    protected virtual void OnEscapePressed() { } // Called when Escape is pressed

    /*
    // COMMENTED OUT: We no longer handle ESC in each UI script. The UIManager handles ESC globally.
    private void Update()
    {
        // Only listen if UI is active and input is not globally blocked
        if (IsUIActive() && Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed();
        }
    }
    */
}
