// First, let's create a Settings model class to hold our settings
using System.Text.Json;

namespace TetrisModel
{
    public class GameSettings
    {
        // Default values
        private const int DefaultPointsMultiplier = 1;
        private const bool DefaultHoldingBlockEnabled = true;
        private const bool DefaultDropBlockEnabled = true;
        private const bool DefaultGhostBlockVisible = true;
        private const bool DefaultInvertControls = false;

        // Constraints
        private const int MinPointsMultiplier = 1;
        private const int MaxPointsMultiplier = 10;

        // Properties with validation
        private int _pointsMultiplier = DefaultPointsMultiplier;
        public int PointsMultiplier
        {
            get => _pointsMultiplier;
            set => _pointsMultiplier = ValidatePointsMultiplier(value);
        }

        public bool HoldingBlockEnabled { get; set; } = DefaultHoldingBlockEnabled;
        public bool DropBlockEnabled { get; set; } = DefaultDropBlockEnabled;
        public bool GhostBlockVisible { get; set; } = DefaultGhostBlockVisible;
        public bool InvertControls { get; set; } = DefaultInvertControls;

        // Constructor defaults to default values
        public GameSettings()
        {
            ResetToDefaults();
        }

        // Validate that the points multiplier is within the valid range
        private int ValidatePointsMultiplier(int value)
        {
            if (value < MinPointsMultiplier || value > MaxPointsMultiplier)
                return DefaultPointsMultiplier;
            return value;
        }

        // Reset all settings to their default values
        public void ResetToDefaults()
        {
            _pointsMultiplier = DefaultPointsMultiplier;
            HoldingBlockEnabled = DefaultHoldingBlockEnabled;
            DropBlockEnabled = DefaultDropBlockEnabled;
            GhostBlockVisible = DefaultGhostBlockVisible;
            InvertControls = DefaultInvertControls;
        }

        // Save settings to a JSON file
        public void SaveToFile(string filePath = "settings.json")
        {
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }

        // Load settings from a JSON file
        public static GameSettings LoadFromFile(string filePath = "settings.json")
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<GameSettings>(jsonString);

                    // Ensure loaded settings meet validation requirements
                    settings.PointsMultiplier = settings.ValidatePointsMultiplier(settings.PointsMultiplier);

                    return settings;
                }
            }
            catch (Exception)
            {
                // If there's any error, return default settings
            }

            return new GameSettings();
        }
    }
}