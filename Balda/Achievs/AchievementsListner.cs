using Balda.Achievs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balda
{
    //public delegate void GameEnded(object sender, EventArgs e);

    class AchievementsListner
    {
        public List<string> achQueue = new List<string>();
        public string name = "";
        private AchievementsProcessor achProc;
        public AchievementsListner(AchievementsProcessor AchProc)
        {
            achProc = AchProc;
            achProc.eWon += new AchievementsProcessor.EasyWon(EasyWonAch);
            achProc.fLetters += new AchievementsProcessor.FiveLetters(FiveLettersAch);
        }

        private void EasyWonAch(object sender, AchEventArgs e)
        {
            
        }

        private void FiveLettersAch(object sender, AchEventArgs e)
        {
            foreach (var x in e.ach)
            {
                achQueue.Add(x);
            }
        }

    }
}
