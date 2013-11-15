﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Balda
{
    public partial class Menu : PhoneApplicationPage
    {
        public Menu()
        {
            InitializeComponent();
            NewGameButton.Click += NewGame;
            SettingsButton.Click += Settings;
            NewTwoPlayersGame.Click += NewPlayerGame;
        }

        void NewGame(object sender, RoutedEventArgs e)
        {
            Balda.GameLogic.Instance.compOponent = true;
            Uri uri = new Uri(@"/Pages/MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        void NewPlayerGame(object sender, RoutedEventArgs e)
        {
            Balda.GameLogic.Instance.compOponent = false;
            Uri uri = new Uri(@"/Pages/MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        void Settings(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(@"/Pages/Settings.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }
    }
}