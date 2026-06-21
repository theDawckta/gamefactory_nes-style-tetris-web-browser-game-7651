using UnityEngine;

namespace Game.Gameplay
{
    public class PlayfieldRenderer : MonoBehaviour
    {
        public float CellSize = 0.5f;
        public Vector2 OriginOffset = Vector2.zero;
        public Sprite[] BlockSprites;
        public Sprite WellBorderSprite;
        public GameplayController GameplayController;

        public SpriteRenderer[,] Cells { get; private set; }
        public SpriteRenderer WellBorder { get; private set; }

        private void Awake()
        {
            CreateGrid();
            CreateWellBorder();
        }

        private void Update()
        {
            Render();
        }

        private void CreateGrid()
        {
            Cells = new SpriteRenderer[GameRules.PLAYFIELD_VISIBLE_HEIGHT, GameRules.PLAYFIELD_WIDTH];
            for (int visRow = 0; visRow < GameRules.PLAYFIELD_VISIBLE_HEIGHT; visRow++)
            {
                int modelRow = visRow + GameRules.PLAYFIELD_BUFFER_ROWS;
                for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
                {
                    var cellGo = new GameObject($"Cell_{modelRow}_{col}");
                    cellGo.transform.SetParent(transform);
                    float x = OriginOffset.x + col * CellSize;
                    float y = OriginOffset.y - visRow * CellSize;
                    cellGo.transform.localPosition = new Vector3(x, y, 0f);
                    Cells[visRow, col] = cellGo.AddComponent<SpriteRenderer>();
                }
            }
        }

        private void CreateWellBorder()
        {
            var borderGo = new GameObject("WellBorder");
            borderGo.transform.SetParent(transform);
            float centerX = OriginOffset.x + (GameRules.PLAYFIELD_WIDTH - 1) * CellSize * 0.5f;
            float centerY = OriginOffset.y - (GameRules.PLAYFIELD_VISIBLE_HEIGHT - 1) * CellSize * 0.5f;
            borderGo.transform.localPosition = new Vector3(centerX, centerY, 0f);
            WellBorder = borderGo.AddComponent<SpriteRenderer>();
            WellBorder.sprite = WellBorderSprite;
            WellBorder.sortingOrder = -1;
        }

        public void Render()
        {
            if (Cells == null || GameplayController == null) return;

            var playfield = GameplayController.Playfield;

            for (int visRow = 0; visRow < GameRules.PLAYFIELD_VISIBLE_HEIGHT; visRow++)
            {
                int modelRow = visRow + GameRules.PLAYFIELD_BUFFER_ROWS;
                for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
                {
                    int cellValue = playfield.GetCell(modelRow, col);
                    if (BlockSprites != null && cellValue < BlockSprites.Length)
                        Cells[visRow, col].sprite = BlockSprites[cellValue];
                }
            }

            var activePiece = GameplayController.ActivePiece;
            int typeIndex = (int)activePiece.Type;
            int rot = activePiece.Rotation % 4;

            for (int i = 0; i < 4; i++)
            {
                int modelRow = activePiece.Row + PlayfieldModel.CellOffsets[typeIndex, rot, i * 2];
                int col = activePiece.Col + PlayfieldModel.CellOffsets[typeIndex, rot, i * 2 + 1];
                int visRow = modelRow - GameRules.PLAYFIELD_BUFFER_ROWS;

                if (visRow >= 0 && visRow < GameRules.PLAYFIELD_VISIBLE_HEIGHT &&
                    col >= 0 && col < GameRules.PLAYFIELD_WIDTH)
                {
                    int spriteIndex = typeIndex + 1;
                    if (BlockSprites != null && spriteIndex < BlockSprites.Length)
                        Cells[visRow, col].sprite = BlockSprites[spriteIndex];
                }
            }
        }
    }
}
