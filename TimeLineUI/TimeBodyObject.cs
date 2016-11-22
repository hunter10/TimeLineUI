using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    class TimeBodyObject : SelectObject
    {
        private TimeObject sObj;
        private TimeObject eObj;

        private TimeLineObject mParent;

        private EventObjectMng eventMng;

        int BoxHeight = 15;

        public EventObjectMng GetEventObjectMng()
        {
            return eventMng;
        }

        public void SetTimeLineObject(TimeLineObject obj)
        {
            mParent = obj;
        }

        public TimeLineObject GetTimeLineObject()
        {
            return mParent;
        }

        public TimeObject GetEndObject(OBJTYPE type)
        {
            if (type == OBJTYPE.START)
                return sObj;
            else if (type == OBJTYPE.END)
                return eObj;

            return null;
        }

        public TimeBodyObject(Point p, int tickWidth)
        {
            brush = Brushes.Black;
            ObjType = OBJTYPE.BODY;

            Point endP = new Point(p.X + (tickWidth * 10), p.Y);
            sObj = new TimeObject("", p, Brushes.Brown, 0, OBJTYPE.START, this);
            eObj = new TimeObject("", endP, Brushes.Brown, 10, OBJTYPE.END, this);

            pos = new Point(sObj.pos.X, sObj.pos.Y);
        }

        public TimeBodyObject(string n, Point p, Brush b, int i, OBJTYPE t)
        {
            name = n;
            pos = p;
            brush = b;
            tickIdx = i; // 보디 오브젝트에서는 시작 인덱스
            ObjType = t;

            HoverType = OBJTYPE.NONE;
        }

        public override void DrawMark(Graphics g)
        {
            //Console.WriteLine("{0}, {1}", eObj.pos, sObj.pos);

            _DrawMark(g,
                      sObj.pos,
                      new Size(eObj.pos.X - sObj.pos.X, BoxHeight),
                      brush);

            sObj.DrawMark(g);
            eObj.DrawMark(g);
        }

        protected override void _DrawMark(Graphics g, Point p, Size s, Brush brush)
        {
            Pen pen = Pens.Black;
            Rectangle rect = new Rectangle(p.X,
                                           p.Y,
                                           s.Width,
                                           s.Height);

            g.DrawRectangle(pen, rect);
        }

        public void SetEventMng(EventObjectMng mng, int rTickWidth)
        {
            eventMng = mng;
            eventMng.SetTickWidth(rTickWidth);
        }

        public override SelectObject CheckBoxPos(Point p)
        {
            SelectObject obj = sObj.CheckBoxPos(p);
            if (obj != null)
                return obj;

            obj = eObj.CheckBoxPos(p);
            if (obj != null)
                return obj;

            if (eventMng != null)
            {
                obj = eventMng.CheckPos(p);
                if (obj != null)
                    return obj;
            }

            bool result = false;
            // 몸체는?
            if ((p.X > (sObj.pos.X)) &&
                (p.X < eObj.pos.X) &&
                (p.Y > sObj.pos.Y) &&
                (p.Y < sObj.pos.Y + BoxHeight))
            {
                ObjType = OBJTYPE.BODY;
                return this;
            }

            if (result)
            {
                //if (MouseEnterNotice != null)
                //    MouseEnterNotice(this, EventArgs.Empty);
                return this;
            }
            else
            {
                HoverType = OBJTYPE.NONE;
                //if (MouseLeaveNotice != null)
                //    MouseLeaveNotice(this, EventArgs.Empty);
                return null;
            }
        }

        public override bool CheckPanelBound(Point p, int min, int max, Panel timePan, Panel rulerPan)
        {
            bool sResult = sObj.CheckPanelBound(p, min, max, timePan, rulerPan);
            bool eResult = eObj.CheckPanelBound(p, min, max, timePan, rulerPan);

            if (!sResult || !eResult)
                return false;
            return true;
        }
    }
}
