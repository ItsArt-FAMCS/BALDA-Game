/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;

namespace Balda
{
    /// <summary>
    /// Contains the logic for generating and managing puzzles
    /// </summary>
    public class GameLogic
    {
		public BoardModel Model { get; private set; }
        private Processor.BaldaProcessor bProc = Processor.BaldaProcessor.Instance;
        public const int WordLength = 7;
		public const int BlocksPerSide = 3;
        public Processor.DifficultyLevel dificulty = Processor.DifficultyLevel.Addaptive; //normal diff by default
        public int size = 7;
        public bool compOponent = true;
        protected int[] randOrder;
        protected bool solutionFound = false;
		private char[][] copyCells;
        protected Random randGen = new Random();
        public MainPage GameObj;
        public int EmptyCells { get; set; }
        public int PlayerMoves { get; set; }
        public string achs;
        private static readonly GameLogic _instance = new GameLogic();
        public static GameLogic Instance
        {
            get { return _instance; }
        }
        protected GameLogic()
        {
			Model = new BoardModel(size, size);
            achs = ReadFile("achfile.txt");
        }

        public void WriteToFile(string filePath, string text)
        {
            
            using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(filePath))
                    myIsolatedStorage.DeleteFile(filePath);
                var stream = myIsolatedStorage.CreateFile(filePath);
                using (var isoStream = new StreamWriter(stream))
                {
                    isoStream.WriteLine(text);
                }
            }
        }

        private string ReadFile(string filePath)
        {
            //this verse is loaded for the first time so fill it from the text file
            var resrouceStream = Application.GetResourceStream(new Uri(filePath, UriKind.Relative));
            if (resrouceStream != null)
            {
                var myFileStream = resrouceStream.Stream;
                if (myFileStream.CanRead)
                {
                    var myStreamReader = new StreamReader(myFileStream);

                    //read the content here
                    return myStreamReader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates a new puzzle
        /// </summary>
        public void GeneratePuzzle(string Word)
        {
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
					Model.BoardNumbers[i][j].Value = ' ';
            string word = Processor.BaldaProcessor.Instance.InitializeGame(dificulty, size);
            
            for (int i = 0; i < size; i++)
            {
                Model.BoardNumbers[size / 2][i].Value = word[i];
            }
        }

        /// <summary>
        /// Places the number selected by the player on the game board
        /// </summary>
        /// <param name="x">X coordinate to set</param>
        /// <param name="y">Y coordinate to set</param>
        /// <param name="value">Number to set</param>
        public List<Point> SetNumberByPlayer(int x, int y, char value)
        {
            Model.BoardNumbers[x][y].Value = value;
            return null;
        }

        public void AIMove()
        {

        }

    }
}
