using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class EventObject : AEventBase
    {
        public bool IsPlay { get; set; }

        // 스크립트 정보
        // 전체라인에서 "[" 위치값, "]" 끝위치값, 라인변환시 라인넘버

        public EventObject(ScriptInfo scriptInfo, int r_uniqueID, TOTALEVENTTYPE eType)
        {
            strName = TotalEventMng.lstEventType[(int)eType];
            nTickIdx = -1;

            nUniqueID = r_uniqueID;
            nLayerdepth_index = scriptInfo.nLayerIdx;

            eObjectType = EVENTOBJECTTYPE.EVENT;
            eTotalEventType = eType;

            mScriptInfo = scriptInfo;

            IsPlay = true;
        }

        virtual public void Play(long now)
        {
            if (eTotalEventType == TOTALEVENTTYPE.PLAYGROUP) // 차후에 PLAYGROUP 객체따로 생성해야 할듯
            {
                // 그룹오브젝트 실행
                long delayTime = long.Parse(mScriptInfo.lstData[1]) * 10000;
                if (now >= delayTime && IsPlay == true)
                {
                    IsPlay = false;
                    // 그룹인덱스로 그룹찾기

                    Console.WriteLine("PLAYGROUP 실행 {0}그룹, {1}딜레이", mScriptInfo.lstData[0], mScriptInfo.lstData[1]);
                    GroupObject gObj = TotalEventMng.FindGroupObjectByGroupIndex(int.Parse(mScriptInfo.lstData[0]));
                    (gObj as EGroup).IsPlay = true;
                }
            }
        }
    }
}
