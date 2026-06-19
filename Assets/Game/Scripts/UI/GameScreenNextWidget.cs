using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MonoBehaviour widget that populates the #next-region of a Game Screen UIDocument.
/// Displays a 4x4 grid of Image VisualElements showing the next piece centered
/// in the preview area. Each cell displays either the piece-type block sprite
/// or the empty block sprite.
/// Font is sourced from CoreAssets/Runtime/Fonts/default.asset via USS.
/// </summary>
public class GameScreenNextWidget : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Serialized configuration
    // -----------------------------------------------------------------------

    /// <summary>
    /// Array of 8 block sprites: index 0 = empty, indices 1-7 = piece types
    /// I, O, T, S, Z, J, L in PieceType enum order.
    /// </summary>
    [SerializeField, Tooltip("8 sprites: index 0 = empty, 1-7 = piece types (I,O,T,S,Z,J,L)")]
    private Sprite[] _blockSprites;

    // -----------------------------------------------------------------------
    // Internal state
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reference to the #next-region VisualElement.
    /// </summary>
    private VisualElement _nextRegion;

    /// <summary>
    /// The 4x4 grid of Image VisualElements. Indexed as [row, col].
    /// </summary>
    private Image[,] _gridImages;

    // -----------------------------------------------------------------------
    // Properties
    // -----------------------------------------------------------------------

    /// <summary>
    /// The block sprites array.
    /// </summary>
    public Sprite[] BlockSprites
    {
        get { return _blockSprites; }
    }

    /// <summary>
    /// Sets the block sprites array. Used for programmatic setup
    /// when the Inspector is not available (e.g. tests).
    /// </summary>
    public void SetBlockSprites(Sprite[] sprites)
    {
        _blockSprites = sprites;
    }

    // -----------------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[GameScreenNextWidget] No UIDocument component found on this GameObject.");
            return;
        }

        _nextRegion = doc.rootVisualElement.Q<VisualElement>("next-region");
        if (_nextRegion == null)
        {
            Debug.LogError("[GameScreenNextWidget] Could not find #next-region element.");
            return;
        }

        CreateGrid();
    }

    // -----------------------------------------------------------------------
    // Grid creation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a 4x4 grid of Image VisualElements inside the #next-region.
    /// Each cell is an Image that will display either the empty block sprite
    /// or a piece-type block sprite.
    /// </summary>
    private void CreateGrid()
    {
        var gridContainer = new VisualElement();
        gridContainer.name = "next-grid";
        gridContainer.style.flexDirection = FlexDirection.Column;
        gridContainer.style.alignItems = Align.Center;
        gridContainer.style.justifyContent = Justify.Center;

        _gridImages = new Image[4, 4];

        for (int row = 0; row < 4; row++)
        {
            var rowContainer = new VisualElement();
            rowContainer.name = "next-grid-row-" + row;
            rowContainer.style.flexDirection = FlexDirection.Row;
            rowContainer.style.justifyContent = Justify.Center;

            for (int col = 0; col < 4; col++)
            {
                var image = new Image();
                image.name = "next-grid-cell-" + row + "-" + col;
                image.style.width = 18;
                image.style.height = 18;
                image.style.marginLeft = 1;
                image.style.marginRight = 1;
                image.style.marginTop = 1;
                image.style.marginBottom = 1;
                _gridImages[row, col] = image;
                rowContainer.Add(image);
            }

            gridContainer.Add(rowContainer);
        }

        _nextRegion.Add(gridContainer);
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Redraws the 4x4 preview grid to show the spawn rotation (rotation 0)
    /// of the given piece type, centered in the grid. Empty cells show the
    /// empty block sprite (index 0).
    /// </summary>
    /// <param name="nextPiece">The piece type to display in the preview.</param>
    public void UpdateNextPiece(PieceType nextPiece)
    {
        if (_gridImages == null)
            return;

        // Get the cell offsets for rotation 0 from PlayfieldModel's static table
        int[] offsets = PlayfieldModel.GetCellOffsets(nextPiece, 0);
        int cellCount = offsets.Length / 2;

        // Compute the bounding box of the piece cells
        int minRow = int.MaxValue;
        int maxRow = int.MinValue;
        int minCol = int.MaxValue;
        int maxCol = int.MinValue;

        for (int i = 0; i < cellCount; i++)
        {
            int dr = offsets[i * 2];
            int dc = offsets[i * 2 + 1];
            if (dr < minRow) minRow = dr;
            if (dr > maxRow) maxRow = dr;
            if (dc < minCol) minCol = dc;
            if (dc > maxCol) maxCol = dc;
        }

        // Compute centering offsets within the 4x4 grid
        int pieceHeight = maxRow - minRow + 1;
        int pieceWidth = maxCol - minCol + 1;
        int rowOffset = (4 - pieceHeight) / 2;
        int colOffset = (4 - pieceWidth) / 2;

        int spriteIndex = (int)nextPiece + 1; // PieceType index + 1 = sprite index 1-7

        // Clear all cells to empty sprite
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (_blockSprites != null && _blockSprites.Length > 0)
                {
                    _gridImages[row, col].image = _blockSprites[0];
                }
            }
        }

        // Fill piece cells with the appropriate block sprite
        for (int i = 0; i < cellCount; i++)
        {
            int dr = offsets[i * 2];
            int dc = offsets[i * 2 + 1];
            int gridRow = dr - minRow + rowOffset;
            int gridCol = dc - minCol + colOffset;

            if (gridRow >= 0 && gridRow < 4 && gridCol >= 0 && gridCol < 4)
            {
                if (_blockSprites != null && _blockSprites.Length > 0)
                {
                    _gridImages[gridRow, gridCol].image = _blockSprites[spriteIndex];
                }
            }
        }
    }
}
