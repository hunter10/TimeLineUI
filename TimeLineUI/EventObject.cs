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

        public int offsetTick; // 시작틱에서부터 얼마 떨어져 있는지 오프셋값
        public int index; // 같은 틱 안에서 구별되기 위한 식별값 - 고유값 아님 이동/삭제시 변경됨 주의바람.

        public int uniqueID { get; set; }               // 이 이벤트가 속한 오브젝트 아이디
        public int layerdepth_index { get; set; }        

        public int GroupID { get; set; }                // 이 이벤트가 속한 오브젝트가 속할 그룹인덱스
        public int GroupDelay { get; set; }             // 이 이벤트가 속한 오브젝트가 속할 그룹인덱스의 딜레이

        public string[] eventData;                      // 현재 쓰지는 않고 담아두기만...

        // 이벤트 이름, 유니크 번호, 레이어인덱스, 나머지 속성값
        public EventObject(string eventName, int r_uniqueID, int r_layerdepth_index, string[] rData, int rGroupID=-1, int rGroupDelay=0)
        {
            uniqueID = r_uniqueID;
            layerdepth_index = r_layerdepth_index;

            GroupID = rGroupID;
            GroupDelay = rGroupDelay;

            name = eventName;
            pos = Point.Empty;
            brush = Brushes.Green;
            tickIdx = -1;
            offsetTick = -1;

            ObjType = OBJTYPE.EVENT;
            HoverType = OBJTYPE.NONE;
            hoverbrush = Brushes.Coral;

            mParent = null;

            eventData = rData;
        }

        public EventObject(int tickIdx, int startOffset, string eventName, Point tickPos, SelectObject parent)
        {
            uniqueID = 0;
            layerdepth_index = 0;

            GroupID = -1;
            GroupDelay = 0;

            name = eventName;
            pos = tickPos;
            brush = Brushes.Green;
            this.tickIdx = tickIdx;         // 스크립트와 연관
            offsetTick = startOffset;       // 실제 드로잉시 - 시작점에서 얼마 떨어져 있는지

            ObjType = OBJTYPE.EVENT;
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
