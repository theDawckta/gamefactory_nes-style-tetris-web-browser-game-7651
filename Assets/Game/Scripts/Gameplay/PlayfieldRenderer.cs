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

        private Vector2 _gridOrigin;
        private Sprite[] _effectiveSprites;

        // NES Tetris palette: index 0=empty, 1-7 map to piece types I,O,T,S,Z,J,L
        private static readonly Color[] NesColors =
        {
            Color.clear,
            new Color(0.00f, 0.73f, 1.00f), // I  — cyan
            new Color(1.00f, 0.89f, 0.00f), // O  — yellow
            new Color(0.67f, 0.00f, 1.00f), // T  — purple
            new Color(0.00f, 0.80f, 0.00f), // S  — green
            new Color(1.00f, 0.07f, 0.07f), // Z  — red
            new Color(0.00f, 0.00f, 0.80f), // J  — blue
            new Color(1.00f, 0.50f, 0.00f), // L  — orange
        };

        private void Awake()
        {
            // InitializeSprites deferred to first Render() so BlockSprites can be
            // assigned after construction (e.g., from Inspector or PlayMode tests)
            ComputeGridOrigin();
            CreateGrid();
            CreateWellBorder();
        }

        private void InitializeSprites()
        {
            // Use BlockSprites only when all 8 slots are populated with non-null sprites
            if (BlockSprites != null && BlockSprites.Length >= 8 && BlockSprites[1] != null)
            {
                _effectiveSprites = BlockSprites;
                return;
            }

            // 8×8 pixel texture: 1-pixel dark border on every edge so adjacent blocks
            // of the same piece look like separate cells rather than a solid blob.
            const int texSize = 8;
            float ppu = texSize / CellSize; // sprite occupies exactly 1 cell in world space
            _effectiveSprites = new Sprite[8];
            _effectiveSprites[0] = null;
            for (int i = 1; i < 8; i++)
            {
                Color main = NesColors[i];
                Color border = new Color(main.r * 0.35f, main.g * 0.35f, main.b * 0.35f);

                var tex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);
                tex.filterMode = FilterMode.Point;
                for (int px = 0; px < texSize; px++)
                    for (int py = 0; py < texSize; py++)
                    {
                        bool onBorder = px == 0 || px == texSize - 1 || py == 0 || py == texSize - 1;
                        tex.SetPixel(px, py, onBorder ? border : main);
                    }
                tex.Apply();
                _effectiveSprites[i] = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.one * 0.5f, ppu);
            }
        }

        // Computes the local-space position of the top-left grid cell so the well
        // is centered on Camera.main. OriginOffset fine-tunes from that center.
        private void ComputeGridOrigin()
        {
            Vector2 camLocal;
            if (Camera.main != null)
            {
                Vector3 camWorld = Camera.main.transform.position;
                Vector3 camLocalPos = transform.InverseTransformPoint(camWorld);
                camLocal = new Vector2(camLocalPos.x, camLocalPos.y);
            }
            else
            {
                camLocal = Vector2.zero;
            }

            float halfW = (GameRules.PLAYFIELD_WIDTH - 1) * CellSize * 0.5f;
            float halfH = (GameRules.PLAYFIELD_VISIBLE_HEIGHT - 1) * CellSize * 0.5f;
            _gridOrigin = new Vector2(camLocal.x - halfW, camLocal.y + halfH) + OriginOffset;
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
                    float x = _gridOrigin.x + col * CellSize;
                    float y = _gridOrigin.y - visRow * CellSize;
                    cellGo.transform.localPosition = new Vector3(x, y, 0f);
                    Cells[visRow, col] = cellGo.AddComponent<SpriteRenderer>();
                }
            }
        }

        private void CreateWellBorder()
        {
            var borderGo = new GameObject("WellBorder");
            borderGo.transform.SetParent(transform);
            float centerX = _gridOrigin.x + (GameRules.PLAYFIELD_WIDTH - 1) * CellSize * 0.5f;
            float centerY = _gridOrigin.y - (GameRules.PLAYFIELD_VISIBLE_HEIGHT - 1) * CellSize * 0.5f;
            borderGo.transform.localPosition = new Vector3(centerX, centerY, 0f);
            WellBorder = borderGo.AddComponent<SpriteRenderer>();
            WellBorder.sprite = WellBorderSprite;
            WellBorder.sortingOrder = -1;
        }

        public void Render()
        {
            if (Cells == null || GameplayController == null) return;
            if (_effectiveSprites == null) InitializeSprites();

            var playfield = GameplayController.Playfield;

            for (int visRow = 0; visRow < GameRules.PLAYFIELD_VISIBLE_HEIGHT; visRow++)
            {
                int modelRow = visRow + GameRules.PLAYFIELD_BUFFER_ROWS;
                for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
                {
                    int cellValue = playfield.GetCell(modelRow, col);
                    if (_effectiveSprites != null && cellValue < _effectiveSprites.Length)
                        Cells[visRow, col].sprite = _effectiveSprites[cellValue];
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
                    if (_effectiveSprites != null && spriteIndex < _effectiveSprites.Length)
                        Cells[visRow, col].sprite = _effectiveSprites[spriteIndex];
                }
            }
        }
    }
}
