﻿using System;
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
        }

        void NewGame(object sender, RoutedEventArgs e)
        {
            PageSwitch ps = this.Parent as PageSwitch;
            ps.Navigate(new Balda.MainPage());
            //Uri uri = new Uri("//MainPage.xaml");
            //(Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        void Settings(object sender, RoutedEventArgs e)
        {
            PageSwitch ps = this.Parent as PageSwitch;
            ps.Navigate(new Settings());
        }
    }
}