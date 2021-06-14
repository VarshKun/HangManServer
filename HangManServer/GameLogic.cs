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
            public Dictionary<uint, Player> players;
            public wordCategory category;
            public List<string> wordlist;
            public MatchStatus status;
        }

        private Dictionary<wordCategory, string[]> wordDictionary = new Dictionary<wordCategory, string[]>() { 
            { wordCategory.General, new [] { "ability", "microwave", "unknown","thriftless","bagpipes","awkward","fuchsia","jigsaw","pneumonia","fluffiness","abruptly","equip","jackpot","avenue","joyful","scratch","ivory","jogging","keyhole","rickshaw","cobweb","quiz","oak","joker","huge","jazz"} },
            { wordCategory.Animals, new [] { "dog", "alpaca","donkey","kangaroo","nymph","orangutan","panda","squirell","vulture","zebra","tadpole","butterfly","wasp","yak","pelican","leopard","macaw","parrot","rabbit","hamster","firefly","eagle","coyote","albatross","chipmunk","dolphin"} },
            { wordCategory.Foods,  new [] { "lasagna", "sandwich","barbecue","eggplant","gingerbread","asparagus","grapefruit","lobster","marshmallow","spaghetti","watermelon","pepperoni","oyster","mozarella","dragonfruit","cauliflower","biscuit","coleslaw","mayonnaise","breadfruit","noodles","oatmeal","pumpkin","quinoa","tapioca","watercress" } },
            { wordCategory.Games,  new [] { "billiard", "carrom","bowling","basketball","pubg","rugby","football","chess","minecraft","volleyball","baseball","valorant","hockey","archery","cricket","fortnite","puzzle","apex","surfing","cyberpunk","poker","domino","golf","snowboarding","ludo","scrabble"} },
            { wordCategory.Jobs, new [] { "policeman", "fireman","babysitter","dentist","investigator","musician","artist","receptionist","pilot","zoologist","politician","optician","accountant","carpenter","dermatologist","hairdresser","nurse","referee","sailor","teacher","programmer","linguist","lawyer","gardener","chef","barber","psychologist" } },
            { wordCategory.Objects,  new [] { "ball", "phone","diary","bottle","tissue","glasses","watch","photo","stamp","camera","pencil","dictionary","toothbrush","wallet","lipstick","purse","scissors","notebook","newspaper","laptop","eraser","calculator","umbrella","key","coin","jewelry","knife","tablecloth","shoe","magnet","flashlight","blanket","handkerchief"} },
            { wordCategory.Pokemon,  new [] { "charizard", "bulbasaur", "mewtoo","calyrex","spectrier","glastrier","regidrago","dragapult","pikachu","charmander","squirtle","charmeleon","butterfree","pidgey","raichu","clefairy","jigglypuff","golbat","gloom","wigglytuff" } },
        };
       
        private static Dictionary<uint, Match> matchList;

        private static System.Random random;

        public GameLogic()
        {
            random = new System.Random();
            matchList = new Dictionary<uint, Match>();
        }

        public string GetResponse(string request)
        {
            string[] requestData = request.Split(new char[] { '/' });

            if (requestData.Length == 0)
                return @"{""error"":""blank request""}";

            switch(requestData[0])
            {                

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

                    if (!matchList.ContainsKey(matchId))
                        return @"{""error"":""matchId not found""}";

                    Match m1 = matchList[matchId];
                    
                    if(!m1.players.ContainsKey(playerId))
                        return @"{""error"":""playerId not found""}";

                    if(m1.status != MatchStatus.Started)
                        return @"{""error"":""Match not active!""}";

                    Player updatedPlayer = matchList[matchId].players[playerId];
                    updatedPlayer.score = newscore;

                    matchList[matchId].players[playerId] = updatedPlayer;

                    if(newscore >= matchList[matchId].maxscore)
                    {
                        Match tmpMatch = matchList[matchId];
                        tmpMatch.status = MatchStatus.Ended;
                        matchList[matchId] = tmpMatch;
                    }

                    Console.WriteLine($"Player {updatedPlayer.name} with id {updatedPlayer.Id} score updated to {newscore}...");

                    return @"{""updatescore"":""ok""}";

                case "matchstatus":
                    //request format: matchstatus/[matchId]]
                    if (requestData.Length != 2)
                        return @"{""error"":""missing request data""}";

                    uint matchId2;
                    if(!uint.TryParse(requestData[1], out matchId2))
                        return @"{""error"":""bad matchId""}";                                      

                    if (!matchList.ContainsKey(matchId2))
                        return @"{""error"":""matchId not found""}";

                    Match m = matchList[matchId2];

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

                    if (!matchList.ContainsKey(mjoinId))
                        return @"{""error"":""matchId not found""}";

                    Match matchToJoin = matchList[mjoinId];

                    string _playername = requestData[2];

                    short _avatarIndex;
                    if (!short.TryParse(requestData[3], out _avatarIndex))
                        return @"{""error"":""bad avatarIndex""}";

                    if (matchToJoin.status == MatchStatus.Waiting && matchList[mjoinId].players.Count < matchList[mjoinId].maxplayers)
                    {
                        Player newPlayer = new Player()
                        {
                            avatarIndex = _avatarIndex,
                            name = _playername,
                            score = 0
                        };

                        uint randPlayerId = (uint)random.Next(999999, 999999999);
                        while (matchToJoin.players.ContainsKey(randPlayerId))
                        {
                            randPlayerId = (uint)random.Next(999999, 999999999);
                        }

                        newPlayer.Id = randPlayerId;
                        matchList[mjoinId].players.Add(randPlayerId, newPlayer);

                        if (matchList[mjoinId].players.Count == matchList[mjoinId].maxplayers)
                        {
                            Match tmpMatch = matchList[mjoinId];
                            tmpMatch.status = MatchStatus.Started;
                            matchList[mjoinId] = tmpMatch;
                        }                            

                        Console.WriteLine($"New Player {_playername} joined match {mjoinId}");

                        return $"{{\"playerid\":\"{newPlayer.Id}\"}}";
                    }
                    else
                        return @"{""error"":""match already started""}";

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
                    while(matchList.ContainsKey(randMatchId))
                    {
                        randMatchId = (uint)random.Next(10000, 999999);                        
                    }
                    matchList.Add(randMatchId, new Match()
                    {
                        Id = randMatchId,
                        category = matchCategory,
                        maxplayers = _maxplayers,
                        maxscore = _maxscore,
                        players = new Dictionary<uint, Player>(),
                        wordlist = wordDictionary[matchCategory].ToList(),
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
