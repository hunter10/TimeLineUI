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
        MOTION,                 // 모션변경
        SETPLAY,                // 플레이
        SHOW,                   // 보임/숨김
        ROLL,                   // 반복배경
        BACKUP,                 // 상태백업
        MOTIONINC,              // 모션증가
        MOTIONDEC,              // 모션감소
        PLAYGROUP,              // 그룹플레이
        //END_GROUP,              // 그룹생성종료
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
            "MOTION",                 // 모션변경
            "SETPLAY",                // 플레이
            "SHOW",                   // 보임/숨김
            "ROLL",                   // 반복배경
            "BACKUP",                 // 상태백업
            "MOTIONINC",              // 모션증가
            "MOTIONDEC",              // 모션감소
            "PLAYGROUP",              // 그룹플레이
            //"END GROUP",              // 그룹생성종료
        };

        public int Group { get; set; }      // 그룹 인덱스

        static public int nEventGap = 5;

        private int nTickWidth = 0;

        // 이 키값은 절대값이 아니라 Bodyd에서 얼마나 떨어졌는지 계산된 오프셋 값임.
        private SortedDictionary<int, List<EventObject>> dicEvents = new SortedDictionary<int, List<EventObject>>();

        // 그룹안에는 이벤트, 오브젝트들이 묶여있을수 있다.
        // int 그룹인덱스
        // List 0 시작인덱스
        // List 1 끝인덱스
        public Dictionary<int, List<int>> GroupParser(string[] workString, int LayerCount)
        {
            // 문자열안에 여러개의 그룹이 올수 있으므로
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();

            // 찾을 액션문자열
            string strAction = "[";
            strAction += lstEventType[(int)EVENTTYPE.GROUP];
            strAction += "=";

            int key=-1;
            for (int j = 0; j < LayerCount; j++)
            {
                if (workString[j].Contains(strAction))
                {
                    string text = workString[j];

                    // 찾을 문자열이 여러개 섞여있을때 인덱스리스트 얻기
                    List<int> indexlist = GetPositions(workString[j], strAction);

                    foreach (int idx in indexlist)
                    {
                        string str = text.Substring(idx);

                        int findEndGroupIndex = str.IndexOf("[END GROUP]"); // [GROUP=0] ... [END GROUP] 사이의 값 찾기
                        int findGroupindex = str.IndexOf("]");              // [GROUP=0] 그룹의 인덱스 얻어오기

                        int startindex = strAction.Length;
                        int EndGroupLength = findEndGroupIndex - strAction.Length;
                        int GroupLength = findGroupindex - strAction.Length;

                        string GroupValues = str.Substring(startindex, GroupLength);

                        string[] values = GroupValues.Split(',');
                        key = int.Parse(values[0]);


                        List<int> findIndex = new List<int>();
                        findIndex.Add(idx);
                        findIndex.Add(findEndGroupIndex);

                        result.Add(key, findIndex);
                    }

                    
                }
            }

            return result;
        }






        
        public List<EventObject> ScriptParser(string[] script_text, int LayerCount)
        {
            List<EventObject> result = new List<EventObject>();








            // 같은 액션이라도 그룹에 묶여있는것이 있을수 있고
            // 아닌것이 있을수 있는데 이걸 어떻게 구분해내느냐가 관건
            Dictionary<int, List<int>> dicGgroupInActions = GroupParser(script_text, LayerCount);






            // 그룹이라는 단어가 나오면 그 인덱스들을 기억한 다음 엔드그룹이라는 인덱스도 기억 - 딕셔너리 처리
            // 이벤트단어를 찾았을때 그룹인덱스와 엔드그룹인덱스안에 있다면 해당그룹임.













            for (int i = 0; i < lstEventType.Count; i++)
            {
                // 찾을 액션문자열
                string strAction = "[";
                strAction += lstEventType[i];
                strAction += "=";

                for (int j = 0; j < LayerCount; j++)
                {
                    if (script_text[j].Contains(strAction))
                    {
                        string text = script_text[j];

                        // 찾을 문자열이 여러개 섞여있을때 인덱스리스트 얻기
                        List<int> indexlist = GetPositions(script_text[j], strAction);

                        foreach (int idx in indexlist)
                        {
                            string str = text.Substring(idx);

                            int findindex = -1;
                            findindex = str.IndexOf("]");

                            int startindex = strAction.Length;                          
                            int length = findindex - strAction.Length;

                            string tempValues = str.Substring(startindex, length);

                            string[] values = tempValues.Split(',');
                            Console.WriteLine("액션생성 {0}, 유니크 {1}, 레이어 {2}", lstEventType[i], values[0], j);

                            // 나머지 values 데이터들도 갖고 있어야 함.


                            // 액션이 하나라도 있다면 액션 매니저에 추가...
                            // 임시로 오브젝트 처리도 되어야 함.

                            // 틱인덱스           int tickIdx = 딜레이 타임을 틱인덱스로 변환해서 처리
                            // 오프셋
                            // 오브젝트 이름      string objName = lstEventType[i];
                            // 틱기반 위치값
                            // 부모               SelectObject parent = 유니크변호 기반으로 찾아야 함.


                            // 이벤트 이름, 유니크 번호, 나머지 속성값
                            EventObject temp = new EventObject(lstEventType[i], int.Parse(values[0]), j, values);
                            result.Add(temp);

                        }
                    }


                }
            }


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
