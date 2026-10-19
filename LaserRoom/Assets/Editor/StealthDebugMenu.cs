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

    [MenuItem(MenuRoot + "Simulate Double-Tap Sprint (start run)")]
    static void SimulateSprintStart() => SimulateSprint(Vector2.up, "forward");

    [MenuItem(MenuRoot + "Simulate Double-Tap Sprint (left run)")]
    static void SimulateSprintLeft() => SimulateSprint(Vector2.left, "left");

    [MenuItem(MenuRoot + "Simulate Double-Tap Sprint (right run)")]
    static void SimulateSprintRight() => SimulateSprint(Vector2.right, "right");

    [MenuItem(MenuRoot + "Simulate Double-Tap Sprint (back run)")]
    static void SimulateSprintBack() => SimulateSprint(Vector2.down, "back");

    static void SimulateSprint(Vector2 dir, string label)
    {
        if (!RequirePlayMode()) return;

        DoubleTapSprint sprint = Object.FindFirstObjectByType<DoubleTapSprint>();
        if (sprint == null)
        {
            Debug.LogWarning("Stealth Debug: no DoubleTapSprint on the player.");
            return;
        }

        sprint.DebugDoubleTapRun(dir);
        Debug.Log($"Stealth Debug: scripted sprint run started ({label} double-tap, held).");
    }

    [MenuItem(MenuRoot + "Simulate Double-Tap Sprint (stop run)")]
    static void SimulateSprintStop()
    {
        if (!RequirePlayMode()) return;

        DoubleTapSprint sprint = Object.FindFirstObjectByType<DoubleTapSprint>();
        if (sprint == null)
        {
            Debug.LogWarning("Stealth Debug: no DoubleTapSprint on the player.");
            return;
        }

        sprint.DebugEndSprintRun();
        Debug.Log("Stealth Debug: scripted sprint run stopped.");
    }

    [MenuItem(MenuRoot + "Simulate Crouch Hold (C down)")]
    static void SimulateCrouchDown()
    {
        if (!RequirePlayMode()) return;

        PlayerCrouch crouch = Object.FindFirstObjectByType<PlayerCrouch>();
        if (crouch == null)
        {
            Debug.LogWarning("Stealth Debug: no PlayerCrouch on the player.");
            return;
        }

        crouch.DebugSetCrouchHold(true);
        Debug.Log("Stealth Debug: crouch key held (C down).");
    }

    [MenuItem(MenuRoot + "Simulate Crouch Release (C up)")]
    static void SimulateCrouchUp()
    {
        if (!RequirePlayMode()) return;

        PlayerCrouch crouch = Object.FindFirstObjectByType<PlayerCrouch>();
        if (crouch == null)
        {
            Debug.LogWarning("Stealth Debug: no PlayerCrouch on the player.");
            return;
        }

        crouch.DebugSetCrouchHold(false);
        Debug.Log("Stealth Debug: crouch key released (C up).");
    }

    [MenuItem(MenuRoot + "Teleport Player Into Scanner 1 Sweep")]
    static void TeleportScanner1() => TeleportIntoScanner(1);

    /// <summary>Drops the player in the middle of a scanner's sweep band.</summary>
    static void TeleportIntoScanner(int index)
    {
        if (!RequirePlayMode()) return;

        GameObject scannerGo = GameObject.Find("Scanner " + index);
        if (scannerGo == null)
        {
            Debug.LogWarning($"Stealth Debug: 'Scanner {index}' not found in the scene.");
            return;
        }

        PlayerHealth hp = Object.FindFirstObjectByType<PlayerHealth>();
        if (hp == null)
        {
            Debug.LogWarning("Stealth Debug: no PlayerHealth in the scene.");
            return;
        }

        Vector3 dest = new Vector3(0f, hp.transform.position.y, scannerGo.transform.position.z);
        CharacterController cc = hp.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        hp.transform.position = dest;
        if (cc != null) cc.enabled = true;

        Debug.Log($"Stealth Debug: player teleported into scanner {index} sweep (z {dest.z:0.0}).");
    }

    [MenuItem(MenuRoot + "Measure Corridor (log)")]
    static void MeasureCorridor()
    {
        if (!RequirePlayMode()) return;

        PlayerHealth hp = Object.FindFirstObjectByType<PlayerHealth>();
        if (hp == null)
        {
            Debug.LogWarning("Stealth Debug: no PlayerHealth in the scene.");
            return;
        }

        Vector3 p = hp.transform.position;
        Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out RaycastHit down, 12f, ~0, QueryTriggerInteraction.Ignore);
        Physics.Raycast(new Vector3(0f, p.y + 1.2f, p.z), Vector3.right, out RaycastHit right, 12f, ~0, QueryTriggerInteraction.Ignore);
        Physics.Raycast(new Vector3(0f, p.y + 1.2f, p.z), Vector3.left, out RaycastHit left, 12f, ~0, QueryTriggerInteraction.Ignore);
        Camera cam = Camera.main;

        float floorY = down.collider != null ? p.y + 2f - down.distance : float.NaN;
        Debug.Log($"Stealth Debug MEASURE: player y {p.y:0.###}, z {p.z:0.###}; floorY {floorY:0.###} ('{down.collider?.name}'); " +
                  $"right gap {right.distance:0.###} ('{right.collider?.name}'); left gap {left.distance:0.###} ('{left.collider?.name}'); " +
                  $"camera y {(cam != null ? cam.transform.position.y : float.NaN):0.###}.");
    }

    [MenuItem(MenuRoot + "Enable Run In Background (dev)")]
    static void EnableRunInBackground()
    {
        PlayerSettings.runInBackground = true;
        Debug.Log("Stealth Debug: Player Setting 'Run In Background' enabled - play mode keeps ticking while the editor is unfocused.");
    }

    [MenuItem(MenuRoot + "Restart Round (reset timer + unfreeze)")]
    static void RestartRound()
    {
        if (!RequirePlayMode()) return;

        Time.timeScale = 1f;
        GameTimer timer = Object.FindFirstObjectByType<GameTimer>();
        if (timer != null) timer.StartTimer();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Stealth Debug: round restarted (timer reset, timeScale 1).");
    }

    static bool RequirePlayMode()
    {
        if (Application.isPlaying) return true;
        Debug.LogWarning("Stealth Debug: enter play mode first.");
        return false;
    }
}
