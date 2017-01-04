using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    // 순서는 기존소스 순서대로...
    public enum TOTALEVENTTYPE
    {
        NONE,
        CAMERA,
        INIT,
        CLEAR,                  // 클리어
        CLEARGROUP,
        STOP,
        APP,
        SOUND,                  // 사운드 - 예외처리(가상의 사운드 오브젝트생성후 추가되어야 함)
        DISABLE,
        CALL_BUTTON,            // 콜버튼
        LOADPROJECT,
        PROJECT_BUTTON,
        DOWNLOAD,
        SCREEN_SHOT,
        MOTIONINC,              // 모션증가
        MOTIONDEC,              // 모션감소
        MOTION,                 // 모션변경
        MOVE_LINEAR,            // 직선이동
        MOVE_DECELERATE,        // 감속이동
        MOVE_POSITION,          // 좌표설정
        ROTATE,                 // 회전
        ZOOM,                   // 줌 인/ 아웃
        FADE_IN,                // 페이드 인
        FADE_OUT,               // 페이드 아웃
        BLUETOOTH,
        PLAY_BUTTON,
        PLAYGROUP,              // 그룹플레이
        PLAY,                   // 플레이이벤트
        CHECK_BUTTON,           // 체크버튼         
        CHECK_TARGET,
        //END_BUTTON,           // 파싱할때 예외처리
        //END_TARGET,           // 파싱할때 예외처리
        CHECK_COUNT,
        //END_COUNT,            // 파싱할때 예외처리
        GROUP,                  // 그룹설정
        //END_GROUP,            // 그룹생성종료 - 파싱할때 예외처리
        EFFECT_BUTTON,
        QUAKE,
        CRASH_EFFECT,           // 충돌효과
        STAR,
        TARGET,                 // 타겟설정
        SCROLL,                 // 스크롤
        SETPLAY,                // 플레이
        SHOW,                   // 보임/숨김
        ROLL,                   // 반복배경
        BACKUP,                 // 상태백업
        STOPGAME,

        // 이하 신규액션
        JUMP,
        WADDLE,
        SHEAR,
        BLINK,
        SWING,
        FLOAT,
        SPRING,
        WAIT,
        TIMESET,
        FADEX,
        FLIP,
        DRAG,
        CAMERAX,
        PHOTO,
        SENDBLUETOOTH,
        ANIMATION,
        SENDNET,
        X_CRASH,
        URL,
        X_SOUND,
        LANGUAGE,
        OBJECT_DEAD,
    }

    public enum SCRITEDITTYPE
    {
        NONE,
        DELETE,
        REPLACE,
        ADD,

    }

    public struct DrawEventObjectInfo
    {
        public string strEventName;
        public int nObjectUniID;
        public Guid LinkUniID;           // 이 이벤트랑 연결된 유니크 아이디
        public int nLayerdepth_index;
        public string[] strData;
        public int nGroupID;
        public int nGroupDelay;
    }

    public class TotalEventMng
    {
        // 레이어별 리스트를 갖고있음.
        static public List<List<GroupObject>> mlstGroupObjects = new List<List<GroupObject>>();
        static public List<List<EventObject>> mlstEventObjects = new List<List<EventObject>>();

        static public List<string> lstEventType = new List<string> {
            "NONE",
            "CAMERA",
            "INIT",
            "CLEAR",                  // 클리어
            "CLEARGROUP",
            "STOP",
            "APP",
            "SOUND",                  // 사운드 - 예외처리(가상의 사운드 오브젝트생성후 추가되어야 함)
            "DISABLE",
            "CALL BUTTON",            // 문제작업용  
            "LOADPROJECT",
            "PROJECT BUTTON",
            "DOWNLOAD",
            "SCREEN SHOT",
            "MOTIONINC",              // 모션증가
            "MOTIONDEC",              // 모션감소
            "MOTION",                 // 모션변경
            "MOVE LINEAR",            // 직선이동
            "MOVE DECELERATE",        // 감속이동
            "MOVE POSITION",          // 좌표설정
            "ROTATE",                 // 회전
            "ZOOM",                   // 줌 인/ 아웃
            "FADE IN",                // 페이드 인
            "FADE OUT",               // 페이드 아웃
            "BLUETOOTH",
            "PLAY BUTTON",
            "PLAYGROUP",              // 그룹플레이
            "PLAY",                   // 플레이이벤트
            "CHECK BUTTON",           // 일반 버튼속성
            "CHECK TARGET",
            //"END BUTTON",           // 파싱할때 예외처리
            //"END TARGET",           // 파싱할때 예외처리
            "CHECK COUNT",
            //"END COUNT",            // 파싱할때 예외처리
            "GROUP",                  // 그룹설정
            //"END GROUP",            // 그룹생성종료 - 파싱할때 예외처리
            "EFFECT BUTTON",
            "QUAKE",
            "CRASH EFFECT",           // 충돌효과
            "STAR",
            "TARGET",                 // 타겟설정
            "SCROLL",                 // 스크롤
            "SETPLAY",                // 플레이
            "SHOW",                   // 보임/숨김
            "ROLL",                   // 반복배경
            "BACKUP",                 // 상태백업
            "STOPGAME",

            // 이하 신규액션
            "JUMP",
            "WADDLE",
            "SHEAR",
            "BLINK",
            "SWING",
            "FLOAT",
            "SPRING",
            "WAIT",
            "TIMESET",
            "FADEX",
            "FLIP",
            "DRAG",
            "CAMERAX",
            "PHOTO",
            "SENDBLUETOOTH",
            "ANIMATION",
            "SENDNET",
            "X_CRASH",
            "URL",
            "X_SOUND",
            "LANGUAGE",
            "OBJECT_DEAD",
        };

        static public int nSoundObjectID = int.MaxValue;
        static public int nCameraObjectID = int.MaxValue - 1;

        static private int nMaxLayerCount;

        static public List<DrawEventObjectInfo> MakeDrawObjects(int nLayerIndex)
        {
            List<DrawEventObjectInfo> result = new List<DrawEventObjectInfo>();

            foreach (EventObject obj in mlstEventObjects[nLayerIndex])
            {
                DrawEventObjectInfo info = new DrawEventObjectInfo();
                info.strEventName = obj.strName;
                info.nObjectUniID = obj.nObjectUniID; // 자신의 유니크아이디가 아니라 찾아야할 유니크 아이디
                info.LinkUniID = obj.uniID;
                info.nLayerdepth_index = obj.mScriptInfo.nLayerIdx;
                info.strData = obj.mScriptInfo.lstData.ToArray();

                info.nGroupID = -1;
                info.nGroupDelay = 0;

                result.Add(info);
            }

            foreach (GroupObject gObj in mlstGroupObjects[nLayerIndex])
            {
                foreach (var eObj in gObj.lstEventObjects)
                {
                    // 그룹안에 있는 사운드는 Group 빼고는 타임라인에 추가하면 안됨
                    if (eObj.Value.eTotalEventType == TOTALEVENTTYPE.SOUND &&
                        gObj.eTotalEventType != TOTALEVENTTYPE.GROUP)
                    {
                        continue;
                    }

                    DrawEventObjectInfo info = new DrawEventObjectInfo();
                    info.strEventName = eObj.Value.strName;
                    info.nObjectUniID = eObj.Value.nObjectUniID;   // 자신의 유니크아이디가 아니라 찾아야할 유니크 아이디
                    info.LinkUniID = eObj.Value.uniID;
                    info.nLayerdepth_index = eObj.Value.mScriptInfo.nLayerIdx;
                    info.strData = eObj.Value.mScriptInfo.lstData.ToArray();

                    info.nGroupID = gObj.GroupID;
                    info.nGroupDelay = gObj.GroupDelay;

                    result.Add(info);
                }
            }

            return result;
        }

        static public void ScriptParser(string[] script_text, int MaxLayerCount)//, AppBusObjectManager mng)
        {
            nMaxLayerCount = MaxLayerCount;
            //mAppBusObjMng = mng;

            mlstGroupObjects.Clear();
            mlstEventObjects.Clear();

            for (int i = 0; i < MaxLayerCount; i++)
            {
                List<GroupObject> tempGroupObjects;
                List<EventObject> tempEventObjects;
                _ScriptParser(script_text, i, out tempGroupObjects, out tempEventObjects);
                mlstGroupObjects.Add(tempGroupObjects);
                mlstEventObjects.Add(tempEventObjects);
            }

            // PLAYGROUP 리스트 돌면서
            // 찾은 그룹에 PLAYGROUP에서 찾은 딜레이값 적용해야 함.
            List<EventObject> lstPlayGroups = FindEventObjectByType(TOTALEVENTTYPE.PLAYGROUP);
            foreach (EventObject obj in lstPlayGroups)
            {
                List<GroupObject> gObjs = FindGroupObjectByGroupIndex(int.Parse(obj.mScriptInfo.lstData[0]));
                foreach (GroupObject gObj in gObjs)
                {
                    if (gObj.eTotalEventType == TOTALEVENTTYPE.GROUP)
                    {
                        //int uniqe = int.Parse(obj.mScriptInfo.lstData[0]);

                        int delay = int.Parse(obj.mScriptInfo.lstData[1]);
                        gObj.GroupDelay = delay;
                    }
                }
            }


            // 이하 테스트 문장
            // 파싱이 끝난 오브젝트들을 가지고 편집테스트 직접적으로 script_text 변경됨 - 내부테스트 끝나고 바깥(AppBusMain)에도 적용
            // 1. 삭제
            // 2. 대치
            // 3. 추가




            // 오브젝트 스크립트 삭제
            //EventObject tempobj = FindEventObjectByUniqueID(100);
            //string delResult = DeleteScript(script_text, tempobj);

            // 그룹 스크립트 삭제
            //GroupObject workObj = FindGroupObjectByGroupIndex(0);
            //string delGResult = EditScript(script_text, workObj, SCRITEDITTYPE.DELETE);

            //// 새 스크립트 대치
            //GroupObject workObj1 = FindGroupObjectByGroupIndex(0);
            //string repGResult = EditScript(script_text, workObj1, SCRITEDITTYPE.REPLACE, "test");

            //// 새 스크립트 추가
            //string addResult = EditScript(script_text, null, SCRITEDITTYPE.ADD, "new string");
        }

        

        static public string EditScript(string[] workString, AEventBase OrgObject, SCRITEDITTYPE editType, string newString = "", int workLayer = 0)
        {
            if (editType != SCRITEDITTYPE.ADD)
            {
                string strTotal = workString[OrgObject.mScriptInfo.nLayerIdx];
                string strHead = strTotal.Substring(0, OrgObject.mScriptInfo.nStartIdx);

                string strBody = strTotal.Substring(OrgObject.mScriptInfo.nStartIdx, OrgObject.mScriptInfo.nLength);

                int tailStartIdx = OrgObject.mScriptInfo.nStartIdx + OrgObject.mScriptInfo.nLength;
                int tailLength = strTotal.Length - tailStartIdx;
                string strTail = strTotal.Substring(tailStartIdx, tailLength);

                if (editType == SCRITEDITTYPE.DELETE)
                    return strHead + strTail;
                else if (editType == SCRITEDITTYPE.REPLACE)
                    return strHead + newString + strTail;
                else
                    return "";
            }
            else
            {
                string strTotal = workString[workLayer];
                return strTotal + newString + "\r\n";
            }
        }

        static private void _ScriptParser(string[] script_text, int nLayerIndex, out List<GroupObject> groupResult, out List<EventObject> eventResult)
        {
            groupResult = new List<GroupObject>();
            eventResult = new List<EventObject>();

            // 그룹타입은 총 4가지
            // "CHECK BUTTON" - "END BUTTON",           
            // "CHECK TARGET" - "END TARGET",
            // "CHECK COUNT" - "END COUNT",
            // "GROUP" - "END GROUP"            

            // 문제타입 1가지
            // "CALL BUTTON" - "FINISH GROUP"


            // [GROUP] ~ [END GROUP] 묶음찾기
            string strFirstWord = lstEventType[(int)TOTALEVENTTYPE.GROUP];
            string strLastWord = "[END GROUP]";
            Dictionary<Guid, ScriptInfo> dicGroup = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
            foreach (var obj in dicGroup)
            {
                foreach (var sub in obj.Value.lstData)
                    Console.WriteLine("일반 그룹-엔드그룹 (키 {0}, 시작포인트/폭 {1})", obj.Key, sub);

                EGroup eGroupObj = new EGroup(obj.Value, TOTALEVENTTYPE.GROUP, int.Parse(obj.Value.lstData[0]), 0);
                groupResult.Add(eGroupObj);
            }

            // [CHECK BUTTON] ~ [END BUTTON] 묶음찾기
            strFirstWord = lstEventType[(int)TOTALEVENTTYPE.CHECK_BUTTON];
            strLastWord = "[END BUTTON]";
            Dictionary<Guid, ScriptInfo> dicCheckButton = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
            foreach (var obj in dicCheckButton)
            {
                foreach (var sub in obj.Value.lstData)
                    Console.WriteLine("체크버튼 그룹-엔드그룹 (키 {0}, 시작포인트/폭 {1})", obj.Key, sub);

                ECheckButton eCheckButtonObj = new ECheckButton(obj.Value, TOTALEVENTTYPE.CHECK_BUTTON, int.Parse(obj.Value.lstData[0]), 0);
                groupResult.Add(eCheckButtonObj);
            }

            // [CALL BUTTON] ~ [FINISH GROUP] 묶음찾기
            strFirstWord = lstEventType[(int)TOTALEVENTTYPE.CALL_BUTTON];
            strLastWord = "[FINISH GROUP]";
            Dictionary<Guid, ScriptInfo> dicCallButton = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
            foreach (var obj in dicCheckButton)
            {
                foreach (var sub in obj.Value.lstData)
                    Console.WriteLine("콜버튼 그룹-엔드그룹 (키 {0}, 시작포인트/폭 {1})", obj.Key, sub);

                // 콜버튼쪽은 아직 처리 안됐음....
            }

            Console.WriteLine("-------------------");


            // 모든 이벤트 문자열 묶음 찾기
            for (int i = 0; i < lstEventType.Count; i++)
            {
                // 찾을 액션문자열
                string strAction = lstEventType[i];
                string strActionLast = "]";

                // dicActionGroup 의 키값이 해당단어의 검색된 위치인덱스임.
                Dictionary<Guid, ScriptInfo> dicActionGroup = _EventCoreParser(script_text, nLayerIndex, strAction, strActionLast);
                //foreach (var obj in dicActionGroup)
                //{
                //    foreach (var sub in obj.Value)
                //        Console.WriteLine("이벤트 {0} (키 {1}, 값 {2})", lstEventType[i], obj.Key, sub);
                //}

                // 여기서 부터는 그룹에 속한 이벤트처리
                foreach (var action in dicActionGroup)
                {
                    // 미리 각 액션에 해당하는 클래스로 액션클래스를 할당한 다음
                    // 이벤트리스트에 추가할지 그룹리스트에 추가할지 처리하면 됨.

                    EventObject eventObj = null;
                    if (i == (int)TOTALEVENTTYPE.PLAYGROUP)
                    {
                        Console.WriteLine("액션{0}이 실행할 그룹인덱스 {1}, 위치값 {2}, 딜레이값 {3}", lstEventType[i], action.Value.lstData[0], action.Key, action.Value.lstData[1]);

                        // 플레이 그룹은 가리키는 오브젝트 유니크 아이디가 없음.
                        eventObj = new EventObject(action.Value, -1, TOTALEVENTTYPE.PLAYGROUP);
                        eventResult.Add(eventObj);
                    }
                    else
                    {
                        if (i == (int)TOTALEVENTTYPE.CHECK_BUTTON ||
                            i == (int)TOTALEVENTTYPE.CHECK_TARGET ||
                            i == (int)TOTALEVENTTYPE.CHECK_COUNT ||
                            i == (int)TOTALEVENTTYPE.GROUP ||
                            i == (int)TOTALEVENTTYPE.CALL_BUTTON)
                            continue;

                        eventObj = MakeEvent(action, lstEventType[i], (TOTALEVENTTYPE)i);
                    }


                    int nGroupScriptIdx = -1;
                    if (dicGroup != null)
                        nGroupScriptIdx = _FindGroupInAction(dicGroup, eventObj.mScriptInfo.nStartIdx);

                    int nCheckButtonScriptIdx = -1;
                    if (dicCheckButton != null)
                        nCheckButtonScriptIdx = _FindGroupInAction(dicCheckButton, eventObj.mScriptInfo.nStartIdx);

                    int nCallButtonScriptIdx = -1;
                    if (dicCallButton != null)
                        nCallButtonScriptIdx = _FindGroupInAction(dicCallButton, eventObj.mScriptInfo.nStartIdx);



                    if (nGroupScriptIdx >= 0)
                    {
                        Console.WriteLine("액션{0}의 그룹시작값 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nGroupScriptIdx, action.Key, action.Value.lstData[0]);

                        // 그룹오브젝트의 자식들로 추가
                        AddActionInGroupByScriptIdx(groupResult, eventObj, nGroupScriptIdx, nLayerIndex);
                    }
                    else if (nCheckButtonScriptIdx >= 0)
                    {
                        Console.WriteLine("액션{0}의 체크버튼 그룹시작값 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nCheckButtonScriptIdx, action.Key, action.Value.lstData[0]);

                        // 체크버튼그룹 오브젝트의 자식들로 추가
                        AddActionInGroupByScriptIdx(groupResult, eventObj, nCheckButtonScriptIdx, nLayerIndex, 100);
                    }
                    else if (nCallButtonScriptIdx >= 0)
                    {
                        //Console.WriteLine("액션{0}의 그룹인덱스 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nCallButtonGropuIdx, action.Key, action.Value[0]);
                        // 콜 버튼그룹 오브젝트의 자식들로 추가
                    }
                    else // 그룹에 안 속한 것들
                    {
                        eventResult.Add(eventObj);
                    }
                }
            }

            //// 그룹별 액션 추가작업이 끝났다면 그룹별 대표 오브젝트 지정 
            //// 각 그룹별로 대표오브젝트 지정 - 일단 체크버튼만
            //foreach (var obj in dicCheckButton)
            //{
            //    int findUniqueID = Utils.getID_gObject(nLayerIndex, obj.Key);
            //    int findIdx = groupResult.FindIndex(r => r.nUniqueID == findUniqueID);
            //    groupResult[findIdx].SetLeaderObject();
            //}

            Console.WriteLine("---------------------------------------------" + nLayerIndex);

            //return result;
        }

        static private EventObject MakeEvent(KeyValuePair<Guid, ScriptInfo> action, string actionName, TOTALEVENTTYPE type)
        {
            int uniqueID = 0;
            EventObject eventObj = null;

            if (int.TryParse(action.Value.lstData[0], out uniqueID))
            {
                if (type == TOTALEVENTTYPE.MOVE_LINEAR)
                {
                    eventObj = new EMoveLinear(action.Value, uniqueID, TOTALEVENTTYPE.MOVE_LINEAR);
                }
                else if (type == TOTALEVENTTYPE.FADE_IN)
                {
                    eventObj = new EFade(action.Value, uniqueID, TOTALEVENTTYPE.FADE_IN);
                }
                else if (type == TOTALEVENTTYPE.FADE_OUT)
                {
                    eventObj = new EFade(action.Value, uniqueID, TOTALEVENTTYPE.FADE_OUT);
                }
                else if (type == TOTALEVENTTYPE.ZOOM)
                {
                    eventObj = new EScale(action.Value, uniqueID, TOTALEVENTTYPE.ZOOM);
                }
                else // 아직 모든 이벤트가 적용이 안되어 있으므로
                {
                    eventObj = new EventObject(action.Value, uniqueID, type);
                }
            }
            else
            {
                // 유니크 아이디가 없는 액션들...
            }

            return eventObj;
        }

        static private void AddActionInGroupByGroupID(List<GroupObject> lstGroup, EventObject eObj, int nGroupID, int nLayerIndex, int revise = 1)
        {
            Guid key = eObj.uniID;

            List<GroupObject> gObjs = FindGroupObjectByGroupIndex(nGroupID);
            // 그룹인덱스로만 검색하면 2개이상이 나올 가능성이 있으나 옛날 번개툴일때 문제이고
            // 앱버스일때는 일단 그룹인덱스 겹치지 않는다는 전재하에서 작업
            // 그러므로 얻어온 리스트중에 첫번째거로만 작업해도 큰 차이없음.
            if (gObjs == null)
            {
                MessageBox.Show("맞는 그룹이 없습니다.");
                return;
            }
            GroupObject gObj = gObjs[0];

            gObj.lstEventObjects.Add(key, eObj);
        }

        // 그룹에 액션들추가 - 현재는 일부분만 구현 - 약간 예외성격의 메소드임 다시 작성해야 함.
        static private void AddActionInGroupByScriptIdx(List<GroupObject> lstGroup, EventObject eObj, int nGroupScriptStartIdx, int nLayerIndex, int revise = 1)
        {
            Guid key = eObj.uniID;

            int findIdx = lstGroup.FindIndex(r => r.mScriptInfo.nStartIdx == nGroupScriptStartIdx);
            if (findIdx == -1)
            {
                Console.WriteLine(" 맞는 그룹이 없습니다.");
                return;
            }

            lstGroup[findIdx].lstEventObjects.Add(key, eObj);

            Console.WriteLine("{0} EventCount : {1}", findIdx, lstGroup[findIdx].lstEventObjects.Count);
        }

        // 이 함수가 만드는 키는 찾는 그룹이나 단어에 따라 달라짐
        // [GROUP=그룹인덱스] ~ [END GROUP] 를 찾는다면
        // 키 : 그룹인덱스
        // 값 : 0("[GROUP=" 를 찾은 문자열 위치), 1("[END GROUP]" 를 찾은 문자열 위치)
        //
        // [PLAYGROUP] 를 찾는다면
        // 키 : PLAYGROUP이 참조하는 그룹인덱스 - 그룹당 하나의 PLAYGROUP만 짝지워지므로 찾기 쉬우라고
        // 값 : 0(PLAYGROUP이 참조하는 그룹인덱스), 1(딜레이값)  
        //
        // 일반액션들(FADE IN, MOVE, ...) 를 찾는다면
        // 키 : 해당액션을 찾은 문자열 위치
        // 값 : 액션뒤에 나오는 배열그대로 (쓰는건 알아서)
        //
        static private Dictionary<Guid, ScriptInfo> _EventCoreParser(string[] workString, int nLayerIndex, string firstWord, string findLastWord)
        {
            string strFirstWord = "[";
            strFirstWord += firstWord;
            strFirstWord += "=";

            // 문자열안에 여러개의 그룹이 올수 있으므로
            Dictionary<Guid, ScriptInfo> result = new Dictionary<Guid, ScriptInfo>();

            string text = workString[nLayerIndex];

            // 찾을 문자열이 여러개 섞여있을때 인덱스리스트 얻기
            List<int> lstfindFirstIndex = _GetPositions(workString[nLayerIndex], strFirstWord);
            List<int> lstfindLastIndex = _GetPositions(workString[nLayerIndex], findLastWord);

            Guid key = Guid.Empty;
            int pairIndex = 0;
            foreach (int nFirstWordIndex in lstfindFirstIndex)
            {
                string str = text.Substring(nFirstWordIndex);

                int nLastWordIndex = lstfindLastIndex[pairIndex];
                if (nLastWordIndex < 0) return null;

                pairIndex++;

                // 한 액션의 정보 [액션=데이터]
                int nEventEndIdx = str.IndexOf("]");
                int nEventLength = nEventEndIdx - strFirstWord.Length;
                string tempEventValues = str.Substring(strFirstWord.Length, nEventLength);
                string[] strEventValues = tempEventValues.Split(',');


                // Console.WriteLine("액션 {0}, 유니크 {1}, 레이어 {2}, 딜레이 {3}, 인덱스 {4}", findFirstWord, values[0], j, values[values.Length - 1], idx);

                // 그룹세트일때 스크립트 정보에 몸통정보가 별도 처리
                if (findLastWord == "[END BUTTON]" ||
                    findLastWord == "[END TARGET]" ||
                    findLastWord == "[END COUNT]" ||
                    findLastWord == "[END GROUP]" ||
                    findLastWord == "[FINISH GROUP]")
                {
                    string GroupHead = string.Format("{0}{1}]", strFirstWord, tempEventValues);
                    int nGroupBodyStartIdx = nFirstWordIndex + GroupHead.Length;
                    int nGroupBodyEndIdx = nLastWordIndex;
                    int nGroupLength = nGroupBodyEndIdx - nGroupBodyStartIdx;

                    key = Guid.NewGuid();

                    ScriptInfo info = new ScriptInfo("");
                    info.nLayerIdx = nLayerIndex;                       // 추후 삭제될 예정
                    info.lstData.Add(nGroupBodyStartIdx.ToString());    // 그룹일때는 몸통의 최초인덱스 - 추후 변경될 예정(어떤 정보로 교체할지 미정)
                    info.lstData.Add(nGroupBodyEndIdx.ToString());      // 그룹일때는 몸통의 마지막인덱스 - 추후 변경될 예정(어떤 정보로 교체할지 미정)
                    info.nStartIdx = nFirstWordIndex;                                        // 첫단어의 최초인덱스, 추후 삭제될 예정
                    info.nLength = (nLastWordIndex - nFirstWordIndex) + findLastWord.Length; // 마지막 단어의 길이까지 포함한 전체길이, 추후 삭제될 예정

                    //info.strScriptData = 그룹은 스크립트 정보 없음.

                    // 테스트 문장
                    //string full = text.Substring(info.nStartIdx, info.nLength);
                    //string body = text.Substring(nGroupBodyStartIdx, nGroupLength);

                    result.Add(key, info);
                }
                else
                {
                    int intParseResult = 0;
                    if (int.TryParse(strEventValues[0], out intParseResult)) // 유니크 오브젝트 아이디가 있는 명령어라면...
                    {
                        key = Guid.NewGuid();

                        ScriptInfo info = new ScriptInfo("");
                        info.nLayerIdx = nLayerIndex;                       // 추후 삭제될 예정
                        info.lstData = strEventValues.ToList();
                        info.nStartIdx = nFirstWordIndex;                   // 추후 삭제될 예정
                        info.nLength = nEventEndIdx + 1;                    // 추후 삭제될 예정
                        info.strScriptData = text.Substring(info.nStartIdx, info.nLength);

                        //Console.WriteLine("info.strScriptData : {0}", info.strScriptData);

                        result.Add(key, info);
                    }
                    else // 예외나 에러처리...
                    {
                        if (firstWord == "SOUND")
                        {
                            key = Guid.NewGuid();

                            ScriptInfo info = new ScriptInfo("");
                            info.nLayerIdx = nLayerIndex;                       // 추후 삭제될 예정
                            info.lstData = strEventValues.ToList();
                            info.nStartIdx = nFirstWordIndex;                   // 추후 삭제될 예정
                            info.nLength = nEventEndIdx + 1;                    // 추후 삭제될 예정
                            info.strScriptData = text.Substring(info.nStartIdx, info.nLength);

                            // 0번째에 강제로 유니크 아이디 삽입
                            info.lstData.Insert(0, nSoundObjectID.ToString());

                            result.Add(key, info);
                        }
                        else // if(firstWord == "X_CRASH" || firstWord == "QUAKE")
                        {
                            key = Guid.NewGuid();

                            ScriptInfo info = new ScriptInfo("");
                            info.nLayerIdx = nLayerIndex;                       // 추후 삭제될 예정
                            info.lstData = strEventValues.ToList();
                            info.nStartIdx = nFirstWordIndex;                   // 추후 삭제될 예정
                            info.nLength = nEventEndIdx + 1;                    // 추후 삭제될 예정
                            info.strScriptData = text.Substring(info.nStartIdx, info.nLength);

                            // 0번째에 강제로 유니크 아이디 삽입 (일단 모두 카메라 오브젝트로 몰기)
                            info.lstData.Insert(0, nCameraObjectID.ToString());

                            result.Add(key, info);
                        }
                    }
                }
            }

            return result;
        }

        static private int _FindGroupInAction(Dictionary<Guid, ScriptInfo> dicGroup, int actionIdx)
        {
            // 그룹이라는 단어가 나오면 그 인덱스들을 기억한 다음 엔드그룹이라는 인덱스도 기억 - 딕셔너리 처리
            // 이벤트단어를 찾았을때 그룹인덱스와 엔드그룹인덱스안에 있다면 해당그룹임.
            foreach (KeyValuePair<Guid, ScriptInfo> dicObj in dicGroup)
            {
                //Console.WriteLine("Key:{0} First:{1}-Last:{2}", dicObj.Key, dicObj.Value[0], dicObj.Value[1]);
                int nF = int.Parse(dicObj.Value.lstData[0]);
                int nL = int.Parse(dicObj.Value.lstData[1]);

                if (actionIdx > nF && actionIdx < nF + nL)
                {
                    return dicObj.Value.nStartIdx;
                }
            }

            return -1;
        }

        // 여러개의 같은 단어의 위치리스트 구하기
        static private List<int> _GetPositions(string source, string searchString)
        {
            List<int> ret = new List<int>();
            int len = searchString.Length;
            int start = -len;
            while (true)
            {
                start = source.IndexOf(searchString, start + len);
                if (start == -1)
                {
                    break;
                }
                else
                {
                    ret.Add(start);
                }
            }
            return ret;
        }

        static public void ClickProcess(int mousex, int mousey)
        {
            Point location = new Point(mousex, mousey);
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                foreach (var gObj in mlstGroupObjects[i])
                {
                    gObj.Click(mousex, mousey);
                }
            }
        }

        static public void PlayProcess(long now)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                if (mlstGroupObjects.Count > 0)
                {
                    foreach (var gObj in mlstGroupObjects[i])
                    {
                        gObj.Play(now);
                    }
                }

                if (mlstEventObjects.Count > 0)
                {
                    foreach (var eObj in mlstEventObjects[i])
                    {
                        eObj.Play(now);
                    }
                }
            }
        }

        //static public AppBusObject FindAppBusObjectByUniqueID(int uniqueID)
        //{
        //    // 토탈이벤트 매니저에서 값을 가져와야 함.
        //    return mAppBusObjMng.GetObjectUniqueID(uniqueID);
        //}

        // 그룹아이디가 같은게 여러개일수 있음.
        static public List<GroupObject> FindGroupObjectByGroupIndex(int GroupID)
        {
            List<GroupObject> result = new List<GroupObject>();
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = mlstGroupObjects[i].FindIndex(r => r.GroupID == GroupID);
                if (index > -1)
                {
                    result.Add(mlstGroupObjects[i][index]);
                }
            }

            return result;
        }

        // 이벤트가 자체적으로 갖고 있는 유니크 아이디임. 
        // 이벤트가 가르키고 있는 유니크 아이디로 찾는것은 아래에 따로처리
        static public EventObject FindEventObjectBySelfUniqueID(Guid uniID, out int workLayer)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = mlstEventObjects[i].FindIndex(r => r.uniID == uniID);
                if (index > -1)
                {
                    workLayer = i;
                    return mlstEventObjects[i][index];
                }
            }

            workLayer = 0;
            return null;
        }

        // 그룹의 유니크 아이디 찾기
        static public GroupObject FindGroupObjectBySelfUniqueID(Guid uniqueID)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = mlstGroupObjects[i].FindIndex(r => r.uniID == uniqueID);
                if (index > -1)
                    return mlstGroupObjects[i][index];
            }

            return null;
        }

        // 그룹 유니크아이디로 그룹안에 있는 이벤트 오브젝트들을 찾기
        static public EventObject FindEventObjectInGroupBySelfUniqueID(Guid uniID, out int workLayer, out Guid groupUniID, out Guid key)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                foreach (GroupObject gObj in mlstGroupObjects[i])
                {
                    foreach (var eObj in gObj.lstEventObjects)
                    {
                        if (eObj.Value.uniID == uniID)
                        {
                            workLayer = i;
                            groupUniID = gObj.uniID;
                            key = eObj.Key;
                            return eObj.Value;
                        }
                    }
                }
            }

            workLayer = -1;
            groupUniID = Guid.Empty;
            key = Guid.Empty;
            return null;
        }

        static public List<EventObject> FindEventObjectByType(TOTALEVENTTYPE type)
        {
            List<EventObject> result = new List<EventObject>();

            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = mlstEventObjects[i].FindIndex(r => r.eTotalEventType == type);
                if (index > -1)
                    result.Add(mlstEventObjects[i][index]);
            }


            return result;
        }

        // 주의
        // FindEventObjectByUniqueID 이 함수는 이 이벤트가 가르키는 유니크 아이디로 찾는것임.
        // 즉 현재 EventObject는 따로 유니크 아이디가 없음. 자체적으로 스크립트 인덱스값만을 식별자로 가지고 있는것임.
        // 그룹은 임시로 부여되는 유니크 아이디가 있음.
        static public AEventBase FindTotalEventObjectBySelfUniqueID(Guid uniqueID)
        {
            int workLayer = -1; // 여기선 안씀
            EventObject eObj = FindEventObjectBySelfUniqueID(uniqueID, out workLayer);

            if (eObj != null)
            {
                // 그대로 리턴
                return eObj;
            }
            else
            {
                // 그룹의 유니크 아이디일수도 있고, 그룹안에 있는 이벤트오브젝트를 찾아와야 될수도 있고
                GroupObject gObj = FindGroupObjectBySelfUniqueID(uniqueID);
                if (gObj == null)
                {
                    workLayer = -1;         // 여기선 안씀
                    Guid GroupUniID = Guid.Empty;   // 여기선 안씀
                    Guid Key = Guid.Empty;
                    return FindEventObjectInGroupBySelfUniqueID(uniqueID, out workLayer, out GroupUniID, out Key);
                }
                else
                {
                    return gObj;
                }
            }
        }
    }
}
