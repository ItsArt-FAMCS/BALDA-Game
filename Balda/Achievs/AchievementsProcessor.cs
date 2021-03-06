﻿using Balda.Achievs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balda
{
    class AchievementsProcessor
    {
        //list of ach
        bool fletters;
        bool tletters;
        bool ftletters;
        bool ewon;
        bool nwon;
        bool iwon;
        //
        public delegate void EasyWon(object sender, AchEventArgs e);
        public event EasyWon eWon;
        public delegate void FiveLetters(object sender, AchEventArgs e);
        public event FiveLetters fLetters;
        private bool newAch = false;
        protected virtual void OnChanged(AchEventArgs e)
        {
            if (fLetters != null)
                fLetters(this, e);
        }
        private string str;
        private StringBuilder st;
        public AchievementsProcessor()
        {
            str = Balda.GameLogic.Instance.achs;
            fletters = isTrue(str[0]);
            tletters = isTrue(str[1]);
            ftletters = isTrue(str[2]);
            ewon = isTrue(str[3]);
            nwon = isTrue(str[4]);
            iwon = isTrue(str[5]);
            st = new StringBuilder(str);
        }

        private bool isTrue(char ch)
        {
            if (ch == '0')
                return false;
            return true;
        }

        public void gameEnded(Balda.Processor.DifficultyLevel diff, bool hasWon)
        {
            AchEventArgs args = new AchEventArgs();
            if (diff == Balda.Processor.DifficultyLevel.Easy && hasWon && !ewon)
            {
                args.ach.Add("\"Гроза школьников!\"");
                ewon = true;
                st[3] = '1';
                newAch = true;
            }

            if (diff == Balda.Processor.DifficultyLevel.Addaptive && hasWon && !nwon)
            {
                args.ach.Add("\"Победи себя\"");
                nwon = true;
                st[4] = '1';
                newAch = true;
            }

            if (diff == Balda.Processor.DifficultyLevel.Insane && hasWon && !iwon)
            {
                args.ach.Add("\"Верховный эрудит\"");
                iwon = true;
                st[4] = '1';
                newAch = true;
            }
            if (newAch)
            {
                Balda.GameLogic.Instance.WriteToFile("achfile.txt", st.ToString());
                OnChanged(args);
                newAch = false;
            }
        }

        public void wordPicked(string word)
        {
            AchEventArgs args = new AchEventArgs();
            if (word.Length >= 5 && !fletters)
            {
                args.ach.Add("\"Что-то знаете\"");
                fletters = true;
                st[0] = '1';
                newAch = true;
            }

            if(word.Length >= 10 && !tletters)
            {
                args.ach.Add("\"Мастер слова\"");
                tletters = true;
                st[1] = '1';
                newAch = true;
            }

            if (word.Length >= 15 && !ftletters)
            {
                args.ach.Add("\"Высокопревосходительство (политетрафторэтиленацетоксипропилбутан)\"");
                ftletters = true;
                st[2] = '1';
                newAch = true;
            }
            if (newAch)
            {
                Balda.GameLogic.Instance.WriteToFile("achfile.txt", st.ToString());
                OnChanged(args);
                newAch = false;
            }
        }
    }
}
