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
        private List<TimeLine> drawTicks = new List<TimeLine>();

        private TimeLine nowTimeLine = null;     // 타임 세로선
        private Timer timer = new System.Windows.Forms.Timer();

        int s_offset = -1; // 클릭한 위치에서 시작점까지 거리 (보디를 구하기 위해)
        int e_offset = -1; // 클릭한 위치에서 끝점까지 거리 (보디를 구하기 위해)

        // 2틱당 1프레임
        // 5프레임당 1초
        const int tickPerFrame = 2;             // 프레임당 틱갯수
        const int framePerSec = 5;              // 초당 5프레임
        const int tickWidth = 20;               // 이 간격을 0.1초로 계산
        const int totalTime = 4;                // 전체 작업시간(초)
        const int centerOffsetIdx = 8;          // 센터를 표시하기 위한 오른쪽에 떨어진 간격

        // 눈금용 데이터들
        const int boxGapWidth = 15;             // 박스 왼쪽에서 시작위치까지의 간격
        const int boxGapHeight = 15;            // 박스 위에서 시작위치까지의 간격
        const int halfFrameHeight = 5;          // 0.5 프레임당 눈금길이
        const int fullFrameHeight = 15;         // 1 프레임당 눈금길이

        int maxIdx = 0;                         // 최대 틱 인덱스
        int maxtIdxPerPanel = 0;                // 한 판넬에 표시하는 최대 틱 인덱스
        int currIdx = 0;                        // 현재 틱 위치 인덱스
        int currOffsetIdx = 0;                  // 시작위치에서 얼마큼 떨어져 있는지 
        int minOffsetIdxWithCenter = 0;         // 센터 적용된 최소 오프셋값
        int maxOffsetIdxWithCenter = 0;         // 센터 적용된 최대 오프셋값

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

        public MainForm()
        {
            InitializeComponent();
            ControlInit();
            TimeInit();

            picBox_TimeEdit.Invalidate();
        }

        private void ControlInit()
        {
            picBox_TimeEdit.Height = panel_TimeEdit.Height;
            //picBox_TimeEdit.Width = 30000;
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
        }

        private void TimeInit()
        {
            nowTimeLine = new TimeLine(new Point(boxGapWidth, boxGapHeight), new Point(boxGapHeight, picBox_TimeEdit.Height));
            

            timer.Interval = 100; // 0.1초
            timer.Tick += new EventHandler(timer_Tick);

            //trackBar1.Width = (tickWidth * 10) * totalTime; // 정확하진 않지만 트랙바의 Width로 전체 시간 세팅

            // 전체 틱 갯수 = 시간 * 초당 프레임갯수 * 프레임당 틱갯수 (맨 마지막거 그릴려면 1개 더)
            int totalTickCount = (totalTime * framePerSec * tickPerFrame) + 1;

            // 최대 인덱스 값
            maxIdx = totalTickCount - 1;

            // 픽처박스 넓이 = 양쪽 갭 길이 + 최대 인덱스 * 틱 폭
            picBox_TimeEdit.Width = (boxGapWidth * 2) + (maxIdx * tickWidth);
            
            // 수동 스크롤된후 자동으로 화면이동시 중앙으로 비슷하게 이동하기 위해
            int tickCountPerPanel = ((panel_TimeEdit.Width - (boxGapWidth * 2)) / tickWidth) - centerOffsetIdx;   // 한 화면당 나타낼 틱갯수 

            // 판넬당 표시되는 최대 인덱스 값
            maxtIdxPerPanel = tickCountPerPanel - 1;

            // 최대오프셋값 = (전체 인덱스 - 판넬당 최대인덱스) 
            int maxOffsetZeroStartIdx = maxIdx - maxtIdxPerPanel;

            // 센터 적용된 최대오프셋값 = 최대오프셋값 - 오른쪽에서 떨어질 간격
            maxOffsetIdxWithCenter = maxOffsetZeroStartIdx - centerOffsetIdx;
            minOffsetIdxWithCenter = 0;

            panel2.HorizontalScroll.SmallChange = tickWidth;    // 양 사이드의 화살표 누를때 움직일 값
            panel2.HorizontalScroll.LargeChange = tickWidth;    // 썸네일 밖에 영역을 누를때 움직일 값

            currOffsetIdx = 0;

            //pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_MouseClick);
            //pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
            //pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
            //pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);

            DrawHorizontalTick();
        }

        private void DrawHorizontalTick()
        {
            // 맨 마지막 눈금까지 그릴려면 + 1
            for (int i = 0; i < maxIdx + 1; i++)
            {
                int x = boxGapWidth + (i * tickWidth);

                int hy = boxGapHeight + halfFrameHeight;
                int fy = boxGapHeight + fullFrameHeight;

                int y;
                if ((i % 2) == 0)
                    y = fy;
                else
                    y = hy;

                TimeLine tickObj = new TimeLine(new Point(x, boxGapHeight), new Point(x, y));
                drawTicks.Add(tickObj);
            }
        }

        private void btnObjAdd_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("lstTimeLineObj.Count : {0}", lstTimeLineObj.Count);

            for (int i = 0; i < 5; i++)
            {
                int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + 5 + boxGapHeight + fullFrameHeight;
                TimeLineObject obj = new TimeLineObject(new Point(boxGapWidth, y), new Point(boxGapWidth, y));
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

            // 맨 마지막 요소 선택안할걸로 처리
            dGrid_TimeLineObj.Rows[dGrid_TimeLineObj.RowCount - 1].Selected = false;
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

            for (int i = 0; i < drawTicks.Count; i++)
            {
                TimeLine lineObj = drawTicks[i];
                memGraphics.DrawLine(Pens.Red, lineObj.SPos, lineObj.EPos);

                if ((i % 2) == 0)
                {
                    memGraphics.DrawString((i / 2).ToString(), new Font("Arial", 8), Brushes.Black, new Point(lineObj.SPos.X - 4, lineObj.SPos.Y - 15));
                }
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
                    //dGrid_TimeLineObj.SelectedRows.Select(r => r.Index);

                    

                    if (selectedTimeLineObj.objType == TIMEOBJTYPE.END)
                    {
                        contextMenuStrip1.Show(picBox_TimeEdit, e.Location);
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
                    e_offset = selectedTimeLineObj.EPos.X - e.X;
                }

                //Console.WriteLine("type={0}", selectedTimeLineObj.currType);
                //Console.WriteLine("dGrid_TimeLineObj VerticalScrollingOffset:{0}, RowHeight:{1}", dGrid_TimeLineObj.VerticalScrollingOffset, dGrid_TimeLineObj.RowTemplate.Height);
                //Console.WriteLine("{0} ({1},{2}) AutoScroll:{3}", selectedTimeLineObj.Name, e.X, e.Y, panel_TimeEdit.AutoScrollPosition);
            }

            ConvPointToFrame(e.Location);
            picBox_TimeEdit.Invalidate();
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
                

                if (selectedTimeLineObj.objType == TIMEOBJTYPE.START) // 시작점 움직이기
                {
                    selectedTimeLineObj.SPos = new Point(e.X, selectedTimeLineObj.SPos.Y);
                }
                else if (selectedTimeLineObj.objType == TIMEOBJTYPE.END) // 끝점 움직이기
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










        

        private void btnGoFirst_Click(object sender, EventArgs e)
        {
            nowTimeLine.SPos = new Point(boxGapWidth, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(boxGapWidth, nowTimeLine.EPos.Y);

            picBox_TimeEdit.Invalidate();
        }

        private void btnOneStepPrev_Click(object sender, EventArgs e)
        {
            if (currIdx <= 0) return;

            currIdx -= 1;

            nowTimeLine.SPos = new Point(nowTimeLine.SPos.X - tickWidth, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(nowTimeLine.EPos.X - tickWidth, nowTimeLine.EPos.Y);

            int offsetIdxZeroStart = currIdx - maxtIdxPerPanel;
            if (offsetIdxZeroStart < currOffsetIdx)
            {
                currOffsetIdx -= 1;
                if (currOffsetIdx <= 0)
                    currOffsetIdx = 0;

                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("     1 스크롤 발생! panel2.HorizontalScroll.Value:{0}", panel2.HorizontalScroll.Value);
            }

            picBox_TimeEdit.Invalidate();
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

            nowTimeLine.SPos = new Point(nowTimeLine.SPos.X + tickWidth, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(nowTimeLine.EPos.X + tickWidth, nowTimeLine.EPos.Y);

            if (currIdx >= (maxtIdxPerPanel + currOffsetIdx))
            {
                currOffsetIdx += 1;
                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);
            }

            picBox_TimeEdit.Invalidate();

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
            currIdx += 1;

            MoveToCurrTimeLine();

            if (currIdx >= (maxtIdxPerPanel + currOffsetIdx))
            {
                // 자동 스크롤이 되어야 함.
                currOffsetIdx += 1;
                int x = currOffsetIdx * tickWidth;
                int y = panel_TimeEdit.AutoScrollPosition.Y;
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("     1 스크롤 발생! panel2.HorizontalScroll.Value:{0}", panel2.HorizontalScroll.Value);

            }

            nowTimeLine.SPos = new Point(nowTimeLine.SPos.X + tickWidth, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(nowTimeLine.EPos.X + tickWidth, nowTimeLine.EPos.Y);

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

            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                currOffsetIdx = panel_TimeEdit.HorizontalScroll.Value / tickWidth;
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

            nowTimeLine = new TimeLine(nowTimeLine.SPos, new Point(nowTimeLine.EPos.X, panel_TimeEdit.Height));
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

            // 데이터 처리
            int index = lstTimeLineObj.FindIndex(r => r.Name.Equals(selectedTimeLineObj.Name));
            lstTimeLineObj.RemoveAt(index);

            // 왼쪽 판넬 처리
            Console.WriteLine("dGrid_TimeLineObj.CurrentCell.RowIndex : {0}", dGrid_TimeLineObj.CurrentCell.RowIndex);
            dGrid_TimeLineObj.Rows.RemoveAt(dGrid_TimeLineObj.CurrentCell.RowIndex);

            // 타임 에디터 갱신처리
            for(int i=0;i<lstTimeLineObj.Count;i++)
            {
                TimeLineObject obj = lstTimeLineObj[i];
                int y = (i * dGrid_TimeLineObj.RowTemplate.Height) + 5;

                obj.SPos = new Point(obj.SPos.X, y);
                obj.EPos = new Point(obj.EPos.X, y);
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

        // 현재 타임 라인 위치로 화면 이동
        private void MoveToCurrTimeLine()
        {
            int x = currOffsetIdx * tickWidth;
            int y = panel2.AutoScrollPosition.Y;
            panel2.AutoScrollPosition = new Point(x, y);
        }

        // 클릭한 점에서부터 현재 프레임 위치 얻어내기
        private void ConvPointToFrame(Point p)
        {
            int clickTickIdx = (p.X - boxGapWidth) / tickWidth;
            Console.WriteLine("({0},{1}) idx {2}", p.X, p.Y, clickTickIdx);
            int sx = boxGapWidth + (clickTickIdx * tickWidth);

            nowTimeLine.SPos = new Point(sx, nowTimeLine.SPos.Y);
            nowTimeLine.EPos = new Point(sx, nowTimeLine.EPos.Y);

            currIdx = clickTickIdx;

            // 임의의 위치가 원점부터 얼만큼 떨어져있는지 (판넬당 틱 갯수)
            int offsetIdxZeroStart = clickTickIdx - maxtIdxPerPanel;

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

                //Console.WriteLine("             1 스크롤 발생 clickTickIdx {0}, OffsetIdxZeroStart {1}, currOffsetIdx {2}", clickTickIdx, offsetIdxZeroStart, currOffsetIdx);
            }

            Console.WriteLine("1 OffsetIdxZeroStart {0} currOffsetIdx {1}", offsetIdxZeroStart, currOffsetIdx);
        }
    }
}
