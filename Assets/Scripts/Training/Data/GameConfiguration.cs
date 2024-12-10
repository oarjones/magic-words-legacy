using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{

    public static class GameConfiguration
    {
        //public static List<LevelGame> GameLevels = new()
        //{
        //    new LevelGame()
        //    {
        //        Level = gameLevel.Grifo,
        //        LevelsMode = new List<LevelGameModeData>()
        //        {
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.CatchLetter,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1,
        //                        DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 8.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 2, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 7.5f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 40.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 3, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 7.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 50.0f}
        //                        }
        //                    }
        //                }
        //            },
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.PointsChallenge,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 8.0f },
        //                            { GameDifficulty.PointsToWin, 25f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 2, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 7.5f },
        //                            { GameDifficulty.PointsToWin, 275f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 40.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 3, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 7.0f },
        //                            { GameDifficulty.PointsToWin, 300f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 50.0f}
        //                        }
        //                    }
        //                }
        //            },
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.VsAlgorithm,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 8.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f},
        //                            new GameTool() { Tool = GameAidTool.FreezeTrap, ToolMode = GameAidToolModeType.ByNumEquiped, NumEquiped=1, RechargeTime = 0f},
        //                            new GameTool() { Tool = GameAidTool.MoveToTile, ToolMode = GameAidToolModeType.ByNumEquiped, NumEquiped=1, RechargeTime = 0f}
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    },
        //    new LevelGame()
        //    {
        //        Level = gameLevel.Fenix,
        //        LevelsMode = new List<LevelGameModeData>()
        //        {
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.CatchLetter,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1,
        //                        DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 6.5f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 2, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 6.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 40.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 3, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 5.5f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 4f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 50.0f}
        //                        }
        //                    }
        //                }
        //            },
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.PointsChallenge,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 6.5f },
        //                            { GameDifficulty.PointsToWin, 150.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 2, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 6f },
        //                            { GameDifficulty.PointsToWin, 175.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 40.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 3, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 5.5f },
        //                            { GameDifficulty.PointsToWin, 200.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 4f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 50.0f}
        //                        }
        //                    }
        //                }
        //            },
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.NLetterWordChallenge,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.WordNumLetters, 6 },
        //                            { GameDifficulty.Timer, 7.0f },
        //                            { GameDifficulty.PointsToWin, 150.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 2, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.WordNumLetters, 6 },
        //                            { GameDifficulty.Timer, 6.0f },
        //                            { GameDifficulty.PointsToWin, 150.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 40.0f}
        //                        }
        //                    },
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 3, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.WordNumLetters, 6 },
        //                            { GameDifficulty.Timer, 5.0f },
        //                            { GameDifficulty.PointsToWin, 150.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.08f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 50.0f}
        //                        }
        //                    }
        //                }
        //            },                    
        //            new LevelGameModeData()
        //            {
        //                Mode = GameMode.VsAlgorithm,
        //                Fases = new List<GameLevelPhase>()
        //                {
        //                    new GameLevelPhase()
        //                    {
        //                        FaseNumber = 1, DifficultyValues = new Dictionary<GameDifficulty, float>
        //                        {
        //                            { GameDifficulty.Timer, 8.0f },
        //                            { GameDifficulty.GameBoardSize, 4f },
        //                            { GameDifficulty.EmptyTiles, 0f },
        //                            { GameDifficulty.TileChangeLetter, 0f },
        //                            { GameDifficulty.LetterComplexity, 0.05f },
        //                            { GameDifficulty.WordMinLetter, 3f },
        //                            { GameDifficulty.RepeatWord, 1f }
        //                        },
        //                        Tools = new List<GameTool>()
        //                        {
        //                            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f}
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //};
    }

    //public static class GameLevelIcon
    //{
    //    public static Dictionary<int, string> LevelIcon = new Dictionary<int, string>()
    //    {
    //        { 1, "" },
    //        { 2, "" }
    //    };
    //}

}
