using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{

    public class SignLetter
    {
        public int Index { get; set;}
        public TMPro.TextMeshPro LetterObj { get; set; }
        public float Width { get; set; }
    }

    public class OpponentAction
    {
        public string actionId { get; set; }
    }

    public class UpdateTile
    {
        public int index { get; set; }
        public string playerOccupied { get; set; }
        public int tileAction { get; set; } = 0;
        public string letter { get; set; }
        public float actionTime { get; set; }
    }

    public class GameUpdateAction
    {
        public string gameId { get; set; }
        public double createdAt { get; set; }
        public int action { get; set; }
        public List<UpdateTile> tiles { get; set; }
        public bool OponnentUpdated { get; set; }
        public string oponnentId { get; set; }

        public string word { get; set; }
        public bool resetTimer { get; set; }
    }

    public class Word
    {
        public string word { get; set; } = string.Empty;
        public int numberOfLetters { get; set; } = 0;
        public decimal complexityratio { get; set; } = 0;
    }




    public class BoardTile
    {
        public PosVector posVector { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int tileNumber { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public bool isObjectiveTile { get; set; }
        public string playerInitial { get; set; }
        public string playerOccupied { get; set; }
        public string letter { get; set; }
        public TileAction action { get; set; } = TileAction.None;
        public GameTileState tileState { get; set; } = GameTileState.Unselected;
        //public bool IsCurrentPlayerTile { get; set; } = false;
        //public bool IsCurrentOpponentTile { get; set; } = false;        
        //public GameActor actor { get; set; } = GameActor.None;
        public int index { get; set; }
    }



    public class PosVector
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class GameBoard
    {
        public string gameId { get; set; }
        public List<BoardTile> boardTiles { get; set; }
    }



    public class Game
    {
        public string gameId { get; set; }
        public GameData data { get; set; }
    }



    public class GameData
    {
        public GameStatus status { get; set; } = GameStatus.Pending;
        public GameType type { get; set; } = GameType.Multiplayer;
        public string langCode { get; set; }
        public double createdAt { get; set; }
        public Dictionary<string, GamePlayerData> playersInfo { get; set; }
        public GameBoard gameBoard { get; set; } = new GameBoard();
    }


    public class GamePlayerData
    {
        public string userName { get; set; }
        public int level { get; set; }
        public bool master { get; set; }
        public bool gameBoardLoaded { get; set; }
    }

    public class PlayerWaitRoom
    {

        public string userName { get; set; }
        public int level { get; set; } = 0;
        public string langCode { get; set; } = "es-ES";
        public double createdAt { get; set; }
    }

    public class UserStatistics
    {
        public decimal avscorepergame { get; set; } = 0;
        public decimal avtimepergame { get; set; } = 0;
        public decimal avwongames { get; set; } = 0;
        public List<GameStatistics> gamestatistics { get; set; } = new List<GameStatistics>();

    }

    public class GameStatistics
    {
        public Dictionary<string, GameStatisticsData> gamestatisticsData { get; set; } = new Dictionary<string, GameStatisticsData>();
    }

    public class GameStatisticsData
    {
        public GameReult result { get; set; } = GameReult.unfinished;
        public double startedat { get; set; }
        public double finishedat { get; set; } = -1;
        public decimal avcomplexityratio { get; set; } = 0;
        public decimal avlettersinwords { get; set; } = 0;
        public string opponentId { get; set; }
        public List<string> words { get; set; } = new List<string>();

    }


    /* NIVELES  */

    //[Serializable]
    //public class PlayerGameLevel
    //{
    //    public Dictionary<gameLevel, Dictionary<GameMode, List<PlayerGameLevelPhase>>> LevelPhaseStatus { get; set; }

    //    public gameLevel CurrentLevel { get; set; }
    //}

    //[Serializable]
    //public class PlayerGameLevelPhase
    //{
    //    public int FaseNumber { get; set; }
    //    public bool Complete { get; set; } = false;
    //}

    //public class LevelGame
    //{
    //    public gameLevel Level { get; set; }
    //    public List<LevelGameModeData> LevelsMode { get; set; }
    //}



    //public class LevelGameModeData
    //{
    //    public GameMode Mode { get; set; }

    //    public List<GameLevelPhase> Fases { get; set; }
    //}

    //public class GameLevelPhase
    //{
    //    public int FaseNumber { get; set; }
    //    public Dictionary<GameDifficulty, float> DifficultyValues { get; set; }
    //    public List<GameTool> Tools { get; set; }
    //}

    /* NIVELES  */

    public class GameTool
    {
        public GameAidTool Tool { get; set; }
        public GameAidToolModeType ToolMode { get; set; }
        public short NumEquiped { get; set; }
        public float RechargeTime { get; set; }
    }


    public class WordCombination
    {
        public string Letters { get; set; }
        public List<Hex> Cells { get; set; }
        public Dictionary<int, char> Changes { get; set; }

        public WordCombination(string letters, List<Hex> cells)
        {
            Letters = letters;
            Cells = cells;
            Changes = new Dictionary<int, char>();
        }
    }

}
