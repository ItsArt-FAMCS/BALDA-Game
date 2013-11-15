/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Balda
{
    /// <summary>
    /// The dialog displayed when the puzzle is solved.
    /// Contains puzzle solving time and moves, and a texbox for player's name.
    /// </summary>
    public partial class GameOver : UserControl
    {
        HighscoreItem score;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerScore">The score of the player. At least time and moves must be filled.</param>
        public GameOver(HighscoreItem playerScore)
        {
            InitializeComponent();
            if (GameLogic.Instance.compOponent)
            {
                if (playerScore.player1 > playerScore.player2)
                {
                    textBlockHeading.Text = "Вы победили!";
                }
                else if (playerScore.player1 < playerScore.player2)
                {
                    textBlockHeading.Text = "Вы проиграли :( Попробуйте ещё раз.";
                }
                else
                {
                    textBlockHeading.Text = "Ничья.";
                }
            }
            else
            {
                if (playerScore.player1 > playerScore.player2)
                {
                    textBlockHeading.Text = "Первый игрок победил!";
                }
                else if (playerScore.player1 < playerScore.player2)
                {
                    textBlockHeading.Text = "Второй игрок победил!";
                }
                else
                {
                    textBlockHeading.Text = "Ничья.";
                }
            }
            // Start the fade in animation
            fadeInAnimation.Begin();

            // Show the position textblock and player name textbox only if the
            // score is good enough to make it to the list.
            score = playerScore;
           // int position = Highscores.IsNewHighscore(score);
            //score.Index = position;

            textBlockTime.Text = "Your time was " + score.Time.ToString();
        }

        /// <summary>
        /// Called when the player presses a key on the keyboard
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void PlayerName_KeyDown(object sender, KeyEventArgs e)
        {
            // Add the score to the list and start fading out when the enter
            // key is pressed
            if (e.Key == Key.Enter)
            {
                Focus();
            }
        }

        /// <summary>
        /// Called when the fade out animation is completed
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            // Remove this control from the parent UI element (game grid). Fade
            // out is just a visual effect, and this control would still exist
            // and receive events when it's not visible.
            (Parent as Panel).Children.Remove(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            fadeOutAnimation.Begin();
        }
    }
}
