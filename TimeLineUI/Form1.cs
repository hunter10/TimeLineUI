using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLineUI
{
    public partial class MainForm : Form
    {
        private List<TimeLineObject> lstTimeLineObj = new List<TimeLineObject>();
        private TimeLineObject selectedTimeLineObj = null;
        private NowTimeLine nowTimeLine = null;
        private Timer timer = new System.Windows.Forms.Timer();

        public MainForm()
        {
            InitializeComponent();
            ControlInit();
        }

        private void ControlInit()
        {
            picBox_TimeEdit.Height = 200;
            picBox_TimeEdit.Width = 5000;
            picBox_TimeEdit.Paint += new PaintEventHandler(picBox_TimeEdit_Paint);
            picBox_TimeEdit.MouseClick += new MouseEventHandler(picBox_TimeEdit_MouseClick);
            picBox_TimeEdit.MouseMove += new MouseEventHandler(picBox_TimeEdit_MouseMove);
            picBox_TimeEdit.MouseUp += new MouseEventHandler(picBox_TimeEdit_MouseUp);
            picBox_TimeEdit.MouseDown += new MouseEventHandler(picBox_TimeEdit_MouseDown);

            nowTimeLine = new NowTimeLine(new Point(20, 0), new Point(20, 300));
            picBox_TimeEdit.Invalidate();

            // 스크롤되는 간격 조정
            panel_TimeEdit.HorizontalScroll.SmallChange = 10;
            panel_TimeEdit.HorizontalScroll.LargeChange = 1;
        }

        public class NowTimeLine
        {
            public Point SPos { get; set; }
            public Point EPos { get; set; }

            public NowTimeLine(Point point1, Point point2)
            {
                this.SPos = point1;
                this.EPos = point2;
            }
        }

        

        private void btnObjAdd_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("lstTimeLineObj.Count : {0}", lstTimeLineObj.Count);
            int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + 5;
            TimeLineObject obj = new TimeLineObject(new Point(5, y), new Point(5, y));
            obj.Name = string.Format("test{0}", lstTimeLineObj.Count);
            obj.Lock = false;
            obj.View = true;
            lstTimeLineObj.Add(obj);
            Console.WriteLine(string.Format("{0},{1}", obj.SPos, obj.EPos));

            // 좌측 판넬 세팅
            dGrid_TimeLineObj.Rows.Add(obj.Name, obj.Lock, obj.View);

            // 우측 판넬 세팅
            picBox_TimeEdit.Invalidate();
        }

        private void picBox_TimeEdit_Paint(object sender, PaintEventArgs e)
        {
            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                obj.DrawIcon(e.Graphics, TIMEOBJTYPE.START);
                obj.DrawIcon(e.Graphics, TIMEOBJTYPE.END);
                obj.DrawFrameBox(e.Graphics);
            }

            e.Graphics.DrawLine(Pens.Red, nowTimeLine.SPos, nowTimeLine.EPos);
        }

        private void picBox_TimeEdit_MouseClick(object sender, MouseEventArgs e)
        {
            picBox_TimeEdit.Refresh();
        }

        int startposoffset = -1;
        int endposoffset = -1;
        private void picBox_TimeEdit_MouseDown(object sender, MouseEventArgs e)
        {
            if (lstTimeLineObj.Count <= 0)
                return;
            
            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                selectedTimeLineObj = obj.CheckIconPos(new Point(e.X, e.Y));
                if (selectedTimeLineObj == null) continue;
                break;
            }

            if (selectedTimeLineObj != null)
            {
                if (selectedTimeLineObj.currType == TIMEOBJTYPE.BODY)
                {
                    startposoffset = e.X - selectedTimeLineObj.SPos.X;
                    endposoffset = selectedTimeLineObj.EPos.X - e.X;
                }

                Console.WriteLine("type={0}", selectedTimeLineObj.currType);
            }

            //Console.WriteLine("{0},{1} AutoScroll:{2}", e.X, e.Y, panel_TimeEdit.AutoScrollPosition);
        }

        private void picBox_TimeEdit_MouseMove(object sender, MouseEventArgs e)
        {
            //if (mouseClicked)
            if (e.Button == MouseButtons.Left)
            {
                if (selectedTimeLineObj == null)
                    return;

                if(CheckPanelBound(new Point(e.X, 0), selectedTimeLineObj) == false)
                    return;
                

                if (selectedTimeLineObj.currType == TIMEOBJTYPE.START) // 시작점 움직이기
                {
                    selectedTimeLineObj.SPos = new Point(e.X, selectedTimeLineObj.SPos.Y);
                }
                else if (selectedTimeLineObj.currType == TIMEOBJTYPE.END) // 끝점 움직이기
                {
                    selectedTimeLineObj.EPos = new Point(e.X, selectedTimeLineObj.EPos.Y);
                }
                else // 보디 움직이기
                {
                    selectedTimeLineObj.SPos = new Point(e.X - startposoffset, selectedTimeLineObj.SPos.Y);
                    selectedTimeLineObj.EPos = new Point(e.X + endposoffset, selectedTimeLineObj.EPos.Y);
                }

                picBox_TimeEdit.Refresh();
            }
        }

        private void picBox_TimeEdit_MouseUp(object sender, MouseEventArgs e)
        {
            // 좌표 계산이 들어가야 함.
            selectedTimeLineObj = null;
        }

        // 이 함수는 타임라인 오브젝트에서 체크해야 할듯
        private bool CheckPanelBound(Point p, TimeLineObject obj)
        {
            int nEnd_Right = p.X + obj.EIcon.Width;  
            int nEnd_Left = p.X;

            // 맨 왼쪽 오버조건
            if (nEnd_Right > picBox_TimeEdit.Width) return false;

            // 맨 오른쪽 오버조건
            // 일단 패스


            // 가장자리 스크롤되기 위한 조건 
            int nPanelRight = panel_TimeEdit.Width + panel_TimeEdit.HorizontalScroll.Value;
            int nPanelLeft = panel_TimeEdit.HorizontalScroll.Value;

            // 오브젝트가 오른쪽 가장자리에 닿았을때
            if (nEnd_Right > nPanelRight)
            {
                int x = (nEnd_Right - panel_TimeEdit.Width);
                if (x >= panel_TimeEdit.HorizontalScroll.Maximum)
                    x = panel_TimeEdit.HorizontalScroll.Maximum;

                panel_TimeEdit.HorizontalScroll.Value = x;
            }

            // 오브젝트가 왼쪽 가장자리에 닿았을때
            if (nEnd_Left < nPanelLeft)
            {
                int x = nEnd_Left;
                if (x <= panel_TimeEdit.HorizontalScroll.Minimum)
                    x = panel_TimeEdit.HorizontalScroll.Minimum;

                panel_TimeEdit.HorizontalScroll.Value = x;
            }

            //Console.WriteLine(string.Format("panel_TimeEdit.AutoScrollPosition : {0}", panel_TimeEdit.AutoScrollPosition));

            return true;
        }










        // 타임 컨트롤은 좀 나중에....

        private void btnGoFirst_Click(object sender, EventArgs e)
        {

        }

        private void btnOneStepPrev_Click(object sender, EventArgs e)
        {

        }

        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            timer.Interval = 10; // 1초
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void btnOneStepNext_Click(object sender, EventArgs e)
        {

        }

        private void btnGoLast_Click(object sender, EventArgs e)
        {

        }

        private void btnGoReverse_Click(object sender, EventArgs e)
        {

        }

        // 매 1초마다 Tick 이벤트 핸들러 실행
        void timer_Tick(object sender, EventArgs e)
        {
            // UI 쓰레드에서 실행. 
            // UI 컨트롤 직접 엑세스 가능

            nowTimeLine.SPos = new Point(nowTimeLine.SPos.X + 1, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(nowTimeLine.EPos.X + 1, nowTimeLine.EPos.Y);

            // 스크롤 처리가 들어가야 함.
            
            picBox_TimeEdit.Invalidate();
        }
    }
}
