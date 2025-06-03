// Modify GameState.cs to incorporate the settings
using TetrisModel;

namespace TetrisLogic
{
    public class GameState
    {
        private Block currentBlock;
        private GameSettings _settings;

        public Block CurrentBlock
        {
            get => currentBlock;
            private set
            {
                currentBlock = value;
                currentBlock.Reset();
                for (int i = 0; i < 2; i++)
                {
                    currentBlock.Move(1, 0);

                    if (!BlockFits())
                    {
                        currentBlock.Move(-1, 0);
                    }
                }
            }
        }

        public GameGrid GameGrid { get; private set; } // 22 rows, 10 columns
        public BlockQueue BlockQueue { get; private set; }
        public bool GameOver { get; private set; }
        public int Score { get; private set; }
        public Block HeldBlock { get; private set; }
        public bool CanHold { get; set; }

        // Add settings property
        public GameSettings Settings
        {
            get => _settings;
            set => _settings = value ?? new GameSettings();
        }

        public GameState()
        {
            GameGrid = new GameGrid(22, 10);
            BlockQueue = new BlockQueue();
            _settings = GameSettings.LoadFromFile();
            CurrentBlock = BlockQueue.GetAndUpdate();
            CanHold = true;
        }

        private bool BlockFits()
        {
            foreach (Position p in CurrentBlock.TilesPositions())
            {
                if (!GameGrid.IsEmpty(p.Row, p.Column))
                {
                    return false;
                }
            }

            return true;
        }

        public void HoldBlock()
        {
            // Check if holding is enabled in settings
            if (!CanHold || !_settings.HoldingBlockEnabled)
            {
                return;
            }

            if (HeldBlock == null)
            {
                HeldBlock = CurrentBlock;
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
            else
            {
                Block tmp = CurrentBlock;
                CurrentBlock = HeldBlock;
                HeldBlock = tmp;
            }

            CanHold = false;
        }

        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();

            if (!BlockFits())
            {
                CurrentBlock.RotateCCW();
            }
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();

            if (!BlockFits())
            {
                CurrentBlock.RotateCW();
            }
        }

        public void MoveBlockLeft()
        {
            CurrentBlock.Move(0, -1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
            }
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, -1);
            }
        }

        public bool IsGameOver()
        {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
        }

        private int CalculateScore(int linesCleared)
        {
            int baseScore;
            switch (linesCleared)
            {
                case 1: baseScore = 40; break;
                case 2: baseScore = 100; break;
                case 3: baseScore = 300; break;
                case 4: baseScore = 1200; break;
                default: baseScore = 0; break;
            }

            // Apply the points multiplier from settings
            return baseScore * _settings.PointsMultiplier;
        }

        private void PlaceBlock()
        {
            foreach (Position p in CurrentBlock.TilesPositions())
            {
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            int linesCleared = GameGrid.ClearFullRows();
            Score += CalculateScore(linesCleared);

            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;
            }
        }

        public void MoveBlockDown()
        {
            CurrentBlock.Move(1, 0);

            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
            }
        }

        private int TilesDropDistance(Position p)
        {
            int drop = 0;

            while (GameGrid.IsEmpty(p.Row + drop + 1, p.Column))
            {
                drop++;
            }

            return drop;
        }

        public int BlockDropDistance()
        {
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilesPositions())
            {
                drop = System.Math.Min(drop, TilesDropDistance(p));
            }

            return drop;
        }

        public void DropBlock()
        {
            // Check if dropping is enabled in settings
            if (!_settings.DropBlockEnabled)
            {
                return;
            }

            CurrentBlock.Move(BlockDropDistance(), 0);
            PlaceBlock();
        }

        public void Reset()
        {
            for (int r = 0; r < GameGrid.Rows; r++)
            {
                for (int c = 0; c < GameGrid.Columns; c++)
                {
                    GameGrid[r, c] = 0;
                }
            }
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
            HeldBlock = null;
            Score = 0;
            GameOver = false;
            CanHold = true;
        }
    }
}