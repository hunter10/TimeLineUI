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

    public enum ENDTYPE
    {
        NONE,
        BOX,
        ICON
    }

    class TimeLineObject
    {
        public delegate void MyEventHandler(object sender, EventArgs e);
        public event MyEventHandler EnterNotice;
        public event MyEventHandler LeaveNotice;
        //public bool bHover = false;

        public String Name { get; set; }
        public bool Lock { get; set; }
        public bool View { get; set; }

        //public Point SPos { get; set; }
        //public Point EPos { get; set; }
        //public Image SIcon { get; set; }
        //public Image EIcon { get; set; }

        //public OBJTYPE HoverType { get; set; }
        //public OBJTYPE ObjType { get; set; }

        public ANITYPE aniType { get; set; }
        public ENDTYPE endType { get; set; }

        private int m_tickWidth { get; set; }
        private int nameGap = 5; // 끝점에서 이름까지 갭

        //public int STick { get; set; }                      // 시작 포인트 틱
        //public int ETick { get; set; }                      // 끝 포인트 틱

        //const int EndIconWidth = 6;
        //const int EndIconHeight = 16;





        //public List<SelectObject> lstTimeBodyObjects = new List<SelectObject>();
        public TimeBodyObject bodyObj = null;


        // 총 시작이벤트(틱)별로 모음이 있고 틱당 여러개의 이벤트가 모여있음. 리스트 + 맵
        // EventObjectGrp 이 여기 포함되어 있어야 함.
        public EventObjectMng eventMng = new EventObjectMng();


        public void AddEvent(int tickIdx, string objName, Point objPos)
        {
            //EventObjectGrp workG = eventMng.Get_Group(tickIdx);
            //if(workG == null)
            //{
            //    eventMng.Add_Group(new EventObjectGrp(tickIdx));
            //    workG = eventMng.Get_Group(tickIdx);

            //    bodyObj.SetEventMng(eventMng);
            //}






            EventObject obj = new EventObject("", objPos, Brushes.Green, tickIdx, OBJTYPE.EVENT, bodyObj);
            eventMng.Add_EventObject(tickIdx, obj);

            bodyObj.SetEventMng(eventMng);



            //workG.Add_Event(obj);

        }

        /*
        // 끝점 시작점은 아이콘 폭만큼 빼면됨.
        public int ConvEndPosStart(int PosX, int TickWidth)
        {
            if (endType == ENDTYPE.ICON)
                return PosX - EIcon.Width;
            else
                return PosX;
        }
        */

        public TimeLineObject(Point point1, int tickWidth, ENDTYPE eType, string name)
        {
            m_tickWidth = tickWidth;
            endType = eType;
            aniType = ANITYPE.STOP_AT_END;

            bodyObj = new TimeBodyObject(point1, tickWidth);
            bodyObj.name = name;
            bodyObj.SetTimeLineObject(this);
        }

        /*
        public TimeLineObject(Point point1, Point point2, int tickWidth, ENDTYPE eType)
        {
            m_tickWidth = tickWidth;
            endType = eType;
            aniType = ANITYPE.STOP_AT_END;


            if (endType == ENDTYPE.ICON)
            {
                SIcon = Image.FromFile(@"..\..\Resources\go-first-2.png");
                EIcon = Image.FromFile(@"..\..\Resources\go-last-2.png");
                EPos = new Point((point1.X + (m_tickWidth * 10)) - EIcon.Width, point2.Y);
            }
            else
            {
                SIcon = null;
                EIcon = null;
                EPos = new Point((point1.X + (m_tickWidth * 10)), point2.Y);
            }

            SPos = new Point(point1.X, point1.Y);
            STick = 0;
            ETick = 10;
        }
        */

        /*
        public void DrawIcon(Graphics g, Image image, Point point)
        {
            g.DrawImage(image, new Rectangle(point.X, point.Y, image.Width, image.Height));
        }
        */

        /*
        public void DrawEnd(Graphics g, OBJTYPE type)
        {
            if (type == OBJTYPE.START)
            {
                if(type == HoverType)
                    DrawHoverEndBox(g, SPos, Brushes.Yellow);

                if (endType == ENDTYPE.ICON)
                    DrawIcon(g, SIcon, SPos);
                else
                    DrawEndBox(g, SPos, Brushes.Brown);
            }
            else
            {
                if (type == HoverType)
                    DrawHoverEndBox(g, EPos, Brushes.Yellow);

                if (endType == ENDTYPE.ICON)
                    DrawIcon(g, EIcon, EPos);
                else
                    DrawEndBox(g, EPos, Brushes.Brown);
            }
        }
        */

        /*
        public void DrawName(Graphics g)
        {
            if(endType == ENDTYPE.ICON)
                g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(EPos.X + EIcon.Width + nameGap, EPos.Y - 1));
            else
                g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(EPos.X + nameGap, EPos.Y - 1));
        }
        */

        /*
        public TimeLineObject CheckBoxPos(Point p)
        {
            int iconWidth = EndIconWidth;
            int iconHeight = EndIconHeight;
           
            int iconWidthHalf = iconWidth / 2;

            //Console.WriteLine("CheckBoxPos p:{0}, SPos:{1}, iconWidth:{2}, iconHeight:{3}", p, SPos, iconWidth, iconHeight);

            bool result = false;
            ObjType = OBJTYPE.NONE;
            if ((p.X > SPos.X - iconWidthHalf) &&
               (p.X < SPos.X + iconWidthHalf) &&
               (p.Y > SPos.Y) &&
               (p.Y < SPos.Y + iconHeight))
            {
                HoverType = ObjType = OBJTYPE.START;
                //return this;
                result = true;
            }

            if ((p.X > EPos.X - iconWidthHalf) &&
               (p.X < EPos.X + iconWidthHalf) &&
               (p.Y > EPos.Y) &&
               (p.Y < EPos.Y + iconHeight))
            {
                HoverType = ObjType = OBJTYPE.END;
                //return this;
                result = true;
            }

            if ((p.X > (SPos.X + iconWidthHalf)) &&
                (p.X < EPos.X - iconWidthHalf) &&
                (p.Y > SPos.Y) &&
                (p.Y < SPos.Y + iconHeight))
            {
                HoverType = ObjType = OBJTYPE.BODY;
                //return this;
                result = true;
            }

            if (result)
            {
                if(EnterNotice != null)
                    EnterNotice(this, EventArgs.Empty);
                return this;
            }
            else
            {
                HoverType = OBJTYPE.NONE;
                if (LeaveNotice != null)
                    LeaveNotice(this, EventArgs.Empty);
                return null;
            }
        }
        */

        public SelectObject CheckPos(Point p)
        {
            return bodyObj.CheckBoxPos(p);
        }

        /*
        public TimeLineObject CheckIconPos(Point p)
        {
            int iconWidth = EIcon.Width;
            int iconHeight = EIcon.Height;

            //Console.WriteLine("CheckIconPos p:{0}, SPos:{1}, iconWidth:{2}, iconHeight:{3}", p, SPos, iconWidth, iconHeight);

            if ((p.X > SPos.X) &&
               (p.X < SPos.X + iconWidth) &&
               (p.Y > SPos.Y) &&
               (p.Y < SPos.Y + iconHeight))
            {
                ObjType = OBJTYPE.START;
                return this;
            }

            if ((p.X > EPos.X) &&
               (p.X < EPos.X + iconWidth) &&
               (p.Y > EPos.Y) &&
               (p.Y < EPos.Y + iconHeight))
            {
                ObjType = OBJTYPE.END;
                return this;
            }

            if ((p.X > (SPos.X + iconWidth)) &&
                (p.X < EPos.X) &&
                (p.Y > SPos.Y) &&
                (p.Y < SPos.Y + iconHeight))
            {
                ObjType = OBJTYPE.BODY;
                return this;
            }


            EventObject obj = eventMng.CheckPos(p);
            if(obj != null)
            {
                ObjType = obj.ObjType;
                //return 
            }


            ObjType = OBJTYPE.NONE;
            return null;
        }
        */

        /*
        public TimeLineObject CheckEndPos(Point p)
        {
            if(endType == ENDTYPE.ICON)
            {
                return CheckIconPos(p);
            }
            else if (endType == ENDTYPE.BOX)
            {
                return CheckBoxPos(p);
            }

            return null;
        }
        */

        /*
        public void DrawFrameBox(Graphics g)
        {
            Pen drawLine = new Pen(Color.Black);

            int iconWidth = EndIconWidth;
            int iconHeight = EndIconHeight;
            if (endType == ENDTYPE.ICON)
            {
                iconWidth = EIcon.Width;
                iconHeight = EIcon.Height;
            }
            else if(endType == ENDTYPE.BOX) 
            {
                iconWidth = 0;
            }

            Rectangle rect = new Rectangle(SPos.X,
                                           SPos.Y,
                                           (EPos.X - SPos.X)+ iconWidth,
                                           iconHeight);

            g.DrawRectangle(drawLine, rect);
        }
        */

        /*
        public void DrawHoverEndBox(Graphics g, Point p, Brush brush)
        {
            Rectangle rect = new Rectangle(p.X - 3,
                                           p.Y - 2,
                                           EndIconWidth,
                                           EndIconHeight + 4);

            g.FillRectangle(brush, Rectangle.Inflate(rect, 2, 2));
        }

        public void DrawEndBox(Graphics g, Point p, Brush brush)
        {
            Rectangle rect = new Rectangle(p.X - 3,
                                           p.Y - 2,
                                           EndIconWidth,
                                           EndIconHeight + 4);

            g.FillRectangle(brush, rect);
        }
        */

        public void DrawMarks(Graphics g)
        {
            bodyObj.DrawMark(g);

            eventMng.DrawEvents(g);
        }

        /*
        // 오른쪽 아이콘이 왼쪽 아이콘/ 또는 그 반대의 경우로 넘어가는지 체크
        public bool CheckObjectBound(Point p)
        {
            int iconWidth = EndIconWidth;
            int iconHeight = EndIconHeight;
            if (endType == ENDTYPE.ICON)
            {
                iconWidth = EIcon.Width;
                iconHeight = EIcon.Height;
            }

            int nERight = 0;
            int nSLeft = 0;
            OBJTYPE type = ObjType;

            if (type == OBJTYPE.END)
            {
                nSLeft = SPos.X;
                if (p.X <= nSLeft) return false;
            }
            else if (type == OBJTYPE.START)
            {
                nERight = EPos.X + iconWidth;
                if (p.X >= nERight) return false; // 시간 기준이 각 아이콘의 우측기준인데 끝점은 오른쪽면이 기준임.
            }
            //Console.WriteLine("nSRight:{0}, nSLeft:{1} / nERight:{2}, nELeft:{3}", nSRight, nSLeft, nERight, nELeft);
            return true;
        }
        */
    }
}
