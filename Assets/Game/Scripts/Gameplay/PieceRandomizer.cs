using System;
using System.Collections.Generic;

/// <summary>
/// 7-bag randomizer for Tetromino piece selection.
/// Each bag contains one of each PieceType in a shuffled order.
/// When the bag is empty, a new bag is created and shuffled.
/// </summary>
public class PieceRandomizer
{
    private readonly System.Random _random;
    private readonly Queue<PieceType> _bag;

    /// <summary>
    /// Creates a new PieceRandomizer with the given seed.
    /// </summary>
    public PieceRandomizer(int seed)
    {
        _random = new System.Random(seed);
        _bag = new Queue<PieceType>();
        RefillBag();
    }

    /// <summary>
    /// Returns the next piece type from the bag.
    /// Automatically refills when the bag is empty.
    /// </summary>
    public PieceType Next()
    {
        if (_bag.Count == 0)
        {
            RefillBag();
        }
        return _bag.Dequeue();
    }

    /// <summary>
    /// Peeks at the next piece type without removing it from the bag.
    /// Returns default(PieceType) if the bag is empty (should not happen
    /// in normal use since Next() refills automatically).
    /// </summary>
    public PieceType Peek()
    {
        if (_bag.Count == 0)
        {
            RefillBag();
        }
        return _bag.Peek();
    }

    /// <summary>
    /// Fills the bag with one of each PieceType in a random order.
    /// </summary>
    private void RefillBag()
    {
        var pieces = new List<PieceType>((int)PieceType.L + 1);
        for (int i = 0; i <= (int)PieceType.L; i++)
        {
            pieces.Add((PieceType)i);
        }

        // Fisher-Yates shuffle
        for (int i = pieces.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            var temp = pieces[i];
            pieces[i] = pieces[j];
            pieces[j] = temp;
        }

        _bag.Clear();
        foreach (var piece in pieces)
        {
            _bag.Enqueue(piece);
        }
    }
}
