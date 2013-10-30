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
    public partial class PageSwitch : UserControl
    {
        public PageSwitch()
        {
            InitializeComponent();
            if (this.Content == null)
            {
               this.Content = new Menu();
            }
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }
    }
}
