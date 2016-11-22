using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    public class EventObject : SelectObject
    {
        static public int BoxWidth = 4;// = 6;
        static public int BoxHeight = 12;// = 12;

        private SelectObject mParent;

        public int index; // 같은 틱 안에서 구별되기 위한 식별값

        public EventObject(int tickIdx, string objName, Point tickPos, SelectObject parent)
        {
            name = objName;
            pos = tickPos;
            brush = Brushes.Green;
            this.tickIdx = tickIdx;
            ObjType = OBJTYPE.EVENT;

            HoverType = OBJTYPE.NONE;
            
            hoverbrush = Brushes.Coral;

            mParent = parent;
        }

        public EventObject(string n, Point p, Brush b, int i, OBJTYPE t, SelectObject parent)
        {
            name = n;
            pos = p;
            brush = b;
            tickIdx = i;
            ObjType = t;
            
            HoverType = OBJTYPE.NONE;

            hoverbrush = Brushes.Coral;

            mParent = parent;
        }

        public override SelectObject GetParent()
        {
            return mParent;
        }

        public void DrawName(Graphics g)
        {
            g.DrawString(name, new Font("Arial", 8), Brushes.Black, new Point(pos.X + 5, pos.Y - 7));
        }

        public override void DrawMark(Graphics g)
        {
            if (HoverType == ObjType)
            {
                _DrawHoverMark(g,
                               new Point(pos.X, pos.Y),
                               new Size(BoxWidth, BoxHeight),
                               hoverbrush,
                               1);

                DrawName(g);
            }

            _DrawMark(g, 
                      new Point(pos.X, pos.Y),
                      new Size(BoxWidth, BoxHeight), 
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
    }
}
