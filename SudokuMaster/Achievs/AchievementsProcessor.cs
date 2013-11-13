using balda.Achievs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace balda
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
            }

            if (diff == Balda.Processor.DifficultyLevel.Addaptive && hasWon && !nwon)
            {
                args.ach.Add("\"Победи себя\"");
                nwon = true;
                st[4] = '1';
            }

            if (diff == Balda.Processor.DifficultyLevel.Insane && hasWon && !iwon)
            {
                args.ach.Add("\"Верховный эрудит\"");
                iwon = true;
                st[4] = '1';
            }
            Balda.GameLogic.Instance.WriteToFile("achfile.txt", st.ToString());
            OnChanged(args);
        }

        public void wordPicked(string word)
        {
            AchEventArgs args = new AchEventArgs();
            if (word.Length >= 5 && !fletters)
            {
                args.ach.Add("\"Я умею использовать не только короткие слова!\"");
                ftletters = true;
                st[0] = '1';
            }

            if(word.Length >= 10 && !tletters)
            {
                args.ach.Add("\"Мастер слова\"");
                tletters = true;
                st[1] = '1';
            }

            if (word.Length >= 15 && !ftletters)
            {
                args.ach.Add("\"Высокопревосходительство (политетрафторэтиленацетоксипропилбутан)\"");
                ftletters = true;
                st[2] = '1';
            }
            Balda.GameLogic.Instance.WriteToFile("achfile.txt", st.ToString());
            OnChanged(args);
        }
    }
}
