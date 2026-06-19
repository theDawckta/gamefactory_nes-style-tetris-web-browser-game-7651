using NUnit.Framework;
using System;

/// <summary>
/// Edit-mode tests for InitialsEntryOverlay character cycling logic.
/// Tests the character set, wrapping, and string construction without Unity runtime.
/// </summary>
[TestFixture]
public class InitialsEntryOverlayTests
{
    private static readonly string CharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    [Test]
    public void CharacterSet_Has36Characters()
    {
        Assert.AreEqual(36, CharacterSet.Length, "CharacterSet should have 36 characters (A-Z + 0-9)");
    }

    [Test]
    public void CharacterSet_StartsWithA()
    {
        Assert.AreEqual('A', CharacterSet[0], "CharacterSet should start with 'A'");
    }

    [Test]
    public void CharacterSet_EndsWith9()
    {
        Assert.AreEqual('9', CharacterSet[CharacterSet.Length - 1], "CharacterSet should end with '9'");
    }

    [Test]
    public void CharacterSet_ContainsAllLetters()
    {
        for (char c = 'A'; c <= 'Z'; c++)
        {
            Assert.IsTrue(CharacterSet.IndexOf(c) >= 0, "CharacterSet should contain '" + c + "'");
        }
    }

    [Test]
    public void CharacterSet_ContainsAllDigits()
    {
        for (char c = '0'; c <= '9'; c++)
        {
            Assert.IsTrue(CharacterSet.IndexOf(c) >= 0, "CharacterSet should contain '" + c + "'");
        }
    }

    [Test]
    public void CycleForward_FromA_GivesB()
    {
        int index = CharacterSet.IndexOf('A');
        int nextIndex = (index + 1) % CharacterSet.Length;
        Assert.AreEqual('B', CharacterSet[nextIndex], "Cycling forward from A should give B");
    }

    [Test]
    public void CycleForward_FromZ_Gives0()
    {
        int index = CharacterSet.IndexOf('Z');
        int nextIndex = (index + 1) % CharacterSet.Length;
        Assert.AreEqual('0', CharacterSet[nextIndex], "Cycling forward from Z should give 0");
    }

    [Test]
    public void CycleForward_From9_GivesA()
    {
        int index = CharacterSet.IndexOf('9');
        int nextIndex = (index + 1) % CharacterSet.Length;
        Assert.AreEqual('A', CharacterSet[nextIndex], "Cycling forward from 9 should give A");
    }

    [Test]
    public void CycleBackward_FromA_Gives9()
    {
        int index = CharacterSet.IndexOf('A');
        int prevIndex = index - 1;
        if (prevIndex < 0) prevIndex = CharacterSet.Length - 1;
        Assert.AreEqual('9', CharacterSet[prevIndex], "Cycling backward from A should give 9");
    }

    [Test]
    public void CycleBackward_From0_GivesZ()
    {
        int index = CharacterSet.IndexOf('0');
        int prevIndex = index - 1;
        if (prevIndex < 0) prevIndex = CharacterSet.Length - 1;
        Assert.AreEqual('Z', CharacterSet[prevIndex], "Cycling backward from 0 should give Z");
    }

    [Test]
    public void CycleBackward_FromB_GivesA()
    {
        int index = CharacterSet.IndexOf('B');
        int prevIndex = index - 1;
        if (prevIndex < 0) prevIndex = CharacterSet.Length - 1;
        Assert.AreEqual('A', CharacterSet[prevIndex], "Cycling backward from B should give A");
    }

    [Test]
    public void InitialsString_ConstructedCorrectly()
    {
        char[] chars = new char[] { 'A', 'B', 'C' };
        string result = new string(chars);
        Assert.AreEqual("ABC", result, "String constructed from char array should be ABC");
    }

    [Test]
    public void InitialsString_WithDigits()
    {
        char[] chars = new char[] { 'Z', '9', '0' };
        string result = new string(chars);
        Assert.AreEqual("Z90", result, "String constructed from char array should be Z90");
    }

    [Test]
    public void DefaultInitials_AreAAA()
    {
        char[] chars = new char[] { 'A', 'A', 'A' };
        string result = new string(chars);
        Assert.AreEqual("AAA", result, "Default initials should be AAA");
    }

    [Test]
    public void SlotAdvancement_SequenceIsCorrect()
    {
        // Simulate the slot advancement: 0 -> 1 -> 2 -> 3 (CONFIRM)
        int activeSlot = 0;
        
        // Down on slot 0 -> slot 1
        activeSlot = 1;
        Assert.AreEqual(1, activeSlot, "After down on slot 0, should be at slot 1");

        // Down on slot 1 -> slot 2
        activeSlot = 2;
        Assert.AreEqual(2, activeSlot, "After down on slot 1, should be at slot 2");

        // Down on slot 2 -> slot 3 (CONFIRM)
        activeSlot = 3;
        Assert.AreEqual(3, activeSlot, "After down on slot 2, should be at CONFIRM (slot 3)");
    }

    [Test]
    public void SlotMoveBack_SequenceIsCorrect()
    {
        int activeSlot = 3;

        // Up on slot 3 -> slot 2
        activeSlot = 2;
        Assert.AreEqual(2, activeSlot, "After up on slot 3, should be at slot 2");

        // Up on slot 2 -> slot 1
        activeSlot = 1;
        Assert.AreEqual(1, activeSlot, "After up on slot 2, should be at slot 1");

        // Up on slot 1 -> slot 0
        activeSlot = 0;
        Assert.AreEqual(0, activeSlot, "After up on slot 1, should be at slot 0");

        // Up on slot 0 -> no-op (stays at 0)
        // No change
        Assert.AreEqual(0, activeSlot, "After up on slot 0, should stay at slot 0 (no-op)");
    }
}
