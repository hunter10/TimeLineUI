using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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

        CRASH_CREATE,           // 충돌생성
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
        public int nUniqueID;
        public int nLayerdepth_index;
        public string[] strData;
        public int nGroupID;
        public int nGroupDelay;
    }

    public class TotalEventMng
    {
        // 레이어별 리스트를 갖고있음.
        static public List<List<GroupObject>> lstGroupObjects = new List<List<GroupObject>>();
        static public List<List<EventObject>> lstEventObjects = new List<List<EventObject>>();

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

            "CRASH CREATE",           // 충돌생성
        };

        static public int nSoundObjectID = int.MaxValue;
        static public int nCameraObjectID = int.MaxValue - 1;

        static private int nMaxLayerCount;

        static public List<DrawEventObjectInfo> MakeDrawObjects(int nLayerIndex)
        {
            List<DrawEventObjectInfo> result = new List<DrawEventObjectInfo>();

            foreach (EventObject obj in lstEventObjects[nLayerIndex])
            {
                DrawEventObjectInfo info = new DrawEventObjectInfo();
                info.strEventName = obj.strName;
                info.nUniqueID = obj.nUniqueID;
                info.nLayerdepth_index = obj.mScriptInfo.nLayerIdx;
                info.strData = obj.mScriptInfo.lstData.ToArray();

                info.nGroupID = -1;
                info.nGroupDelay = 0;

                result.Add(info);
            }

            foreach (GroupObject gObj in lstGroupObjects[nLayerIndex])
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
                    info.nUniqueID = eObj.Value.nUniqueID;
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

            lstGroupObjects.Clear();
            lstEventObjects.Clear();

            for (int i = 0; i < MaxLayerCount; i++)
            {
                List<GroupObject> tempGroupObjects;
                List<EventObject> tempEventObjects;
                _ScriptParser(script_text, i, out tempGroupObjects, out tempEventObjects);
                lstGroupObjects.Add(tempGroupObjects);
                lstEventObjects.Add(tempEventObjects);
            }

            // PLAYGROUP 리스트 돌면서
            // 찾은 그룹에 PLAYGROUP에서 찾은 딜레이값 적용해야 함.
            List<EventObject> lstPlayGroups = FindEventObjectByType(TOTALEVENTTYPE.PLAYGROUP);
            foreach (EventObject obj in lstPlayGroups)
            {
                GroupObject gObj = FindGroupObjectByGroupIndex(int.Parse(obj.mScriptInfo.lstData[0]));
                int delay = int.Parse(obj.mScriptInfo.lstData[1]);

                gObj.GroupDelay = delay;
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

        static public AEventBase FindTotalEventObjectByUniqueID(int id)
        {
            EventObject eObj = FindEventObjectByUniqueID(id);

            if (eObj != null)
            {
                // 그대로 리턴
                return eObj;
            }
            else
            {
                return FindGroupObjectByGroupIndex(id);
            }
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
            // key : 그룹인덱스
            // value : 0([GROUP= 위치), 1([END GROUP]위치)
            string strFirstWord = lstEventType[(int)TOTALEVENTTYPE.GROUP];
            string strLastWord = "[END GROUP]";
            Dictionary<int, ScriptInfo> dicGroup = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
            foreach (var obj in dicGroup)
            {
                foreach (var sub in obj.Value.lstData)
                    Console.WriteLine("일반 그룹-엔드그룹 (키 {0}, 시작포인트/폭 {1})", obj.Key, sub);

                // 그룹 오브젝트 생성
                // 번개툴에서는 그룹의 첫번째 오브젝트의 유니크아이디가 그룹아이디지만 앱버스에서는 고유아이디를 부여해서 관리.
                int nUniqueID = Utils.getID_gObject(nLayerIndex, obj.Key);
                EGroup eGroupObj = new EGroup(obj.Value, nUniqueID, TOTALEVENTTYPE.GROUP, obj.Key, 0);
                groupResult.Add(eGroupObj);
            }

            // [CHECK BUTTON] ~ [END BUTTON] 묶음찾기
            // key : 그룹인덱스
            // value : 0([CHECK BUTTON= 위치), 1([END BUTTON]위치)
            strFirstWord = lstEventType[(int)TOTALEVENTTYPE.CHECK_BUTTON];
            strLastWord = "[END BUTTON]";
            Dictionary<int, ScriptInfo> dicCheckButton = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
            foreach (var obj in dicCheckButton)
            {
                foreach (var sub in obj.Value.lstData)
                    Console.WriteLine("체크버튼 그룹-엔드그룹 (키 {0}, 시작포인트/폭 {1})", obj.Key, sub);

                // 체크버튼 오브젝트 생성
                // 번개툴에서는 그룹의 첫번째 오브젝트의 유니크아이디가 그룹아이디지만 앱버스에서는 고유아이디를 부여해서 관리.
                // 체크버튼은 현재 유니크 아이디에 임시로 100을 곱해서 쓰고 있음
                int nUniqueID = Utils.getID_gObject(nLayerIndex, obj.Key * 100);
                ECheckButton eCheckButtonObj = new ECheckButton(obj.Value, nUniqueID, TOTALEVENTTYPE.CHECK_BUTTON, obj.Key, 0);
                groupResult.Add(eCheckButtonObj);
            }

            // [CALL BUTTON] ~ [FINISH GROUP] 묶음찾기
            // key : 그룹인덱스
            // value : 0([CALL BUTTON= 위치), 1([FINISH GROUP]위치)
            strFirstWord = lstEventType[(int)TOTALEVENTTYPE.CALL_BUTTON];
            strLastWord = "[FINISH GROUP]";
            Dictionary<int, ScriptInfo> dicCallButton = _EventCoreParser(script_text, nLayerIndex, strFirstWord, strLastWord);
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
                Dictionary<int, ScriptInfo> dicActionGroup = _EventCoreParser(script_text, nLayerIndex, strAction, strActionLast);
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
                        int nUniqueID = Utils.getID_gObject(nLayerIndex, action.Key);
                        eventObj = new EventObject(action.Value, nUniqueID, TOTALEVENTTYPE.PLAYGROUP);

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


                    int nGroupIdx = _FindGroupInAction(dicGroup, eventObj.mScriptInfo.nStartIdx);
                    int nCheckButtonGropuIdx = _FindGroupInAction(dicCheckButton, eventObj.mScriptInfo.nStartIdx);
                    int nCallButtonGropuIdx = _FindGroupInAction(dicCallButton, eventObj.mScriptInfo.nStartIdx);



                    if (nGroupIdx >= 0)
                    {
                        Console.WriteLine("액션{0}의 그룹인덱스 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nGroupIdx, action.Key, action.Value.lstData[0]);

                        // 그룹오브젝트의 자식들로 추가
                        AddActionInGroup(groupResult, eventObj, nGroupIdx, nLayerIndex);
                    }
                    else if (nCheckButtonGropuIdx >= 0)
                    {
                        Console.WriteLine("액션{0}의 그룹인덱스 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nCheckButtonGropuIdx, action.Key, action.Value.lstData[0]);

                        // 체크버튼그룹 오브젝트의 자식들로 추가
                        // 임시로 체크버튼의 유니크아이디를 찾으려면 100을 곱해서 찾는다.
                        AddActionInGroup(groupResult, eventObj, nCheckButtonGropuIdx, nLayerIndex, 100);
                    }
                    else if (nCallButtonGropuIdx >= 0)
                    {
                        //Console.WriteLine("액션{0}의 그룹인덱스 {1}, 위치값 {2}, 유니크아이디 {3}", lstEventType[i], nCallButtonGropuIdx, action.Key, action.Value[0]);
                        // 콜 버튼그룹 오브젝트의 자식들로 추가
                        int b = 0;
                    }
                    else // 그룹에 안 속한 것들
                    {
                        eventResult.Add(eventObj);
                    }
                }
            }

            // 그룹별 액션 추가작업이 끝났다면 그룹별 대표 오브젝트 지정 
            // 각 그룹별로 대표오브젝트 지정 - 일단 체크버튼만
            foreach (var obj in dicCheckButton)
            {
                int findUniqueID = Utils.getID_gObject(nLayerIndex, obj.Key);
                int findIdx = groupResult.FindIndex(r => r.nUniqueID == findUniqueID);
                groupResult[findIdx].SetLeaderObject();
            }

            Console.WriteLine("---------------------------------------------" + nLayerIndex);

            //return result;
        }

        static private EventObject MakeEvent(KeyValuePair<int, ScriptInfo> action, string actionName, TOTALEVENTTYPE type)
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

        // 그룹에 액션들추가 - 현재는 일부분만 구현 - 약간 예외성격의 메소드임 다시 작성해야 함.
        static private void AddActionInGroup(List<GroupObject> lstGroup, EventObject eObj, int nGroupID, int nLayerIndex, int revise = 1)
        {
            int key = eObj.mScriptInfo.nStartIdx; //eObj.nUniqueID; // 추후 모든 오브젝트에 유니크할당작업이 마무리되면 유니크아이디를 키값으로...

            int findUniqueID = Utils.getID_gObject(nLayerIndex, nGroupID * revise);
            int findIdx = lstGroup.FindIndex(r => r.nUniqueID == findUniqueID);
            lstGroup[findIdx].lstEventObjects.Add(key, eObj);
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
        static private Dictionary<int, ScriptInfo> _EventCoreParser(string[] workString, int nLayerIndex, string firstWord, string findLastWord)
        {
            string strFirstWord = "[";
            strFirstWord += firstWord;
            strFirstWord += "=";

            // 문자열안에 여러개의 그룹이 올수 있으므로
            Dictionary<int, ScriptInfo> result = new Dictionary<int, ScriptInfo>();

            string text = workString[nLayerIndex];

            // 찾을 문자열이 여러개 섞여있을때 인덱스리스트 얻기
            List<int> lstfindFirstIndex = _GetPositions(workString[nLayerIndex], strFirstWord);
            int key = -1;
            foreach (int idx in lstfindFirstIndex)
            {
                string str = text.Substring(idx);

                int findLastWordIndex = str.IndexOf(findLastWord);
                if (findLastWordIndex < 0) return null;

                int startindex = strFirstWord.Length;

                // 지정한 특정 단어의 정보 [액션= ~ [지정단어]
                int findLength = findLastWordIndex - strFirstWord.Length;
                string subValues = str.Substring(startindex, findLength);
                string[] values = subValues.Split(',');

                // 한 액션의 정보 [액션=데이터]
                int nEventEndIdx = str.IndexOf("]");
                int nEventLength = nEventEndIdx - strFirstWord.Length;
                string tempEventValues = str.Substring(startindex, nEventLength);
                string[] strEventValues = tempEventValues.Split(',');


                // Console.WriteLine("액션 {0}, 유니크 {1}, 레이어 {2}, 딜레이 {3}, 인덱스 {4}", findFirstWord, values[0], j, values[values.Length - 1], idx);


                int intParseResult = 0;
                if (int.TryParse(values[0], out intParseResult))
                {
                    key = idx;

                    ScriptInfo info = new ScriptInfo(nLayerIndex);
                    info.lstData = values.ToList();

                    info.nStartIdx = idx;
                    info.nLength = nEventEndIdx + 1;

                    // 테스트 문장
                    //string temp = text.Substring(info.nStartIdx, info.nLength);

                    result.Add(key, info);

                }
                else // 예외나 에러처리...
                {
                    if (findLastWord == "[END BUTTON]" ||
                        findLastWord == "[END TARGET]" ||
                        findLastWord == "[END COUNT]" ||
                        findLastWord == "[END GROUP]" ||

                        findLastWord == "[FINISH GROUP]")
                    {
                        // GROUP 인덱스가 키값
                        key = int.Parse(strEventValues[0]);

                        ScriptInfo info = new ScriptInfo(nLayerIndex);
                        info.lstData.Add(idx.ToString());
                        info.lstData.Add(findLastWordIndex.ToString());

                        info.nStartIdx = idx;
                        info.nLength = (findLastWordIndex - idx) + findLastWord.Length;

                        // 테스트 문장
                        //string temp = text.Substring(info.nStartIdx, info.nLength);

                        result.Add(key, info);
                    }
                    else if (firstWord == "SOUND")
                    {
                        key = idx;

                        ScriptInfo info = new ScriptInfo(nLayerIndex);
                        info.lstData = values.ToList();

                        info.nStartIdx = idx;
                        info.nLength = nEventEndIdx + 1;

                        // 0번째에 강제로 유니크 아이디 삽입
                        info.lstData.Insert(0, nSoundObjectID.ToString());

                        result.Add(key, info);
                    }
                }
            }

            return result;
        }

        static private int _FindGroupInAction(Dictionary<int, ScriptInfo> dicGroup, int actionIdx)
        {
            // 그룹이라는 단어가 나오면 그 인덱스들을 기억한 다음 엔드그룹이라는 인덱스도 기억 - 딕셔너리 처리
            // 이벤트단어를 찾았을때 그룹인덱스와 엔드그룹인덱스안에 있다면 해당그룹임.
            foreach (KeyValuePair<int, ScriptInfo> dicObj in dicGroup)
            {
                //Console.WriteLine("Key:{0} First:{1}-Last:{2}", dicObj.Key, dicObj.Value[0], dicObj.Value[1]);
                int nF = int.Parse(dicObj.Value.lstData[0]);
                int nL = int.Parse(dicObj.Value.lstData[1]);

                if (actionIdx > nF && actionIdx < nF + nL)
                {
                    return dicObj.Key;
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
                foreach (var gObj in lstGroupObjects[i])
                {
                    gObj.Click(mousex, mousey);
                }
            }
        }

        static public void PlayProcess(long now)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                if (lstGroupObjects.Count > 0)
                {
                    foreach (var gObj in lstGroupObjects[i])
                    {
                        gObj.Play(now);
                    }
                }

                if (lstEventObjects.Count > 0)
                {
                    foreach (var eObj in lstEventObjects[i])
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

        // 인자가 유니크아이디가 아니라 그룹아이디임 주의요망...
        static public GroupObject FindGroupObjectByGroupIndex(int GroupID)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = lstGroupObjects[i].FindIndex(r => r.GroupID == GroupID);
                if (index > -1)
                    return lstGroupObjects[i][index];
            }

            return null;
        }

        static public EventObject FindEventObjectByUniqueID(int uniqueID)
        {
            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = lstEventObjects[i].FindIndex(r => r.nUniqueID == uniqueID);
                if (index > -1)
                    return lstEventObjects[i][index];
            }

            return null;
        }

        static public List<EventObject> FindEventObjectByType(TOTALEVENTTYPE type)
        {
            List<EventObject> result = new List<EventObject>();

            for (int i = 0; i < nMaxLayerCount; i++)
            {
                int index = lstEventObjects[i].FindIndex(r => r.eTotalEventType == type);
                if (index > -1)
                    result.Add(lstEventObjects[i][index]);
            }


            return result;
        }
    }
}
