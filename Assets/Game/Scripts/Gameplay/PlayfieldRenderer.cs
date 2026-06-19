using System;
using UnityEngine;

/// <summary>
/// Renders the playfield grid and active piece using Unity SpriteRenderer components
/// in world space. Reads from PlayfieldModel and the active PieceState from
/// GameplayController to display locked cells and the falling piece.
/// </summary>
public class PlayfieldRenderer : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Serialized configuration
    // -----------------------------------------------------------------------

    /// <summary>
    /// Size of each grid cell in world units.
    /// </summary>
    [SerializeField, Tooltip("Size of each grid cell in world units")]
    private float _cellSize = 0.5f;

    /// <summary>
    /// World-space offset for the bottom-left corner of the visible grid.
    /// </summary>
    [SerializeField, Tooltip("World-space position of the bottom-left corner of the visible grid")]
    private Vector2 _originOffset = Vector2.zero;

    /// <summary>
    /// Array of 8 block sprites: index 0 = empty, indices 1-7 = piece types
    /// I, O, T, S, Z, J, L in PieceType enum order.
    /// </summary>
    [SerializeField, Tooltip("8 sprites: index 0 = empty, 1-7 = piece types (I,O,T,S,Z,J,L)")]
    private Sprite[] _blockSprites;

    /// <summary>
    /// Sprite used for the well border frame.
    /// </summary>
    [SerializeField, Tooltip("Sprite for the well border frame")]
    private Sprite _borderSprite;

    /// <summary>
    /// Reference to the GameplayController to read game state from.
    /// </summary>
    [SerializeField, Tooltip("Reference to the GameplayController component")]
    private GameplayController _gameplayController;

    // -----------------------------------------------------------------------
    // Internal state
    // -----------------------------------------------------------------------

    /// <summary>
    /// 2D array of SpriteRenderer references for each visible cell.
    /// Indexed as [row - bufferRows, col] where row is in PlayfieldModel coordinates.
    /// </summary>
    private SpriteRenderer[,] _cellRenderers;

    /// <summary>
    /// SpriteRenderer for the well border, positioned behind the grid.
    /// </summary>
    private SpriteRenderer _borderRenderer;

    // -----------------------------------------------------------------------
    // Properties
    // -----------------------------------------------------------------------

    /// <summary>
    /// The size of each cell in world units.
    /// </summary>
    public float CellSize
    {
        get { return _cellSize; }
    }

    /// <summary>
    /// The world-space origin offset of the visible grid.
    /// </summary>
    public Vector2 OriginOffset
    {
        get { return _originOffset; }
    }

    /// <summary>
    /// The block sprites array.
    /// </summary>
    public Sprite[] BlockSprites
    {
        get { return _blockSprites; }
    }

    /// <summary>
    /// Sets the GameplayController reference. Used for programmatic setup
    /// when the Inspector is not available (e.g. tests).
    /// </summary>
    public void SetGameplayController(GameplayController controller)
    {
        _gameplayController = controller;
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
        CreateBorder();
        CreateCellGrid();
    }

    private void Start()
    {
        // Subscribe to GameplayController events if reference is set
        if (_gameplayController != null)
        {
            _gameplayController.OnStateChanged += OnStateChanged;
        }
    }

    private void Update()
    {
        RenderPlayfield();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (_gameplayController != null)
        {
            _gameplayController.OnStateChanged -= OnStateChanged;
        }
    }

    // -----------------------------------------------------------------------
    // Grid creation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates the well border SpriteRenderer as a child GameObject,
    /// positioned behind the visible grid to frame it.
    /// </summary>
    private void CreateBorder()
    {
        var borderGo = new GameObject("WellBorder");
        borderGo.transform.SetParent(this.transform);

        _borderRenderer = borderGo.AddComponent<SpriteRenderer>();
        _borderRenderer.sortingOrder = -1; // Behind the grid cells

        if (_borderSprite != null)
        {
            _borderRenderer.sprite = _borderSprite;
        }

        // Position the border to frame the 10x20 visible grid
        float borderX = _originOffset.x - _cellSize * 0.5f;
        float borderY = _originOffset.y - _cellSize * 0.5f;
        borderGo.transform.position = new Vector3(borderX, borderY, 0f);
        borderGo.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Instantiates a 10 x 20 grid of SpriteRenderer GameObjects for the
    /// visible rows (PlayfieldModel rows 2-21). Buffer rows 0-1 are not rendered.
    /// Each cell is a child GameObject named "Cell_[row]_[col]".
    /// </summary>
    private void CreateCellGrid()
    {
        int visibleHeight = GameRules.PLAYFIELD_VISIBLE_HEIGHT;
        int width = GameRules.PLAYFIELD_WIDTH;
        int bufferRows = GameRules.PLAYFIELD_BUFFER_ROWS;

        _cellRenderers = new SpriteRenderer[visibleHeight, width];

        for (int visualRow = 0; visualRow < visibleHeight; visualRow++)
        {
            int modelRow = bufferRows + visualRow;

            for (int col = 0; col < width; col++)
            {
                var cellGo = new GameObject("Cell_" + modelRow + "_" + col);
                cellGo.transform.SetParent(this.transform);

                var renderer = cellGo.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 0;

                // Position the cell in world space
                // Column: left to right from origin
                // Row: bottom to top from origin (row 2 is bottom, row 21 is top)
                float worldX = _originOffset.x + col * _cellSize;
                float worldY = _originOffset.y + visualRow * _cellSize;
                cellGo.transform.position = new Vector3(worldX, worldY, 0f);

                _cellRenderers[visualRow, col] = renderer;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Rendering
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when the GameplayController state changes to trigger a re-render.
    /// </summary>
    private void OnStateChanged()
    {
        // Render is already called every frame in Update();
        // This event hook ensures we re-render on state changes even if
        // Update is not running (e.g. paused).
        RenderPlayfield();
    }

    /// <summary>
    /// Renders the entire playfield: locked cells from the model and
    /// the active falling piece overlaid on top.
    /// </summary>
    private void RenderPlayfield()
    {
        if (_cellRenderers == null)
            return;

        PlayfieldModel model = null;
        PieceState activePiece = default;
        bool hasActivePiece = false;

        // Get playfield data from GameplayController if available
        if (_gameplayController != null)
        {
            model = _gameplayController.Playfield;
            activePiece = _gameplayController.ActivePiece;
            // Check if there's a valid active piece by verifying the type is defined
            hasActivePiece = activePiece.Type >= PieceType.I && activePiece.Type <= PieceType.L;
        }

        int visibleHeight = GameRules.PLAYFIELD_VISIBLE_HEIGHT;
        int width = GameRules.PLAYFIELD_WIDTH;
        int bufferRows = GameRules.PLAYFIELD_BUFFER_ROWS;

        // Track which cells are occupied by the active piece
        bool[,] activePieceCells = new bool[visibleHeight, width];

        if (hasActivePiece && model != null)
        {
            var offsets = PlayfieldModel.GetCellOffsets(activePiece.Type, activePiece.Rotation);
            int cellCount = offsets.Length / 2;

            for (int i = 0; i < cellCount; i++)
            {
                int modelRow = activePiece.Row + offsets[i * 2];
                int modelCol = activePiece.Col + offsets[i * 2 + 1];

                // Only mark cells in the visible area
                if (modelRow >= bufferRows && modelRow < bufferRows + visibleHeight &&
                    modelCol >= 0 && modelCol < width)
                {
                    int visualRow = modelRow - bufferRows;
                    activePieceCells[visualRow, modelCol] = true;
                }
            }
        }

        // Render each visible cell
        for (int visualRow = 0; visualRow < visibleHeight; visualRow++)
        {
            int modelRow = bufferRows + visualRow;

            for (int col = 0; col < width; col++)
            {
                int gridValue = 0;

                // Check if this cell is part of the active piece
                if (activePieceCells[visualRow, col])
                {
                    // Active piece cell: use the piece type sprite
                    gridValue = (int)activePiece.Type + 1;
                }
                else if (model != null)
                {
                    // Locked cell from the playfield model
                    gridValue = model.GetCell(modelRow, col);
                }

                // Assign the sprite based on grid value
                // gridValue 0 = empty, 1-7 = piece type
                SetCellSprite(_cellRenderers[visualRow, col], gridValue);
            }
        }
    }

    /// <summary>
    /// Sets the sprite on a SpriteRenderer based on the grid cell value.
    /// Value 0 = empty sprite (index 0), values 1-7 = piece type sprites (indices 1-7).
    /// </summary>
    /// <param name="renderer">The SpriteRenderer to update.</param>
    /// <param name="gridValue">The cell value: 0 for empty, 1-7 for piece types.</param>
    private void SetCellSprite(SpriteRenderer renderer, int gridValue)
    {
        if (_blockSprites == null || _blockSprites.Length == 0)
        {
            // No sprites assigned yet -- keep current sprite (will be null initially)
            return;
        }

        // Clamp gridValue to valid sprite index range [0, 7]
        int spriteIndex = Mathf.Clamp(gridValue, 0, _blockSprites.Length - 1);
        renderer.sprite = _blockSprites[spriteIndex];
    }
}
