using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class GroupObject : AEventBase
    {
        public int GroupID { get; set; }                // 그룹인덱스(GROUP, CALL BUTTON, CHECHK BUTTON)가 가지고 있는 인덱스
        public int GroupDelay { get; set; }             // 그룹인덱스의 딜레이 (없을수도 있음)
        public bool IsClick { get; set; }

        //virtual public void Parse() { }
        virtual public void Click(int x, int y) { }
        virtual public void Play(long now) { }

        public SortedList<Guid, EventObject> lstEventObjects = new SortedList<Guid, EventObject>(); // 그룹에 포함된 이벤트 리스트들
        public EventObject mLeaderObj;                  // 이 그룹의 클릭처리를 위한 대표오브젝트

        public GroupObject(ScriptInfo scriptInfo, TOTALEVENTTYPE eType, int rGroupID, int rGroupDelay = 0)
        {
            strName = "";
            nTickIdx = 0;

            uniID = Guid.NewGuid();
            nLayerdepth_index = scriptInfo.nLayerIdx;

            GroupID = rGroupID;
            GroupDelay = rGroupDelay;

            eObjectType = EVENTOBJECTTYPE.GROUP;
            eTotalEventType = eType;

            mScriptInfo = scriptInfo;

            IsClick = false;
        }

        public void SetLeaderObject()
        {
            if (lstEventObjects.Count > 0)
                mLeaderObj = lstEventObjects.Values[0];
        }
    }
}
