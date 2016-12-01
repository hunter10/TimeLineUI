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

        public TimeObject GetStartObj()
        {
            return GetChildObject(OBJTYPE.START);
        }

        public TimeObject GetEndObj()
        {
            return GetChildObject(OBJTYPE.END);
        }

        public TimeObject GetChildObject(OBJTYPE type)
        {
            if (type == OBJTYPE.START)
                return sObj;
            else if (type == OBJTYPE.END)
                return eObj;

            return null;
        }

        public TimeBodyObject(Point startP, int tickWidth, int startTick = 0, int endTick = 10)
        {
            brush = Brushes.Black;
            ObjType = OBJTYPE.BODY;

            Point endP = new Point(startP.X + (tickWidth * endTick), startP.Y);
            sObj = new TimeObject("", startP, startTick, OBJTYPE.START, this);
            eObj = new TimeObject("", endP, endTick, OBJTYPE.END, this);

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
            SelectObject obj;

            // 이벤트와 각 엔드포인트가 겹쳤을때 이벤트부터 처리되기위해서 젤 먼저 처리
            // 이벤트 체크
            if (eventMng != null)
            {
                obj = eventMng.CheckPos(p);
                if (obj != null)
                    return obj;
            }

            // 스타트포인트 체크
            obj = sObj.CheckBoxPos(p);
            if (obj != null)
                return obj;

            // 엔드포인드 사각박스체크
            obj = eObj.CheckBoxPos(p);
            if (obj != null)
                return obj;

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
