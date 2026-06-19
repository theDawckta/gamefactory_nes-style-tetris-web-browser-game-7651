using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Initials Entry Overlay: 3-character initials input overlay that sits on top of Game Over screen.
/// Character set: A-Z then 0-9 (36 characters). Arrow keys navigate and select characters.
/// Derives from BaseScreen for show/hide lifecycle.
/// </summary>
public class InitialsEntryOverlay : BaseScreen
{
    /// <summary>
    /// Fired when the player confirms their initials. Arguments: initials string, score.
    /// </summary>
    public event System.Action<string, int> OnInitialsSubmitted;

    private static readonly string CharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private char[] currentChars = new char[3];
    private int activeSlot; // 0, 1, 2 = char slots; 3 = CONFIRM slot

    private int storedScore;

    private Label _charLabel0;
    private Label _charLabel1;
    private Label _charLabel2;
    private VisualElement _confirmRegion;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _charLabel0 = root.Q<Label>("char-0");
        _charLabel1 = root.Q<Label>("char-1");
        _charLabel2 = root.Q<Label>("char-2");
        _confirmRegion = root.Q<VisualElement>("confirm-region");
    }

    /// <summary>
    /// Shows the overlay for a given score. Resets all characters to 'A' and cursor to slot 0.
    /// </summary>
    /// <param name="score">The score to associate with the initials entry.</param>
    public void ShowForScore(int score)
    {
        storedScore = score;
        currentChars[0] = 'A';
        currentChars[1] = 'A';
        currentChars[2] = 'A';
        activeSlot = 0;

        Show();

        UpdateDisplay();
    }

    private void Update()
    {
        if (!IsVisible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleCharacter(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CycleCharacter(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ConfirmSlot();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveBack();
        }
    }

    private void CycleCharacter(int direction)
    {
        if (activeSlot >= 3)
        {
            // On CONFIRM slot, cycling has no effect
            return;
        }

        int charIndex = CharacterSet.IndexOf(currentChars[activeSlot]);
        charIndex += direction;

        // Wrap around
        if (charIndex < 0)
        {
            charIndex = CharacterSet.Length - 1;
        }
        else if (charIndex >= CharacterSet.Length)
        {
            charIndex = 0;
        }

        currentChars[activeSlot] = CharacterSet[charIndex];
        UpdateDisplay();
    }

    private void ConfirmSlot()
    {
        if (activeSlot < 2)
        {
            // Advance to next slot
            activeSlot++;
            UpdateDisplay();
        }
        else if (activeSlot == 2)
        {
            // Move to CONFIRM slot and reveal it
            activeSlot = 3;
            UpdateDisplay();
        }
        else if (activeSlot == 3)
        {
            // CONFIRM slot confirmed - submit
            Submit();
        }
    }

    private void MoveBack()
    {
        if (activeSlot > 0)
        {
            activeSlot--;
            UpdateDisplay();
        }
        // No-op on slot 0
    }

    private void Submit()
    {
        OnInitialsSubmitted?.Invoke(new string(currentChars), storedScore);
        Hide();
    }

    private void UpdateDisplay()
    {
        // Update character labels
        _charLabel0.text = currentChars[0].ToString();
        _charLabel1.text = currentChars[1].ToString();
        _charLabel2.text = currentChars[2].ToString();

        // Update cursor (active slot highlight)
        ClearCursor();

        if (activeSlot < 3)
        {
            Label activeLabel = null;
            switch (activeSlot)
            {
                case 0: activeLabel = _charLabel0; break;
                case 1: activeLabel = _charLabel1; break;
                case 2: activeLabel = _charLabel2; break;
            }
            if (activeLabel != null)
            {
                activeLabel.AddToClassList("char-slot-active");
            }
        }

        // Show/hide CONFIRM region
        if (activeSlot == 3)
        {
            _confirmRegion.style.display = DisplayStyle.Flex;
            _confirmRegion.AddToClassList("confirm-region-active");
        }
        else
        {
            _confirmRegion.style.display = DisplayStyle.None;
            _confirmRegion.RemoveFromClassList("confirm-region-active");
        }
    }

    private void ClearCursor()
    {
        _charLabel0.RemoveFromClassList("char-slot-active");
        _charLabel1.RemoveFromClassList("char-slot-active");
        _charLabel2.RemoveFromClassList("char-slot-active");
    }
}
