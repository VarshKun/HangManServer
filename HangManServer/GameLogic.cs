using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HangManServer
{
    class GameLogic
    {
        [Serializable]
        private enum wordCategory
        {
            General,
            Animals,
            Foods,
            Games,
            Jobs,
            Objects,
            Pokemon
        }

        [Serializable]
        private enum MatchStatus
        {
            Waiting,
            Started,
            Ended
        }

        [Serializable]
        private struct Player
        {
            public string name;
            public short avatarIndex;
            public short score;
            public uint Id;
        }

        [Serializable]
        private struct Match
        {
            public uint Id;
            public short maxplayers;
            public short maxscore;
            public List<Player> players;
            public wordCategory category;
            public List<string> wordlist;
            public MatchStatus status;
        }

        private Dictionary<wordCategory, string[]> wordDictionary = new Dictionary<wordCategory, string[]>() { 
            { wordCategory.General, new [] { "ability", "absolutely" } },
            { wordCategory.Animals, new [] { "dog", "cat" } },
            { wordCategory.Foods,  new [] { "pizza", "orange" } },
            { wordCategory.Games,  new [] { "billiard", "carrom" } },
            { wordCategory.Jobs, new [] { "policeman", "fireman" } },
            { wordCategory.Objects,  new [] { "ball", "phone" } },
            { wordCategory.Pokemon,  new [] { "charizard", "bulbasaur", "mewtoo" } },
        };
       
        private static List<Match> matchList;

        private static System.Random random;

        public GameLogic()
        {
            random = new System.Random();
            matchList = new List<Match>();
        }

        public string GetResponse(string request)
        {
            string[] requestData = request.Split(new char[] { '/' });

            if (requestData.Length == 0)
                return @"{""error"":""blank request""}";

            switch(requestData[0])
            {
                case "startmatch":
                

                case "updatescore":
                    //request format: updatescore/[matchId]]/[playerId]/[score]
                    if (requestData.Length != 4)
                        return @"{""error"":""missing request data""}";

                    uint matchId;
                    if (!uint.TryParse(requestData[1], out matchId))
                        return @"{""error"":""bad matchId""}";

                    uint playerId;
                    if (!uint.TryParse(requestData[2], out playerId))
                        return @"{""error"":""bad playerId""}";

                    short newscore;
                    if (!short.TryParse(requestData[3], out newscore))
                        return @"{""error"":""bad score""}";

                    if (!matchList.Any(m => m.Id == matchId))
                        return @"{""error"":""matchId not found""}";

                    Match m1 = matchList.FirstOrDefault(m => m.Id == matchId);
                    
                    if(!m1.players.Any(p => p.Id == playerId))
                        return @"{""error"":""playerId not found""}";

                    var players = matchList.FirstOrDefault(m => m.Id == matchId).players;
                    Player p = players.FirstOrDefault(p => p.Id == playerId);

                    matchList.FirstOrDefault(m => m.Id == matchId).players.Remove(p);                    
                    p.score = newscore;
                    matchList.FirstOrDefault(m => m.Id == matchId).players.Add(p);

                    Console.WriteLine($"Player {p.name} with id {p.Id} score updated to {newscore}...");

                    return @"{""updatescore"":""ok""}";

                case "matchstatus":
                    //request format: matchstatus/[matchId]]
                    if (requestData.Length != 2)
                        return @"{""error"":""missing request data""}";

                    uint matchId2;
                    if(!uint.TryParse(requestData[1], out matchId2))
                        return @"{""error"":""bad matchId""}";                                      

                    if (!matchList.Any(m => m.Id == matchId2))
                        return @"{""error"":""matchId not found""}";

                    Match m = matchList.FirstOrDefault(m => m.Id == matchId2);

                    string json = JsonConvert.SerializeObject(m);

                    Console.WriteLine($"Query matchstatus for {matchId2}");
                    return json;

                case "joinmatch":
                    //request format: joinmatch/[matchId]/[playername]/[avatarIndex]
                    if (requestData.Length != 4)
                        return @"{""error"":""missing request data""}";

                    uint mjoinId;
                    if (!uint.TryParse(requestData[1], out mjoinId))
                        return @"{""error"":""bad matchId""}";

                    if (!matchList.Any(m => m.Id == mjoinId))
                        return @"{""error"":""matchId not found""}";

                    Match matchToJoin = matchList.FirstOrDefault(m => m.Id == mjoinId);

                    string _playername = requestData[2];

                    short _avatarIndex;
                    if (!short.TryParse(requestData[3], out _avatarIndex))
                        return @"{""error"":""bad avatarIndex""}";

                    if (matchToJoin.status == MatchStatus.Waiting)
                    {
                        Player newPlayer = new Player()
                        {
                            avatarIndex = _avatarIndex,
                            name = _playername,
                            score = 0
                        };

                        uint randPlayerId = (uint)random.Next(999999, 999999999);
                        while (matchToJoin.players.Any(p => p.Id == randPlayerId))
                        {
                            randPlayerId = (uint)random.Next(999999, 999999999);
                        }

                        newPlayer.Id = randPlayerId;
                        matchList.FirstOrDefault(m => m.Id == mjoinId).players.Add(newPlayer);

                        Console.WriteLine($"New Player {_playername} joined match {mjoinId}");

                        return $"{{\"playerid\":\"{newPlayer.Id}\"}}";
                    }
                    else
                        return @"{""error"":""match started""}";

                case "newmatch":
                    //request format: newmatch/[category]/[maxplayers]/[maxscore]
                    if (requestData.Length != 4)
                        return @"{""error"":""missing request data""}";

                    wordCategory matchCategory;
                    if (!Enum.TryParse(requestData[1], true, out matchCategory))
                        return @"{""error"":""bad category""}";

                    short _maxplayers = 0;
                    if (!short.TryParse(requestData[2], out _maxplayers))
                        return @"{""error"":""bad maxplayers""}";

                    short _maxscore = 0;
                    if (!short.TryParse(requestData[3], out _maxscore))
                        return @"{""error"":""bad maxscore""}";

                    uint randMatchId = (uint)random.Next(10000, 999999);
                    while(matchList.Any(m => m.Id == randMatchId))
                    {
                        randMatchId = (uint)random.Next(10000, 999999);                        
                    }
                    matchList.Add(new Match()
                    {
                        Id = randMatchId,
                        category = matchCategory,
                        maxplayers = _maxplayers,
                        maxscore = _maxscore,
                        players = new List<Player>(),
                        wordlist = new List<string>(),
                        status = MatchStatus.Waiting
                    });

                    Console.WriteLine($"New match created: {randMatchId} category: {matchCategory} maxplayers: {_maxplayers} maxscore: {_maxscore}");

                    return $"{{\"matchId\":\"{randMatchId}\"}}";                    
                
                default:
                    return @"{""error"":""unknown request""}";
            }            
        }
    }
}
