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
    
    public class EventObjectMng
    {
        static public int nEventGap = 5;

        private int nTickWidth = 0;

        // 이 키값은 절대값이 아니라 Bodyd에서 얼마나 떨어졌는지 계산된 오프셋 값임.
        private SortedDictionary<int, List<DrawEventObject>> dicEvents = new SortedDictionary<int, List<DrawEventObject>>();

        public void SetTickWidth(int rWidth)
        {
            nTickWidth = rWidth;
        }

        public SortedDictionary<int, List<DrawEventObject>> GetDic()
        {
            return dicEvents;
        }

        public int GetCount()
        {
            return dicEvents.Count;
        }

        public void TotalPrint()
        {
            foreach (KeyValuePair<int, List<DrawEventObject>> obj in dicEvents)
            {
                Console.WriteLine("키{0}", obj.Key);
                foreach (DrawEventObject lstObj in obj.Value)
                {
                    Console.WriteLine("     Name:{0}, Index:{1}, tick:{2}, Pos:{3}", lstObj.name, lstObj.index, lstObj.tickIdx, lstObj.pos);
                }
            }
        }

        public void Add_EventObject(DrawEventObject val)
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
                List<DrawEventObject> temp = new List<DrawEventObject>();
                
                val.index = 0;
                temp.Add(val);
                dicEvents.Add(key, temp);
            }
        }

        public List<DrawEventObject> Get_EventObjects(int key)
        {
            if (dicEvents.ContainsKey(key))
            {
                return dicEvents[key];
            }

            return null;
        }

        public DrawEventObject Get_EventObject(int key, int index)
        {
            if (dicEvents.ContainsKey(key))
            {
                return (dicEvents[key])[index];
            }

            return null;
        }

        public void DrawEvents(Graphics g)
        {
            foreach (KeyValuePair<int, List<DrawEventObject>> obj in dicEvents)
            {
                EVENTDRAWSTATE resultState = CheckDrawEvent(obj.Value.Count, DrawEventObject.BoxWidth);  

                if (resultState == EVENTDRAWSTATE.OK) // 틱 당 이벤트 다 그리기
                {
                    foreach (DrawEventObject lstObj in obj.Value)
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
                foreach (DrawEventObject lstObj in dicEvents[key])
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

            foreach (KeyValuePair<int, List<DrawEventObject>> obj in dicEvents)
            {
                int i = 0;
                foreach (DrawEventObject lstObj in obj.Value)
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
            foreach (KeyValuePair<int, List<DrawEventObject>> obj in dicEvents)
            {
                foreach (DrawEventObject lstObj in obj.Value)
                {
                    SelectObject eObj = lstObj.CheckBoxPos(p);
                    if (eObj != null) return eObj;
                }
            }

            return null;
        }
    }
}
