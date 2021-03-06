﻿/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Balda.Achievements;

namespace Balda
{
    /// <summary>
    /// Main page of the application, the game itself
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Possible game states; game not started yet, ongoing and game over
        /// </summary>
        enum GameState
        {
            NotStarted = 0,
            Ongoing,
            GameOver
        };

        const String gameStateFile = "gamestate.dat";
        private GameState gameState = GameState.NotStarted;
        private DispatcherTimer gameTimer;
        private DateTime gameStartTime;
        private DateTime gamePausedTime;
        private TimeSpan gameTimeElapsed;
        private Cell[][] cells;
        private int pScore = 0;
        private int cScore = 0;
        private bool secondPlayer = false;
        public List<string> usedWords = new List<string>();
        public List<string> achList = new List<string>();
        private bool containsNewLetter = false;
        private Processor.BaldaProcessor bProc = Processor.BaldaProcessor.Instance;
        private List<Cell> listOfCoords;
        private Balda.Achievements.AchievmentGain achGain;
        private Balda.AchievementsProcessor achProc;
        private Balda.AchievementsListner achListner;
        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        public MainPage()
        {
            InitializeComponent();

            this.SupportedOrientations = SupportedPageOrientation.Portrait;
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += StatusTimerTick;
            achProc = new Balda.AchievementsProcessor();
            achListner = new Balda.AchievementsListner(achProc);
            gamePausedTime = new DateTime();

            cells = CreateGrid();
            // For tombstoning; listen for deactivated event and restore state
            // if the application was deactivated earlier.
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(App_Deactivated);
            RestoreState();
            NewGame();
            if (GameLogic.Instance.isGameOngoing)
            {
                this.cells = GameLogic.Instance.mainPage.cells;
                for (int i = 0; i < GameLogic.Instance.size; i++)
                    for (int j = 0; j < GameLogic.Instance.size; j++)
                        cells[i][j].Value = GameLogic.Instance.mainPage.cells[i][j].Value;
                // 
                this.word = GameLogic.Instance.mainPage.word;
                this.usedWords = GameLogic.Instance.mainPage.usedWords;
                this.started = GameLogic.Instance.mainPage.started;
                this.secondPlayer = GameLogic.Instance.mainPage.secondPlayer;
                this.pScore = GameLogic.Instance.mainPage.pScore;
                this.previousCell = GameLogic.Instance.mainPage.previousCell;
                this.playerTextBox.Text = GameLogic.Instance.mainPage.playerTextBox.Text;
                this.playerScore.Text = GameLogic.Instance.mainPage.playerScore.Text;
                //this.numberSelection = GameLogic.Instance.mainPage.numberSelection;
                this.newLetter = GameLogic.Instance.mainPage.newLetter;
                this.newCompLetter = GameLogic.Instance.mainPage.newCompLetter;
                this.listOfCoords = GameLogic.Instance.mainPage.listOfCoords;
                this.letterPicked = false;

                this.cScore = GameLogic.Instance.mainPage.cScore;
                this.containsNewLetter = GameLogic.Instance.mainPage.containsNewLetter;
                this.computerTextBOx.Text = GameLogic.Instance.mainPage.computerTextBOx.Text;
                this.compScore.Text = GameLogic.Instance.mainPage.compScore.Text;
                this.bProc = GameLogic.Instance.mainPage.bProc;
                this.BoardGrid = GameLogic.Instance.mainPage.BoardGrid;
                this.AIfields = GameLogic.Instance.mainPage.AIfields;

                // this.UpdateLayout();
                //UpdateStatus();
            }



            //  });

        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="r">Event arguments</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (gameState == GameState.Ongoing && gamePausedTime > gameStartTime)
            {
                gameStartTime += DateTime.Now - gamePausedTime;
                UpdateStatus();
            }
        }

        /// <summary>
        /// Event handler for the highscores -button.
        /// Navigates to the highscores page.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void HighscoresButton_Click(object sender, EventArgs e)
        {
            gamePausedTime = DateTime.Now;

            NavigationService.Navigate(new Uri("/Highscores/HighscoresPage.xaml",
                UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Event handler for the new game -button
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void NewGameButton_Click(object sender, EventArgs e)
        {
            if (gameState == GameState.Ongoing)
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to start new game? \nAny game progress will be lost.", "", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    NewGame();
                    cells = CreateGrid();
                    letterPicked = false;
                    word = "";
                    if (AIfields != null)
                        AIfields.Clear();
                    started = false; //rename
                    previousCell = null;
                }
            }
            else
                NewGame();
        }


        /// <summary>
        /// Event handler for the status timer
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void StatusTimerTick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        /// <summary>
        /// Generates a new puzzle and starts the game
        /// </summary>
        private void NewGame()
        {
            // Deployment.Current.Dispatcher.BeginInvoke(() =>
            // {
            // Close the GameOver dialog if it was still active
            GameOver gameOver = LayoutRoot.Children.OfType<GameOver>().SingleOrDefault();
            LayoutRoot.Children.Remove(gameOver);

            numberSelection.Visibility = System.Windows.Visibility.Collapsed;

            // Display wait note (spinning circle)
            waitIndicator.Visibility = System.Windows.Visibility.Visible;
            waitIndicator.StartSpin();

            // Disable databinding while generating puzzle
            DataContext = null;

            // Puzzle generation takes couple of seconds, do it in another thread
            ThreadPool.QueueUserWorkItem(dummy => Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!GameLogic.Instance.isGameOngoing) GameLogic.Instance.GeneratePuzzle(word);
                DataContext = GameLogic.Instance.Model; // let's turn on databinding again
                gameTimer.Start();
                gameStartTime = DateTime.Now;
                gameState = GameState.Ongoing;
                UpdateStatus();
                waitIndicator.Visibility = System.Windows.Visibility.Collapsed;
                waitIndicator.StopSpin();
            }));
            //});

        }

        /// <summary>
        /// Updates status to UI; player moves, empty cells and game time
        /// </summary>
        /// 
        private System.DateTime achShowedTime;
        private void UpdateStatus()
        {
            gameTimeElapsed = DateTime.Now - gameStartTime;
            if (achGain != null && (DateTime.Now - achShowedTime).TotalSeconds >= 3)
            {
                achGain.FadeOut();
                achGain = null;
            }
            if (achListner.achQueue.Count() != 0 && achGain == null)
            {
                achGain = new AchievmentGain(achListner.achQueue[0]);
                achGain.Visibility = System.Windows.Visibility.Visible;
                achGain.SetValue(Grid.RowSpanProperty, 3);
                achGain.SetValue(Grid.ColumnSpanProperty, 2);
                achGain.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Top);
                achGain.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                achGain.SetValue(MarginProperty, new Thickness(10, 0, 0, 0));
                LayoutRoot.Children.Add(achGain);
                achShowedTime = DateTime.Now;
                achListner.achQueue.RemoveAt(0);
            }
            //GameTime.Text  = String.Format("{0:D1}:{1:D2}:{2:D2}",
            //    gameTimeElapsed.Hours, gameTimeElapsed.Minutes, gameTimeElapsed.Seconds);

            //Empty.Text = GameLogic.Instance.EmptyCells.ToString();
            //Moves.Text = GameLogic.Instance.PlayerMoves.ToString();

        }

        /// <summary>
        /// Ends current game. Called when all the cells are filled.
        /// </summary>
        private void GameEnds()
        {
            //gameTimer.Stop();
            bool won = pScore > cScore ? true : false;
            achProc.gameEnded(GameLogic.Instance.dificulty, won);

            // Display the score with GameOver dialog
            var score = new HighscoreItem();
            score.Time = new TimeSpan(gameTimeElapsed.Days, gameTimeElapsed.Hours,
                gameTimeElapsed.Minutes, gameTimeElapsed.Seconds, 0);
            score.player1 = pScore;
            score.player2 = cScore;
            //TODO: move this to XAML
            var gameOver = new GameOver(score);
            // Main page is divided into 2x3 grid. Make sure the row and column
            // properties are set properly (position 0,0 with span 2,3) to make
            // the dialog visible anywhere on the page.

            gameOver.SetValue(Grid.RowSpanProperty, 3);
            gameOver.SetValue(Grid.ColumnSpanProperty, 2);
            gameOver.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
            gameOver.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            gameOver.SetValue(MarginProperty, new Thickness(10, 0, 0, 0));
            GameLogic.Instance.isGameOngoing = false;
            LayoutRoot.Children.Add(gameOver);
            gameState = GameState.GameOver;

            SoundHelper.PlaySound(SoundHelper.SoundType.GameEndSound);
        }

        /// <summary>
        /// Creates the grid cells and populates the board grid with the cells
        /// </summary>
        /// <returns>9x9 array of empty cells</returns>
        /// 
        private BitmapImage darkImage = new BitmapImage(new Uri("/gfx/darkGridItem.png", UriKind.Relative));
        private BitmapImage lightImage = new BitmapImage(new Uri("/gfx/lightGridItem.png", UriKind.Relative));
        private Cell[][] CreateGrid()
        {

            bool lightCell = false;
            int size = GameLogic.Instance.size;
            var cells = new Cell[size][];
            listOfCoords = new List<Cell>();
            for (int row = 0; row < size; row++)
            {
                cells[row] = new Cell[size];
                if (row % size != 0)
                    lightCell = !lightCell;

                for (int col = 0; col < size; col++)
                {
                    // switch image type (light or dark) after each 3 cells in row
                    if (col % size == 0)
                        lightCell = !lightCell;

                    Cell c = new Cell();
                    c.SetValue(Grid.RowProperty, row);
                    c.SetValue(Grid.ColumnProperty, col);
                    c.BackgroundImage.Source = lightCell ? lightImage : darkImage;

                    // install event handler
                    c.MouseLeftButtonDown += new MouseButtonEventHandler(OnCellTouched);
                    c.MouseEnter += new MouseEventHandler(OnCellEnter);
                    c.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(OnCellLost);
                    // set binding to proper BoardValue from BoardViewModel
                    var b = new Binding(string.Format("BoardNumbers[{0}][{1}].Value", row, col));
                    c.SetBinding(Cell.ValueProperty, b);

                    var b2 = new Binding(string.Format("BoardNumbers[{0}][{1}].SetByGame", row, col));
                    c.SetBinding(Cell.SetByGameProperty, b2);

                    cells[row][col] = c;

                    BoardGrid.Children.Add(c);
                }
            }

            return cells;
        }

        private bool letterPicked = false;
        private string word = "";
        private Cell newLetter;
        private List<Processor.Field> AIfields;

        private void OnCellTouched(object sender, MouseButtonEventArgs e)
        {
            Cell cell = sender as Cell;
            if (!cell.IsPlayerSettable)
                return;
            if (letterPicked)
            {
                //do nothing
            }
            else
            {
                if (cell.Value == ' ' /*|| cell == newLetter*/)
                {
                    if (newCompLetter != null)
                    {
                        newCompLetter.BackgroundImage.Source = lightImage;
                        foreach (var x in AIfields)
                        {
                            cells[x.X][x.Y].BackgroundImage.Source = lightImage;
                        }
                    }
                    // This lambda experssion will allow us to have access to destination cell in a clean way
                    numberSelection.OnSelectedNumber = (selectedNumber => OnNumberChoosen(cell, selectedNumber));

                    // Place the dialog above the cell, but make sure the dialog fits on the screen.
                    numberSelection.KeyboardMargin = GetPositionForCell(cell);

                    // Change the visibility + start fade in animation
                    numberSelection.ShowKeyboard();
                    newLetter = cell;
                    letterPicked = true;
                }
            }
        }

        private bool started = false; //rename
        private Cell previousCell;

        private void OnCellEnter(object sender, MouseEventArgs e)
        {
            if (letterPicked)
            {
                started = true;
                var cell = sender as Cell;
                if (previousCell != null)
                {
                    if (Math.Abs((int)previousCell.GetValue(Grid.RowProperty) - (int)cell.GetValue(Grid.RowProperty)) +
                         Math.Abs((int)previousCell.GetValue(Grid.ColumnProperty) - (int)cell.GetValue(Grid.ColumnProperty)) == 1)
                    {
                        if (cell == newLetter)
                            containsNewLetter = true;
                        cell.BackgroundImage.Source = darkImage;
                        word += cell.Value;
                        listOfCoords.Add(cell);
                        previousCell = cell;
                    }
                    else
                    {
                        word = "";
                        foreach (var x in listOfCoords)
                        {

                            x.BackgroundImage.Source = lightImage;
                        }
                        previousCell = null;
                        listOfCoords = new List<Cell>();
                        started = false;
                        containsNewLetter = false;
                    }
                }
                else
                {
                    if (cell == newLetter)
                        containsNewLetter = true;
                    cell.BackgroundImage.Source = darkImage;
                    word += cell.Value;
                    listOfCoords.Add(cell);
                    previousCell = cell;
                }

            }

        }
        private Cell newCompLetter;

        private void OnCellLost(object sender, ManipulationCompletedEventArgs e)
        {
            if (started)
            {
                string finalWord = word;
                word = "";



                if (bProc.IsLegalWord(finalWord) && containsNewLetter)
                {
                    achProc.wordPicked(finalWord);
                    letterPicked = false;
                    started = false;
                    if (!GameLogic.Instance.compOponent)
                    {
                        if (secondPlayer)
                        {
                            computerTextBOx.Text = finalWord;
                            cScore += finalWord.Length;
                            compScore.Text = cScore.ToString();
                            secondPlayer = false;
                        }
                        else
                        {
                            playerTextBox.Text = finalWord;
                            pScore += finalWord.Length;
                            playerScore.Text = pScore.ToString();
                            secondPlayer = true;
                        }

                    }
                    else
                    {
                        playerTextBox.Text = finalWord;
                        pScore += finalWord.Length;
                        playerScore.Text = pScore.ToString();
                    }
                    if (bProc.IsGameOver())
                        GameEnds();
                    bProc.AddWord(finalWord, new Processor.Field((int)newLetter.GetValue(Grid.RowProperty), (int)newLetter.GetValue(Grid.ColumnProperty))
                    {
                        Value = newLetter.Value
                    });


                    foreach (var x in listOfCoords)
                    {
                        x.BackgroundImage.Source = lightImage;
                    }
                    usedWords.Add(finalWord);

                    if (GameLogic.Instance.compOponent)
                        AIMove();
                    if (bProc.IsGameOver())
                        GameEnds();

                }
                else
                {
                    word = "";
                    letterPicked = false;
                    started = false;
                    containsNewLetter = false;
                    GameLogic.Instance.SetNumberByPlayer((int)newLetter.GetValue(Grid.RowProperty),
                                                          (int)newLetter.GetValue(Grid.ColumnProperty),
                                                          ' ');
                    foreach (var x in listOfCoords)
                    {

                        x.BackgroundImage.Source = lightImage;
                    }
                }
                previousCell = null;
            }
        }

        private void AIMove()
        {
            var way = bProc.AIProcess();
            var field = way.NewField;
            string compWord = way.Word;
            usedWords.Add(compWord);
            computerTextBOx.Text = compWord;
            cScore += compWord.Length;
            compScore.Text = cScore.ToString();
            cells[field.X][field.Y].Value = field.Value;
            newCompLetter = cells[field.X][field.Y];
            GameLogic.Instance.Model.BoardNumbers[field.X][field.Y].Value = field.Value;
            AIfields = way.GetFields();
            foreach (var x in AIfields)
            {
                cells[x.X][x.Y].BackgroundImage.Source = darkImage;
            }
            newCompLetter.BackgroundImage.Source = darkImage;
            containsNewLetter = false;
        }

        /// <summary>
        /// Helper method to get the absolute position with respect to the screen borders
        /// </summary>
        private Thickness GetPositionForCell(Cell cell)
        {
            var pos = new Point(cell.ActualWidth / 2 - numberSelection.KeyboardSize.Width / 2, cell.ActualHeight / 2 - numberSelection.KeyboardSize.Height / 2);
            pos = cell.TransformToVisual(LayoutRoot).Transform(pos);

            if (pos.X < 0)
                pos.X = 0;
            else if (pos.X > LayoutRoot.ActualWidth - numberSelection.KeyboardSize.Width)
                pos.X = LayoutRoot.ActualWidth - numberSelection.KeyboardSize.Width;

            if (pos.Y < 0)
                pos.Y = 0;
            else if (pos.Y > LayoutRoot.ActualHeight - numberSelection.KeyboardSize.Height)
                pos.Y = LayoutRoot.ActualHeight - numberSelection.KeyboardSize.Height;

            return new Thickness(20, 100, 0, 0);
        }

        /// <summary>
        /// Action triggered when user selected a number
        /// </summary>
        private void OnNumberChoosen(Cell sender, char number)
        {
            int x = (int)sender.GetValue(Grid.RowProperty);
            int y = (int)sender.GetValue(Grid.ColumnProperty);
            GameLogic.Instance.SetNumberByPlayer((int)sender.GetValue(Grid.RowProperty), (int)sender.GetValue(Grid.ColumnProperty), number);
            cells[x][y].Blink();
            SoundHelper.PlaySound(SoundHelper.SoundType.CellSelectedSound);
        }

        void App_Deactivated(object sender, DeactivatedEventArgs e)
        {

        }


        //back button 

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            GameLogic.Instance.isGameOngoing = true;
            GameLogic.Instance.mainPage = this;
        }
        /// <summary>
        /// Reads the game state from a file and continues the game from where
        /// it was left.
        /// </summary>
        private void RestoreState()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(gameStateFile))
                return;

            int emptyCells = 0;
            using (IsolatedStorageFileStream stream = store.OpenFile(gameStateFile, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    // Read the state and stats
                    gameState = (GameState)reader.ReadInt32();
                    GameLogic.Instance.PlayerMoves = reader.ReadInt32();
                    gameTimeElapsed = new TimeSpan(reader.ReadInt64());
                    gameStartTime = DateTime.Now - gameTimeElapsed;

                    // Read contents of the cells

                }
            }

            store.DeleteFile(gameStateFile);

            if (gameState == GameState.Ongoing)
            {
                GameLogic.Instance.EmptyCells = emptyCells;
                gameTimer.Start();
            }
            else
            {
                GameLogic.Instance.EmptyCells = 0;
            }

            DataContext = GameLogic.Instance.Model;
            UpdateStatus();
        }


        ///// <summary>
        ///// Event handler for orientation changes.
        ///// Repositions UI elements depending on the orientation.
        ///// </summary>
        ///// <param name="sender">Sender of the event</param>
        ///// <param name="r">Event arguments</param>
        //private void PhoneApplicationPage_OrientationChanged(object sender,OrientationChangedEventArgs e)
        //{
        //    if (e.Orientation == PageOrientation.Landscape ||
        //        e.Orientation == PageOrientation.LandscapeLeft ||
        //        e.Orientation == PageOrientation.LandscapeRight)
        //    {
        //       // Logo.SetValue(Grid.RowProperty, 1);
        //        //Logo.SetValue(Grid.ColumnSpanProperty, 1);

        //        BoardGrid.SetValue(Grid.RowProperty, 0);
        //        BoardGrid.SetValue(Grid.ColumnProperty, 1);
        //        BoardGrid.SetValue(Grid.RowSpanProperty, 3);
        //        BoardGrid.SetValue(Grid.ColumnSpanProperty, 2);

        //        waitIndicator.SetValue(Grid.RowProperty, 0);
        //        waitIndicator.SetValue(Grid.ColumnProperty, 1);
        //        waitIndicator.SetValue(Grid.RowSpanProperty, 3);
        //        waitIndicator.SetValue(Grid.ColumnSpanProperty, 2);

        //        Statistics.SetValue(Grid.RowProperty, 1);
        //        Statistics.SetValue(Grid.RowSpanProperty, 2);
        //        Statistics.SetValue(Grid.ColumnSpanProperty, 1);

        //        if(e.Orientation == PageOrientation.LandscapeLeft)
        //            LayoutRoot.Margin = new Thickness(0 ,0 ,72 ,0);
        //        if (e.Orientation == PageOrientation.LandscapeRight)
        //            LayoutRoot.Margin = new Thickness(72, 0, 0, 0);

        //        LayoutRoot.RowDefinitions[0].Height = new GridLength(90);
        //        LayoutRoot.RowDefinitions[1].Height = new GridLength(90);

        //        for (int t = 0; t < Statistics.ColumnDefinitions.Count; t++)
        //            Statistics.ColumnDefinitions[t].Width = new GridLength(0);

        //        Statistics.ColumnDefinitions[0].Width = new GridLength(10);
        //        Statistics.ColumnDefinitions[1].Width = new GridLength(35, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[2].Width = new GridLength(65, GridUnitType.Star);

        //        Statistics.RowDefinitions[0].Height = new GridLength(10);
        //        Statistics.RowDefinitions[1].Height = new GridLength(100, GridUnitType.Star);
        //        Statistics.RowDefinitions[2].Height = new GridLength(100, GridUnitType.Star);
        //        Statistics.RowDefinitions[3].Height = new GridLength(100, GridUnitType.Star);
        //        Statistics.RowDefinitions[4].Height = new GridLength(10);

        //        Statistics.Height = 192;

        //        MovesImage.SetValue(Grid.ColumnProperty, 1);
        //        MovesImage.SetValue(Grid.RowProperty, 1);

        //        EmptyImage.SetValue(Grid.ColumnProperty, 1);
        //        EmptyImage.SetValue(Grid.RowProperty, 2);

        //        GameTimeImage.SetValue(Grid.ColumnProperty, 1);
        //        GameTimeImage.SetValue(Grid.RowProperty, 3);

        //        Moves.SetValue(Grid.ColumnProperty, 2);
        //        Moves.SetValue(Grid.RowProperty, 1);

        //        Empty.SetValue(Grid.ColumnProperty, 2);
        //        Empty.SetValue(Grid.RowProperty, 2);

        //        GameTime.SetValue(Grid.ColumnProperty, 2);
        //        GameTime.SetValue(Grid.RowProperty, 3);
        //    }
        //    else
        //    {
        //       // Logo.SetValue(Grid.RowProperty, 0);
        //        //Logo.SetValue(Grid.ColumnSpanProperty, 2);

        //        BoardGrid.SetValue(Grid.RowProperty, 1);
        //        BoardGrid.SetValue(Grid.ColumnProperty, 0);
        //        BoardGrid.SetValue(Grid.RowSpanProperty, 1);
        //        BoardGrid.SetValue(Grid.ColumnSpanProperty, 2);

        //        waitIndicator.SetValue(Grid.RowProperty, 1);
        //        waitIndicator.SetValue(Grid.ColumnProperty, 0);
        //        waitIndicator.SetValue(Grid.RowSpanProperty, 1);
        //        waitIndicator.SetValue(Grid.ColumnSpanProperty, 2);

        //        Statistics.SetValue(Grid.RowProperty, 3);
        //        Statistics.SetValue(Grid.RowSpanProperty, 1);
        //        Statistics.SetValue(Grid.ColumnSpanProperty, 2);

        //        LayoutRoot.Margin = new Thickness(0, 0, 0, 72);
        //        LayoutRoot.RowDefinitions[0].Height = new GridLength(120);
        //        LayoutRoot.RowDefinitions[1].Height = new GridLength(460);

        //        for (int t = 0; t < Statistics.RowDefinitions.Count; t++)
        //            Statistics.RowDefinitions[t].Height = new GridLength(0);

        //        Statistics.ColumnDefinitions[0].Width = new GridLength(18);
        //        Statistics.ColumnDefinitions[1].Width = new GridLength(60, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[2].Width = new GridLength(75, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[3].Width = new GridLength(60, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[4].Width = new GridLength(75, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[5].Width = new GridLength(60, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[6].Width = new GridLength(90, GridUnitType.Star);
        //        Statistics.ColumnDefinitions[7].Width = new GridLength(18);

        //        Statistics.RowDefinitions[0].Height = new GridLength(100, GridUnitType.Star);

        //        Statistics.Height = 64;

        //        MovesImage.SetValue(Grid.ColumnProperty, 1);
        //        MovesImage.SetValue(Grid.RowProperty, 0);

        //        EmptyImage.SetValue(Grid.ColumnProperty, 3);
        //        EmptyImage.SetValue(Grid.RowProperty, 0);

        //        GameTimeImage.SetValue(Grid.ColumnProperty, 5);
        //        GameTimeImage.SetValue(Grid.RowProperty, 0);

        //        Moves.SetValue(Grid.ColumnProperty, 2);
        //        Moves.SetValue(Grid.RowProperty, 0);

        //        Empty.SetValue(Grid.ColumnProperty, 4);
        //        Empty.SetValue(Grid.RowProperty, 0);

        //        GameTime.SetValue(Grid.ColumnProperty, 6);
        //        GameTime.SetValue(Grid.RowProperty, 0);
        //    }
        //}
    }
}
