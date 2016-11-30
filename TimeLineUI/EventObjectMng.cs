using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    public enum EVENTDRAWSTATE
    {
        NONE,
        EMPTY,          // 이벤트가 하나도 없을때
        MINOVER,        // 숫자도 표시할수 없을정도로 작을때 - 이벤트 마크 하나만 표시
        WIDTHOVER,      // 이벤트 전체 갯수 폭이 틱 폭을 넘어갈때 - 이벤트 마크와 숫자로 가려진 갯수 표시
        OK              // 다 출력할수 있을때
    }

    // 순서는 스크립트 생성 버튼 역할.xlsx대로
    public enum EVENTTYPE
    {
        NONE,
        MOVE_LINEAR,            // 직선이동
        MOVE_DECELERATE,        // 감속이동
        ROTATE,                 // 회전
        ZOOM,                   // 줌 인/ 아웃
        FADE_IN,                // 페이드 인
        FADE_OUT,               // 페이드 아웃
        GROUP,                  // 그룹설정
        CRASH_CREATE,           // 충돌생성
        CRASH_EFFECT,           // 충돌효과
        TARGET,                 // 타겟설정
        MOVE_POSITION,          // 좌표설정
        PLAY,                   // 플레이이벤트
        CLEAR,                  // 클리어
        SOUND,                  // 사운드 - 예외처리(가상의 사운드 오브젝트생성후 추가되어야 함)
        MOTION,                 // 모션변경
        SCROLL,                 // 스크롤
        SETPLAY,                // 플레이
        SHOW,                   // 보임/숨김
        ROLL,                   // 반복배경
        BACKUP,                 // 상태백업
        MOTIONINC,              // 모션증가
        MOTIONDEC,              // 모션감소
        PLAYGROUP,              // 그룹플레이
        END_GROUP,              // 그룹생성종료
    }
    
    public class EventObjectMng
    {
        public List<string> lstEventType = new List<string> {
            "NONE",
            "MOVE LINEAR",            // 직선이동
            "MOVE DECELERATE",        // 감속이동
            "ROTATE",                 // 회전
            "ZOOM",                   // 줌 인/ 아웃
            "FADE IN",                // 페이드 인
            "FADE OUT",               // 페이드 아웃
            "GROUP",                  // 그룹설정
            "CRASH CREATE",           // 충돌생성
            "CRASH EFFECT",           // 충돌효과
            "TARGET",                 // 타겟설정
            "MOVE POSITION",          // 좌표설정
            "PLAY",                   // 플레이이벤트
            "CLEAR",                  // 클리어
            "SOUND",                  // 사운드 - 예외처리(가상의 사운드 오브젝트생성후 추가되어야 함)
            "MOTION",                 // 모션변경
            "SCROLL",                 // 스크롤
            "SETPLAY",                // 플레이
            "SHOW",                   // 보임/숨김
            "ROLL",                   // 반복배경
            "BACKUP",                 // 상태백업
            "MOTIONINC",              // 모션증가
            "MOTIONDEC",              // 모션감소
            "PLAYGROUP",              // 그룹플레이
            "END GROUP",              // 그룹생성종료
        };

        public int Group { get; set; }      // 그룹 인덱스

        static public int nEventGap = 5;

        private int nTickWidth = 0;

        // 이 키값은 절대값이 아니라 Bodyd에서 얼마나 떨어졌는지 계산된 오프셋 값임.
        private SortedDictionary<int, List<EventObject>> dicEvents = new SortedDictionary<int, List<EventObject>>();

        public const int nSoundObjectID = 1000;
        public const int nCameraObjectID = 1010;

        public Dictionary<int, List<String>> EventCoreParser(string[] workString, int LayerCount, string firstWord, string findLastWord)
        {
            string strFirstWord = "[";
            strFirstWord += firstWord;
            strFirstWord += "=";


            // 문자열안에 여러개의 그룹이 올수 있으므로
            Dictionary<int, List<String>> result = new Dictionary<int, List<String>>();

            for (int j = 0; j < LayerCount; j++)
            {
                string text = workString[j];

                // 찾을 문자열이 여러개 섞여있을때 인덱스리스트 얻기
                List<int> lstfindFirstIndex = GetPositions(workString[j], strFirstWord);
                int key = -1;
                foreach (int idx in lstfindFirstIndex)
                {
                    string str = text.Substring(idx);

                    int findLastIndex = str.IndexOf(findLastWord);

                    int startindex = strFirstWord.Length;

                    int findLength = findLastIndex - strFirstWord.Length;
                    string subValues = str.Substring(startindex, findLength);
                    string[] values = subValues.Split(',');

                    // 그룹-엔드그룹 처리때만 쓰임 그룹의 인덱스를 얻기위해
                    int findGroupindex = str.IndexOf("]");                              
                    int GroupLength = findGroupindex - strFirstWord.Length;            
                    string subGroupValues = str.Substring(startindex, GroupLength);     
                    string[] groupvalues = subGroupValues.Split(',');                   


                    // Console.WriteLine("액션 {0}, 유니크 {1}, 레이어 {2}, 딜레이 {3}, 인덱스 {4}", findFirstWord, values[0], j, values[values.Length - 1], idx);


                    int intParseResult = 0;
                    if(int.TryParse(values[0], out intParseResult))
                    {
                        if(firstWord == "PLAYGROUP")
                            key = int.Parse(groupvalues[0]);
                        else
                            key = idx;

                        List<string> findValues = values.ToList();
                        result.Add(key, findValues);
                        
                    }
                    else // 예외나 에러처리...
                    {
                        if (findLastWord == "[END GROUP]") 
                        {
                            // GROUP 인덱스, END GROUP 인덱스
                            key = int.Parse(groupvalues[0]);

                            List<string> findValues = new List<string>();
                            findValues.Add(idx.ToString());
                            findValues.Add(findLastIndex.ToString());

                            result.Add(key, findValues);
                        }
                        else if(firstWord == "SOUND")
                        {
                            key = idx;
                            List<string> findValues = values.ToList();
                            result.Add(key, findValues);
                        }
                    }
                }
            }

            return result;
        }

        private int FindGroupInAction(Dictionary<int, List<string>> dicGroup, int actionIdx)
        {
            // 그룹이라는 단어가 나오면 그 인덱스들을 기억한 다음 엔드그룹이라는 인덱스도 기억 - 딕셔너리 처리
            // 이벤트단어를 찾았을때 그룹인덱스와 엔드그룹인덱스안에 있다면 해당그룹임.
            foreach (KeyValuePair<int, List<string>> dicObj in dicGroup)
            {
                //Console.WriteLine("Key:{0} First:{1}-Last:{2}", dicObj.Key, dicObj.Value[0], dicObj.Value[1]);
                int nF = int.Parse(dicObj.Value[0]);
                int nL = int.Parse(dicObj.Value[1]);

                if(actionIdx > nF && actionIdx < nL)
                {
                    return dicObj.Key;
                }
            }

            return -1;
        }
        
        public List<EventObject> ScriptParser(string[] script_text, int LayerCount)
        {
            List<EventObject> result = new List<EventObject>();

            // [GROUP] ~ [END GROUP] 묶음찾기
            // key : [GROUP= 이 있는 위치
            // value : 0([GROUP= 위치), 1([END GROUP]위치)
            string strFirstWord1 = lstEventType[(int)EVENTTYPE.GROUP];
            string strLastWor1 = "[END GROUP]";
            Dictionary<int, List<string>> dicGgroupInActions = EventCoreParser(script_text, LayerCount, strFirstWord1, strLastWor1);
            foreach (var obj in dicGgroupInActions)
            {
                foreach(var sub in obj.Value)
                    Console.WriteLine("그룹-엔드그룹 {0}, {1}", obj.Key, sub);
            }

            // [PLAYGROUP] 묶음찾기
            // key : [PLYAGROUP= 가 있는 위치
            // value : 0(그룹인덱스), 1(딜레이값)
            string strTemp1 = lstEventType[(int)EVENTTYPE.PLAYGROUP];
            string strLastWord2 = "]";
            Dictionary<int, List<string>> dicPlayGroup = EventCoreParser(script_text, LayerCount, strTemp1, strLastWord2);
            foreach (var obj in dicPlayGroup)
            {
                foreach (var sub in obj.Value)
                    Console.WriteLine("PLAYGROUP {0}, {1}", obj.Key, sub);
            }

            // 모든 이벤트 문자열 묶음 찾기
            Dictionary<int, List<string>> dicActionGroup;
            for (int i = 0; i < lstEventType.Count; i++)
            {
                // 제외할 액션들
                if(i == (int)EVENTTYPE.GROUP || 
                   i == (int) EVENTTYPE.PLAYGROUP || 
                   i == (int)EVENTTYPE.END_GROUP)
                {
                    continue;
                }

                // 찾을 액션문자열
                string strAction = lstEventType[i];
                string strActionLast = "]";

                // dicActionGroup 의 키값이 해당단어의 검색된 위치인덱스임.
                dicActionGroup = EventCoreParser(script_text, LayerCount, strAction, strActionLast);

                // 여기서 부터는 이벤트별 처리
                if (dicActionGroup.Count > 0)
                {
                    foreach(var eventObj in dicActionGroup)
                    {
                        int nGroupIdx = FindGroupInAction(dicGgroupInActions, eventObj.Key);
                        Console.WriteLine("액션{0}의 그룹인덱스 {1}", lstEventType[i], nGroupIdx);
                        int nGroupDelay = 0;
                        if (nGroupIdx > -1)
                        {
                            Console.WriteLine("     딜레이값 {0}", dicPlayGroup[nGroupIdx][1]);
                            nGroupDelay = int.Parse(dicPlayGroup[nGroupIdx][1]);
                        }

                        int uniqueID = 0;
                        if (int.TryParse(eventObj.Value[0], out uniqueID))
                        {
                            EventObject temp = new EventObject(lstEventType[i], uniqueID, 0, eventObj.Value.ToArray());
                            temp.tickIdx = int.Parse(eventObj.Value[eventObj.Value.Count - 1]);
                            result.Add(temp);
                        }
                        else
                        {
                            // 예외처리...
                            if (i == (int)EVENTTYPE.SOUND) // 사운드는 그룹으로만 작동
                            {
                                EventObject temp = new EventObject(lstEventType[i], nSoundObjectID, 0, eventObj.Value.ToArray(), nGroupIdx, nGroupDelay);
                                temp.tickIdx = nGroupDelay; // 플레이그룹의 딜레이로 세팅
                                result.Add(temp);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("---------------------------------------------");


            return result;
        }


        public List<int> GetPositions(string source, string searchString)
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

        public void SetTickWidth(int rWidth)
        {
            nTickWidth = rWidth;
        }

        public SortedDictionary<int, List<EventObject>> GetDic()
        {
            return dicEvents;
        }

        public int GetCount()
        {
            return dicEvents.Count;
        }

        public void TotalPrint()
        {
            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                Console.WriteLine("키{0}", obj.Key);
                foreach (EventObject lstObj in obj.Value)
                {
                    Console.WriteLine("     Name:{0}, Index:{1}, tick:{2}, Pos:{3}", lstObj.name, lstObj.index, lstObj.tickIdx, lstObj.pos);
                }
            }
        }

        public void Add_EventObject(EventObject val)
        {
            int key = val.offsetTick;
            if (dicEvents.ContainsKey(key))
            {
                int x = val.pos.X + (nEventGap * (dicEvents[key]).Count);
                int y = val.pos.Y;
                val.pos = new Point(x, y);
                val.index = (dicEvents[key]).Count;
                               
                (dicEvents[key]).Add(val);
                //Console.WriteLine("키:{0}, 이벤트 갯수:{1}", key, (dicEvents[key]).Count);
            }
            else
            {
                List<EventObject> temp = new List<EventObject>();
                
                val.index = 0;
                temp.Add(val);
                dicEvents.Add(key, temp);
            }
        }

        public List<EventObject> Get_EventObjects(int key)
        {
            if (dicEvents.ContainsKey(key))
            {
                return dicEvents[key];
            }

            return null;
        }

        public EventObject Get_EventObject(int key, int index)
        {
            if (dicEvents.ContainsKey(key))
            {
                return (dicEvents[key])[index];
            }

            return null;
        }

        public void DrawEvents(Graphics g)
        {
            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                EVENTDRAWSTATE resultState = CheckDrawEvent(obj.Value.Count, EventObject.BoxWidth);  

                if (resultState == EVENTDRAWSTATE.OK) // 틱 당 이벤트 다 그리기
                {
                    foreach (EventObject lstObj in obj.Value)
                    {
                        lstObj.DrawMark(g);
                    }
                }
                else if(resultState == EVENTDRAWSTATE.WIDTHOVER) // 틱당 이벤트 하나만 그리기 + 갯수
                {
                    // 틱당 이벤트 하나만 그리기
                    obj.Value[0].DrawMark(g);
                    DrawCount(g, obj.Value.Count, obj.Value[0].pos);
                }
                else if(resultState == EVENTDRAWSTATE.MINOVER) // 틱당 이벤트 하나만 그리기
                {
                    obj.Value[0].DrawMark(g);
                }
                
            }
        }

        public void DrawCount(Graphics g, int count, Point pos)
        {
            g.DrawString(String.Format("{0}",count), new Font("Arial", 8), Brushes.Red, new Point(pos.X+2, pos.Y));
        }


        private EVENTDRAWSTATE CheckDrawEvent(int count, int eventWidth)
        {
            if (count == 0)
                return EVENTDRAWSTATE.EMPTY;

            int totalWidth = (nEventGap + eventWidth) * count;
            int singleWidth = eventWidth + 8; // 8 : 대략적인 폰트 폭

            if (singleWidth >= nTickWidth)
                return EVENTDRAWSTATE.MINOVER;
            else if (totalWidth >= nTickWidth)
                return EVENTDRAWSTATE.WIDTHOVER;

            return EVENTDRAWSTATE.OK;
        }

        
        public void MoveProcess(int startTickKey, int rTickWidth, int rStartGap, int yPos)
        {
            //Console.WriteLine("startTickKey {0}", startTickKey);
            
            // 각 이벤트 오브젝트의 틱값도 바뀌어야 함.
            foreach (var obj in dicEvents)
            {
                foreach (var lstObj in obj.Value)
                {
                    int newTick = startTickKey + lstObj.offsetTick;
                    lstObj.tickIdx = newTick;
                }
            }

            // 각 이벤트 틱의 Pos만 바뀌면 됨
            ReCalcPosition(rTickWidth, rStartGap, yPos);
        }

        public void Remove_EventObject(int key, int index)
        {
            if (dicEvents.ContainsKey(key))
            {
                (dicEvents[key]).RemoveAt(index);
            }

            if (dicEvents[key].Count == 0)
                dicEvents.Remove(key);
        }

        // 오브젝트 위치가 변경되거나 삭제시 인덱스 재계산
        public void ReCalcIndex(int key)
        {
            if (!dicEvents.ContainsKey(key))
            {
                //Console.WriteLine("그런 키 없음. {0}", key);
                return;
            }
            else
            {
                int i = 0;
                foreach (EventObject lstObj in dicEvents[key])
                {
                    lstObj.index = i;
                    i++;
                }
            }
        }

        // 오브젝트 위치가 변경되거나 삭제시 위치값 재계산
        public void ReCalcPosition(int rTickWidth, int rStartGap, int yPos)
        {
            nTickWidth = rTickWidth;

            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                int i = 0;
                foreach (EventObject lstObj in obj.Value)
                {
                    Point convPos = TimeLineUtil.ConvTickIdxToPoint(rTickWidth, rStartGap, lstObj.tickIdx, yPos);
                    int x = convPos.X + (i * nEventGap);
                    int y = convPos.Y;

                    lstObj.pos = new Point(x, y);

                    i++;
                }
            }
        }

        public SelectObject CheckPos(Point p)
        {
            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                foreach (EventObject lstObj in obj.Value)
                {
                    SelectObject eObj = lstObj.CheckBoxPos(p);
                    if (eObj != null) return eObj;
                }
            }

            return null;
        }
    }
}
