using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class EScale : EventObject
    {
        public EScale(ScriptInfo scriptInfo, int r_uniqueID, TOTALEVENTTYPE eType) :
            base(scriptInfo, r_uniqueID, eType)
        {

        }

        public override void Play(long now)
        {
            base.Play(now);
            //Console.WriteLine("{0} Play!", strName);

            //BayabaAnimation ani = (BayabaAnimation)TotalEventMng.FindAppBusObjectByUniqueID(nUniqueID);

            //ani.GameObject.Zoom(float.Parse(mScriptInfo.lstData[1]),
            //                    float.Parse(mScriptInfo.lstData[2]),
            //                    Int32.Parse(mScriptInfo.lstData[3]),
            //                    Int32.Parse(mScriptInfo.lstData[4]),
            //                    0,
            //                    0,
            //                    0);
        }
    }
}
