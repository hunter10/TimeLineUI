using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    class TimeObject : SelectObject
    {
        static public int BoxWidth = 6;
        static public int BoxHeight = 16;

        private SelectObject mParent;

        private SelectObject mLimitObj;

        public TimeObject(string n, Point p, int i, OBJTYPE t, SelectObject parent)
        {
            name = n;
            pos = p;
            brush = Brushes.Brown;
            tickIdx = i;
            ObjType = t;

            HoverType = OBJTYPE.NONE;

            hoverbrush = Brushes.LimeGreen;

            mParent = parent;
        }

        public void SetLimitObject(SelectObject obj)
        {
            mLimitObj = obj;
        }

        public override SelectObject GetParent()
        {
            return mParent;
        }

        public override void DrawMark(Graphics g)
        {
            if (HoverType == ObjType)
            {
                _DrawHoverMark(g,
                               new Point(pos.X - 3, pos.Y - 2),
                               new Size(BoxWidth, BoxHeight + 4),
                               hoverbrush,
                               2);
            }

            _DrawMark(g,
                      new Point(pos.X - 3, pos.Y - 2),
                      new Size(BoxWidth, BoxHeight + 4),
                      brush);
        }
        
        public override SelectObject CheckBoxPos(Point p)
        {
            int iconWidth = BoxWidth;
            int iconHeight = BoxHeight;

            int iconWidthHalf = iconWidth / 2;

            //Console.WriteLine("CheckBoxPos p:{0}, SPos:{1}, iconWidth:{2}, iconHeight:{3}", p, SPos, iconWidth, iconHeight);

            bool result = false;

            if ((p.X > pos.X - iconWidthHalf) &&
               (p.X < pos.X + iconWidthHalf) &&
               (p.Y > pos.Y) &&
               (p.Y < pos.Y + iconHeight))
            {
                HoverType = ObjType;
                result = true;
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
            // 가장자리 스크롤되기 위한 조건 
            int nPanelRight = timePan.Width + timePan.HorizontalScroll.Value;
            int nPanelLeft = timePan.HorizontalScroll.Value;

            // 오른쪽 면 처리
            {
                // 맨 오른쪽 오버조건
                if (p.X > max) return false;

                // 오브젝트가 오른쪽 가장자리에 닿았을때
                if (p.X > nPanelRight)
                {
                    int x = (p.X - timePan.Width);
                    if (x >= timePan.HorizontalScroll.Maximum)
                        x = timePan.HorizontalScroll.Maximum;

                    timePan.HorizontalScroll.Value = x;
                    rulerPan.AutoScrollPosition = new Point(rulerPan.HorizontalScroll.Value, rulerPan.VerticalScroll.Value);
                }
            }
            
            // 왼쪽면 처리
            {
                // 맨 왼쪽 오버조건
                if (p.X <= min) return false;

                // 오브젝트가 왼쪽 가장자리에 닿았을때
                if (p.X < nPanelLeft)
                {
                    int x = p.X;
                    if (x <= timePan.HorizontalScroll.Minimum)
                        x = timePan.HorizontalScroll.Minimum;

                    timePan.HorizontalScroll.Value = x;
                    rulerPan.AutoScrollPosition = new Point(rulerPan.HorizontalScroll.Value, rulerPan.VerticalScroll.Value);
                }
            }

            return true;
        }

        // 오른쪽 아이콘이 왼쪽 아이콘/ 또는 그 반대의 경우로 넘어가는지 체크
        public override bool CheckObjectLimit(Point p)
        {
            TimeBodyObject body = (mParent as TimeBodyObject);

            if (ObjType == OBJTYPE.END)
            {
                int nSLeft = body.GetEndObject(OBJTYPE.START).pos.X;
                if (p.X <= nSLeft) return false;
            }
            else if (ObjType == OBJTYPE.START)
            {
                int nERight = body.GetEndObject(OBJTYPE.END).pos.X;
                if (p.X >= nERight) return false; 
            }

            return true;
        }
    }
}
