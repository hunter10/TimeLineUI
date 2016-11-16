using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    public enum TIMEOBJTYPE
    {
        NONE,
        START,
        END,
        BODY
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
        public bool bHover = false;

        public String Name { get; set; }
        public bool Lock { get; set; }
        public bool View { get; set; }

        public Point SPos { get; set; }
        public Point EPos { get; set; }
        public Image SIcon { get; set; }
        public Image EIcon { get; set; }

        public TIMEOBJTYPE HoverType { get; set; }
        public TIMEOBJTYPE ObjType { get; set; }
        public ANITYPE aniType { get; set; }
        public ENDTYPE endType { get; set; }

        private int m_tickWidth { get; set; }
        private int nameGap = 5; // 끝점에서 이름까지 갭

        public int STick { get; set; }                      // 시작 포인트 틱
        public int ETick { get; set; }                      // 끝 포인트 틱

        const int EndIconWidth = 6;
        const int EndIconHeight = 16;




        // 총 시작이벤트(틱)별로 모음이 있고 틱당 여러개의 이벤트가 모여있음. 리스트 + 맵
        



        // 끝점 시작점은 아이콘 폭만큼 빼면됨.
        public int ConvEndPosStart(int PosX, int TickWidth)
        {
            if (endType == ENDTYPE.ICON)
                return PosX - EIcon.Width;
            else
                return PosX;
        }

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

        public void DrawIcon(Graphics g, Image image, Point point)
        {
            g.DrawImage(image, new Rectangle(point.X, point.Y, image.Width, image.Height));
        }


        public void DrawEnd(Graphics g, TIMEOBJTYPE type)
        {
            if (type == TIMEOBJTYPE.START)
            {
                if(type == HoverType)
                    DrawHoverEndBox(g, SPos);

                if (endType == ENDTYPE.ICON)
                    DrawIcon(g, SIcon, SPos);
                else
                    DrawEndBox(g, SPos, bHover);
            }
            else
            {
                if (type == HoverType)
                    DrawHoverEndBox(g, EPos);

                if (endType == ENDTYPE.ICON)
                    DrawIcon(g, EIcon, EPos);
                else
                    DrawEndBox(g, EPos, bHover);
            }
        }

        public void DrawName(Graphics g)
        {
            if(endType == ENDTYPE.ICON)
                g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(EPos.X + EIcon.Width + nameGap, EPos.Y - 1));
            else
                g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(EPos.X + nameGap, EPos.Y - 1));
        }

        public TimeLineObject CheckBoxPos(Point p)
        {
            int iconWidth = EndIconWidth;
            int iconHeight = EndIconHeight;
           
            int iconWidthHalf = iconWidth / 2;

            //Console.WriteLine("CheckBoxPos p:{0}, SPos:{1}, iconWidth:{2}, iconHeight:{3}", p, SPos, iconWidth, iconHeight);

            bool result = false;
            ObjType = TIMEOBJTYPE.NONE;
            if ((p.X > SPos.X - iconWidthHalf) &&
               (p.X < SPos.X + iconWidthHalf) &&
               (p.Y > SPos.Y) &&
               (p.Y < SPos.Y + iconHeight))
            {
                HoverType = ObjType = TIMEOBJTYPE.START;
                //return this;
                result = true;
            }

            if ((p.X > EPos.X - iconWidthHalf) &&
               (p.X < EPos.X + iconWidthHalf) &&
               (p.Y > EPos.Y) &&
               (p.Y < EPos.Y + iconHeight))
            {
                HoverType = ObjType = TIMEOBJTYPE.END;
                //return this;
                result = true;
            }

            if ((p.X > (SPos.X + iconWidthHalf)) &&
                (p.X < EPos.X - iconWidthHalf) &&
                (p.Y > SPos.Y) &&
                (p.Y < SPos.Y + iconHeight))
            {
                HoverType = ObjType = TIMEOBJTYPE.BODY;
                //return this;
                result = true;
            }

            if (result)
            {
                
                bHover = true;
                if(EnterNotice != null)
                    EnterNotice(this, EventArgs.Empty);
                return this;
            }
            else
            {
                HoverType = TIMEOBJTYPE.NONE;
                bHover = false;
                if (LeaveNotice != null)
                    LeaveNotice(this, EventArgs.Empty);
                return null;
            }
        }

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
                ObjType = TIMEOBJTYPE.START;
                return this;
            }

            if ((p.X > EPos.X) &&
               (p.X < EPos.X + iconWidth) &&
               (p.Y > EPos.Y) &&
               (p.Y < EPos.Y + iconHeight))
            {
                ObjType = TIMEOBJTYPE.END;
                return this;
            }

            if ((p.X > (SPos.X + iconWidth)) &&
                (p.X < EPos.X) &&
                (p.Y > SPos.Y) &&
                (p.Y < SPos.Y + iconHeight))
            {
                ObjType = TIMEOBJTYPE.BODY;
                return this;
            }

            ObjType = TIMEOBJTYPE.NONE;
            return null;
        }

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

        public void DrawHoverEndBox(Graphics g, Point p)
        {
//            Brush brush = Brushes.Brown;
            Brush brushOutLine = Brushes.Yellow;
            Rectangle rect = new Rectangle(p.X - 3,
                                           p.Y - 2,
                                           EndIconWidth,
                                           EndIconHeight + 4);

            g.FillRectangle(brushOutLine, Rectangle.Inflate(rect, 2, 2));
        }

        public void DrawEndBox(Graphics g, Point p, bool hover = false)
        {
            //Pen drawLine = new Pen(Color.Brown);
            //Pen drawOutLine = new Pen(Color.Yellow);

            Brush brush = Brushes.Brown;
            Brush brushOutLine = Brushes.Yellow;
            Rectangle rect = new Rectangle(p.X-3,
                                           p.Y-2,
                                           EndIconWidth,
                                           EndIconHeight + 4);

            //if (hover)
            //    g.FillRectangle(brushOutLine, Rectangle.Inflate(rect, 2, 2));

            g.FillRectangle(brush, rect);

            
        }


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
            TIMEOBJTYPE type = ObjType;

            if (type == TIMEOBJTYPE.END)
            {
                nSLeft = SPos.X;
                if (p.X <= nSLeft) return false;
            }
            else if (type == TIMEOBJTYPE.START)
            {
                nERight = EPos.X + iconWidth;
                if (p.X >= nERight) return false; // 시간 기준이 각 아이콘의 우측기준인데 끝점은 오른쪽면이 기준임.
            }
            //Console.WriteLine("nSRight:{0}, nSLeft:{1} / nERight:{2}, nELeft:{3}", nSRight, nSLeft, nERight, nELeft);
            return true;
        }
    }
}
