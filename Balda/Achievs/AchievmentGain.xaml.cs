using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Balda.Achievements
{
    public partial class AchievmentGain : UserControl
    {
        public AchievmentGain(String text)
        {
            InitializeComponent();
            textBlock.Text += text;
            var t = DateTime.Now.Second;
            fadeInAnimation.Begin();
         
        }

        public void FadeOut()
        {
            fadeOutAnimation.Begin();
        }

        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            // Remove this control from the parent UI element (game grid). Fade
            // out is just a visual effect, and this control would still exist
            // and receive events when it's not visible.
            (Parent as Panel).Children.Remove(this);
        }
    }
}
