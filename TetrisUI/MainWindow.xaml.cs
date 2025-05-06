using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TetrisController2;

namespace TetrisUI
{
    public partial class MainWindow : Window, IGameView
    {
        public event EventHandler<KeyEventArgs> KeyInput;
        public event EventHandler PlayAgainRequested;
        public event EventHandler ViewLoaded;

        private readonly GameController _controller;
        private readonly ImageSource[] _tileImages;
        private readonly ImageSource[] _blockImages;

        public MainWindow()
        {
            InitializeComponent();

            _tileImages = LoadTileImages();
            _blockImages = LoadBlockImages();

            _controller = new GameController(this, _tileImages, _blockImages);
            _controller.GameOver += OnGameOver;

            ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        private ImageSource[] LoadTileImages()
        {
            return new ImageSource[]
            {
                new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
            };
        }

        private ImageSource[] LoadBlockImages()
        {
            return new ImageSource[]
            {
                new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
                new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
            };
        }

        public void AddGameCanvasChild(Image image, int row, int col, int cellSize)
        {
            Canvas.SetTop(image, (row - 2) * cellSize + 10);
            Canvas.SetLeft(image, col * cellSize);
            GameCanvas.Children.Add(image);
        }

        public void UpdateScore(int score) => ScoreText.Text = $"Punkty: {score}";
        public void UpdateNextBlockImage(ImageSource image) => NextImage.Source = image;
        public void UpdateHeldBlockImage(ImageSource image) => HoldImage.Source = image;

        public void ShowGameScreen()
        {
            GameScreen.Visibility = Visibility.Visible;
            MainMenu.Visibility = Visibility.Hidden;
            RankingScreen.Visibility = Visibility.Hidden;
        }

        public void ShowGameOver(int score)
        {
            FinalScoreText.Text = $"Score: {score}";
            GameOverMenu.Visibility = Visibility.Visible;
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => ShowGameOver(_controller.CurrentScore));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) => KeyInput?.Invoke(sender, e);
        private void PlayAgain_Click(object sender, RoutedEventArgs e) => PlayAgainRequested?.Invoke(sender, e);
        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void Ranking_Click(object sender, RoutedEventArgs e)
        {
            // Your existing ranking screen logic
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            // Your existing back to menu logic
        }

        private void SubmitScore_Click(object sender, RoutedEventArgs e)
        {
            string playerName = PlayerNameInput.Text.Trim();
            if (!string.IsNullOrEmpty(playerName))
            {
                _ranking.Add((playerName, _controller.CurrentScore));
                _ranking = _ranking.OrderByDescending(entry => entry.Item2).Take(20).ToList();

                SaveRanking();

                UpdateRankingDisplay();

                NameInputPanel.Visibility = Visibility.Hidden;
            }
        }

        private List<(string, int)> _ranking = new List<(string, int)>();

        private void SaveRanking()
        {
            File.WriteAllLines("ranking.txt", _ranking.Select(x => $"{x.Item1},{x.Item2}"));
        }

        private void UpdateRankingDisplay()
        {
            RankingList.Items.Clear();
            foreach (var entry in _ranking)
            {
                RankingList.Items.Add($"{entry.Item1} - {entry.Item2}");
            }
        }

        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
            {
                await Task.Run(() => _controller.StartNewGame());
            }
        }
    }
}