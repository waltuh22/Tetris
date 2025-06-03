// Modified GameController with ghost block visibility control
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TetrisLogic;
using TetrisModel;

namespace TetrisController2
{
    public class GameController : IGameController
    {
        private readonly GameState _gameState;
        private readonly IGameView _view;
        private readonly ImageSource[] _tileImages;
        private readonly ImageSource[] _blockImages;
        private Image[,] _imageControls;

        public int CurrentScore => _gameState.Score;

        public event EventHandler GameOver;
        public event EventHandler<GameSettingsChangedEventArgs> SettingsChanged;

        public GameController(IGameView view, ImageSource[] tileImages, ImageSource[] blockImages)
        {
            _view = view;
            _tileImages = tileImages;
            _blockImages = blockImages;
            _gameState = new GameState();

            _view.KeyInput += (sender, e) => HandleInput(e.Key);
            _view.PlayAgainRequested += (sender, e) => StartNewGame();
            _view.ViewLoaded += (sender, e) => OnViewLoaded();
            _view.SaveSettingsRequested += (sender, e) => SaveSettings();
            _view.ResetSettingsRequested += (sender, e) => ResetSettings();
        }

        private void OnViewLoaded()
        {
            _imageControls = SetupGameCanvas();
            // Initialize settings in the view
            _view.UpdateSettings(_gameState.Settings);
        }

        private Image[,] SetupGameCanvas()
        {
            var grid = _gameState.GameGrid;
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    var imageControl = new Image { Width = cellSize, Height = cellSize };
                    _view.AddGameCanvasChild(imageControl, r, c, cellSize);
                    imageControls[r, c] = imageControl;
                }
            }
            return imageControls;
        }

        public void HandleInput(Key key)
        {
            if (_gameState.GameOver) return;
            // Check if we need to invert the rotate controls
            bool invertRotation = _gameState.Settings.InvertControls;

            switch (key)
            {
                case Key.Left:
                    if (invertRotation)
                        _gameState.MoveBlockRight();
                    else
                        _gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    if (invertRotation)
                        _gameState.MoveBlockLeft();
                    else
                        _gameState.MoveBlockRight();
                    break;
                case Key.Down: _gameState.MoveBlockDown(); break;
                case Key.Up:
                    if (invertRotation)
                        _gameState.RotateBlockCCW();
                    else
                        _gameState.RotateBlockCW();
                    break;
                case Key.Z:
                    if (invertRotation)
                        _gameState.RotateBlockCW();
                    else
                        _gameState.RotateBlockCCW();
                    break;
                case Key.C: _gameState.HoldBlock(); break;
                case Key.Space: _gameState.DropBlock(); break;
                default:
                    return;
            }
            UpdateView();
        }

        public void StartNewGame()
        {
            _gameState.Reset();
            _view.ShowGameScreen();
            _ = GameLoop();
        }

        private async Task GameLoop()
        {
            UpdateView();

            while (!_gameState.GameOver)
            {
                int delay = CalculateDelay();
                await Task.Delay(delay);
                _gameState.MoveBlockDown();
                UpdateView();
            }

            GameOver?.Invoke(this, EventArgs.Empty);
            _view.ShowGameOver(_gameState.Score);
        }

        private int CalculateDelay()
        {
            double factor = Math.Log10(_gameState.Score + 10); // +10 to avoid log(0)
            int delay = (int)(1000 - factor * 150); // slower reduction
            return Math.Max(150, delay);
        }

        private void UpdateView()
        {
            DrawGrid();
            if (_gameState.Settings.GhostBlockVisible)
            {
                DrawGhostBlock();
            }
            DrawCurrentBlock();
            DrawNextBlock();
            DrawHeldBlock();
            _view.UpdateScore(_gameState.Score);
        }

        private void DrawGrid()
        {
            var grid = _gameState.GameGrid;
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    _imageControls[r, c].Source = _tileImages[grid[r, c]];
                    _imageControls[r, c].Opacity = 1;
                }
            }
        }

        private void DrawCurrentBlock()
        {
            foreach (Position p in _gameState.CurrentBlock.TilesPositions())
            {
                _imageControls[p.Row, p.Column].Source = _tileImages[_gameState.CurrentBlock.Id];
                _imageControls[p.Row, p.Column].Opacity = 1;
            }
        }

        private void DrawGhostBlock()
        {
            int dropDistance = _gameState.BlockDropDistance();
            foreach (Position p in _gameState.CurrentBlock.TilesPositions())
            {
                _imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                _imageControls[p.Row + dropDistance, p.Column].Source = _tileImages[_gameState.CurrentBlock.Id];
            }
        }

        private void DrawNextBlock()
        {
            _view.UpdateNextBlockImage(_blockImages[_gameState.BlockQueue.NextBlock.Id]);
        }

        private void DrawHeldBlock()
        {
            _view.UpdateHeldBlockImage(_gameState.HeldBlock == null
                ? _blockImages[0]
                : _blockImages[_gameState.HeldBlock.Id]);
        }

        public void UpdateSettings(GameSettings newSettings)
        {
            _gameState.Settings = newSettings;
            // Notify view about settings change
            SettingsChanged?.Invoke(this, new GameSettingsChangedEventArgs(newSettings));
            UpdateView();
        }

        public void SaveSettings()
        {
            _gameState.Settings.SaveToFile();
        }

        public void ResetSettings()
        {
            _gameState.Settings.ResetToDefaults();
            SettingsChanged?.Invoke(this, new GameSettingsChangedEventArgs(_gameState.Settings));
            _gameState.Settings.SaveToFile();
            UpdateView();
        }
    }

    // Event arguments for settings changes
    public class GameSettingsChangedEventArgs : EventArgs
    {
        public GameSettings Settings { get; }

        public GameSettingsChangedEventArgs(GameSettings settings)
        {
            Settings = settings;
        }
    }

    public interface IGameController
    {
        void StartNewGame();
        void HandleInput(Key key);
        int CurrentScore { get; }
        event EventHandler GameOver;
        event EventHandler<GameSettingsChangedEventArgs> SettingsChanged;
        void UpdateSettings(GameSettings settings);
        void SaveSettings();
        void ResetSettings();
    }

    public interface IGameView
    {
        event EventHandler<KeyEventArgs> KeyInput;
        event EventHandler PlayAgainRequested;
        event EventHandler ViewLoaded;
        event EventHandler SaveSettingsRequested;
        event EventHandler ResetSettingsRequested;

        void AddGameCanvasChild(Image image, int row, int col, int cellSize);
        void UpdateScore(int score);
        void UpdateNextBlockImage(ImageSource image);
        void UpdateHeldBlockImage(ImageSource image);
        void ShowGameScreen();
        void ShowGameOver(int score);
        void UpdateSettings(GameSettings settings);
    }
}