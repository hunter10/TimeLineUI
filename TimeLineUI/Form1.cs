using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;

namespace TimeLineUI
{
    public partial class MainForm : Form
    {
        private List<TimeLineObject> lstTimeLineObj = new List<TimeLineObject>();
        private TimeLineObject selectedTimeLineObj = null;  // 선택한 오브젝트

        private NowTimeLine nowTimeLine = null;     // 타임 세로선
        private Timer timer = new System.Windows.Forms.Timer();

        int s_offset = -1; // 클릭한 위치에서 시작점까지 거리
        int e_offset = -1; // 클릭한 위치에서 끝점까지 거리

        public MainForm()
        {
            InitializeComponent();
            ControlInit();
        }

        private void ControlInit()
        {
            picBox_TimeEdit.Height = panel_TimeEdit.Height;
            picBox_TimeEdit.Width = 5000;
            picBox_TimeEdit.Paint += new PaintEventHandler(picBox_TimeEdit_Paint);
            picBox_TimeEdit.MouseClick += new MouseEventHandler(picBox_TimeEdit_MouseClick);
            picBox_TimeEdit.MouseMove += new MouseEventHandler(picBox_TimeEdit_MouseMove);
            picBox_TimeEdit.MouseUp += new MouseEventHandler(picBox_TimeEdit_MouseUp);
            picBox_TimeEdit.MouseDown += new MouseEventHandler(picBox_TimeEdit_MouseDown);

            

            nowTimeLine = new NowTimeLine(new Point(20, 0), new Point(20, panel_TimeEdit.Height));
            picBox_TimeEdit.Invalidate();

            // 스크롤되는 간격 조정
            panel_TimeEdit.HorizontalScroll.SmallChange = 10;   // 양 사이드의 화살표 누를때
            panel_TimeEdit.HorizontalScroll.LargeChange = 1;    // 썸네일 밖에 영역을 누를때

            panel_TimeEdit.VerticalScroll.SmallChange = 23;     // 양 사이드의 화살표 누를때
            panel_TimeEdit.VerticalScroll.LargeChange = 23;     // 썸네일 밖에 영역을 누를때
            //panel_TimeEdit.AutoScrollMargin = 23;
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

            for (int i = 0; i < 5; i++)
            {
                int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + 5;
                TimeLineObject obj = new TimeLineObject(new Point(5, y), new Point(5, y));
                obj.Name = string.Format("test{0}", lstTimeLineObj.Count);
                obj.Lock = false;
                obj.View = true;
                lstTimeLineObj.Add(obj);
                Console.WriteLine(string.Format("{0} ({1},{2}) Create", obj.Name, obj.SPos, obj.EPos));

                // 좌측 판넬 세팅
                dGrid_TimeLineObj.Rows.Add(obj.Name, obj.Lock, obj.View);

                // 우측 판넬 세팅
                picBox_TimeEdit.Invalidate();
            }
        }

        private void picBox_TimeEdit_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bitmap = new Bitmap(this.Width, this.Height);
            Graphics memGraphics = Graphics.FromImage(bitmap);
            memGraphics.Clear(this.BackColor);

            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                obj.DrawIcon(memGraphics, TIMEOBJTYPE.START);
                obj.DrawIcon(memGraphics, TIMEOBJTYPE.END);
                obj.DrawName(memGraphics);
                obj.DrawFrameBox(memGraphics);
            }

            // 현재 타임라인 세로선
            memGraphics.DrawLine(Pens.Red, nowTimeLine.SPos, nowTimeLine.EPos);

            e.Graphics.DrawImageUnscaled(bitmap, 0, 0);

            memGraphics.Dispose();
            bitmap.Dispose();
        }

        private void picBox_TimeEdit_MouseClick(object sender, MouseEventArgs e)
        {
            picBox_TimeEdit.Refresh();

            if(e.Button == MouseButtons.Right)
            {
                foreach (TimeLineObject obj in lstTimeLineObj)
                {
                    selectedTimeLineObj = obj.CheckIconPos(new Point(e.X, e.Y));
                    if (selectedTimeLineObj == null) continue;
                    break;
                }

                if (selectedTimeLineObj != null)
                {
                    if (selectedTimeLineObj.currType == TIMEOBJTYPE.END)
                    {
                        contextMenuStrip1.Show(picBox_TimeEdit, e.Location);
                    }
                }
            }

            //if (selectedTimeLineObj == null) Console.WriteLine("selectedTimeLineObj is NULL!!!");
        }

        
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
                    s_offset = e.X - selectedTimeLineObj.SPos.X;
                    e_offset = selectedTimeLineObj.EPos.X - e.X;
                }

                //Console.WriteLine("type={0}", selectedTimeLineObj.currType);
                //Console.WriteLine("dGrid_TimeLineObj VerticalScrollingOffset:{0}, RowHeight:{1}", dGrid_TimeLineObj.VerticalScrollingOffset, dGrid_TimeLineObj.RowTemplate.Height);
                //Console.WriteLine("{0} ({1},{2}) AutoScroll:{3}", selectedTimeLineObj.Name, e.X, e.Y, panel_TimeEdit.AutoScrollPosition);
            }
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
                    selectedTimeLineObj.SPos = new Point(e.X - s_offset, selectedTimeLineObj.SPos.Y);
                    selectedTimeLineObj.EPos = new Point(e.X + e_offset, selectedTimeLineObj.EPos.Y);
                }

                picBox_TimeEdit.Refresh();
            }
        }

        private void picBox_TimeEdit_MouseUp(object sender, MouseEventArgs e)
        {
            // 좌표를 시간으로 변환하는 계산이 들어가야 함.
            

            s_offset = -1;
            e_offset = -1;
        }

        // 이 함수는 타임라인 오브젝트에서 체크해야 할듯
        private bool CheckPanelBound(Point p, TimeLineObject obj)
        {
            int nEnd_Right = p.X + obj.EIcon.Width;  
            int nEnd_Left = p.X;

            // 맨 왼쪽 오버조건
            if (nEnd_Right > picBox_TimeEdit.Width) return false;

            
            // 맨 오른쪽 오버조건은 일단 패스


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

        private void dGrid_TimeLineObj_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                Console.WriteLine("dGrid_TimeLineObj VerticalScrollingOffset:{0}, RowHeight:{1}", dGrid_TimeLineObj.VerticalScrollingOffset, dGrid_TimeLineObj.RowTemplate.Height);
                Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
                int x = Math.Abs(panel_TimeEdit.AutoScrollPosition.X);
                int y = dGrid_TimeLineObj.VerticalScrollingOffset;

                // panel_TimeEdit.VerticalScroll.Value 에 값을 입력하는것은 작동 안함. 이해안됨
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);

                Console.WriteLine("1 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
            }
        }

        private void panel_TimeEdit_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                int moveIndex = panel_TimeEdit.VerticalScroll.Value / dGrid_TimeLineObj.RowTemplate.Height;
                Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}, Index:{2}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value, moveIndex);

                dGrid_TimeLineObj.FirstDisplayedScrollingRowIndex = moveIndex;
            }
        }

        //private void button1_Click(object sender, EventArgs e) // 스크롤 D
        //{
        //    Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
        //    //panel_TimeEdit.VerticalScroll.Value = dGrid_TimeLineObj.RowTemplate.Height;
        //    int x = Math.Abs(panel_TimeEdit.AutoScrollPosition.X);
        //    int y = Math.Abs(panel_TimeEdit.AutoScrollPosition.Y) + 23;
        //    panel_TimeEdit.AutoScrollPosition = new Point(x, y);
        //    Console.WriteLine("1 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
        //}

        //private void button2_Click(object sender, EventArgs e) // 스크롤 U
        //{
        //    Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
        //    //panel_TimeEdit.VerticalScroll.Value = 0;// dGrid_TimeLineObj.RowTemplate.Height;
        //    int x = Math.Abs(panel_TimeEdit.AutoScrollPosition.X);
        //    int y = Math.Abs(panel_TimeEdit.AutoScrollPosition.Y) - 23;
        //    panel_TimeEdit.AutoScrollPosition = new Point(x, y);
        //    Console.WriteLine("1 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
        //}

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //Console.WriteLine("panel_TimeEdit.Height {0}", panel_TimeEdit.Height);
            picBox_TimeEdit.Height = panel_TimeEdit.Height;

            nowTimeLine = new NowTimeLine(nowTimeLine.SPos, new Point(nowTimeLine.EPos.X, panel_TimeEdit.Height));
        }

        // 값이 바뀌고 난후에 들어오는 이벤트...
        private void dGrid_TimeLineObj_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if(e.ColumnIndex == 0)
            {
                DataGridViewTextBoxCell cell = dGrid_TimeLineObj.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewTextBoxCell;
                lstTimeLineObj[e.RowIndex].Name = (String)cell.Value;

                //Console.WriteLine("CellValueChanged Row:{0}, Col:{1}, Value:{2}", e.RowIndex, e.ColumnIndex, Convert.ToString(cell.Value));
            }
        }

        // 셀을 선택할때 들어오는 이벤트...
        private void dGrid_TimeLineObj_SelectionChanged(object sender, EventArgs e)
        {
            if (dGrid_TimeLineObj.CurrentCell == null)
                return;

            int rowIdx = dGrid_TimeLineObj.CurrentCell.RowIndex;
            int colIdx = dGrid_TimeLineObj.CurrentCell.ColumnIndex;

            if (colIdx == 1 || colIdx == 2)
            {
                DataGridViewCheckBoxCell cell = dGrid_TimeLineObj.CurrentCell as DataGridViewCheckBoxCell;
                if (Convert.ToBoolean(cell.Value) == true)
                    cell.Value = false;
                else
                    cell.Value = true;

                //Console.WriteLine("SelectionChanged Row:{0}, Col:{1}, Value:{2}", rowIdx, colIdx, Convert.ToBoolean(cell.Value));
                //Console.WriteLine("0 {0}, Lock:{1}, View{2}", lstTimeLineObj[rowIdx].Name, lstTimeLineObj[rowIdx].Lock, lstTimeLineObj[rowIdx].View);

                if (colIdx == 1) // Lock
                {
                    lstTimeLineObj[rowIdx].Lock = (Boolean)cell.Value;
                }

                if(colIdx == 2) // View
                {
                    lstTimeLineObj[rowIdx].View = (Boolean)cell.Value;
                }
                //Console.WriteLine("1 {0}, Lock:{1}, View:{2}", lstTimeLineObj[rowIdx].Name, lstTimeLineObj[rowIdx].Lock, lstTimeLineObj[rowIdx].View);
            }
        }

        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(selectedTimeLineObj != null)
                Console.WriteLine("{0}", selectedTimeLineObj.Name);
        }

        private void stopAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("b");
        }

        private void replayAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("c");
        }
    }
}
