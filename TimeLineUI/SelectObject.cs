using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    public abstract class SelectObject
    {
        public delegate void ObjectEventHandler(object sender, EventArgs e);
        public event ObjectEventHandler MouseEnterNotice;
        public event ObjectEventHandler MouseLeaveNotice;

        public string name { get; set; }
        public int tickIdx { get; set; }
        public Point pos { get; set; }
        public Brush brush { get; set; }
        public Brush hoverbrush { get; set; }

        public OBJTYPE HoverType { get; set; }
        public OBJTYPE ObjType { get; set; }

        public virtual SelectObject GetParent() { return null; }
        public virtual void DrawMark(Graphics g)
        {
            if (HoverType == ObjType)
                _DrawHoverMark(g, pos, new Size(), hoverbrush, 2);

            _DrawMark(g, pos, new Size(), brush);
        }

        protected virtual void _DrawHoverMark(Graphics g, Point p, Size s, Brush brush, int offset)
        {
            Rectangle rect = new Rectangle(p.X,
                                           p.Y,
                                           s.Width,
                                           s.Height);

            g.FillRectangle(brush, Rectangle.Inflate(rect, offset, offset));
        }

        protected virtual void _DrawMark(Graphics g, Point p, Size s, Brush brush)
        {
            Rectangle rect = new Rectangle(p.X,
                                           p.Y,
                                           s.Width,
                                           s.Height);

            g.FillRectangle(brush, rect);
        }

        public virtual SelectObject CheckBoxPos(Point p) { return this; }
        public virtual bool CheckObjectLimit(Point p) { return true; }
        public virtual bool CheckPanelBound(Point p, int min, int max, Panel timePan, Panel rulerPan) { return true; }
    }
}
