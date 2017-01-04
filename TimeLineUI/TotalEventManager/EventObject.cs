using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLineUI
{
    public class EventObject : AEventBase
    {
        public bool IsPlay { get; set; }

        public EventObject(ScriptInfo scriptInfo, int r_uniqueID, TOTALEVENTTYPE eType)
        {
            strName = TotalEventMng.lstEventType[(int)eType];
            nTickIdx = -1;

            uniID = Guid.NewGuid();

            nObjectUniID = r_uniqueID;
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
                    List<GroupObject> gObjs = TotalEventMng.FindGroupObjectByGroupIndex(int.Parse(mScriptInfo.lstData[0]));
                    if (gObjs == null)
                    {
                        MessageBox.Show("맞는 그룹이 없습니다.");
                        return;
                    }

                    // 그룹인덱스로만 검색하면 2개이상이 나올 가능성이 있으나 옛날 번개툴일때 문제이고
                    // 앱버스일때는 일단 그룹인덱스 겹치지 않는다는 전재하에서 작업
                    // 그러므로 얻어온 리스트중에 첫번째거로만 작업해도 큰 차이없음.
                    GroupObject gObj = gObjs[0];
                    (gObj as EGroup).IsPlay = true;
                }
            }
        }
    }
}
