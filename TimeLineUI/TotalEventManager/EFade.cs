using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class EFade : EventObject
    {
        public EFade(ScriptInfo scriptInfo, int r_uniqueID, TOTALEVENTTYPE eType) :
            base(scriptInfo, r_uniqueID, eType)
        {

        }

        public override void Play(long now)
        {
            base.Play(now);
            //Console.WriteLine("{0} Play!", strName);

            //BayabaAnimation ani = (BayabaAnimation)TotalEventMng.FindAppBusObjectByUniqueID(nUniqueID);

            //if (eTotalEventType == TOTALEVENTTYPE.FADE_IN)
            //{
            //    ani.GameObject.TransIn(Int32.Parse(mScriptInfo.lstData[1]),
            //                           Int32.Parse(mScriptInfo.lstData[2]),
            //                           0,
            //                           0);
            //}
            //else if (eTotalEventType == TOTALEVENTTYPE.FADE_OUT)
            //{
            //    ani.GameObject.TransOut(Int32.Parse(mScriptInfo.lstData[1]),
            //                            Int32.Parse(mScriptInfo.lstData[2]),
            //                            0,
            //                            0);
            //}

        }
    }
}
