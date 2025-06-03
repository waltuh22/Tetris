using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TetrisController2;
using TetrisModel;

namespace TetrisUI
{
    public partial class MainWindow : Window, IGameView
    {
        public event EventHandler<KeyEventArgs> KeyInput;
        public event EventHandler PlayAgainRequested;
        public event EventHandler ViewLoaded;
        public event EventHandler SaveSettingsRequested;
        public event EventHandler ResetSettingsRequested;

        private readonly GameController _controller;
        private readonly ImageSource[] _tileImages;
        private readonly ImageSource[] _blockImages;
        private List<(string Name, int Score)> _ranking = new List<(string, int)>();

        public MainWindow()
        {
            InitializeComponent();
            LoadRanking();

            _tileImages = LoadTileImages();
            _blockImages = LoadBlockImages();

            _controller = new GameController(this, _tileImages, _blockImages);
            _controller.GameOver += OnGameOver;
            _controller.SettingsChanged += OnSettingsChanged;

            // Make sure Settings Screen is initially hidden
            if (SettingsScreen != null)
            {
                SettingsScreen.Visibility = Visibility.Hidden;
            }

            // Initially show the main menu
            ShowMainMenu();

            this.Loaded += (s, e) => ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void LoadRanking()
        {
            if (File.Exists("ranking.txt"))
            {
                _ranking = File.ReadAllLines("ranking.txt")
                    .Select(line => line.Split(','))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                    .Select(parts => (parts[0], int.Parse(parts[1])))
                    .OrderByDescending(entry => entry.Item2)
                    .Take(20)
                    .ToList();
            }
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

        // IGameView implementation
        public void AddGameCanvasChild(Image image, int row, int col, int cellSize)
        {
            Dispatcher.Invoke(() =>
            {
                Canvas.SetTop(image, (row - 2) * cellSize + 10);
                Canvas.SetLeft(image, col * cellSize);
                GameCanvas.Children.Add(image);
            });
        }

        public void UpdateScore(int score) => Dispatcher.Invoke(() => ScoreText.Text = $"Punkty: {score}");
        public void UpdateNextBlockImage(ImageSource image) => Dispatcher.Invoke(() => NextImage.Source = image);
        public void UpdateHeldBlockImage(ImageSource image) => Dispatcher.Invoke(() => HoldImage.Source = image);

        public void ShowMainMenu()
        {
            Dispatcher.Invoke(() =>
            {
                GameScreen.Visibility = Visibility.Hidden;
                MainMenu.Visibility = Visibility.Visible;
                RankingScreen.Visibility = Visibility.Hidden;
                SettingsScreen.Visibility = Visibility.Hidden;
            });
        }

        public void ShowGameScreen()
        {
            Dispatcher.Invoke(() =>
            {
                GameScreen.Visibility = Visibility.Visible;
                MainMenu.Visibility = Visibility.Hidden;
                RankingScreen.Visibility = Visibility.Hidden;
                SettingsScreen.Visibility = Visibility.Hidden;
            });
        }

        public void ShowGameOver(int score)
        {
            Dispatcher.Invoke(() =>
            {
                FinalScoreText.Text = $"Zdobyte punkty: {score}";
                GameScreen.Visibility = Visibility.Hidden;
                MainMenu.Visibility = Visibility.Hidden;
                RankingScreen.Visibility = Visibility.Visible;
                SettingsScreen.Visibility = Visibility.Hidden;
                NameInputPanel.Visibility = Visibility.Visible;
                UpdateRankingDisplay();
            });
        }

        public void ShowSettingsScreen()
        {
            Dispatcher.Invoke(() =>
            {
                GameScreen.Visibility = Visibility.Hidden;
                MainMenu.Visibility = Visibility.Hidden;
                RankingScreen.Visibility = Visibility.Hidden;
                SettingsScreen.Visibility = Visibility.Visible;
            });
        }

        public void UpdateSettings(GameSettings settings)
        {
            Dispatcher.Invoke(() =>
            {
                PointsMultiplierSlider.Value = settings.PointsMultiplier;
                PointsMultiplierValue.Text = $"{settings.PointsMultiplier}x";
                HoldingBlockEnabledCheckbox.IsChecked = settings.HoldingBlockEnabled;
                DropBlockEnabledCheckbox.IsChecked = settings.DropBlockEnabled;
                GhostBlockVisibleCheckbox.IsChecked = settings.GhostBlockVisible;
                InvertControlsCheckbox.IsChecked = settings.InvertControls;
            });
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            ShowGameOver(_controller.CurrentScore);
        }

        private void OnSettingsChanged(object sender, GameSettingsChangedEventArgs e)
        {
            UpdateSettings(e.Settings);
        }

        // UI Event Handlers
        private void Window_KeyDown(object sender, KeyEventArgs e) => KeyInput?.Invoke(sender, e);

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            ShowGameScreen();
            PlayAgainRequested?.Invoke(sender, e);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void Ranking_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                GameScreen.Visibility = Visibility.Hidden;
                MainMenu.Visibility = Visibility.Hidden;
                RankingScreen.Visibility = Visibility.Visible;
                SettingsScreen.Visibility = Visibility.Hidden;
                NameInputPanel.Visibility = Visibility.Hidden;
                UpdateRankingDisplay();
            });
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsScreen();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
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

        private void SaveRanking()
        {
            File.WriteAllLines("ranking.txt", _ranking.Select(entry => $"{entry.Name},{entry.Score}"));
        }

        private void UpdateRankingDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                RankingList.Items.Clear();
                foreach (var entry in _ranking)
                {
                    RankingList.Items.Add($"{entry.Name} - {entry.Score}");
                }
            });
        }

        // Settings Event Handlers
        private void PointsMultiplier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Check if UI is initialized
            if (PointsMultiplierValue == null || _controller == null) return;

            int value = (int)e.NewValue;
            PointsMultiplierValue.Text = $"{value}x";

            try
            {
                // Create updated settings
                GameSettings updatedSettings = CreateCurrentSettings();
                updatedSettings.PointsMultiplier = value;

                // Update controller with new settings
                _controller.UpdateSettings(updatedSettings);
            }
            catch (Exception ex)
            {
                // Just silently handle the exception during initialization
                System.Diagnostics.Debug.WriteLine($"Settings update error: {ex.Message}");
            }
        }

        private void HoldingBlockEnabled_Changed(object sender, RoutedEventArgs e)
        {
            if (HoldingBlockEnabledCheckbox == null || _controller == null) return;

            try
            {
                // Create updated settings
                GameSettings updatedSettings = CreateCurrentSettings();
                updatedSettings.HoldingBlockEnabled = HoldingBlockEnabledCheckbox.IsChecked ?? true;

                // Update controller with new settings
                _controller.UpdateSettings(updatedSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings update error: {ex.Message}");
            }
        }

        private void DropBlockEnabled_Changed(object sender, RoutedEventArgs e)
        {
            if (DropBlockEnabledCheckbox == null || _controller == null) return;

            try
            {
                // Create updated settings
                GameSettings updatedSettings = CreateCurrentSettings();
                updatedSettings.DropBlockEnabled = DropBlockEnabledCheckbox.IsChecked ?? true;

                // Update controller with new settings
                _controller.UpdateSettings(updatedSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings update error: {ex.Message}");
            }
        }

        private void GhostBlockVisible_Changed(object sender, RoutedEventArgs e)
        {
            if (GhostBlockVisibleCheckbox == null || _controller == null) return;

            try
            {
                // Create updated settings
                GameSettings updatedSettings = CreateCurrentSettings();
                updatedSettings.GhostBlockVisible = GhostBlockVisibleCheckbox.IsChecked ?? true;

                // Update controller with new settings
                _controller.UpdateSettings(updatedSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings update error: {ex.Message}");
            }
        }

        private void InvertControls_Changed(object sender, RoutedEventArgs e)
        {
            if (InvertControlsCheckbox == null || _controller == null) return;

            try
            {
                // Create updated settings
                GameSettings updatedSettings = CreateCurrentSettings();
                updatedSettings.InvertControls = InvertControlsCheckbox.IsChecked ?? false;

                // Update controller with new settings
                _controller.UpdateSettings(updatedSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings update error: {ex.Message}");
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsRequested?.Invoke(sender, e);
            MessageBox.Show("Ustawienia zosta³y zapisane.", "Zapisano", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettingsRequested?.Invoke(sender, e);
        }

        private GameSettings CreateCurrentSettings()
        {
            // Create a new default settings object first
            var settings = new GameSettings();

            // Only modify properties if controls are initialized
            if (PointsMultiplierSlider != null)
                settings.PointsMultiplier = (int)PointsMultiplierSlider.Value;

            if (HoldingBlockEnabledCheckbox != null)
                settings.HoldingBlockEnabled = HoldingBlockEnabledCheckbox.IsChecked ?? true;

            if (DropBlockEnabledCheckbox != null)
                settings.DropBlockEnabled = DropBlockEnabledCheckbox.IsChecked ?? true;

            if (GhostBlockVisibleCheckbox != null)
                settings.GhostBlockVisible = GhostBlockVisibleCheckbox.IsChecked ?? true;

            if (InvertControlsCheckbox != null)
                settings.InvertControls = InvertControlsCheckbox.IsChecked ?? false;

            return settings;
        }
    }
}