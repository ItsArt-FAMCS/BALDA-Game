using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace balda
{
    public partial class Menu : PhoneApplicationPage
    {
        public Menu()
        {
            InitializeComponent();
            NewGameButton.Click += new RoutedEventHandler(NewGame);
            SettingsButton.Click += new RoutedEventHandler(Settings);
            NewTwoPlayersGame.Click += new RoutedEventHandler(NewPlayerGame);
        }

        void NewGame(object sender, RoutedEventArgs e)
        {
            Balda.GameLogic.Instance.compOponent = true;
            Uri uri = new Uri("//MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        void NewPlayerGame(object sender, RoutedEventArgs e)
        {
            Balda.GameLogic.Instance.compOponent = false;
            Uri uri = new Uri("//MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        void Settings(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("//Settings.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }
    }
}