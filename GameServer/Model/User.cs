using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Model
{
    public class User
    {
        public int Id = -1;
        public string Name;
        public int Level = 1;
        public int Exp = 0;
        public int Win = 0;
        public int Lose = 0;
        public int Ran = 0;
        public int AccountId;
        public int[] HeroList;
    }
}
