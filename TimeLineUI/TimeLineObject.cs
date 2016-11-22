﻿using System;
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
        public string Name { get; set; }
        public bool Lock { get; set; }
        public bool View { get; set; }
        
        public ANITYPE aniType { get; set; }
        
        private int m_tickWidth { get; set; }
        private int nameGap = 5; // 끝점에서 이름까지 갭

        public TimeBodyObject bodyObj = null;

        // 총 시작이벤트(틱)별로 모음이 있고 틱당 여러개의 이벤트가 모여있음. 리스트 + 맵
        // EventObjectGrp 이 여기 포함되어 있어야 함.
        public EventObjectMng eventMng = new EventObjectMng();


        public void AddEvent(int tickIdx, string objName, Point tickPos)
        {
            // objPos를 다시 계산해야 함
            
            // 메인 좌표(objPos) + (eventgap * 이벤트갯수)

            //EventObject obj = new EventObject(objName, tickPos, Brushes.Green, tickIdx, OBJTYPE.EVENT, bodyObj);
            EventObject obj = new EventObject(tickIdx, objName, tickPos, bodyObj);
            eventMng.Add_EventObject(tickIdx, tickPos, obj);

            if(bodyObj.GetEventObjectMng() == null)
                bodyObj.SetEventMng(eventMng, m_tickWidth);
        }

        public TimeLineObject(Point point1, int tickWidth, string name)
        {
            m_tickWidth = tickWidth;
            aniType = ANITYPE.STOP_AT_END;

            bodyObj = new TimeBodyObject(point1, tickWidth);
            bodyObj.name = name;
            bodyObj.SetTimeLineObject(this);
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
            DrawName(g, bodyObj.GetEndObject(OBJTYPE.END));
            eventMng.DrawEvents(g);
        }
    }
}
