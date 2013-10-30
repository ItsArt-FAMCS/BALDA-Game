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
            NewGameButton.Click += new RoutedEventHandler(Switch_Click);
        }

        void Switch_Click(object sender, RoutedEventArgs e)
        {
            PageSwitch ps = this.Parent as PageSwitch;
            ps.Navigate(new Balda.MainPage());
        }
    }
}