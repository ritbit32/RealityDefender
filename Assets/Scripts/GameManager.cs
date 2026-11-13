using System.Collections;
using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Part 3 - Game state: score, targets remaining, "WAVE CLEARED", play again, and quitting.
/// Put this on an empty "GameManager" object in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [Tooltip("Shown when all targets are destroyed. Put the PLAY AGAIN button inside it so they show/hide together.")]
    [SerializeField] GameObject waveClearedText;
    [Tooltip("Combat UI (FIRE button, crosshair) hidden while WAVE CLEARED is showing.")]
    [SerializeField] GameObject[] hideWhenCleared;

    [Header("Rules")]
    [SerializeField] int pointsPerTarget = 10;
    [Tooltip("If on, a new wave of targets spawns a few seconds after WAVE CLEARED.")]
    [SerializeField] bool autoNextWave = false;
    [SerializeField] float nextWaveDelay = 3f;

    int _score;
    int _targetsRemaining;
    GameBase _gameBase;

    void Awake()
    {
        Instance = this;
        if (waveClearedText) waveClearedText.SetActive(false);
        RefreshScore();
    }

    void Update()
    {
        // Android back button / back gesture arrives as the Escape key.
        if (BackPressed())
            QuitGame();
    }

    /// <summary>Hook the EXIT button's OnClick to this. (Does nothing in the Editor, only in a build.)</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame called - the app would close here on the phone.");
#endif
        Application.Quit();
    }

    public void SetGameBase(GameBase gameBase) => _gameBase = gameBase;

    public void RegisterTarget()
    {
        _targetsRemaining++;
        SetWaveClearedUI(false);
    }

    public void TargetDestroyed()
    {
        _score += pointsPerTarget;
        _targetsRemaining = Mathf.Max(0, _targetsRemaining - 1);
        RefreshScore();

        if (_targetsRemaining == 0)
        {
            SetWaveClearedUI(true);
            if (autoNextWave && _gameBase != null) StartCoroutine(NextWaveRoutine());
        }
    }

    /// <summary>
    /// Hook the PLAY AGAIN button's OnClick to this. Resets the score and spawns a fresh wave
    /// on the same anchored base, so the player doesn't have to re-scan the room.
    /// </summary>
    public void RestartWave()
    {
        if (_gameBase == null || _targetsRemaining > 0) return;

        StopAllCoroutines();
        _score = 0;
        RefreshScore();
        SetWaveClearedUI(false);
        _gameBase.SpawnWave();
    }

    IEnumerator NextWaveRoutine()
    {
        yield return new WaitForSeconds(nextWaveDelay);
        _gameBase.SpawnWave();
    }

    /// <summary>Shows WAVE CLEARED (+ PLAY AGAIN) and hides the combat UI, or the other way round.</summary>
    void SetWaveClearedUI(bool cleared)
    {
        if (waveClearedText) waveClearedText.SetActive(cleared);
        if (hideWhenCleared == null) return;
        foreach (var go in hideWhenCleared)
            if (go) go.SetActive(!cleared);
    }

    void RefreshScore()
    {
        if (scoreText) scoreText.text = "SCORE: " + _score;
    }

    static bool BackPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
