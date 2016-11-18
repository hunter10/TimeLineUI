using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    class TimeObject : SelectObject
    {
        public int BoxWidth;// = 6;
        public int BoxHeight;// = 12;

        private SelectObject mParent;

        private SelectObject mLimitObj;

        //public TimeObject()
        //{
        //    brush = Brushes.Black;
        //    hoverbrush = Brushes.Yellow;
        //}

        public TimeObject(string n, Point p, Brush b, int i, OBJTYPE t, SelectObject parent)
        {
            name = n;
            pos = p;
            brush = b;
            tickIdx = i;
            ObjType = t;

            HoverType = OBJTYPE.NONE;

            BoxWidth = 6;
            BoxHeight = 16;

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
                               hoverbrush);
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

        // 오른쪽 아이콘이 왼쪽 아이콘/ 또는 그 반대의 경우로 넘어가는지 체크
        public override bool CheckObjectLimit(Point p)
        {
            /*
            int iconWidth = EndIconWidth;
            int iconHeight = EndIconHeight;


            int nERight = 0;
            int nSLeft = 0;
            OBJTYPE type = ObjType;

            if (ObjType == OBJTYPE.END)
            {
                nSLeft = SPos.X;
                if (p.X <= nSLeft) return false;
            }
            else if (ObjType == OBJTYPE.START)
            {
                nERight = EPos.X + iconWidth;
                if (p.X >= nERight) return false; // 시간 기준이 각 아이콘의 우측기준인데 끝점은 오른쪽면이 기준임.
            }
            //Console.WriteLine("nSRight:{0}, nSLeft:{1} / nERight:{2}, nELeft:{3}", nSRight, nSLeft, nERight, nELeft);
            */


            return true;
        }
    }
}
