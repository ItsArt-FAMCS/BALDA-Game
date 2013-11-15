using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Balda;

namespace Balda
{
    public partial class Settings : PhoneApplicationPage
    {
        private bool isDefaultBoard = true;
        private Balda.Processor.DifficultyLevel diff = Balda.Processor.DifficultyLevel.Addaptive;
        public Settings()
        {
            InitializeComponent();
            if (GameLogic.Instance.size == 5)
            {
                BoardSize.Content = "Поле 5х5";
                isDefaultBoard = false;
            }
            if (GameLogic.Instance.dificulty == Balda.Processor.DifficultyLevel.Easy)
            {
                diff = Balda.Processor.DifficultyLevel.Easy;
                Difficulty.Content = "Легко";
            }
            else if (GameLogic.Instance.dificulty == Balda.Processor.DifficultyLevel.Insane)
            {
                diff = Balda.Processor.DifficultyLevel.Insane;
                Difficulty.Content = "Невозможно";
            }
        }

        

        

        private void BoardSize_Click(object sender, RoutedEventArgs e)
        {
            if (isDefaultBoard)
            {
                BoardSize.Content = "Поле 5х5";
                GameLogic.Instance.size = 5;
                isDefaultBoard = false;
            }
            else
            {
                BoardSize.Content = "Поле 7х7";
                GameLogic.Instance.size = 7;
                isDefaultBoard = true;
            }
        }

        private void Difficulty_Click(object sender, RoutedEventArgs e)
        {
            if (diff == Balda.Processor.DifficultyLevel.Addaptive)
            {
                diff = Balda.Processor.DifficultyLevel.Insane;
                Difficulty.Content = "Невозможно";
                
            }
            else if (diff == Balda.Processor.DifficultyLevel.Insane)
            {
                diff = Balda.Processor.DifficultyLevel.Easy;
                Difficulty.Content = "Легко";
            }
            else
            {
                diff = Balda.Processor.DifficultyLevel.Addaptive;
                Difficulty.Content = "Нормально";
            }
            GameLogic.Instance.dificulty = diff;
             
        }
        
       
    }
}