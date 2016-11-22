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

        private SortedDictionary<int, List<EventObject>> dicEvents = new SortedDictionary<int, List<EventObject>>();

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



        public void Add_EventObject(int key, Point tickPos, EventObject val)
        {
            if (dicEvents.ContainsKey(key))
            {
                int x = tickPos.X + (nEventGap * (dicEvents[key]).Count);
                int y = tickPos.Y;
                val.pos = new Point(x, y);
                val.index = (dicEvents[key]).Count;

                Console.WriteLine("0 리스트 갯수 {0}", (dicEvents[key]).Count);

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
                //foreach (EventObject lstObj in obj.Value)
                //{
                //    lstObj.DrawMark(g);
                //}



                
                EVENTDRAWSTATE resultState = CheckDrawEvent(obj.Value.Count, EventObject.BoxWidth);  

                if (resultState == EVENTDRAWSTATE.OK)
                {
                    foreach (EventObject lstObj in obj.Value)
                    {
                        lstObj.DrawMark(g);
                    }
                }
                else if(resultState == EVENTDRAWSTATE.WIDTHOVER)
                {
                    // 틱당 이벤트 하나만 그리기
                    obj.Value[0].DrawMark(g);
                    DrawCount(g, obj.Value.Count, obj.Value[0].pos);
                }
                else if(resultState == EVENTDRAWSTATE.MINOVER)
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
                Console.WriteLine("그런 키 없음. {0}", key);
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
                    Point convPos = Util.ConvTickIdxToPoint(rTickWidth, rStartGap, lstObj.tickIdx, yPos);
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
