using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLineUI
{
    public partial class TimeLineUI : UserControl
    {
        private List<TimeLineObject> lstTimeLineObj = new List<TimeLineObject>();
        private TimeLineObject selectedTimeLineObj = null;  // 선택한 오브젝트
        private List<TimeLine> drawTicks = new List<TimeLine>();

        private TimeLine nowTimeLine_picboxRuler = null;        // 타임 세로선 (눈금자용)
        private TimeLine nowTimeLine_picboxTimeEdit = null;     // 타임 세로선 (타임에디터용)
        private Timer timer = new System.Windows.Forms.Timer();

        private ContextMenu contextMenu1;

        int s_offset = -1; // 클릭한 위치에서 시작점 좌측면까지 픽셀 거리 (보디를 구하기 위해)
        int e_offset = -1; // 클릭한 위치에서 끝점 우측면까지 픽셀 거리 (보디를 구하기 위해)

        // 2틱당 1프레임
        // 5프레임당 1초
        const int timePerFrame = 100;           // 프레임당 시간 (0.1초)
        const int tickPerFrame = 2;             // 프레임당 틱갯수
        const int framePerSec = 5;              // 초당 5프레임
        const int tickWidth = 20;               // 이 간격을 0.1초로 계산
        const int totalTime = 3;                // 전체 작업시간(초)
        

        // 눈금용 데이터들
        const int boxGapWidth = 15;             // 픽쳐박스 왼쪽에서 시작위치까지의 간격 (눈금자, 타임에디터 공용)
        const int rulerPicBoxGapHeight = 15;    // 픽쳐박스 위에서 시작위치까지의 간격 (눈금자용)
        const int timeEditPicBoxGapHeight = 0;  // 픽쳐박스 위에서 시작위치까지의 간격 (타임에디터용)
        const int timeEditPicBox_ObjectGap = 5; // 타임에디터에서 오브젝트간에 높이갭 
        const int halfFrameHeight = 5;          // 0.5 프레임당 눈금길이
        const int fullFrameHeight = 12;         // 1 프레임당 눈금길이

        int maxIdx = 0;                         // 최대 틱 인덱스
        int maxIdxPerPanel = 0;                // 한 판넬에 표시하는 최대 틱 인덱스
        int currIdx = 0;                        // 현재 틱 위치 인덱스
        int currOffsetIdx = 0;                  // 시작위치에서 얼마큼 떨어져 있는지 
        //int minOffsetIdxWithCenter = 0;         // 센터 적용된 최소 오프셋값
        int maxOffsetIdxWithCenter = 0;         // 센터 적용된 최대 오프셋값 
        int centerOffsetIdx = 0;                // 센터를 가기 위한 판넬 오른쪽에서 떨어진 간격 (계산해야함)

        int dGrid_TimeLineObjHeight = 0;
        int picBox_TimeEdit_MinHeight = 0;      // 타임에디터 최소 높이

        public class TimeLine
        {
            public Point SPos { get; set; }
            public Point EPos { get; set; }

            public TimeLine(Point point1, Point point2)
            {
                this.SPos = point1;
                this.EPos = point2;
            }
        }

        public TimeLineUI()
        {
            InitializeComponent();
            ControlInit();
            TimeInit();
        }

        private void ControlInit()
        {
            contextMenu1 = new ContextMenu();
            contextMenu1.MenuItems.Add(new MenuItem().Text = "Delete Group");
            contextMenu1.MenuItems.Add(new MenuItem().Text = "Stop Animation");
            contextMenu1.MenuItems.Add(new MenuItem().Text = "Replay Animation");
            contextMenu1.MenuItems[0].Click += new EventHandler(deleteGroupToolStripMenuItem_Click);
            contextMenu1.MenuItems[1].Click += new EventHandler(stopAnimationToolStripMenuItem_Click);
            contextMenu1.MenuItems[2].Click += new EventHandler(replayAnimationToolStripMenuItem_Click);

            dGrid_TimeLineObj.MouseClick += new MouseEventHandler(dGrid_TimeLineObj_MouseClick);

            picBox_Ruler.Height = panel_Ruler.Height;
            picBox_Ruler.Paint += new PaintEventHandler(picBox_Ruler_Paint);
            picBox_Ruler.MouseDown += new MouseEventHandler(picBox_Ruler_MouseDown);

            // 스크롤바 없애기
            panel_Ruler.AutoScroll = false;
            panel_Ruler.HorizontalScroll.Enabled = false;
            panel_Ruler.HorizontalScroll.Visible = false;
            panel_Ruler.HorizontalScroll.Maximum = 0;
            panel_Ruler.VerticalScroll.Enabled = false;
            panel_Ruler.VerticalScroll.Visible = false;
            panel_Ruler.VerticalScroll.Maximum = 0;
            panel_Ruler.AutoScroll = true;

            // 타임 에디터
            picBox_TimeEdit.Height = panel_TimeEdit.Height;
            picBox_TimeEdit_MinHeight = picBox_TimeEdit.Height;
            picBox_TimeEdit.Paint += new PaintEventHandler(picBox_TimeEdit_Paint);
            picBox_TimeEdit.MouseClick += new MouseEventHandler(picBox_TimeEdit_MouseClick);
            picBox_TimeEdit.MouseMove += new MouseEventHandler(picBox_TimeEdit_MouseMove);
            picBox_TimeEdit.MouseUp += new MouseEventHandler(picBox_TimeEdit_MouseUp);
            picBox_TimeEdit.MouseDown += new MouseEventHandler(picBox_TimeEdit_MouseDown);

            // 스크롤되는 간격 조정
            panel_TimeEdit.HorizontalScroll.SmallChange = 10;   // 양 사이드의 화살표 누를때
            panel_TimeEdit.HorizontalScroll.LargeChange = 1;    // 썸네일 밖에 영역을 누를때
            panel_TimeEdit.VerticalScroll.SmallChange = 23;     // 양 사이드의 화살표 누를때
            panel_TimeEdit.VerticalScroll.LargeChange = 23;     // 썸네일 밖에 영역을 누를때

            // 타임에디터 판넬에 스크롤바가 발생하므로 룰러 클릭영역의 정확한 보정을 위해서 스크롤바 폭만큼 더해줘야 함.
            panel_TimeEdit.Width = panel_Ruler.Width + SystemInformation.VerticalScrollBarWidth + 2;

            btnGoFirst.Click += new EventHandler(btnGoFirst_Click);
            btnOneStepPrev.Click += new EventHandler(btnOneStepPrev_Click);
            btnPlayStop.Click += new EventHandler(btnPlayStop_Click);
            btnPlay.Click += new EventHandler(btnPlay_Click);
            btnOneStepNext.Click += new EventHandler(btnOneStepNext_Click);
            btnGoLast.Click += new EventHandler(btnGoLast_Click);
            btnGoReverse.Click += new EventHandler(btnGoReverse_Click);
        }

        private void TimeInit()
        {
            nowTimeLine_picboxRuler = new TimeLine(new Point(boxGapWidth, rulerPicBoxGapHeight), new Point(boxGapWidth, picBox_Ruler.Height));
            nowTimeLine_picboxTimeEdit = new TimeLine(new Point(boxGapWidth, 0), new Point(boxGapWidth, picBox_TimeEdit.Height));

            timer.Interval = timePerFrame; // 0.1초
            timer.Tick += new EventHandler(timer_Tick);

            //trackBar1.Width = (tickWidth * 10) * totalTime; // 정확하진 않지만 트랙바의 Width로 전체 시간 세팅

            // 전체 틱 갯수 = 시간 * 초당 프레임갯수 * 프레임당 틱갯수 (맨 마지막거 그릴려면 1개 더)
            int totalTickCount = (totalTime * framePerSec * tickPerFrame) + 1;

            // 최대 인덱스 값
            maxIdx = totalTickCount - 1;

            // 픽처박스 넓이 = 양쪽 갭 길이 + 최대 인덱스 * 틱 폭
            picBox_TimeEdit.Width = (boxGapWidth * 2) + (maxIdx * tickWidth);
            picBox_Ruler.Width = picBox_TimeEdit.Width;

            // 수동 스크롤된후 자동으로 화면이동시 중앙으로 비슷하게 이동하기 위해
            //int tickCountPerPanel = ((panel_TimeEdit.Width - (boxGapWidth * 2)) / tickWidth);   // 한 화면당 나타낼 틱갯수 
            int tickCountPerPanel = (panel_TimeEdit.Width - boxGapWidth) / tickWidth;   // 한 화면당 나타낼 틱갯수 

            // 판넬당 표시되는 최대 인덱스 값
            maxIdxPerPanel = tickCountPerPanel - 1;

            // 판넬의 중앙 인덱스
            centerOffsetIdx = maxIdxPerPanel / 2;

            // 최대오프셋값 = (전체 인덱스 - 판넬당 최대인덱스) 
            int maxOffsetZeroStartIdx = maxIdx - maxIdxPerPanel;

            // 센터 적용된 최대오프셋값 = 최대오프셋값 - 오른쪽에서 떨어질 간격
            maxOffsetIdxWithCenter = maxOffsetZeroStartIdx - centerOffsetIdx;
            //minOffsetIdxWithCenter = 0;

            panel2.HorizontalScroll.SmallChange = tickWidth;    // 양 사이드의 화살표 누를때 움직일 값
            panel2.HorizontalScroll.LargeChange = tickWidth;    // 썸네일 밖에 영역을 누를때 움직일 값

            // 화면중앙부터 스크롤 되게 하려는 경계값
            currOffsetIdx = 0;
            
            HorizontalTickInit();
        }

        private void HorizontalTickInit()
        {
            // 맨 마지막 눈금까지 그릴려면 + 1
            for (int i = 0; i < maxIdx + 1; i++)
            {
                int x = boxGapWidth + (i * tickWidth);

                int hy = rulerPicBoxGapHeight + halfFrameHeight;
                int fy = rulerPicBoxGapHeight + fullFrameHeight;

                int y;
                if ((i % 2) == 0)
                    y = fy;
                else
                    y = hy;

                TimeLine tickObj = new TimeLine(new Point(x, rulerPicBoxGapHeight), new Point(x, y));
                drawTicks.Add(tickObj);
            }
        }

        



        private void picBox_TimeEdit_Paint(object sender, PaintEventArgs e)
        {
            //Bitmap bitmap = new Bitmap(this.Width, this.Height);
            Bitmap bitmap = new Bitmap(picBox_TimeEdit.Width, picBox_TimeEdit.Height);
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
            memGraphics.DrawLine(Pens.Red, nowTimeLine_picboxTimeEdit.SPos, nowTimeLine_picboxTimeEdit.EPos);

            e.Graphics.DrawImageUnscaled(bitmap, 0, 0);

            memGraphics.Dispose();
            bitmap.Dispose();
        }

        private void picBox_Ruler_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bitmap = new Bitmap(picBox_Ruler.Width, picBox_Ruler.Height);
            Graphics memGraphics = Graphics.FromImage(bitmap);
            memGraphics.Clear(this.BackColor);

            // 눈금그리기
            for (int i = 0; i < drawTicks.Count; i++)
            {
                TimeLine lineObj = drawTicks[i];
                memGraphics.DrawLine(Pens.Red, lineObj.SPos, lineObj.EPos);

                if ((i % 10) == 0)
                {
                    memGraphics.DrawString((i / 10).ToString(), new Font("Arial", 8), Brushes.Black, new Point(lineObj.SPos.X - 4, lineObj.SPos.Y - 15));
                }
            }

            // 현재 타임라인 세로선
            memGraphics.DrawLine(Pens.Red, nowTimeLine_picboxRuler.SPos, nowTimeLine_picboxRuler.EPos);

            e.Graphics.DrawImageUnscaled(bitmap, 0, 0);

            memGraphics.Dispose();
            bitmap.Dispose();
        }



        private void picBox_TimeEdit_MouseClick(object sender, MouseEventArgs e)
        {
            picBox_TimeEdit.Refresh();

            if (e.Button == MouseButtons.Right)
            {
                // 컨텍스트 메뉴 호출을 위한 작업
                foreach (TimeLineObject obj in lstTimeLineObj)
                {
                    selectedTimeLineObj = obj.CheckIconPos(new Point(e.X, e.Y));
                    if (selectedTimeLineObj == null) continue;
                    break;
                }

                if (selectedTimeLineObj != null)
                {
                    if (selectedTimeLineObj.objType == TIMEOBJTYPE.END)
                    {
                        //contextMenuStrip1.Show(picBox_TimeEdit, e.Location);
                        contextMenu1.Show(picBox_TimeEdit, e.Location);
                    }
                }
            }

            //if (selectedTimeLineObj == null) Console.WriteLine("selectedTimeLineObj is NULL!!!");
        }


        private void picBox_TimeEdit_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                selectedTimeLineObj = obj.CheckIconPos(new Point(e.X, e.Y));
                if (selectedTimeLineObj == null) continue;
                break;
            }

            if (selectedTimeLineObj != null)
            {
                DataGridViewRow row = dGrid_TimeLineObj.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells["name"].Value.ToString().Equals(selectedTimeLineObj.Name))
                        .First();

                dGrid_TimeLineObj.Rows[row.Index].Selected = true;
                dGrid_TimeLineObj.CurrentCell = dGrid_TimeLineObj.Rows[row.Index].Cells[0];

                if (selectedTimeLineObj.objType == TIMEOBJTYPE.BODY)
                {
                    s_offset = e.X - selectedTimeLineObj.SPos.X;
                    //e_offset = selectedTimeLineObj.EPos.X - e.X;
                    e_offset = (selectedTimeLineObj.EPos.X + selectedTimeLineObj.EIcon.Width) - e.X;
                }

                //int convEIdx1 = ConvPointToTickIdx(new Point(e.X, e.Y));
                //int convEIdx2 = ConvPointToTickIdx(new Point(e.X, e.Y), TIMEOBJTYPE.END, 16);


                //Console.WriteLine("{0}, {1}", convEIdx1, 0);
                //Console.WriteLine("type={0}", selectedTimeLineObj.objType);
                //Console.WriteLine("dGrid_TimeLineObj VerticalScrollingOffset:{0}, RowHeight:{1}", dGrid_TimeLineObj.VerticalScrollingOffset, dGrid_TimeLineObj.RowTemplate.Height);
                //Console.WriteLine("{0} ({1},{2}) AutoScroll:{3}", selectedTimeLineObj.Name, e.X, e.Y, panel_TimeEdit.AutoScrollPosition);
            }

            picBox_TimeEdit.Invalidate();
        }

        private void picBox_Ruler_MouseDown(object sender, MouseEventArgs e)
        {
            // 세로 타임라인 조정
            currIdx = ConvPointToTickIdx(e.Location);

            MoveToTimeLine(ConvTickIdxToPoint(currIdx, 0).X);

            // 해당 틱으로 이동과 스크롤처리
            MoveToTickIdxAndAutoScroll(currIdx);


            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void picBox_TimeEdit_MouseMove(object sender, MouseEventArgs e)
        {
            //if (mouseClicked)
            if (e.Button == MouseButtons.Left)
            {
                if (selectedTimeLineObj == null)
                    return;

                if (CheckPanelBound(new Point(e.X, 0), selectedTimeLineObj) == false)
                    return;

                if (selectedTimeLineObj.objType == TIMEOBJTYPE.START) // 시작점 움직이기
                {
                    int convIdx = ConvPointToTickIdx(new Point(e.X, e.Y));
                    selectedTimeLineObj.SPos = ConvTickIdxToPoint(convIdx, selectedTimeLineObj.SPos.Y);
                    selectedTimeLineObj.STick = convIdx;
                }
                else if (selectedTimeLineObj.objType == TIMEOBJTYPE.END) // 끝점 움직이기
                {
                    int convIdx = ConvPointToTickIdx(new Point(e.X, e.Y));
                    Point convPos = ConvTickIdxToPoint(convIdx, e.Y);
                    int convX = selectedTimeLineObj.ConvEndPosStart(convPos.X, tickWidth);
                    selectedTimeLineObj.EPos = new Point(convX, selectedTimeLineObj.EPos.Y);
                    selectedTimeLineObj.ETick = convIdx;
                }
                // 몸통을 잡고 움직이는 것는 클릭위치와 끝점간의 계산때문에 (나누기 연산의 오차)
                // 틱 간격이 작을때 안맞을수 있음.
                // 몸통을 잡고 대략 움직이고 끝점으로 정확히 맞추는걸 추천
                else // 몸통 움직이기
                {
                    int calSX = e.X - s_offset;
                    int calEX = e.X + e_offset;
                    int convSIdx = ConvPointToTickIdx(new Point(calSX, e.Y));
                    int convEIdx = ConvPointToTickIdx(new Point(calEX, e.Y));
                    Point convSPos = ConvTickIdxToPoint(convSIdx, selectedTimeLineObj.EPos.Y);
                    Point convEPos = ConvTickIdxToPoint(convEIdx, selectedTimeLineObj.EPos.Y);

                    int convEPos2 = selectedTimeLineObj.ConvEndPosStart(convEPos.X, tickWidth);

                    selectedTimeLineObj.SPos = convSPos;
                    selectedTimeLineObj.EPos = new Point(convEPos2, convEPos.Y);

                    selectedTimeLineObj.STick = convSIdx;
                    selectedTimeLineObj.ETick = convEIdx;
                }

                picBox_TimeEdit.Refresh();
            }
        }

        private void picBox_TimeEdit_MouseUp(object sender, MouseEventArgs e)
        {
            // 좌표를 시간으로 변환하는 계산이 들어가야 함.
            s_offset = -1;
            e_offset = -1;

            if (selectedTimeLineObj != null)
                Console.WriteLine("S:{0}, E:{1}", selectedTimeLineObj.STick, selectedTimeLineObj.ETick);
        }

        // 이 함수는 타임라인 오브젝트에서 체크해야 할듯
        private bool CheckPanelBound(Point p, TimeLineObject obj)
        {
            int nEnd_Right = p.X + obj.EIcon.Width;
            int nEnd_Left = p.X;

            // 맨 오른쪽 오버조건
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
                panel_Ruler.AutoScrollPosition = new Point(panel_TimeEdit.HorizontalScroll.Value, panel_Ruler.VerticalScroll.Value);
            }

            // 오브젝트가 왼쪽 가장자리에 닿았을때
            if (nEnd_Left < nPanelLeft)
            {
                int x = nEnd_Left;
                if (x <= panel_TimeEdit.HorizontalScroll.Minimum)
                    x = panel_TimeEdit.HorizontalScroll.Minimum;

                panel_TimeEdit.HorizontalScroll.Value = x;
                panel_Ruler.AutoScrollPosition = new Point(panel_TimeEdit.HorizontalScroll.Value, panel_Ruler.VerticalScroll.Value);
            }

            //Console.WriteLine(string.Format("panel_TimeEdit.AutoScrollPosition : {0}", panel_TimeEdit.AutoScrollPosition));

            return true;
        }












        private void btnGoFirst_Click(object sender, EventArgs e)
        {
            nowTimeLine_picboxTimeEdit.SPos = new Point(boxGapWidth, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(boxGapWidth, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(boxGapWidth, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(boxGapWidth, nowTimeLine_picboxRuler.EPos.Y);

            int y = panel_TimeEdit.AutoScrollPosition.Y;
            panel_TimeEdit.AutoScrollPosition = new Point(0, y);
            panel_Ruler.AutoScrollPosition = new Point(0, y);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void btnOneStepPrev_Click(object sender, EventArgs e)
        {
            if (currIdx <= 0) return;

            currIdx -= 1;

            nowTimeLine_picboxTimeEdit.SPos = new Point(nowTimeLine_picboxTimeEdit.SPos.X - tickWidth, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(nowTimeLine_picboxTimeEdit.EPos.X - tickWidth, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(nowTimeLine_picboxRuler.SPos.X - tickWidth, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(nowTimeLine_picboxRuler.EPos.X - tickWidth, nowTimeLine_picboxRuler.EPos.Y);

            int offsetIdxZeroStart = currIdx - maxIdxPerPanel;
            if (offsetIdxZeroStart < currOffsetIdx)
            {
                currOffsetIdx -= 1;
                if (currOffsetIdx <= 0)
                    currOffsetIdx = 0;

                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                panel_Ruler.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("     1 스크롤 발생! panel2.HorizontalScroll.Value:{0}", panel2.HorizontalScroll.Value);
            }

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void btnOneStepNext_Click(object sender, EventArgs e)
        {
            if (currIdx >= maxIdx) return;

            currIdx += 1;

            nowTimeLine_picboxTimeEdit.SPos = new Point(nowTimeLine_picboxTimeEdit.SPos.X + tickWidth, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(nowTimeLine_picboxTimeEdit.EPos.X + tickWidth, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(nowTimeLine_picboxRuler.SPos.X + tickWidth, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(nowTimeLine_picboxRuler.EPos.X + tickWidth, nowTimeLine_picboxRuler.EPos.Y);

            Console.WriteLine("0 btnOneStepNext_Click currIdx {0}, maxIdx {1}, maxIdxPerPanel {2}, currOffsetIdx {3}, centerOffsetIdx:{4}, maxOffsetIdxWithCenter:{5}", currIdx, maxIdx, maxIdxPerPanel, currOffsetIdx, centerOffsetIdx, maxOffsetIdxWithCenter);

            //if (currIdx >= (maxIdxPerPanel + currOffsetIdx))
            if (currIdx >= centerOffsetIdx)
            {
                //if (currOffsetIdx < maxIdxPerPanel) // 현재오프셋값이 화면당 최대값에 도달하면 더이상 스크롤 안하기
                {
                    currOffsetIdx += 1;
                    int x = currOffsetIdx * tickWidth;
                    int y = panel_TimeEdit.AutoScrollPosition.Y;
                    panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                    panel_Ruler.AutoScrollPosition = new Point(x, y);
                    Console.WriteLine("스크롤 된후 currIdx {0} >= centerOffsetIdx {1}", currIdx, centerOffsetIdx);
                }
            }

            Console.WriteLine("1 btnOneStepNext_Click currIdx {0}, maxIdx {1}, maxIdxPerPanel {2}, currOffsetIdx {3}", currIdx, maxIdx, maxIdxPerPanel, currOffsetIdx);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void btnGoLast_Click(object sender, EventArgs e)
        {
            currIdx = maxIdx;

            Point pos = ConvTickIdxToPoint(maxIdx, 0);

            nowTimeLine_picboxTimeEdit.SPos = new Point(pos.X, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(pos.X, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(pos.X, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(pos.X, nowTimeLine_picboxRuler.EPos.Y);

            int y = panel_TimeEdit.AutoScrollPosition.Y;
            panel_TimeEdit.AutoScrollPosition = new Point(pos.X, y);
            panel_Ruler.AutoScrollPosition = new Point(pos.X, y);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void btnGoReverse_Click(object sender, EventArgs e)
        {
            btnGoFirst_Click(sender, e);
            btnPlay_Click(sender, e);
        }

        // 매 1초마다 Tick 이벤트 핸들러 실행
        void timer_Tick(object sender, EventArgs e)
        {
            currIdx += 1;

            if (currIdx >= maxIdx)
                timer.Stop();

            if (currIdx >= (maxIdxPerPanel + currOffsetIdx))
            {
                // 자동 스크롤이 되어야 함.
                currOffsetIdx += 1;
                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                panel_Ruler.AutoScrollPosition = new Point(x, y);
            }

            nowTimeLine_picboxTimeEdit.SPos = new Point(nowTimeLine_picboxTimeEdit.SPos.X + tickWidth, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(nowTimeLine_picboxTimeEdit.EPos.X + tickWidth, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(nowTimeLine_picboxRuler.SPos.X + tickWidth, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(nowTimeLine_picboxRuler.EPos.X + tickWidth, nowTimeLine_picboxRuler.EPos.Y);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void dGrid_TimeLineObj_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                //Console.WriteLine("dGrid_TimeLineObj VerticalScrollingOffset:{0}, RowHeight:{1}", dGrid_TimeLineObj.VerticalScrollingOffset, dGrid_TimeLineObj.RowTemplate.Height);
                //Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
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

            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                currOffsetIdx = panel_TimeEdit.HorizontalScroll.Value / tickWidth;
                panel_Ruler.AutoScrollPosition = new Point(panel_TimeEdit.HorizontalScroll.Value, panel_TimeEdit.VerticalScroll.Value);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //Console.WriteLine("panel_TimeEdit.Height {0}", panel_TimeEdit.Height);
            picBox_TimeEdit.Height = panel_TimeEdit.Height;

            nowTimeLine_picboxTimeEdit = new TimeLine(nowTimeLine_picboxTimeEdit.SPos, new Point(nowTimeLine_picboxTimeEdit.EPos.X, panel_TimeEdit.Height));
            nowTimeLine_picboxRuler = new TimeLine(nowTimeLine_picboxRuler.SPos, new Point(nowTimeLine_picboxRuler.EPos.X, panel_Ruler.Height));
        }




        private void dGrid_TimeLineObj_MouseClick(object sender, MouseEventArgs e)
        {
            if (dGrid_TimeLineObj.CurrentCell == null)
                return;

            int rowIdx = dGrid_TimeLineObj.CurrentCell.RowIndex;
            if (rowIdx == -1 || rowIdx > (lstTimeLineObj.Count - 1))
                return;

            selectedTimeLineObj = lstTimeLineObj[rowIdx];
            if (selectedTimeLineObj == null)
                return;

            //Console.WriteLine("lstTimeLineObj:{0}, selectedTimeLineObj:{1}", lstTimeLineObj[rowIdx].Name, selectedTimeLineObj.Name);

            if (e.Button == MouseButtons.Right)
            {
                int a = 0;

                // 컨텍스트 메뉴 호출을 위한 작업
                if (selectedTimeLineObj != null)
                {
                    //if (selectedTimeLineObj.objType == TIMEOBJTYPE.END)
                    {
                        //contextMenuStrip1.Show(dGrid_TimeLineObj, e.Location);
                        contextMenu1.Show(dGrid_TimeLineObj, e.Location);
                    }
                }
            }
        }

        // 값이 바뀌고 난후에 들어오는 이벤트...
        private void dGrid_TimeLineObj_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if (e.ColumnIndex == 0)
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

                if (colIdx == 2) // View
                {
                    lstTimeLineObj[rowIdx].View = (Boolean)cell.Value;
                }
                //Console.WriteLine("1 {0}, Lock:{1}, View:{2}", lstTimeLineObj[rowIdx].Name, lstTimeLineObj[rowIdx].Lock, lstTimeLineObj[rowIdx].View);
            }
        }

        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedTimeLineObj != null)
                Console.WriteLine("{0}", selectedTimeLineObj.Name);

            // 데이터 처리
            int index = lstTimeLineObj.FindIndex(r => r.Name.Equals(selectedTimeLineObj.Name));
            lstTimeLineObj.RemoveAt(index);

            // 왼쪽 판넬 처리
            Console.WriteLine("dGrid_TimeLineObj.CurrentCell.RowIndex : {0}", dGrid_TimeLineObj.CurrentCell.RowIndex);
            dGrid_TimeLineObj.Rows.RemoveAt(dGrid_TimeLineObj.CurrentCell.RowIndex);

            int tempHeight = 0;
            tempHeight += timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
            // 타임 에디터 갱신처리
            for (int i = 0; i < lstTimeLineObj.Count; i++)
            {
                TimeLineObject obj = lstTimeLineObj[i];
                int y = (i * dGrid_TimeLineObj.RowTemplate.Height) + timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;

                obj.SPos = new Point(obj.SPos.X, y);
                obj.EPos = new Point(obj.EPos.X, y);

                tempHeight += dGrid_TimeLineObj.RowTemplate.Height;
            }

            if (tempHeight <= picBox_TimeEdit_MinHeight)
                picBox_TimeEdit.Height = picBox_TimeEdit_MinHeight;
            else
                picBox_TimeEdit.Height = tempHeight;

            // 하나 빠질때마다 높이 계산

            picBox_TimeEdit.Invalidate();
        }

        private void stopAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedTimeLineObj.aniType = ANITYPE.STOP_AT_END;

            Console.WriteLine("1 {0}, Lock:{1}, View:{2}, AniType:{3}",
                                selectedTimeLineObj.Name,
                                selectedTimeLineObj.Lock,
                                selectedTimeLineObj.View,
                                selectedTimeLineObj.aniType);
        }

        private void replayAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedTimeLineObj.aniType = ANITYPE.REPLAY_AT_END;

            Console.WriteLine("1 {0}, Lock:{1}, View:{2}, AniType:{3}",
                                selectedTimeLineObj.Name,
                                selectedTimeLineObj.Lock,
                                selectedTimeLineObj.View,
                                selectedTimeLineObj.aniType);
        }

        // 룰러에서만 정확히 써야 하는 함수
        // 클릭한 점에서부터 현재 프레임 위치 얻어내기
        // tickWidth가 작으면 연산이 정확히 안됨
        private int ConvPointToTickIdx(Point p)
        {
            int clickTickIdx = (p.X - boxGapWidth) / tickWidth;
            return clickTickIdx;
        }

        // 가로 라인 전용
        private Point ConvTickIdxToPoint(int idx, int y)
        {
            int x = (idx * tickWidth) + boxGapWidth;
            return new Point(x, y);
        }

        private void MoveToTimeLine(int x)
        {
            nowTimeLine_picboxTimeEdit.SPos = new Point(x, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(x, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(x, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(x, nowTimeLine_picboxRuler.EPos.Y);
        }

        private void MoveToTickIdxAndAutoScroll(int clickIdx)
        {
            // 임의의 위치가 원점부터 얼만큼 떨어져있는지 (판넬당 틱 갯수)
            int offsetIdxZeroStart = clickIdx - maxIdxPerPanel;

            //Console.WriteLine("0 offsetIdxZeroStart {0} currOffsetIdx {1}, maxOffsetIdxWithCenter {2} minOffsetIdxWithCenter {3}", offsetIdxZeroStart, currOffsetIdx, maxOffsetIdxWithCenter, minOffsetIdxWithCenter);

            // 어딘가 다른데를 클릭했다면
            if (offsetIdxZeroStart != currOffsetIdx)
            {
                if (offsetIdxZeroStart < 0) // 기준 위치 이하인덱스를 얻었다면
                    currOffsetIdx = 0;
                else if (offsetIdxZeroStart > maxOffsetIdxWithCenter)
                    currOffsetIdx = maxOffsetIdxWithCenter;
                else
                    currOffsetIdx = offsetIdxZeroStart;

                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                panel_Ruler.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("             1 스크롤 발생 clickTickIdx {0}, OffsetIdxZeroStart {1}, currOffsetIdx {2}", clickTickIdx, offsetIdxZeroStart, currOffsetIdx);
            }

            Console.WriteLine("1 OffsetIdxZeroStart {0} currOffsetIdx {1}", offsetIdxZeroStart, currOffsetIdx);
        }

        private void picBox_View_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
                TimeLineObject obj = new TimeLineObject(new Point(boxGapWidth, y), new Point(boxGapWidth, y), tickWidth);
                obj.Name = string.Format("test{0}", lstTimeLineObj.Count);
                obj.Lock = false;
                obj.View = true;
                lstTimeLineObj.Add(obj);

                Console.WriteLine(string.Format("{0} ({1},{2}) Create", obj.Name, obj.SPos, obj.EPos));

                // 좌측 판넬 세팅
                dGrid_TimeLineObj.Rows.Add(obj.Name, obj.Lock, obj.View);
                dGrid_TimeLineObjHeight += dGrid_TimeLineObj.RowTemplate.Height;

                // 우측 판넬 세팅
                if (dGrid_TimeLineObjHeight > picBox_TimeEdit.Height)
                    picBox_TimeEdit.Height = dGrid_TimeLineObjHeight;
                picBox_TimeEdit.Invalidate();
            }

            Console.WriteLine("{0}, {1}, {2}", dGrid_TimeLineObj.Height, picBox_TimeEdit.Height, dGrid_TimeLineObjHeight);


            // 맨 마지막 요소 선택안할걸로 처리
            dGrid_TimeLineObj.Rows[dGrid_TimeLineObj.RowCount - 1].Selected = false;
        }
    }
}
