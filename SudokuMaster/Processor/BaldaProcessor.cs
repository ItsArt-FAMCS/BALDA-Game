using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balda.Processor
{
    public enum DifficultyLevel
    {
        Easy = 1,
        Addaptive = 2,
        Insane = 3
    }

    class BaldaProcessor
    {
        public const int KeyLength = 4;
        public const int Size = 7;
        public const int AdaptiveDelta = 2;
        public const string Alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

        //Singleton
        private static readonly BaldaProcessor _instance = new BaldaProcessor();
        public static BaldaProcessor Instance
        {
            get { return _instance; }
        }
        protected BaldaProcessor() { }

        //Fields
        private List<string> Words { get; set; }
        private Dictionary<string, List<string>> LongWordsContainer { get; set; }
        private List<string> ShortWordsContainer { get; set; }
        private static DifficultyLevel Difficulty { get; set; }
        private static int MembersPoints { get; set; }
        private static int AIPoints { get; set; }
        private static Random random { get; set; }

        private Field[,] Desk { get; set; }

        public List<string> Used { get; set; }

        public string Initialize(String words, DifficultyLevel difficulty = DifficultyLevel.Addaptive)
        {
            InitializeDictionaries(words);
            random = new Random();
            return Restart(difficulty);
        }

        public string Restart(DifficultyLevel difficulty)
        {
            Difficulty = difficulty;
            MembersPoints = 0;
            AIPoints = 0;

            Desk = new Field[Size, Size];

            var startWords = Words.Where(e => e.Length == Size).ToArray();
            var wordNumber = random.Next(0, startWords.Count() - 1);
            var startword = startWords[wordNumber];
            Used = new List<string> { startword };

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    var val = i == Size / 2 ? startword[j] : ' ';
                    var field = new Field(i, j)
                    {
                        Value = val
                    };
                    Desk[i, j] = field;
                }
            }
            return startword;
        }

        public WayView AIProcess()
        {
            Way result = null;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Desk[i, j].Value == ' ')
                    {
                        foreach (var letter in Alphabet)
                        {
                            var startField = new Field(i, j)
                            {
                                Value = letter
                            };
                            var way = GetBestWay(startField);
                            if (way != null && (result == null || result.Text.Length < way.Text.Length))
                            {
                                result = way;
                            }
                        }
                    }
                }
            }
            if (result == null)
            {
                return null;
            }
            var field = result.GetStartField();
            Desk[field.X, field.Y] = new Field(field.X, field.Y)
            {
                Value = field.Value
            };
            var viewResult = new WayView(result, ShortWordsContainer, Used);
            if (!string.IsNullOrEmpty(viewResult.Word))
            {
                Used.Add(viewResult.Word);
            }
            return viewResult;
        }

        public bool IsLegalWord(string word)
        {
            word = word.ToLower().Trim();
            return Words.Contains(word) && !Used.Contains(word);
        }

        public bool AddWord(string word, Field field)
        {
            if (IsLegalWord(word))
            {
                field.Step = null;
                Desk[field.X, field.Y] = field;
                Used.Add(word);
                return true;
            }
            return false;
        }

        protected void InitializeDictionaries(String words)
        {
            Words = words.Split(new[] { ' ', '\r', '\n', '\t' }).Where(e => e != string.Empty).Select(e => e.ToLower().Trim()).ToList();
            LongWordsContainer = new Dictionary<string, List<string>>();
            ShortWordsContainer = new List<string>();

            foreach (var word in Words)
            {
                if (word.Length >= KeyLength)
                {
                    var keys = new List<String>();
                    for (int i = 0; i <= word.Length - KeyLength; i++)
                    {
                        var key = word.Substring(i, KeyLength);
                        if (keys.Contains(key) == false)
                        {
                            keys.Add(key);
                        }
                        var array = key.ToCharArray();
                        Array.Reverse(array);
                        key = new string(array);
                        if (keys.Contains(key) == false)
                        {
                            keys.Add(key);
                        }
                    }
                    foreach (var key in keys)
                    {
                        if (LongWordsContainer.ContainsKey(key))
                        {
                            var list = LongWordsContainer[key];
                            list.Add(word);
                            LongWordsContainer[key] = list;
                        }
                        else
                        {
                            LongWordsContainer.Add(key, new List<string>() { word });
                        }
                    }
                }
                else
                {
                    ShortWordsContainer.Add(word);
                }
            }
        }

        protected List<Way> GetStartWays(Field start)
        {
            var ways = new List<Way>();
            var startWay = new Way(Desk, start);
            var startNeighbors = startWay.GetNeighbors(start);
            foreach (var neighbor in startNeighbors)
            {
                ways.Add(startWay.AddFirst(new Field(neighbor.X, neighbor.Y)
                {
                    Value = neighbor.Value
                }));
                ways.Add(startWay.AddLast(new Field(neighbor.X, neighbor.Y)
                {
                    Value = neighbor.Value
                }));
            }
            return ways;
        }

        protected List<Way> ExtendLastWays(List<Way> ways)
        {
            var result = new List<Way>();
            foreach (var way in ways)
            {
                var neighbors = way.GetNeighbors(way.Last);
                result.AddRange(neighbors.Select(neighbor => way.AddLast(neighbor)));
            }
            return result;
        }

        protected List<Way> ExtendBothWays(List<Way> ways)
        {
            var result = new List<Way>();
            foreach (var way in ways)
            {
                var lastNeighbors = way.GetNeighbors(way.Last);
                result.AddRange(lastNeighbors.Select(neighbor => way.AddLast(neighbor)));

                var firstNeighbors = way.GetNeighbors(way.First);
                result.AddRange(firstNeighbors.Select(neighbor => way.AddFirst(neighbor)));
            }
            return result;
        }

        protected Way GetBestWay(Field start)
        {
            //Get Start Keys
            var ways = GetStartWays(start);
            if (ways.Count == 0)
                return null;
            Way result = null;

            for (int i = 0; i < KeyLength - 2; i++)
            {
                foreach (var way in ways.Where(way => (ShortWordsContainer.Contains(way.Text) && (Used.Contains(way.Text) == false))
                   || (ShortWordsContainer.Contains(way.Reverse) && (Used.Contains(way.Reverse) == false))))
                {
                    result = way;
                }
                ways = ExtendLastWays(ways);
            }
            //Initialize start dictionaries
            foreach (var way in ways)
            {
                way.Words = LongWordsContainer.ContainsKey(way.Text) ? LongWordsContainer[way.Text] : new List<string>();
                if ((way.IsWord && Used.Contains(way.Word) == false) || (way.TwoWords && Used.Contains(way.Reverse) == false))
                {
                    result = way;
                }
            }
            if (Difficulty == DifficultyLevel.Easy && result != null)
            {
                return result;
            }
            if (Difficulty == DifficultyLevel.Addaptive && result != null)
            {
                if (Math.Abs(MembersPoints - AIPoints) <= AdaptiveDelta)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        return result;
                    }
                }
                else if (MembersPoints < AIPoints)
                {
                    return result;
                }
            }
            //Поиск в ширину, короче
            while (ways.Count > 0)
            {
                var tempWays = ExtendBothWays(ways);
                ways = new List<Way>();
                foreach (var way in tempWays)
                {
                    way.ReformWords();
                    if (way.Words.Count > 0)
                    {
                        ways.Add(way);
                    }
                    if ((result == null || way.Text.Length > result.Text.Length)
                        && ((way.IsWord && Used.Contains(way.Word) == false) || (way.TwoWords && Used.Contains(way.Reverse) == false)))
                    {
                        result = way;
                    }
                }
            }
            return result;
        }

    }
}
