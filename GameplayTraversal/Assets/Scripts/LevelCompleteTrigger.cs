using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winPanel;

    [Header("Options")]
    [SerializeField] private bool pauseGameOnWin = false;

    private bool triggered = false;

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (pauseGameOnWin)
            Time.timeScale = 0f;

        Debug.Log("Level Complete!");
    }
}