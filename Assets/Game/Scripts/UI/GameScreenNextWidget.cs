using Game.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenNextWidget : MonoBehaviour
{
    [SerializeField] private Sprite[] _blockSprites = new Sprite[8];

    private Image[] _cells;

    public Sprite[] BlockSprites
    {
        get => _blockSprites;
        set => _blockSprites = value;
    }

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var nextRegion = root.Q("next-region");
        if (nextRegion != null)
            PopulateRegion(nextRegion);
    }

    public void PopulateRegion(VisualElement nextRegion)
    {
        nextRegion.Clear();
        nextRegion.style.alignItems = Align.Center;
        nextRegion.style.justifyContent = Justify.Center;

        var header = new Label("NEXT");
        nextRegion.Add(header);

        var gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Column;
        nextRegion.Add(gridContainer);

        _cells = new Image[16];
        for (int row = 0; row < 4; row++)
        {
            var rowElement = new VisualElement();
            rowElement.style.flexDirection = FlexDirection.Row;
            gridContainer.Add(rowElement);
            for (int col = 0; col < 4; col++)
            {
                var cell = new Image();
                if (_blockSprites != null && _blockSprites.Length > 0)
                    cell.sprite = _blockSprites[0];
                _cells[row * 4 + col] = cell;
                rowElement.Add(cell);
            }
        }
    }

    public void UpdateNextPiece(PieceType nextPiece)
    {
        if (_cells == null) return;

        Sprite emptySprite = (_blockSprites != null && _blockSprites.Length > 0) ? _blockSprites[0] : null;
        for (int i = 0; i < 16; i++)
            _cells[i].sprite = emptySprite;

        int typeIndex = (int)nextPiece;
        int spriteIndex = typeIndex + 1;
        Sprite pieceSprite = (_blockSprites != null && _blockSprites.Length > spriteIndex) ? _blockSprites[spriteIndex] : null;

        for (int cellIdx = 0; cellIdx < 4; cellIdx++)
        {
            int row = PlayfieldModel.CellOffsets[typeIndex, 0, cellIdx * 2];
            int col = PlayfieldModel.CellOffsets[typeIndex, 0, cellIdx * 2 + 1];
            if (row < 4 && col < 4)
                _cells[row * 4 + col].sprite = pieceSprite;
        }
    }
}
