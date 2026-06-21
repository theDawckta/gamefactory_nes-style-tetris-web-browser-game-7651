using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InitialsEntryOverlay : BaseScreen
{
    private static readonly char[] CharSet = BuildCharSet();

    private readonly char[] _currentChars = { 'A', 'A', 'A' };
    private int _activeSlot;
    private int _storedScore;

    private Label _charLabel0;
    private Label _charLabel1;
    private Label _charLabel2;
    private VisualElement _confirmRegion;
    private Label _confirmLabel;

    public event Action<string, int> OnInitialsSubmitted;

    public int ActiveSlot => _activeSlot;
    public char GetCurrentChar(int slot) => _currentChars[slot];

    protected override void Awake()
    {
        base.Awake();
        _charLabel0 = Root.Q<Label>("char-0");
        _charLabel1 = Root.Q<Label>("char-1");
        _charLabel2 = Root.Q<Label>("char-2");
        _confirmRegion = Root.Q("confirm-region");
        _confirmLabel = Root.Q<Label>("confirm-label");
    }

    public void ShowForScore(int score)
    {
        _storedScore = score;
        _currentChars[0] = _currentChars[1] = _currentChars[2] = 'A';
        _activeSlot = 0;
        Show();
        UpdateDisplay();
    }

    private void Update()
    {
        if (!IsVisible) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftArrowKey.wasPressedThisFrame) CycleActiveChar(-1);
        else if (kb.rightArrowKey.wasPressedThisFrame) CycleActiveChar(1);
        else if (kb.downArrowKey.wasPressedThisFrame) AdvanceSlot();
        else if (kb.upArrowKey.wasPressedThisFrame) RetreatSlot();
    }

    public void CycleActiveChar(int direction)
    {
        if (_activeSlot >= 3) return;
        int idx = Array.IndexOf(CharSet, _currentChars[_activeSlot]);
        idx = (idx + direction + CharSet.Length) % CharSet.Length;
        _currentChars[_activeSlot] = CharSet[idx];
        UpdateDisplay();
    }

    public void AdvanceSlot()
    {
        if (_activeSlot < 2)
        {
            _activeSlot++;
        }
        else if (_activeSlot == 2)
        {
            if (_confirmRegion != null)
                _confirmRegion.style.display = DisplayStyle.Flex;
            _activeSlot = 3;
        }
        else if (_activeSlot == 3)
        {
            Submit();
            return;
        }
        UpdateDisplay();
    }

    public void RetreatSlot()
    {
        if (_activeSlot == 0) return;
        if (_activeSlot == 3)
        {
            if (_confirmRegion != null)
                _confirmRegion.style.display = DisplayStyle.None;
            _activeSlot = 2;
        }
        else
        {
            _activeSlot--;
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Label[] labels = { _charLabel0, _charLabel1, _charLabel2 };
        for (int i = 0; i < 3; i++)
        {
            if (labels[i] == null) continue;
            labels[i].text = _currentChars[i].ToString();
            if (_activeSlot == i)
                labels[i].AddToClassList("active-slot");
            else
                labels[i].RemoveFromClassList("active-slot");
        }
        if (_confirmLabel != null)
        {
            if (_activeSlot == 3)
                _confirmLabel.AddToClassList("active-slot");
            else
                _confirmLabel.RemoveFromClassList("active-slot");
        }
    }

    private void Submit()
    {
        OnInitialsSubmitted?.Invoke(new string(_currentChars), _storedScore);
        Hide();
    }

    private static char[] BuildCharSet()
    {
        var set = new char[36];
        for (int i = 0; i < 26; i++) set[i] = (char)('A' + i);
        for (int i = 0; i < 10; i++) set[26 + i] = (char)('0' + i);
        return set;
    }
}
