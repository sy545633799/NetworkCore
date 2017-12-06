using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLServer.logic.match
{
    /// <summary>
    /// 战斗匹配房间模型
    /// </summary>
   public class MatchRoom
    {
       public int id;//房间唯一ID
       public int teamMax = 1;//每支队伍需要匹配到的人数
       public List<int> teamOne = new List<int>();//队伍一 人员ID
       public List<int> teamTwo = new List<int>();//队伍二 人员ID
    }
}
