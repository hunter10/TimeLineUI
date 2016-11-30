using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    public enum OBJTYPE
    {
        NONE,
        TOTALLINE,
        START,
        END,
        BODY,
        EVENT
    }

    public enum ANITYPE
    {
        NONE,
        STOP_AT_END,
        REPLAY_AT_END
    }

    class TimeLineObject
    {
        public int uniqueID { get; set; }           // AppBusObject의 uniqueID와 같음 
        public int layerdepth_index { get; set; }   // AppBusObject의 layerdepth_index와 같음 

        public int Group { get; set; }      // 그룹 인덱스
        public string Name { get; set; }
        public bool Lock { get; set; }
        public bool View { get; set; }
        
        public ANITYPE aniType { get; set; }
        
        private int m_tickWidth { get; set; }
        private int nameGap = 5; // 끝점에서 이름까지 갭

        public TimeBodyObject bodyObj = null;
        public EventObjectMng eventMng = new EventObjectMng();


        public void AddEvent(int tick, int tickOffset, string objName, Point tickPos)
        {
            EventObject obj = new EventObject(tick, tickOffset, objName, tickPos, bodyObj);
            eventMng.Add_EventObject(obj);
        }

        public TimeLineObject(Point startPoint, int tickWidth, string name, int r_uniqueID, int r_layerdepthIndex, int startTick=0, int endTick=10)
        {
            uniqueID = r_uniqueID;
            layerdepth_index = r_layerdepthIndex;

            m_tickWidth = tickWidth;
            aniType = ANITYPE.STOP_AT_END;

            bodyObj = new TimeBodyObject(startPoint, tickWidth, startTick, endTick);
            bodyObj.name = name;
            bodyObj.SetTimeLineObject(this);

            if (bodyObj.GetEventObjectMng() == null)
                bodyObj.SetEventMng(eventMng, m_tickWidth);
        }

        public void DrawName(Graphics g, SelectObject endObj)
        {
            g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(endObj.pos.X + nameGap, endObj.pos.Y - 1));
        }

        public SelectObject CheckPos(Point p)
        {
            return bodyObj.CheckBoxPos(p);
        }

        public void DrawMarks(Graphics g)
        {
            bodyObj.DrawMark(g);
            DrawName(g, bodyObj.GetEndObj());
            eventMng.DrawEvents(g);
        }
    }
}
