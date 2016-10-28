using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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

    class TimeLineObject 
    {
        public String Name { get; set; }
        public bool Lock { get; set; }
        public bool View { get; set; }

        public Point SPos { get; set; }
        public Point EPos { get; set; }
        public Image SIcon { get; set; }
        public Image EIcon { get; set; }

        public TIMEOBJTYPE objType { get; set; }
        public ANITYPE aniType { get; set; }

        private int m_tickWidth { get; set; }
        private int nameGap = 5; // 끝점에서 이름까지 갭

        public int STick { get; set; }                      // 시작 포인트 틱
        public int ETick { get; set; }                      // 끝 포인트 틱

        // 끝점 시작점은 아이콘 폭만큼 빼면됨.
        public int ConvEndPosStart(int PosX, int TickWidth)
        {
            return PosX - EIcon.Width;
        }

        public TimeLineObject(Point point1, Point point2, int tickWidth)
        {
            m_tickWidth = tickWidth;

            SIcon = Image.FromFile(@"..\..\Resources\go-first-2.png");
            SPos = new Point(point1.X, point1.Y);

            EIcon = Image.FromFile(@"..\..\Resources\go-last-2.png");
            EPos = new Point((point1.X + (m_tickWidth*10)) - EIcon.Width, point2.Y);

            aniType = ANITYPE.STOP_AT_END;
        }

        public void DrawIcon(Graphics g, Image image, Point point)
        {
            g.DrawImage(image, new Rectangle(point.X, point.Y, image.Width, image.Height));
        }

        
        public void DrawIcon(Graphics g, TIMEOBJTYPE type)
        {
            if (type == TIMEOBJTYPE.START)
            {
                DrawIcon(g, SIcon, SPos);
            }
            else
            {
                DrawIcon(g, EIcon, EPos);
            }
        }

        public void DrawName(Graphics g)
        {
            g.DrawString(Name, new Font("Arial", 8), Brushes.Black, new Point(EPos.X + EIcon.Width + nameGap, EPos.Y - 1));
        }

        public TimeLineObject CheckIconPos(Point p)
        {
            if ((p.X > SPos.X) &&
               (p.X < SPos.X + SIcon.Width) &&
               (p.Y > SPos.Y) &&
               (p.Y < SPos.Y + SIcon.Height))
            {
                objType = TIMEOBJTYPE.START;
                return this;
            }

            if ((p.X > EPos.X) &&
               (p.X < EPos.X + EIcon.Width) &&
               (p.Y > EPos.Y) &&
               (p.Y < EPos.Y + EIcon.Height))
            {
                objType = TIMEOBJTYPE.END;
                return this;
            }

            if ((p.X > (SPos.X + SIcon.Width)) &&
                (p.X < EPos.X) &&
                (p.Y > SPos.Y) &&
                (p.Y < SPos.Y + SIcon.Height))
            {
                objType = TIMEOBJTYPE.BODY;
                return this;
            }

            objType = TIMEOBJTYPE.NONE;
            return null;
        }

        //public TimeLineObject Conv(Point p)
        //{
            
        //}

        public void DrawFrameBox(Graphics g)
        {
            Pen drawLine = new Pen(Color.Black);
            Rectangle rect = new Rectangle(SPos.X,
                                           SPos.Y,
                                           (EPos.X - SPos.X) + EIcon.Width,
                                           (EPos.Y - SPos.Y) + EIcon.Height);
            g.DrawRectangle(drawLine, rect);
            //Console.WriteLine(string.Format("Start:{0}, End:{1}", SPos, EPos));
        }
    }
}
