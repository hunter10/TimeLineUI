﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    public class EventObject : SelectObject
    {
        public int BoxWidth;// = 6;
        public int BoxHeight;// = 12;

        private SelectObject mParent;

        //public EventObject()
        //{
        //    brush = Brushes.Green;
        //    hoverbrush = Brushes.Coral;

        //    HoverType = OBJTYPE.NONE;
        //    ObjType = OBJTYPE.EVENT;
        //}

        public EventObject(string n, Point p, Brush b, int i, OBJTYPE t, SelectObject parent)
        {
            name = n;
            pos = p;
            brush = b;
            tickIdx = i;
            ObjType = t;

            HoverType = OBJTYPE.NONE;

            BoxWidth = 4;
            BoxHeight = 12;

            hoverbrush = Brushes.Coral;

            mParent = parent;
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
                               new Point(pos.X, pos.Y),
                               new Size(BoxWidth, BoxHeight),
                               hoverbrush);
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
