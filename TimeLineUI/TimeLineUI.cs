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
        private Timer timer = new Timer();

        private ContextMenu contextMenu1;

        int s_offset = -1; // 클릭한 위치에서 시작점 좌측면까지 픽셀 거리 (보디를 구하기 위해)
        int e_offset = -1; // 클릭한 위치에서 끝점 우측면까지 픽셀 거리 (보디를 구하기 위해)

        const int dataGridViewColumn0_Width = 250;  // 스크롤바 발생시 이 값에서 빼서 쓴다.


        // 2틱당 1프레임
        // 5프레임당 1초
        const int timePerFrame = 100;           // 프레임당 시간 (0.1초)
        const int tickPerFrame = 2;             // 프레임당 틱갯수
        const int framePerSec = 5;              // 초당 5프레임
        const int tickWidth = 5;               // 이 간격을 0.1초로 계산
        const int totalTime = 600;                // 전체 작업시간(초)
        

        // 눈금용 데이터들
        const int boxGapWidth = 15;             // 픽쳐박스 왼쪽에서 시작위치까지의 간격 (눈금자, 타임에디터 공용)
        const int rulerPicBoxGapHeight = 15;    // 픽쳐박스 위에서 시작위치까지의 간격 (눈금자용)
        const int timeEditPicBoxGapHeight = 0;  // 픽쳐박스 위에서 시작위치까지의 간격 (타임에디터용)
        const int timeEditPicBox_ObjectGap = 5; // 타임에디터에서 오브젝트간에 높이갭 
        const int halfFrameHeight = 5;          // 0.5 프레임당 눈금길이
        const int fullFrameHeight = 12;         // 1 프레임당 눈금길이

        int maxIdx = 0;                         // 최대 틱 인덱스
        int maxIdxPerPanel = 0;                 // 한 판넬에 표시하는 최대 틱 인덱스
        int currIdx = 0;                        // 현재 틱 위치 인덱스
        int currOffsetIdx = 0;                  // 시작위치에서 얼마큼 떨어져 있는지 
        int maxOffsetIdx_FromZero = 0;          // 시작위치에서부터의 최대 오프셋값
        int offsetStartIdx_FromZero = 0;        // 스크롤이 시작되기 위한 틱 시작값(계산해야함)
        int offsetStart_FromLast = 0;           // 스크롤이 시작되기 위한 틱 시작값(계산해야함)

        int dGrid_TimeLineObjHeight = 0;
        int picBox_TimeEdit_MinHeight = 0;      // 타임에디터 최소 높이

        // 그리드 뷰가 한행 만큼 더 만들어 지므로 오른쪽 픽쳐박스는 한 행 높이 만큼 더하는데 
        // 픽쳐박스는 가로 스크롤뷰가 있으므로 그 높이 만큼 빼면 스크롤이 딱 맞게 됨
        int picBox_TimeEdit_TrimH = 0;          // 픽쳐박스 스크롤보정값 

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

        private void TimeLineUI_Load(object sender, EventArgs e)
        {
            ControlInit();
            TimeInit();
        }

        public TimeLineUI()
        {
            InitializeComponent();
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
            dGrid_TimeLineObj.CellValueChanged += new DataGridViewCellEventHandler(dGrid_TimeLineObj_CellValueChanged);
            dGrid_TimeLineObj.SelectionChanged += new EventHandler(dGrid_TimeLineObj_SelectionChanged);
            dGrid_TimeLineObj.Scroll += new ScrollEventHandler(this.dGrid_TimeLineObj_Scroll);
            dGrid_TimeLineObj.SizeChanged += new EventHandler(this.dGrid_TimeLineObj_SizeChanged);
            dGrid_TimeLineObj.DataSource = null;
            dGrid_TimeLineObj.Rows.Clear();
            dGrid_TimeLineObj.AllowUserToAddRows = false;

            picBox_Ruler.Height = panel_Ruler.Height;
            picBox_Ruler.Paint += new PaintEventHandler(picBox_Ruler_Paint);
            picBox_Ruler.MouseDown += new MouseEventHandler(picBox_Ruler_MouseDown);

            // 눈금판넬 스크롤바 없애기
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
            panel_TimeEdit.HorizontalScroll.SmallChange = tickWidth;    // 양 사이드의 화살표 누를때
            panel_TimeEdit.HorizontalScroll.LargeChange = tickWidth;    // 썸네일 밖에 영역을 누를때
            panel_TimeEdit.VerticalScroll.SmallChange = 23;     // 양 사이드의 화살표 누를때
            panel_TimeEdit.VerticalScroll.LargeChange = 23;     // 썸네일 밖에 영역을 누를때

            // 타임에디터 판넬에 스크롤바가 발생하므로 룰러 클릭영역의 정확한 보정을 위해서 스크롤바 폭만큼 더해줘야 함.
            panel_TimeEdit.Width = panel_Ruler.Width + SystemInformation.VerticalScrollBarWidth + 2;

            // 그리드 뷰가 한행 만큼 더 만들어 지므로 오른쪽 픽쳐박스는 한 행 높이 만큼 더하는데 
            // 픽쳐박스는 가로 스크롤뷰가 있으므로 그 높이 만큼 빼면 스크롤이 딱 맞게 됨
            // 컨트롤끼리의 높이차 보정을 따로 해야 함. (- 12)
            picBox_TimeEdit_TrimH = dGrid_TimeLineObj.RowTemplate.Height - SystemInformation.HorizontalScrollBarHeight - 0;

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

            // 전체 틱 갯수 = 시간 * 초당 프레임갯수 * 프레임당 틱갯수 (경계조건때문에 + 1)
            int totalTickCount = (totalTime * framePerSec * tickPerFrame) + 1;

            // 최대 인덱스 값
            maxIdx = totalTickCount - 1;

            // 픽처박스 넓이 = 양쪽 갭 길이 + 최대 인덱스 * 틱 폭
            picBox_TimeEdit.Width = (boxGapWidth * 2) + (maxIdx * tickWidth);
            picBox_Ruler.Width = picBox_TimeEdit.Width;

            int calWidth = panel_Ruler.Width - boxGapWidth;         // 타임에디터 픽쳐박스의 세로스크롤바의 폭이 빠졌는지 체크
            int tickCountPerPanel = (calWidth / tickWidth) + 1;     // 한 화면당 나타낼 틱갯수 (경계조건때문에 + 1)

            // 판넬당 표시되는 최대 인덱스 값
            maxIdxPerPanel = tickCountPerPanel - 1;

            // 최소 시작 인덱스 ( 임의의 값으로 세팅할수 있음 하지만 될수있는한 안하는게 좋음. )
            offsetStartIdx_FromZero = (maxIdxPerPanel / 2);// + 10;         // 보기 좋은 중앙 보정값( +2 )
            offsetStart_FromLast = maxIdx - offsetStartIdx_FromZero;

            // 최대 오프셋값 = (전체 인덱스 - 판넬당 최대인덱스) 
            maxOffsetIdx_FromZero = maxIdx - maxIdxPerPanel;
            
            
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
                        contextMenu1.Show(picBox_TimeEdit, e.Location);
                    }
                }
            }
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
                    e_offset = (selectedTimeLineObj.EPos.X + selectedTimeLineObj.EIcon.Width) - e.X;
                }
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
                // 아이폰 크기가 틱 간격보다 작을때 안맞을수 있음.
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

            //if (selectedTimeLineObj != null)
            //    Console.WriteLine("S:{0}, E:{1}", selectedTimeLineObj.STick, selectedTimeLineObj.ETick);
        }

        // 이 함수는 타임라인 오브젝트에서 체크해야 할듯
        private bool CheckPanelBound(Point p, TimeLineObject obj)
        {
            int nEnd_Right = p.X + obj.EIcon.Width;
            int nEnd_Left = p.X;

            // 맨 오른쪽 오버조건
            if (nEnd_Right > picBox_TimeEdit.Width) return false;


            // 맨 왼쪽 오버조건
            if (nEnd_Left <= boxGapWidth) return false;


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
            currIdx = 0;
            currOffsetIdx = 0;

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
            
            // Max 영역을 넘어선 영역에서 작동시 - 우측에서부터 제로로 생각해야 함
            if (currIdx < offsetStart_FromLast)
            {
                currOffsetIdx -= 1;
                if (currOffsetIdx <= 0) // 현재오프셋값이 최소값에 도달하면 더이상 스크롤 안하기
                {
                    currOffsetIdx = 0;
                }

                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                panel_Ruler.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("스크롤 된후 currIdx {0} >= offsetStartIdx_FromZero {1} ScrollPos {2}", currIdx, offsetStartIdx_FromZero, panel_TimeEdit.AutoScrollPosition);
            }

            //Console.WriteLine("prev 1 btnOneStepPrev_Click currIdx {0}, maxIdx {1}, maxIdxPerPanel {2}, currOffsetIdx {3}", currIdx, maxIdx, maxIdxPerPanel, currOffsetIdx);

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

            //Console.WriteLine("Next 0 btnOneStepNext_Click currIdx {0}, maxIdx {1}, maxIdxPerPanel {2}, currOffsetIdx {3}, minOffsetZeroStartIdx:{4}, maxOffsetIdxWithCenter:{5}", currIdx, maxIdx, maxIdxPerPanel, currOffsetIdx, minOffsetZeroStartIdx, maxOffsetIdxWithCenter);

            if (currIdx >= offsetStartIdx_FromZero)
            {
                if (currOffsetIdx < maxOffsetIdx_FromZero) // 현재오프셋값이 최대값에 도달하면 더이상 스크롤 안하기
                {
                    currOffsetIdx += 1;
                    int x = currOffsetIdx * tickWidth;
                    int y = panel_TimeEdit.AutoScrollPosition.Y;
                    panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                    panel_Ruler.AutoScrollPosition = new Point(x, y);
                    //Console.WriteLine("스크롤 된후 currIdx {0} >= offsetStartIdx_FromZero {1}, currOffsetIdx{2}, ScrollPos {2}", currIdx, offsetStartIdx_FromZero, currOffsetIdx, panel_TimeEdit.AutoScrollPosition);
                }
            }

            //Console.WriteLine("Next 1 btnOneStepNext_Click currIdx {0}, maxIdx {1}, maxIdxPerPanel {2}, currOffsetIdx {3}", currIdx, maxIdx, maxIdxPerPanel, currOffsetIdx);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }

        private void btnGoLast_Click(object sender, EventArgs e)
        {
            currIdx = maxIdx;
            currOffsetIdx = maxOffsetIdx_FromZero;

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

            if (currIdx >= offsetStartIdx_FromZero)
            {
                if (currOffsetIdx < maxOffsetIdx_FromZero) // 현재오프셋값이 최대값에 도달하면 더이상 스크롤 안하기
                {
                    // 자동 스크롤이 되어야 함.
                    currOffsetIdx += 1;
                    int x = currOffsetIdx * tickWidth;
                    int y = panel_TimeEdit.AutoScrollPosition.Y;
                    panel_TimeEdit.AutoScrollPosition = new Point(x, y);
                    panel_Ruler.AutoScrollPosition = new Point(x, y);
                }
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
                //Console.WriteLine("1 panel_TimeEdit AutoScroll:{0}, VV:{1}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value);
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
            nowTimeLine_picboxTimeEdit = new TimeLine(nowTimeLine_picboxTimeEdit.SPos, new Point(nowTimeLine_picboxTimeEdit.EPos.X, panel_TimeEdit.Height));
            nowTimeLine_picboxRuler = new TimeLine(nowTimeLine_picboxRuler.SPos, new Point(nowTimeLine_picboxRuler.EPos.X, panel_Ruler.Height));
        }

        private void dGrid_TimeLineObj_MouseClick(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo info = dGrid_TimeLineObj.HitTest(e.Location.X, e.Location.Y);

            if (info.RowIndex < 0 || info.ColumnIndex < 0)
                return;

            if (info.RowIndex > lstTimeLineObj.Count)
                return;

            selectedTimeLineObj = lstTimeLineObj[info.RowIndex];
            if (selectedTimeLineObj == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                // 컨텍스트 메뉴 호출을 위한 작업
                if (selectedTimeLineObj != null)
                {
                    dGrid_TimeLineObj.Rows[info.RowIndex].Selected = true;
                    dGrid_TimeLineObj.CurrentCell = dGrid_TimeLineObj.Rows[info.RowIndex].Cells[0];

                    //Console.WriteLine("{0} Selected, dGrid_TimeLineObj.CurrentCell.RowIndex {1}", selectedTimeLineObj.Name, dGrid_TimeLineObj.CurrentCell.RowIndex);
                    contextMenu1.Show(dGrid_TimeLineObj, e.Location);
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
            Console.WriteLine("Find Index {0}", index);
            lstTimeLineObj.RemoveAt(index);

            // 왼쪽 판넬 처리
            Console.WriteLine("dGrid_TimeLineObj.CurrentCell.RowIndex : {0}", dGrid_TimeLineObj.CurrentCell.RowIndex);
            dGrid_TimeLineObj.Rows.RemoveAt(dGrid_TimeLineObj.CurrentCell.RowIndex);
            if (dGrid_TimeLineObj.VerticalScrollbarVisible)
                dGrid_TimeLineObj.Columns[0].Width = dataGridViewColumn0_Width - SystemInformation.VerticalScrollBarWidth;
            else
                dGrid_TimeLineObj.Columns[0].Width = dataGridViewColumn0_Width;

            // 타임 에디터 갱신처리
            int tempHeight = 0;
            tempHeight += timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
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
            {
                picBox_TimeEdit.Height = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + picBox_TimeEdit_TrimH;
            }

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
            if (currIdx < offsetStartIdx_FromZero)
            {
                currOffsetIdx = 0;
            }
            else if (currIdx >= (maxOffsetIdx_FromZero + offsetStartIdx_FromZero))
            {
                currOffsetIdx = maxOffsetIdx_FromZero;
            }
            else
            {
                currOffsetIdx = (currIdx + 1) - offsetStartIdx_FromZero;
            }
            
            int x = currOffsetIdx * tickWidth;
            int y = panel_TimeEdit.AutoScrollPosition.Y;
            panel_TimeEdit.AutoScrollPosition = new Point(x, y);
            panel_Ruler.AutoScrollPosition = new Point(x, y);
            //Console.WriteLine("스크롤 된후 currIdx {0} >= offsetStartIdx_FromZero {1}, currOffsetIdx{2}, ScrollPos {2}", currIdx, offsetStartIdx_FromZero, currOffsetIdx, panel_TimeEdit.AutoScrollPosition);
            
            //Console.WriteLine("1 MoveToTickIdxAndAutoScroll maxIdx {0}, maxIdxPerPanel {1}, currOffsetIdx {2}, offsetStartIdx_FromZero:{3}", maxIdx, maxIdxPerPanel, currOffsetIdx, offsetStartIdx_FromZero);
        }

        

        public void AddTimeObj(string r_name, bool r_lock = false, bool r_view = true)
        {
            if (lstTimeLineObj.Count > 255)
                MessageBox.Show("작업 제한값을 넘었습니다. (255개)");

            int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
            TimeLineObject obj = new TimeLineObject(new Point(boxGapWidth, y), new Point(boxGapWidth, y), tickWidth);
            obj.Name = r_name;
            obj.Lock = r_lock;
            obj.View = r_view;
            lstTimeLineObj.Add(obj);

            // 좌측 판넬 세팅
            dGrid_TimeLineObj.Rows.Add(obj.Name, obj.Lock, obj.View);
            dGrid_TimeLineObjHeight += dGrid_TimeLineObj.RowTemplate.Height;
            if (dGrid_TimeLineObj.VerticalScrollbarVisible)
                dGrid_TimeLineObj.Columns[0].Width = dataGridViewColumn0_Width - SystemInformation.VerticalScrollBarWidth;
            else
                dGrid_TimeLineObj.Columns[0].Width = dataGridViewColumn0_Width;

            //Console.WriteLine("1 dGrid_TimeLineObj Height {0}, Count {1}, Row Height {2}, dGrid_TimeLineObjHeight {3}, SystemInformation.HorizontalScrollBarHeight {4}", dGrid_TimeLineObj.Height, lstTimeLineObj.Count, dGrid_TimeLineObj.RowTemplate.Height, dGrid_TimeLineObjHeight, SystemInformation.HorizontalScrollBarHeight);

            // 우측 판넬 세팅 
            
            int tempH = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + picBox_TimeEdit_TrimH;
            if (picBox_TimeEdit.Height <= picBox_TimeEdit_MinHeight)
                picBox_TimeEdit.Height = picBox_TimeEdit_MinHeight;
            else
                picBox_TimeEdit.Height = tempH;

            
        }

        private void panel_Total_Resize(object sender, EventArgs e)
        {
            int calWidth = panel_Ruler.Width - boxGapWidth;         // 타임에디터 픽쳐박스의 세로스크롤바의 폭이 빠졌는지 체크
            int tickCountPerPanel = (calWidth / tickWidth) + 1;     // 한 화면당 나타낼 틱갯수 (경계조건때문에 + 1)

            if (nowTimeLine_picboxTimeEdit != null)
            {
                nowTimeLine_picboxTimeEdit.SPos = new Point(nowTimeLine_picboxTimeEdit.SPos.X, 0);
                nowTimeLine_picboxTimeEdit.EPos = new Point(nowTimeLine_picboxTimeEdit.EPos.X, panel_TimeEdit.Height);
            }

            // 판넬당 표시되는 최대 인덱스 값
            maxIdxPerPanel = tickCountPerPanel - 1;

            // 최소 시작 인덱스 ( 임의의 값으로 세팅할수 있음 하지만 될수있는한 안하는게 좋음. )
            offsetStartIdx_FromZero = (maxIdxPerPanel / 2);// + 10;         // 보기 좋은 중앙 보정값( +2 )
            offsetStart_FromLast = maxIdx - offsetStartIdx_FromZero;

            // 최대 오프셋값 = (전체 인덱스 - 판넬당 최대인덱스) 
            maxOffsetIdx_FromZero = maxIdx - maxIdxPerPanel;
        }

        private void dGrid_TimeLineObj_SizeChanged(object sender, EventArgs e)
        {
            Console.WriteLine("SizeChanged dGrid_TimeLineObj.Height {0}, dGrid_TimeLineObjHeight {1}", dGrid_TimeLineObj.Height, dGrid_TimeLineObjHeight);
        }

        // 테스트용 임시 데이터 생성용
        private void picBox_View_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
                AddTimeObj(string.Format("test{0}", i), true, true);

            dGrid_TimeLineObj.ClearSelection();
            picBox_TimeEdit.Invalidate();
        }

        // 테스트용 데이터 초기화용
        private void picBox_Lock_Click(object sender, EventArgs e)
        {
            TimeLineUI_Reset();

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
            dGrid_TimeLineObj.Invalidate();
        }

        public void TimeLineUI_Reset()
        {
            lstTimeLineObj.Clear();

            // 좌측 판넬 초기화
            dGrid_TimeLineObj.DataSource = null;
            dGrid_TimeLineObj.Rows.Clear();
            dGrid_TimeLineObj.ClearSelection();

            // 우측 판넬 초기화
            picBox_TimeEdit.Height = picBox_TimeEdit_MinHeight;

            currIdx = 0;
            currOffsetIdx = 0;

            nowTimeLine_picboxTimeEdit.SPos = new Point(boxGapWidth, nowTimeLine_picboxTimeEdit.SPos.Y);
            nowTimeLine_picboxTimeEdit.EPos = new Point(boxGapWidth, nowTimeLine_picboxTimeEdit.EPos.Y);

            nowTimeLine_picboxRuler.SPos = new Point(boxGapWidth, nowTimeLine_picboxRuler.SPos.Y);
            nowTimeLine_picboxRuler.EPos = new Point(boxGapWidth, nowTimeLine_picboxRuler.EPos.Y);

            int y = panel_TimeEdit.AutoScrollPosition.Y;
            panel_TimeEdit.AutoScrollPosition = new Point(0, y);
            panel_Ruler.AutoScrollPosition = new Point(0, y);
        }
    }
}

