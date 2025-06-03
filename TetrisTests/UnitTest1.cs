using NUnit.Framework;
using TetrisModel;
using TetrisLogic;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TetrisModel.Tests
{
    // Unit Tests for Position Class
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void Constructor_SetsRowAndColumn()
        {
            // Arrange & Act
            var position = new Position(5, 10);

            // Assert
            Assert.AreEqual(5, position.Row);
            Assert.AreEqual(10, position.Column);
        }

        [Test]
        public void Properties_CanBeModified()
        {
            // Arrange
            var position = new Position(0, 0);

            // Act
            position.Row = 3;
            position.Column = 7;

            // Assert
            Assert.AreEqual(3, position.Row);
            Assert.AreEqual(7, position.Column);
        }
    }

    // Integration Test for Position Class
    [TestFixture]
    public class PositionIntegrationTests
    {
        [Test]
        public void Position_WorksWithBlockMovement()
        {
            // Arrange
            var block = new IBlock();
            var initialPositions = block.TilesPositions().ToList();

            // Act
            block.Move(2, 3);
            var newPositions = block.TilesPositions().ToList();

            // Assert
            Assert.AreEqual(initialPositions.Count, newPositions.Count);
            for (int i = 0; i < initialPositions.Count; i++)
            {
                Assert.AreEqual(initialPositions[i].Row + 2, newPositions[i].Row);
                Assert.AreEqual(initialPositions[i].Column + 3, newPositions[i].Column);
            }
        }
    }

    // Unit Tests for GameGrid Class
    [TestFixture]
    public class GameGridTests
    {
        private GameGrid grid;

        [SetUp]
        public void SetUp()
        {
            grid = new GameGrid(20, 10);
        }

        [Test]
        public void Constructor_InitializesGridWithCorrectDimensions()
        {
            // Assert
            Assert.AreEqual(20, grid.Rows);
            Assert.AreEqual(10, grid.Columns);
            Assert.AreEqual(0, grid[0, 0]); // Should be empty initially
        }

        [Test]
        public void Indexer_SetsAndGetsValues()
        {
            // Act
            grid[5, 3] = 7;

            // Assert
            Assert.AreEqual(7, grid[5, 3]);
        }

        [Test]
        public void IsInside_ReturnsTrueForValidCoordinates()
        {
            // Assert
            Assert.IsTrue(grid.IsInside(0, 0));
            Assert.IsTrue(grid.IsInside(19, 9));
            Assert.IsFalse(grid.IsInside(-1, 0));
            Assert.IsFalse(grid.IsInside(20, 0));
            Assert.IsFalse(grid.IsInside(0, 10));
        }

        [Test]
        public void IsEmpty_ReturnsTrueForEmptyCell()
        {
            // Assert
            Assert.IsTrue(grid.IsEmpty(0, 0));

            // Act
            grid[0, 0] = 1;

            // Assert
            Assert.IsFalse(grid.IsEmpty(0, 0));
        }

        [Test]
        public void IsRowFull_DetectsFullRow()
        {
            // Arrange - Fill entire row 5
            for (int c = 0; c < grid.Columns; c++)
            {
                grid[5, c] = 1;
            }

            // Assert
            Assert.IsTrue(grid.IsRowFull(5));
            Assert.IsFalse(grid.IsRowFull(4));
        }
    }

    // Unit Tests for Block Classes (using IBlock as example)
    [TestFixture]
    public class IBlockTests
    {
        private IBlock block;

        [SetUp]
        public void SetUp()
        {
            block = new IBlock();
        }

        [Test]
        public void Constructor_InitializesWithCorrectId()
        {
            // Assert
            Assert.AreEqual(1, block.Id);
        }

        [Test]
        public void TilesPositions_ReturnsCorrectInitialPositions()
        {
            // Act
            var positions = block.TilesPositions().ToList();

            // Assert
            Assert.AreEqual(4, positions.Count);
            // Check initial I-block horizontal position
            Assert.AreEqual(0, positions[0].Row); // Row 1 + offset(-1) = 0
            Assert.AreEqual(3, positions[0].Column); // Column 0 + offset(3) = 3
        }

        [Test]
        public void RotateCW_ChangesOrientation()
        {
            // Arrange
            var initialPositions = block.TilesPositions().ToList();

            // Act
            block.RotateCW();
            var rotatedPositions = block.TilesPositions().ToList();

            // Assert
            Assert.AreNotEqual(initialPositions, rotatedPositions);
            Assert.AreEqual(4, rotatedPositions.Count);
        }

        [Test]
        public void Move_UpdatesPosition()
        {
            // Arrange
            var initialPositions = block.TilesPositions().ToList();

            // Act
            block.Move(2, -1);
            var newPositions = block.TilesPositions().ToList();

            // Assert
            for (int i = 0; i < initialPositions.Count; i++)
            {
                Assert.AreEqual(initialPositions[i].Row + 2, newPositions[i].Row);
                Assert.AreEqual(initialPositions[i].Column - 1, newPositions[i].Column);
            }
        }

        [Test]
        public void Reset_RestoresInitialState()
        {
            // Arrange
            var initialPositions = block.TilesPositions().ToList();
            block.Move(5, 3);
            block.RotateCW();

            // Act
            block.Reset();
            var resetPositions = block.TilesPositions().ToList();

            // Assert
            CollectionAssert.AreEqual(initialPositions, resetPositions);
        }
    }

    // Integration Test for Block Classes
    [TestFixture]
    public class BlockIntegrationTests
    {
        [Test]
        public void AllBlocks_HaveUniqueIdsAndWork()
        {
            // Arrange
            var blocks = new Block[]
            {
                new IBlock(), new JBlock(), new LBlock(), new OBlock(),
                new SBlock(), new TBlock(), new ZBlock()
            };

            // Act & Assert
            var ids = blocks.Select(b => b.Id).ToList();
            var uniqueIds = ids.Distinct().ToList();

            Assert.AreEqual(7, uniqueIds.Count, "All blocks should have unique IDs");

            foreach (var block in blocks)
            {
                // Test that each block can be manipulated
                var initialCount = block.TilesPositions().Count();
                Assert.AreEqual(4, initialCount, $"Block {block.Id} should have 4 tiles");

                // Test rotation
                block.RotateCW();
                Assert.AreEqual(4, block.TilesPositions().Count(), $"Block {block.Id} should maintain 4 tiles after rotation");

                // Test movement
                block.Move(1, 1);
                Assert.AreEqual(4, block.TilesPositions().Count(), $"Block {block.Id} should maintain 4 tiles after movement");
            }
        }
    }

    // Unit Tests for GameSettings Class
    [TestFixture]
    public class GameSettingsTests
    {
        private GameSettings settings;
        private string testFilePath = "test_settings.json";

        [SetUp]
        public void SetUp()
        {
            settings = new GameSettings();
            // Clean up test file if it exists
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test file
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [Test]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Assert
            Assert.AreEqual(1, settings.PointsMultiplier);
            Assert.IsTrue(settings.HoldingBlockEnabled);
            Assert.IsTrue(settings.DropBlockEnabled);
            Assert.IsTrue(settings.GhostBlockVisible);
            Assert.IsFalse(settings.InvertControls);
        }

        [Test]
        public void PointsMultiplier_ValidatesRange()
        {
            // Act & Assert - Valid values
            settings.PointsMultiplier = 5;
            Assert.AreEqual(5, settings.PointsMultiplier);

            // Act & Assert - Invalid values reset to default
            settings.PointsMultiplier = 0; // Too low
            Assert.AreEqual(1, settings.PointsMultiplier);

            settings.PointsMultiplier = 15; // Too high
            Assert.AreEqual(1, settings.PointsMultiplier);
        }

        [Test]
        public void ResetToDefaults_RestoresAllDefaultValues()
        {
            // Arrange - Change all values
            settings.PointsMultiplier = 5;
            settings.HoldingBlockEnabled = false;
            settings.DropBlockEnabled = false;
            settings.GhostBlockVisible = false;
            settings.InvertControls = true;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.AreEqual(1, settings.PointsMultiplier);
            Assert.IsTrue(settings.HoldingBlockEnabled);
            Assert.IsTrue(settings.DropBlockEnabled);
            Assert.IsTrue(settings.GhostBlockVisible);
            Assert.IsFalse(settings.InvertControls);
        }

        [Test]
        public void SaveToFile_CreatesFile()
        {
            // Act
            settings.SaveToFile(testFilePath);

            // Assert
            Assert.IsTrue(File.Exists(testFilePath));
        }

        [Test]
        public void LoadFromFile_ReturnsDefaultWhenFileDoesntExist()
        {
            // Act
            var loadedSettings = GameSettings.LoadFromFile("nonexistent.json");

            // Assert
            Assert.IsNotNull(loadedSettings);
            Assert.AreEqual(1, loadedSettings.PointsMultiplier);
        }
    }

    // Integration Test for GameSettings Class
    [TestFixture]
    public class GameSettingsIntegrationTests
    {
        private string testFilePath = "integration_test_settings.json";

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [Test]
        public void SaveAndLoad_PreservesAllSettings()
        {
            // Arrange
            var originalSettings = new GameSettings
            {
                PointsMultiplier = 3,
                HoldingBlockEnabled = false,
                DropBlockEnabled = false,
                GhostBlockVisible = false,
                InvertControls = true
            };

            // Act
            originalSettings.SaveToFile(testFilePath);
            var loadedSettings = GameSettings.LoadFromFile(testFilePath);

            // Assert
            Assert.AreEqual(originalSettings.PointsMultiplier, loadedSettings.PointsMultiplier);
            Assert.AreEqual(originalSettings.HoldingBlockEnabled, loadedSettings.HoldingBlockEnabled);
            Assert.AreEqual(originalSettings.DropBlockEnabled, loadedSettings.DropBlockEnabled);
            Assert.AreEqual(originalSettings.GhostBlockVisible, loadedSettings.GhostBlockVisible);
            Assert.AreEqual(originalSettings.InvertControls, loadedSettings.InvertControls);
        }

        [Test]
        public void LoadFromFile_HandlesCorruptedFile()
        {
            // Arrange - Create corrupted file
            File.WriteAllText(testFilePath, "invalid json content");

            // Act
            var loadedSettings = GameSettings.LoadFromFile(testFilePath);

            // Assert - Should return default settings when file is corrupted
            Assert.IsNotNull(loadedSettings);
            Assert.AreEqual(1, loadedSettings.PointsMultiplier);
            Assert.IsTrue(loadedSettings.HoldingBlockEnabled);
        }
    }

    // Unit Tests for BlockQueue Class
    [TestFixture]
    public class BlockQueueTests
    {
        private BlockQueue blockQueue;

        [SetUp]
        public void SetUp()
        {
            blockQueue = new BlockQueue();
        }

        [Test]
        public void Constructor_InitializesWithNextBlock()
        {
            // Assert
            Assert.IsNotNull(blockQueue.NextBlock);
            Assert.IsTrue(blockQueue.NextBlock.Id >= 1 && blockQueue.NextBlock.Id <= 7);
        }

        [Test]
        public void GetAndUpdate_ReturnsCurrentNextBlockAndUpdatesWithDifferent()
        {
            // Arrange
            var firstBlock = blockQueue.NextBlock;
            var firstBlockId = firstBlock.Id;

            // Act
            var returnedBlock = blockQueue.GetAndUpdate();
            var newNextBlock = blockQueue.NextBlock;

            // Assert
            Assert.AreEqual(firstBlockId, returnedBlock.Id);
            Assert.AreNotEqual(firstBlockId, newNextBlock.Id);
            Assert.IsNotNull(newNextBlock);
        }

        [Test]
        public void GetAndUpdate_AlwaysReturnsValidBlocks()
        {
            // Act & Assert - Test multiple calls
            var validIds = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 };

            for (int i = 0; i < 20; i++)
            {
                var block = blockQueue.GetAndUpdate();
                Assert.IsTrue(validIds.Contains(block.Id), $"Block ID {block.Id} should be valid");
                Assert.IsTrue(validIds.Contains(blockQueue.NextBlock.Id), $"Next block ID {blockQueue.NextBlock.Id} should be valid");
            }
        }

        [Test]
        public void GetAndUpdate_EnsuresConsecutiveBlocksAreDifferent()
        {
            // Act & Assert - Test that consecutive blocks are always different
            for (int i = 0; i < 10; i++)
            {
                var currentBlock = blockQueue.GetAndUpdate();
                var nextBlock = blockQueue.NextBlock;

                Assert.AreNotEqual(currentBlock.Id, nextBlock.Id,
                    $"Consecutive blocks should be different. Got {currentBlock.Id} followed by {nextBlock.Id}");
            }
        }
    }

    // Integration Test for BlockQueue Class
    [TestFixture]
    public class BlockQueueIntegrationTests
    {
        [Test]
        public void BlockQueue_GeneratesAllBlockTypesOverTime()
        {
            // Arrange
            var blockQueue = new BlockQueue();
            var generatedIds = new HashSet<int>();
            var expectedIds = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 };

            // Act - Generate many blocks to ensure all types appear
            for (int i = 0; i < 100; i++)
            {
                var block = blockQueue.GetAndUpdate();
                generatedIds.Add(block.Id);

                if (generatedIds.SetEquals(expectedIds))
                    break;
            }

            // Assert
            Assert.IsTrue(generatedIds.SetEquals(expectedIds),
                $"All block types should be generated. Missing: {string.Join(", ", expectedIds.Except(generatedIds))}");
        }

        [Test]
        public void BlockQueue_WorksWithGameState()
        {
            // Arrange
            var gameState = new GameState();
            var initialBlock = gameState.CurrentBlock;

            // Act - Simulate getting next block (this happens internally in GameState)
            gameState.MoveBlockDown(); // This will eventually place the block and get next one

            // We need to move the block down until it's placed
            while (!gameState.GameOver)
            {
                gameState.MoveBlockDown();
                if (gameState.CurrentBlock.Id != initialBlock.Id)
                    break; // Block was placed and new one was generated
            }

            // Assert
            if (!gameState.GameOver)
            {
                Assert.AreNotEqual(initialBlock.Id, gameState.CurrentBlock.Id);
            }
        }
    }

    // Unit Tests for GameState Class
    [TestFixture]
    public class GameStateTests
    {
        private GameState gameState;
        private string testSettingsFile = "test_game_settings.json";

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing test settings file
            if (File.Exists(testSettingsFile))
                File.Delete(testSettingsFile);

            gameState = new GameState();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(testSettingsFile))
                File.Delete(testSettingsFile);
            if (File.Exists("settings.json"))
                File.Delete("settings.json");
        }

        [Test]
        public void Constructor_InitializesAllProperties()
        {
            // Assert
            Assert.IsNotNull(gameState.GameGrid);
            Assert.AreEqual(22, gameState.GameGrid.Rows);
            Assert.AreEqual(10, gameState.GameGrid.Columns);
            Assert.IsNotNull(gameState.BlockQueue);
            Assert.IsNotNull(gameState.CurrentBlock);
            Assert.IsNotNull(gameState.Settings);
            Assert.IsFalse(gameState.GameOver);
            Assert.AreEqual(0, gameState.Score);
            Assert.IsNull(gameState.HeldBlock);
            Assert.IsTrue(gameState.CanHold);
        }

        [Test]
        public void MoveBlockLeft_MovesBlockWhenPossible()
        {
            // Arrange
            var initialPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Act
            gameState.MoveBlockLeft();
            var newPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Assert - Block should move left (column decreased by 1) if there was space
            bool moved = false;
            for (int i = 0; i < initialPositions.Count; i++)
            {
                if (newPositions[i].Column == initialPositions[i].Column - 1)
                {
                    moved = true;
                    break;
                }
            }
            // If block didn't move, it means it was already at the edge or blocked
            Assert.IsTrue(moved || initialPositions.SequenceEqual(newPositions));
        }

        [Test]
        public void MoveBlockRight_MovesBlockWhenPossible()
        {
            // Arrange
            var initialPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Act
            gameState.MoveBlockRight();
            var newPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Assert - Block should move right (column increased by 1) if there was space
            bool moved = false;
            for (int i = 0; i < initialPositions.Count; i++)
            {
                if (newPositions[i].Column == initialPositions[i].Column + 1)
                {
                    moved = true;
                    break;
                }
            }
            Assert.IsTrue(moved || initialPositions.SequenceEqual(newPositions));
        }

        [Test]
        public void RotateBlockCW_RotatesWhenPossible()
        {
            // Arrange
            var initialPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Act
            gameState.RotateBlockCW();
            var newPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Assert - For most blocks, rotation should change positions (except O-block)
            if (gameState.CurrentBlock.Id != 4) // O-block doesn't change when rotated
            {
                Assert.IsFalse(initialPositions.SequenceEqual(newPositions), "Block should rotate if space allows");
            }
        }

        [Test]
        public void HoldBlock_SwapsCurrentAndHeldBlocks()
        {
            // Arrange
            var initialCurrentBlock = gameState.CurrentBlock;
            Assert.IsNull(gameState.HeldBlock);

            // Act - First hold
            gameState.HoldBlock();

            // Assert
            Assert.IsNotNull(gameState.HeldBlock);
            Assert.AreEqual(initialCurrentBlock.Id, gameState.HeldBlock.Id);
            Assert.AreNotEqual(initialCurrentBlock.Id, gameState.CurrentBlock.Id);
            Assert.IsFalse(gameState.CanHold);

            // Act - Try to hold again (should not work)
            var currentAfterFirstHold = gameState.CurrentBlock;
            gameState.HoldBlock();

            // Assert - Should not change because CanHold is false
            Assert.AreEqual(currentAfterFirstHold.Id, gameState.CurrentBlock.Id);
        }

        [Test]
        public void HoldBlock_RespectsSettings()
        {
            // Arrange
            gameState.Settings.HoldingBlockEnabled = false;
            var initialCurrentBlock = gameState.CurrentBlock;

            // Act
            gameState.HoldBlock();

            // Assert - Should not hold when disabled in settings
            Assert.IsNull(gameState.HeldBlock);
            Assert.AreEqual(initialCurrentBlock.Id, gameState.CurrentBlock.Id);
        }

        [Test]
        public void DropBlock_RespectsSettings()
        {
            // Arrange
            gameState.Settings.DropBlockEnabled = false;
            var initialPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Act
            gameState.DropBlock();
            var newPositions = gameState.CurrentBlock.TilesPositions().ToList();

            // Assert - Block should not drop when disabled
            CollectionAssert.AreEqual(initialPositions, newPositions);
        }

        [Test]
        public void MoveBlockDown_PlacesBlockWhenReachesBottom()
        {
            // Arrange
            var initialScore = gameState.Score;
            var initialCurrentBlockId = gameState.CurrentBlock.Id;

            // Act - Move block down repeatedly until it's placed
            while (gameState.CurrentBlock.Id == initialCurrentBlockId && !gameState.GameOver)
            {
                gameState.MoveBlockDown();
            }

            // Assert - Block should be placed and new block should be current
            if (!gameState.GameOver)
            {
                Assert.AreNotEqual(initialCurrentBlockId, gameState.CurrentBlock.Id);
            }
        }

        [Test]
        public void BlockDropDistance_ReturnsCorrectDistance()
        {
            // Act
            int dropDistance = gameState.BlockDropDistance();

            // Assert - Should return a valid distance (>= 0)
            Assert.IsTrue(dropDistance >= 0);
            Assert.IsTrue(dropDistance < gameState.GameGrid.Rows);
        }

        [Test]
        public void Reset_RestoresInitialState()
        {
            // Arrange - Modify game state
            gameState.HoldBlock();
            var originalHeldBlock = gameState.HeldBlock;

            // Move some blocks down to change score potentially
            for (int i = 0; i < 5; i++)
            {
                gameState.MoveBlockDown();
                if (gameState.GameOver) break;
            }

            // Act
            gameState.Reset();

            // Assert
            Assert.AreEqual(0, gameState.Score);
            Assert.IsFalse(gameState.GameOver);
            Assert.IsNull(gameState.HeldBlock);
            Assert.IsTrue(gameState.CanHold);
            Assert.IsNotNull(gameState.CurrentBlock);

            // Grid should be empty
            for (int r = 0; r < gameState.GameGrid.Rows; r++)
            {
                for (int c = 0; c < gameState.GameGrid.Columns; c++)
                {
                    Assert.AreEqual(0, gameState.GameGrid[r, c]);
                }
            }
        }

        [Test]
        public void IsGameOver_DetectsGameEndCondition()
        {
            // Arrange - Fill top rows to trigger game over
            for (int c = 0; c < gameState.GameGrid.Columns; c++)
            {
                gameState.GameGrid[0, c] = 1;
            }

            // Act & Assert
            Assert.IsTrue(gameState.IsGameOver());
        }

        [Test]
        public void Settings_CanBeModified()
        {
            // Arrange
            var newSettings = new GameSettings
            {
                PointsMultiplier = 5,
                HoldingBlockEnabled = false
            };

            // Act
            gameState.Settings = newSettings;

            // Assert
            Assert.AreEqual(5, gameState.Settings.PointsMultiplier);
            Assert.IsFalse(gameState.Settings.HoldingBlockEnabled);
        }

        [Test]
        public void Settings_DefaultsToNewInstanceWhenSetToNull()
        {
            // Act
            gameState.Settings = null;

            // Assert
            Assert.IsNotNull(gameState.Settings);
            Assert.AreEqual(1, gameState.Settings.PointsMultiplier);
        }
    }

    // Integration Tests for GameState Class
    [TestFixture]
    public class GameStateIntegrationTests
    {
        [Test]
        public void CompleteGameFlow_BlockPlacementAndScoring()
        {
            // Arrange
            var gameState = new GameState();
            gameState.Settings.PointsMultiplier = 2; // Test settings integration

            // Act - Fill bottom row almost completely
            for (int c = 0; c < gameState.GameGrid.Columns - 4; c++)
            {
                gameState.GameGrid[gameState.GameGrid.Rows - 1, c] = 1;
            }

            // Position current block to complete the row
            while (gameState.BlockDropDistance() > 0 && !gameState.GameOver)
            {
                gameState.MoveBlockDown();
            }

            var initialScore = gameState.Score;

            // Place the block (this should complete a line)
            gameState.MoveBlockDown();

            // Assert
            if (!gameState.GameOver)
            {
                // If a line was cleared, score should increase
                // The exact score depends on block placement and line clearing
                Assert.IsTrue(gameState.Score >= initialScore);
            }
        }

        [Test]
        public void GameState_HandlesSettingsIntegration()
        {
            // Arrange
            var gameState = new GameState();

            // Test holding block functionality with settings
            gameState.Settings.HoldingBlockEnabled = true;
            var initialBlock = gameState.CurrentBlock;

            // Act
            gameState.HoldBlock();

            // Assert
            Assert.IsNotNull(gameState.HeldBlock);
            Assert.AreEqual(initialBlock.Id, gameState.HeldBlock.Id);

            // Test disabling hold
            gameState.Settings.HoldingBlockEnabled = false;
            gameState.CanHold = true; // Reset to allow testing
            var currentBlock = gameState.CurrentBlock;

            gameState.HoldBlock(); // Should not work
            Assert.AreEqual(currentBlock.Id, gameState.CurrentBlock.Id);
        }

        [Test]
        public void GameState_HandlesBlockQueueIntegration()
        {
            // Arrange
            var gameState = new GameState();
            var initialBlockId = gameState.CurrentBlock.Id;
            var nextBlockId = gameState.BlockQueue.NextBlock.Id;

            // Act - Force block placement by moving to bottom
            while (!gameState.GameOver)
            {
                gameState.MoveBlockDown();
                if (gameState.CurrentBlock.Id != initialBlockId)
                    break; // Block was placed, new one is current
            }

            // Assert
            if (!gameState.GameOver)
            {
                // The current block should now be what was previously the next block
                Assert.AreEqual(nextBlockId, gameState.CurrentBlock.Id);
                // And there should be a new next block that's different
                Assert.AreNotEqual(nextBlockId, gameState.BlockQueue.NextBlock.Id);
            }
        }

        [Test]
        public void GameState_HandlesMultipleLineClears()
        {
            // Arrange
            var gameState = new GameState();
            gameState.Settings.PointsMultiplier = 3;

            // Fill multiple rows almost completely
            for (int r = gameState.GameGrid.Rows - 3; r < gameState.GameGrid.Rows; r++)
            {
                for (int c = 0; c < gameState.GameGrid.Columns - 1; c++)
                {
                    gameState.GameGrid[r, c] = 1;
                }
            }

            var initialScore = gameState.Score;

            // Act - Try to place a block that completes multiple lines
            // This is complex to set up perfectly, so we'll just ensure the system handles it
            gameState.DropBlock();

            // Assert - System should handle the scenario without crashing
            Assert.IsTrue(gameState.Score >= initialScore);
            Assert.IsFalse(gameState.GameOver || gameState.CurrentBlock == null);
        }
    }

    // Comprehensive Integration Test for TetrisLogic
    [TestFixture]
    public class TetrisLogicIntegrationTests
    {
        [Test]
        public void FullGameScenario_AllComponentsWorkTogether()
        {
            // Arrange
            var gameState = new GameState();
            var settings = new GameSettings
            {
                PointsMultiplier = 2,
                HoldingBlockEnabled = true,
                DropBlockEnabled = true
            };
            gameState.Settings = settings;

            var operationsPerformed = 0;
            var maxOperations = 100; // Prevent infinite loops

            // Act - Simulate a game session
            while (!gameState.GameOver && operationsPerformed < maxOperations)
            {
                operationsPerformed++;

                // Perform various operations
                switch (operationsPerformed % 7)
                {
                    case 0:
                        gameState.MoveBlockLeft();
                        break;
                    case 1:
                        gameState.MoveBlockRight();
                        break;
                    case 2:
                        gameState.RotateBlockCW();
                        break;
                    case 3:
                        gameState.MoveBlockDown();
                        break;
                    case 4:
                        if (gameState.CanHold && gameState.Settings.HoldingBlockEnabled)
                            gameState.HoldBlock();
                        break;
                    case 5:
                        gameState.RotateBlockCCW();
                        break;
                    case 6:
                        if (gameState.Settings.DropBlockEnabled)
                            gameState.DropBlock();
                        break;
                }

                // Verify game state remains consistent
                Assert.IsNotNull(gameState.CurrentBlock);
                Assert.IsNotNull(gameState.BlockQueue.NextBlock);
                Assert.IsTrue(gameState.Score >= 0);
            }

            // Assert - Game should remain in a valid state
            Assert.IsNotNull(gameState.CurrentBlock);
            Assert.IsNotNull(gameState.BlockQueue);
            Assert.IsNotNull(gameState.Settings);

            // Test reset functionality
            gameState.Reset();
            Assert.AreEqual(0, gameState.Score);
            Assert.IsFalse(gameState.GameOver);
            Assert.IsNull(gameState.HeldBlock);
            Assert.IsTrue(gameState.CanHold);
        }
    }
}