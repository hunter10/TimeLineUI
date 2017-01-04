using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class EGroup : GroupObject
    {
        public bool IsPlay { get; set; }

        public EGroup(ScriptInfo scriptInfo, TOTALEVENTTYPE eType, int rGroupID, int rGroupDelay = 0) :
             base(scriptInfo, eType, rGroupID, rGroupDelay)
        {
            IsPlay = false;
        }

        //public override void Parse()
        //{
        //    base.Parse();
        //}

        // 그룹오브젝트는 클릭처리 없이 자동처리
        public override void Click(int mousex, int mousey)
        {
            base.Click(mousex, mousey);
        }

        public override void Play(long now)
        {
            base.Play(now);

            // 자식들 처리
            if (IsPlay)
            {
                IsPlay = false;

                foreach (var obj in lstEventObjects)
                {
                    Console.WriteLine("{0} Play!", obj.Value.strName);
                    obj.Value.Play(now);
                }
            }
        }
    }
}
