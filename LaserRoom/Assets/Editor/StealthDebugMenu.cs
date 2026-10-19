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

    [MenuItem(MenuRoot + "Teleport Player Near Drone 1")]
    static void TeleportNearDrone1() => TeleportNearDrone(1);

    [MenuItem(MenuRoot + "Teleport Player Near Drone 2")]
    static void TeleportNearDrone2() => TeleportNearDrone(2);

    [MenuItem(MenuRoot + "Teleport Player Near Drone 3")]
    static void TeleportNearDrone3() => TeleportNearDrone(3);

    [MenuItem(MenuRoot + "Teleport Player Near Drone 4")]
    static void TeleportNearDrone4() => TeleportNearDrone(4);

    /// <summary>
    /// Drops the player on the corridor centre line just past the +Z end of a drone's
    /// route: inside the drone's sweep but clear of the laser fields, which is exactly
    /// where a player would walk in from.
    /// </summary>
    static void TeleportNearDrone(int index)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Stealth Debug: enter play mode first.");
            return;
        }

        GameObject droneGo = GameObject.Find("Drone " + index);
        if (droneGo == null)
        {
            Debug.LogWarning($"Stealth Debug: 'Drone {index}' not found in the scene.");
            return;
        }

        PatrolDroneHazard drone = droneGo.GetComponent<PatrolDroneHazard>();
        PlayerHealth hp = Object.FindFirstObjectByType<PlayerHealth>();
        if (hp == null)
        {
            Debug.LogWarning("Stealth Debug: no PlayerHealth in the scene.");
            return;
        }

        float z = droneGo.transform.position.z;
        if (drone != null) z += drone.patrolRange;
        Vector3 dest = new Vector3(0f, hp.transform.position.y, z + 2.5f);

        CharacterController cc = hp.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        hp.transform.position = dest;
        if (cc != null) cc.enabled = true;

        Debug.Log($"Stealth Debug: player teleported near drone {index} (z {dest.z:0.0}).");
    }
}
