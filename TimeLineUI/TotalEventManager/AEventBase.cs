using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public enum EVENTOBJECTTYPE
    {
        NONE,
        EVENT,          // 일반 이벤트 타입인지 
        GROUP,          // 그룹 이벤트(GROUP, CHECK BUTTON, CALL BUTTON) 타입인지
        ETC             // 기타(SOUND등) 타입인지
    }

    public struct ScriptInfo
    {
        public List<string> lstData;
        // 라인으로 변환시의 인덱스위치(액션이 속한 라인위치들) - 참고 정보들 - 잘라낸 앞부분에서 줄바꿈 기호갯수를 세야함.
        public List<int> lstLineIdx;

        // 전체스크립트에서 어느 레이어에 있는것이었는지
        public int nLayerIdx;

        // 전체줄에서의 인덱스위치와 길이
        public int nStartIdx;
        public int nLength;

        public ScriptInfo(int layerIdx)
        {
            nLayerIdx = layerIdx;

            nStartIdx = -1;
            nLength = -1;

            lstData = new List<string>();
            lstLineIdx = new List<int>();
        }
    }

    public abstract class AEventBase
    {
        public delegate void ObjectEventHandler(object sender, EventArgs e);
        public event ObjectEventHandler MouseEnterNotice;
        public event ObjectEventHandler MouseLeaveNotice;

        public string strName { get; set; }
        public int nTickIdx { get; set; }

        public int nUniqueID { get; set; }                  // 이 이벤트가 속한 오브젝트 아이디
        public int nLayerdepth_index { get; set; }

        public ScriptInfo mScriptInfo { get; set; }
        public EVENTOBJECTTYPE eObjectType { get; set; }    // 큰 분류(그룹인지, 이벤트인지)
        public TOTALEVENTTYPE eTotalEventType { get; set; } // 세부 분류
    }
}
