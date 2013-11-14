using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

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
        static object locker = new object();

        public const int KeyLength = 4;
        public static int Size;
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
        public List<string> Words { get; set; }
        private Dictionary<string, List<string>> LongWordsContainer { get; set; }
        private List<string> ShortWordsContainer { get; set; }
        private DifficultyLevel Difficulty { get; set; }
        public int MembersPoints { get; set; }
        public int AIPoints { get; set; }
        private static Random random { get; set; }

        public Field[,] Desk { get; private set; }

        public List<string> Used { get; set; }

        public bool IsGameOver()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Desk[i, j].Value == ' ')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string InitializeGame(DifficultyLevel difficulty = DifficultyLevel.Addaptive, int size = 7)
        {
            Size = size;
            Difficulty = difficulty;
            random = new Random();
            return Restart();
        }

        public string Restart()
        {
            lock (locker);

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

        private bool NeedShort()
        {
            bool result = false;
            switch (Difficulty)
            {
                case DifficultyLevel.Easy:
                    result = true;
                    break;
                case DifficultyLevel.Addaptive:
                    if (AIPoints + KeyLength - MembersPoints > AdaptiveDelta)
                    {
                        result = true;
                    }
                    else if (Math.Abs(MembersPoints - AIPoints - KeyLength) <= AdaptiveDelta)
                    {
                        if (random.NextDouble() < 0.5)
                        {
                            result = true;
                        }
                    }
                    break;
            }
            return result;
        }

        public WayView AIProcess()
        {
            Way result = null;

            var needShort = NeedShort();
            var stopSearch = false;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Desk[i, j].Value == ' ' && !stopSearch)
                    {
                        foreach (var letter in Alphabet)
                        {
                            var startField = new Field(i, j)
                            {
                                Value = letter
                            };
                            var way = GetBestWay(startField, needShort);
                            if (way != null && way.StopSearch)
                            {
                                result = way;
                                stopSearch = true;
                                break;
                            }
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
                var word = viewResult.Word;
                Used.Add(word);
                AIPoints += word.Length;
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
                MembersPoints += word.Length;
                return true;
            }
            return false;
        }

        private string ReadFile(string filePath)
        {
            var resrouceStream = Application.GetResourceStream(new Uri(filePath, UriKind.Relative));
            if (resrouceStream != null)
            {
                var myFileStream = resrouceStream.Stream;
                if (myFileStream.CanRead)
                {
                    var myStreamReader = new StreamReader(myFileStream);
                    return myStreamReader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        public void InitializeDictionaries()
        {
            if (Words == null)
            {
                lock (locker)
                {
                    var words = ReadFile("dict/1.txt");
                    InitializeDictionaries(words);
                }
            }
        }

        private void InitializeDictionaries(String words)
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

        protected Way GetBestWay(Field start, bool needShort = false)
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
            if (needShort && result != null)
            {
                result.StopSearch = true;
                return result;
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
