using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class ECheckButton : GroupObject
    {
        public ECheckButton(ScriptInfo scriptInfo, TOTALEVENTTYPE eType, int rGroupID, int rGroupDelay = 0) :
            base(scriptInfo, eType, rGroupID, rGroupDelay)
        {
        }

        //public override void Parse()
        //{
        //    base.Parse();
        //}

        public override void Click(int mousex, int mousey)
        {
            base.Click(mousex, mousey);

            // mLeaderObj의 클릭처리

            //Point location = new Point(mousex, mousey);

            //BayabaAnimation ani = (BayabaAnimation)TotalEventMng.FindAppBusObjectByUniqueID(mLeaderObj.nUniqueID);
            //if (ani.GetSprite().RectCheck(location))
            //{
            //    COM.LOOG("ECheck Button ", ani.ProjectLayerName, ani.uniqueID);
            //    ani.GameObject.click = true;

            //    IsClick = true;
            //}
        }

        public override void Play(long now)
        {
            base.Play(now);

            // 자식들 처리
            if (IsClick)
            {
                IsClick = false;

                foreach (var obj in lstEventObjects)
                {
                    Console.WriteLine("{0} Play!", obj.Value.strName);
                    obj.Value.Play(now);
                }
            }
        }
    }
}
