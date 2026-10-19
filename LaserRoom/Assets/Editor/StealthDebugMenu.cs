using StarterAssets;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dev-only helpers for playtesting the stealth systems by hand.
///
/// Unity blocks editor-side component writes while the game is running (the
/// automation bridge cannot use Undo in play mode), so these menu items perform
/// the small actions from inside the editor instead.
/// </summary>
public static class StealthDebugMenu
{
    const string MenuRoot = "Tools/Stealth Debug/";

    [MenuItem(MenuRoot + "Simulate Flashlight Press (F)")]
    static void SimulateFlashlightPress()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Stealth Debug: enter play mode first.");
            return;
        }

        StarterAssetsInputs inputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs == null)
        {
            Debug.LogWarning("Stealth Debug: no StarterAssetsInputs in the scene.");
            return;
        }

        // Exactly what the F key's performed callback does.
        inputs.flashlight = true;
        Debug.Log("Stealth Debug: flashlight press simulated.");
    }
}
