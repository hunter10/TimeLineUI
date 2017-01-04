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

    public enum GROUPTYPE
    {
        NONE,
        CHECK_BUTTON,
        CHECK_TARGET,
        CHECK_COUNT,
        GROUP,
        CALL_BUTTON,
    }

    public struct ScriptInfo
    {
        // 이 이벤트가 들고 있는 스크립트
        public string strScriptData;

        // 그룹의 파싱에서는 명령의 중간에 있는 인덱스값으로 세팅
        public List<string> lstData;

        // 전체스크립트에서 어느 레이어에 있는것이었는지 - 추후 삭제될 정보
        public int nLayerIdx;

        // 전체줄에서의 인덱스위치와 길이 - 추후 삭제될 정보 2017-1-3부터는 안써야 함.
        public int nStartIdx { set; get; }
        public int nLength { set; get; }

        public ScriptInfo(string scriptData)
        {
            nLayerIdx = -1;
            nStartIdx = -1;
            nLength = -1;

            strScriptData = "";
            lstData = new List<string>();
        }
    }

    public struct GroupInfo
    {
        public int GroupID { get; set; }            // 그룹인덱스가 가지고 있는 인덱스
        public int GroupDelay { get; set; }         // 그룹인덱스의 딜레이 (없을수도 있음)
        public GROUPTYPE GroupType { get; set; }
        // 그룹타입 - 추후 없어질수도 있음(오브젝트에서 처리되면)
    }

    public abstract class AEventBase
    {
        public SelectObject mDrawInfo { get; set; } // 타임에디터 객체정보
        public ScriptInfo mScriptInfo { get; set; } // 스크립트 정보
        public GroupInfo mGroupInfo { get; set; }   // 그룹정보

        public Guid uniID { get; set; }             // GUID    
        public int nObjectUniID { get; set; }       // 이 이벤트가 연결된 오브젝트 유니크 아이디
        public int nLayerdepth_index { get; set; }  // 추후 삭제될 정보 2017-1-3부터는 안써야 함.
        public string strName { get; set; }         // 타임라인에 표시될 이름(mScriptInfo 안에 포함되어 있음)
        public int nTickIdx { get; set; }           // 타임라인에 표시될 딜레이값(mScriptInfo 안에 포함되어 있음)

        public EVENTOBJECTTYPE eObjectType { get; set; }    // 큰 분류(그룹인지, 이벤트인지) - 추후 삭제될 정보 2017-1-3부터는 안써야 함.
        public TOTALEVENTTYPE eTotalEventType { get; set; } // 세부 분류
    }
}
