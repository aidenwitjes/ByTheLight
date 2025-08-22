using UnityEngine;
using System.Collections.Generic;

// Enhanced puzzle manager that links torches to lanterns with well integration and lock feature
public class TorchLanternPuzzle : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private TorchLanternPair[] torchLanternPairs;
    [SerializeField] private bool requireAllTorchesLit = false;
    [SerializeField] private bool showDebugMessages = true;

    [Header("Puzzle Lock Settings")]
    [SerializeField] private bool lockPuzzleOnCompletion = true;
    [SerializeField] private bool preventTorchExtinguishing = true; // Prevent manual extinguishing when locked
    [SerializeField] private bool preventLifetimeExtinguishing = true; // Prevent lifetime extinguishing when locked

    [Header("Well Integration")]
    [SerializeField] private GameObject wellObject;
    [SerializeField] private EdgeCollider2D[] wellEdgeColliders; // Edge colliders that block the well
    [SerializeField] private bool disableWellCollidersOnCompletion = true;

    [Header("Completion Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPuzzleCompleted;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPuzzleReset;
    [SerializeField] private UnityEngine.Events.UnityEvent OnWellOpened;

    private bool puzzleCompleted = false;
    private bool puzzleLocked = false;

    [System.Serializable]
    public class TorchLanternPair
    {
        [Header("Linked Objects")]
        public BaseLight torchLight;
        public BaseLight lanternLight;

        [Header("Settings")]
        public bool invertLogic = false; // If true, lantern turns OFF when torch turns ON
        public float delaySeconds = 0f; // Optional delay before lantern responds

        [HideInInspector] public bool lastTorchState;
        [HideInInspector] public float delayTimer;
    }

    private void Start()
    {
        // Initialize starting states
        foreach (var pair in torchLanternPairs)
        {
            if (pair.torchLight != null)
            {
                pair.lastTorchState = pair.torchLight.IsOn;
            }
        }

        // Well should always be visible, just ensure edge colliders start enabled
        if (wellEdgeColliders != null && disableWellCollidersOnCompletion)
        {
            SetWellColliders(true); // Start with colliders enabled (blocking access)
        }

        // Initial check in case some torches start lit
        CheckPuzzleState();
    }

    private void Update()
    {
        if (!puzzleLocked)
        {
            CheckForStateChanges();
        }
        HandleDelayedActivations();
    }

    private void CheckForStateChanges()
    {
        bool anyChangeDetected = false;

        foreach (var pair in torchLanternPairs)
        {
            if (pair.torchLight == null || pair.lanternLight == null) continue;

            bool currentTorchState = pair.torchLight.IsOn;

            // Check if torch state changed
            if (currentTorchState != pair.lastTorchState)
            {
                pair.lastTorchState = currentTorchState;
                anyChangeDetected = true;

                if (showDebugMessages)
                {
                    Debug.Log($"Torch {pair.torchLight.name} is now {(currentTorchState ? "ON" : "OFF")}");
                }

                // Start delay timer or activate immediately
                if (pair.delaySeconds > 0f)
                {
                    pair.delayTimer = pair.delaySeconds;
                }
                else
                {
                    UpdateLanternState(pair);
                }
            }
        }

        if (anyChangeDetected)
        {
            CheckPuzzleState();
        }
    }

    private void HandleDelayedActivations()
    {
        foreach (var pair in torchLanternPairs)
        {
            if (pair.delayTimer > 0f)
            {
                pair.delayTimer -= Time.deltaTime;

                if (pair.delayTimer <= 0f)
                {
                    UpdateLanternState(pair);
                }
            }
        }
    }

    private void UpdateLanternState(TorchLanternPair pair)
    {
        if (pair.lanternLight == null) return;

        bool targetState = pair.invertLogic ? !pair.torchLight.IsOn : pair.torchLight.IsOn;

        pair.lanternLight.SetLightState(targetState);

        if (showDebugMessages)
        {
            Debug.Log($"Lantern {pair.lanternLight.name} is now {(targetState ? "ON" : "OFF")}");
        }
    }

    private void CheckPuzzleState()
    {
        bool newPuzzleState = IsPuzzleCompleted();

        if (newPuzzleState && !puzzleCompleted)
        {
            // Puzzle just completed
            puzzleCompleted = true;

            if (lockPuzzleOnCompletion)
            {
                LockPuzzle();
            }

            if (disableWellCollidersOnCompletion)
            {
                OpenWell();
            }

            if (showDebugMessages)
                Debug.Log("?? Puzzle Completed!");

            OnPuzzleCompleted?.Invoke();
        }
        else if (!newPuzzleState && puzzleCompleted && !puzzleLocked)
        {
            // Puzzle was reset (only if not locked)
            puzzleCompleted = false;

            if (showDebugMessages)
                Debug.Log("?? Puzzle Reset");

            OnPuzzleReset?.Invoke();
        }
    }

    private void LockPuzzle()
    {
        puzzleLocked = true;

        if (showDebugMessages)
            Debug.Log("?? Puzzle Locked - Torches can no longer be extinguished");

        // Prevent torch interactions if desired
        if (preventTorchExtinguishing)
        {
            foreach (var pair in torchLanternPairs)
            {
                if (pair.torchLight != null)
                {
                    // Force all torches to stay on
                    pair.torchLight.TurnOn();

                    // If it's a FireLight with lifetime, disable the lifetime
                    if (preventLifetimeExtinguishing && pair.torchLight is FireLight fireLight)
                    {
                        DisableFireLightLifetime(fireLight);
                    }
                }
            }
        }
    }

    private void DisableFireLightLifetime(FireLight fireLight)
    {
        // Use reflection to disable lifetime or you could add a public method to FireLight
        // For now, we'll assume you add a public method to disable lifetime
        if (fireLight.GetType().GetMethod("DisableLifetime") != null)
        {
            fireLight.GetType().GetMethod("DisableLifetime").Invoke(fireLight, null);
        }
    }

    private void OpenWell()
    {
        if (wellEdgeColliders != null)
        {
            SetWellColliders(false); // Disable edge colliders to allow access

            if (showDebugMessages)
                Debug.Log("??? Well edge colliders disabled - Well is now accessible!");

            OnWellOpened?.Invoke();
        }
    }

    private void SetWellColliders(bool enabled)
    {
        foreach (var collider in wellEdgeColliders)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
            }
        }
    }

    private bool IsPuzzleCompleted()
    {
        if (torchLanternPairs.Length == 0) return false;

        if (requireAllTorchesLit)
        {
            // All torches must be lit
            foreach (var pair in torchLanternPairs)
            {
                if (pair.torchLight == null || !pair.torchLight.IsOn)
                    return false;
            }
            return true;
        }
        else
        {
            // At least one torch must be lit
            foreach (var pair in torchLanternPairs)
            {
                if (pair.torchLight != null && pair.torchLight.IsOn)
                    return true;
            }
            return false;
        }
    }

    // Public methods for external control
    public void ResetPuzzle()
    {
        if (puzzleLocked)
        {
            if (showDebugMessages)
                Debug.Log("Cannot reset puzzle - it is locked!");
            return;
        }

        foreach (var pair in torchLanternPairs)
        {
            pair.torchLight?.TurnOff();
            pair.lanternLight?.TurnOff();
        }

        if (disableWellCollidersOnCompletion)
        {
            SetWellColliders(true); // Re-enable edge colliders to block access
        }
    }

    public void CompletePuzzle()
    {
        foreach (var pair in torchLanternPairs)
        {
            pair.torchLight?.TurnOn();
            // Lanterns will follow automatically
        }
    }

    public void UnlockPuzzle()
    {
        puzzleLocked = false;
        if (showDebugMessages)
            Debug.Log("?? Puzzle Unlocked");
    }

    // Public getters
    public bool IsPuzzleLocked => puzzleLocked;
    public bool IsPuzzleComplete => puzzleCompleted;

    // Debug method to show current state
    [ContextMenu("Show Puzzle State")]
    public void ShowPuzzleState()
    {
        Debug.Log($"Puzzle Completed: {puzzleCompleted}, Locked: {puzzleLocked}");
        foreach (var pair in torchLanternPairs)
        {
            if (pair.torchLight != null && pair.lanternLight != null)
            {
                Debug.Log($"Torch {pair.torchLight.name}: {pair.torchLight.IsOn} ? Lantern {pair.lanternLight.name}: {pair.lanternLight.IsOn}");
            }
        }
    }
}

// Alternative: Simple single-pair puzzle for quick setup
public class SimpleTorchLanternLink : MonoBehaviour
{
    [Header("Linked Lights")]
    [SerializeField] private BaseLight torchLight;
    [SerializeField] private BaseLight lanternLight;

    [Header("Settings")]
    [SerializeField] private bool invertLogic = false;
    [SerializeField] private float delaySeconds = 0f;

    private bool lastTorchState;
    private float delayTimer;

    private void Start()
    {
        if (torchLight != null)
            lastTorchState = torchLight.IsOn;
    }

    private void Update()
    {
        if (torchLight == null || lanternLight == null) return;

        // Check for torch state change
        bool currentTorchState = torchLight.IsOn;
        if (currentTorchState != lastTorchState)
        {
            lastTorchState = currentTorchState;

            if (delaySeconds > 0f)
            {
                delayTimer = delaySeconds;
            }
            else
            {
                UpdateLantern();
            }
        }

        // Handle delayed activation
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                UpdateLantern();
            }
        }
    }

    private void UpdateLantern()
    {
        bool targetState = invertLogic ? !torchLight.IsOn : torchLight.IsOn;
        lanternLight.SetLightState(targetState);
    }
}