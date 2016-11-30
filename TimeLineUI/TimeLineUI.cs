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
    public enum GRIDCONTROL_COLUMN 
    {
        GROUP,
        NAME,
        LOCK,
        VIEW
    }

    public partial class TimeLineUI : UserControl
    {
        private List<TimeLineObject> lstTimeLineObj = new List<TimeLineObject>();
        private SelectObject selectedObj = null;  // 선택한 오브젝트
        private List<TimeLine> drawTicks = new List<TimeLine>();

        private TimeLine nowTimeLine_picboxRuler = null;       // 타임 세로선 (눈금자용)
        private TimeLine nowTimeLine_picboxTimeEdit = null;    // 타임 세로선 (타임에디터용)

        private TimeLine startPTimeLine_picboxRuler = null;      // 시작아이콘 타임 세로선 (눈금자용)
        private TimeLine startPTimeLine_picboxTimeEdit = null;   // 시작아이콘 타임 세로선 (타임에디터용)

        private TimeLine endPTimeLine_picboxRuler = null;      // 끝아이콘 타임 세로선 (눈금자용)
        private TimeLine endPTimeLine_picboxTimeEdit = null;   // 끝아이콘 타임 세로선 (타임에디터용)

        private Timer timer = new Timer();

        private ContextMenu contextMenuEndMark;
        private ContextMenu contextMenuAddEventObject;
        private ContextMenu contextMenuEventObject;

        int s_offsetIdx = -1; // 클릭한 위치에서 시작점 좌측면까지 픽셀 거리 (보디를 구하기 위해)
        int e_offsetIdx = -1; // 클릭한 위치에서 끝점 우측면까지 픽셀 거리 (보디를 구하기 위해)

        int dataGridViewColumnName_Width = 0;  // 스크롤바 발생시 이 값에서 빼서 쓴다.


        // 2틱당 1프레임
        // 5프레임당 1초
        const int timePerFrame = 100;           // 프레임당 시간 (0.1초)
        const int tickPerFrame = 2;             // 프레임당 틱갯수
        const int framePerSec = 5;              // 초당 5프레임
        const int totalTime = 600;                // 전체 작업시간(초)
        int tickWidth = 5;                      // 이 간격을 0.1초로 계산
        int nCurrZoomRatio = 1;                 // 현재 줌 비율


        // 눈금용 데이터들
        const int boxGapWidth = 15;             // 픽쳐박스 왼쪽에서 시작위치까지의 간격 (눈금자, 타임에디터 공용)
        const int rulerPicBoxGapHeight = 15;    // 픽쳐박스 위에서 시작위치까지의 간격 (눈금자용)
        const int timeEditPicBoxGapHeight = 0;  // 픽쳐박스 위에서 시작위치까지의 간격 (타임에디터용)
        const int timeEditPicBox_ObjectGap = 5; // 타임에디터에서 오브젝트간에 높이갭 
        const int halfFrameHeight = 5;          // 0.5 프레임당 눈금길이
        const int fullFrameHeight = 12;         // 1 프레임당 눈금길이
        const int minTickWidth = 6;             // 최소줌일때 보기 좋은 값

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


        int selectRowIndex = -1;                // 왼쪽 또는 오른쪽 판넬에서 선택했을때의 인덱스
        int nTestMaxObject = 2;                 // 테스트용으로 만들어지는 최대 오브젝트 갯수
        



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
            contextMenuEndMark = new ContextMenu();
            contextMenuEndMark.MenuItems.Add(new MenuItem().Text = "Delete Group");
            contextMenuEndMark.MenuItems.Add(new MenuItem().Text = "Stop Animation");
            contextMenuEndMark.MenuItems.Add(new MenuItem().Text = "Replay Animation");
            contextMenuEndMark.MenuItems[0].Click += new EventHandler(deleteGroupToolStripMenuItem_Click);
            contextMenuEndMark.MenuItems[1].Click += new EventHandler(stopAnimationToolStripMenuItem_Click);
            contextMenuEndMark.MenuItems[2].Click += new EventHandler(replayAnimationToolStripMenuItem_Click);

            contextMenuAddEventObject = new ContextMenu();
            contextMenuAddEventObject.MenuItems.Add(new MenuItem().Text = "Add Event");
            contextMenuAddEventObject.MenuItems[0].Click += new EventHandler(AddEventToolStripMenuItem_Click);
            
            contextMenuEventObject = new ContextMenu();
            contextMenuEventObject.MenuItems.Add(new MenuItem().Text = "Delete Event");
            contextMenuEventObject.MenuItems[0].Click += new EventHandler(DeleteEventToolStripMenuItem_Click);

            dGrid_TimeLineObj.MouseClick += new MouseEventHandler(dGrid_TimeLineObj_MouseClick);
            //dGrid_TimeLineObj.CellValueChanged += new DataGridViewCellEventHandler(dGrid_TimeLineObj_CellValueChanged);
            //dGrid_TimeLineObj.SelectionChanged += new EventHandler(dGrid_TimeLineObj_SelectionChanged);
            dGrid_TimeLineObj.Scroll += new ScrollEventHandler(this.dGrid_TimeLineObj_Scroll);
            dGrid_TimeLineObj.SizeChanged += new EventHandler(this.dGrid_TimeLineObj_SizeChanged);
            dGrid_TimeLineObj.DataSource = null;
            dGrid_TimeLineObj.Rows.Clear();
            dGrid_TimeLineObj.AllowUserToAddRows = false;
            dataGridViewColumnName_Width = dGrid_TimeLineObj.Columns[(int)GRIDCONTROL_COLUMN.NAME].Width;

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
            picBox_TimeEdit.Move += new EventHandler(picBox_TimeEdit_Move);

            // 스크롤되는 간격 조정
            panel_TimeEdit.HorizontalScroll.SmallChange = tickWidth;    // 양 사이드의 화살표 누를때
            panel_TimeEdit.HorizontalScroll.LargeChange = tickWidth;    // 썸네일 밖에 영역을 누를때
            panel_TimeEdit.VerticalScroll.SmallChange = 23;     // 양 사이드의 화살표 누를때
            panel_TimeEdit.VerticalScroll.LargeChange = 23;     // 썸네일 밖에 영역을 누를때
            panel_TimeEdit.Scroll += new ScrollEventHandler(panel_TimeEdit_Scroll);

            // 타임에디터 판넬에 스크롤바가 발생하므로 룰러 클릭영역의 정확한 보정을 위해서 스크롤바 폭만큼 더해줘야 함.
            // 2는 라인두께
            panel_TimeEdit.Width = panel_Ruler.Width + SystemInformation.VerticalScrollBarWidth + 2;
            
            // 그리드 뷰가 한행 만큼 더 만들어 지므로 오른쪽 픽쳐박스는 한 행 높이 만큼 더하는데 
            // 픽쳐박스는 가로 스크롤뷰가 있으므로 그 높이 만큼 빼면 스크롤이 딱 맞게 됨
            // 컨트롤끼리의 높이차 보정을 따로 해야 함. (appbus에 적용시에는 -12)
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

            startPTimeLine_picboxRuler = new TimeLine(new Point(boxGapWidth, rulerPicBoxGapHeight), new Point(boxGapWidth, picBox_Ruler.Height));
            startPTimeLine_picboxTimeEdit = new TimeLine(new Point(boxGapWidth, 0), new Point(boxGapWidth, picBox_TimeEdit.Height));

            endPTimeLine_picboxRuler = new TimeLine(new Point(boxGapWidth, rulerPicBoxGapHeight), new Point(boxGapWidth, picBox_Ruler.Height));
            endPTimeLine_picboxTimeEdit = new TimeLine(new Point(boxGapWidth, 0), new Point(boxGapWidth, picBox_TimeEdit.Height));

            timer.Interval = timePerFrame; // 0.1초
            timer.Tick += new EventHandler(timer_Tick);

            // 전체 틱 갯수 = 시간 * 초당 프레임갯수 * 프레임당 틱갯수 (경계조건때문에 + 1)
            int totalTickCount = (totalTime * framePerSec * tickPerFrame) + 1; 
            
            // 최대 인덱스 값
            maxIdx = totalTickCount - 1;

            // 틱 값을 재조정
            int calcPicBoxWidth = picBox_TimeEdit.Width - boxGapWidth - SystemInformation.VerticalScrollBarWidth;
            tickWidth = calcPicBoxWidth / (totalTickCount - 1);
            if (tickWidth <= 0)
                tickWidth = 1;

            currOffsetIdx = 0;

            // 줌 처리 - 현재 줌값이 2이상만 되어도 픽쳐박스 최대크기가 int값을 넘어가는 문제발생
            trackBar1.Value = 1;
            ZoomProcess(trackBar1.Value, trackBar1.Minimum, trackBar1.Maximum);

            //Console.WriteLine("0 picBox_TimeEdit.Width:{0}, totalTickCount:{1}, tickWidth:{2}", picBox_TimeEdit.Width, totalTickCount, tickWidth);

            // 사운드 오브젝트 추가 - 디폴트
            AddTimeObj(EventObjectMng.nSoundObjectID, 0, "SOUND OBJECT", true, true, -1, 0, maxIdx);

            // 카메라 오브젝트 추가 - 디폴트
            AddTimeObj(EventObjectMng.nCameraObjectID, 0, "CAMERA OBJECT", true, true, -1, 0, maxIdx);

        }

        public void ScriptParser(string[] script_text, int LayerCount)
        {
            EventObjectMng tempMng = new EventObjectMng();
            List<EventObject> tempObjects = tempMng.ScriptParser(script_text, LayerCount);

            // 이프로젝트에서만 작동되는 코드
            // 유니크번호로 오브젝트 찾기 - 없으면 생성 (생성시 해당 유니크번호 할당)
            foreach(EventObject obj in tempObjects)
            {
                if (obj.uniqueID > 0)
                {
                    SelectObject selObj = FindTimeLineObject(obj.uniqueID);
                    if(selObj == null)
                        selObj = AddTimeObj(obj.uniqueID, obj.layerdepth_index, string.Format("Object {0}", obj.uniqueID), true, true, obj.GroupID);

                    AddEvent(selObj, obj.tickIdx, obj.name);

                    // 타임라인오브젝트 그룹속성변경
                    if (obj.GroupID >= 0) {
                        TimeLineObject lineObj = GetSelectObjectToTimeLineObject(selObj);
                        lineObj.Group = obj.GroupID;

                        int selectedIndex = FindIndexTimeLineObject(obj.uniqueID);

                        string strGrp = "";
                        if (obj.GroupID >= 0)
                            strGrp = string.Format("G{0}", obj.GroupID);
                        dGrid_TimeLineObj.Rows[selectedIndex].Cells[0].Value = strGrp;
                    }
                }
            }

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
            dGrid_TimeLineObj.Invalidate();
        }

        private int FindIndexTimeLineObject(int r_uniqueID)
        {
            return lstTimeLineObj.FindIndex(r => r.uniqueID == r_uniqueID);
        }

        private SelectObject FindTimeLineObject(int r_uniqueID)
        {
            int index = lstTimeLineObj.FindIndex(r => r.uniqueID == r_uniqueID);
            //Console.WriteLine("Find Index {0}", index);

            if (index >= 0)
                return lstTimeLineObj[index].bodyObj;
            else
                return null;

        }

        public void AddEvent(SelectObject selObj, int tick, string eventName)
        {
            TimeBodyObject body = GetSelectObjectToTimeBodyObject(selObj);
            if (body != null)
            {
                TimeObject sObj = body.GetStartObj();

                int offsettick = tick - sObj.tickIdx;

                body.GetTimeLineObject().AddEvent(tick, offsettick, eventName, TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tick, sObj.pos.Y));
            }
        }

        private void HorizontalTickInit(int rulerWidth)
        {
            drawTicks.Clear();
            // 맨 마지막 눈금까지 그릴려면 + 1
            for (int i = 0; i < maxIdx + 1; i++)
            {
                int x = boxGapWidth + (i * rulerWidth);

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

            TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);
            if (body != null)
            {
                // 라인전체 칠하기
                Rectangle rect = new Rectangle(panel_TimeEdit.HorizontalScroll.Value,
                                               body.GetStartObj().pos.Y - timeEditPicBox_ObjectGap, 
                                               panel_TimeEdit.Width + panel_TimeEdit.HorizontalScroll.Value, 
                                               dGrid_TimeLineObj.RowTemplate.Height);
                memGraphics.FillRectangle(Brushes.LightBlue, rect);

                // 시작 아이콘 타임라인
                memGraphics.DrawLine(Pens.Blue, startPTimeLine_picboxTimeEdit.SPos, startPTimeLine_picboxTimeEdit.EPos);
                memGraphics.DrawLine(Pens.Blue, endPTimeLine_picboxTimeEdit.SPos, endPTimeLine_picboxTimeEdit.EPos);
            }

            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                obj.DrawMarks(memGraphics);
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

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            // 눈금그리기
            for (int i = 0; i < drawTicks.Count; i++)
            {
                TimeLine lineObj = drawTicks[i];
                memGraphics.DrawLine(Pens.Red, lineObj.SPos, lineObj.EPos);

                //memGraphics.DrawString(i.ToString(), new Font("Arial", 8), Brushes.Black, new Point(lineObj.SPos.X - 4, lineObj.SPos.Y - 15));

                if ((i % 10) == 0)
                {
                    memGraphics.DrawString((i / 10).ToString(), new Font("Arial", 8), Brushes.Black, new Point(lineObj.SPos.X - 4, lineObj.SPos.Y - 15));
                    //memGraphics.DrawString(string.Format("{0:0}:{1:00}", i / 600, i / 10 % 60), new Font("Arial", 8), Brushes.Black, new Point(lineObj.SPos.X, lineObj.SPos.Y - 9), stringFormat);
                }
            }

            memGraphics.DrawString((drawTicks.Count-1).ToString(), new Font("Arial", 8), Brushes.Black, new Point(drawTicks[drawTicks.Count-1].SPos.X - 4, drawTicks[drawTicks.Count - 1].SPos.Y - 15));

            //if (selectedTimeLineObj != null)
            if (selectedObj != null)
            {
                // 시작 아이콘 타임라인
                memGraphics.DrawLine(Pens.Blue, startPTimeLine_picboxRuler.SPos, startPTimeLine_picboxRuler.EPos);
                memGraphics.DrawLine(Pens.Blue, endPTimeLine_picboxRuler.SPos, endPTimeLine_picboxRuler.EPos);
            }

            // 현재 타임라인 세로선
            memGraphics.DrawLine(Pens.Red, nowTimeLine_picboxRuler.SPos, nowTimeLine_picboxRuler.EPos);

            e.Graphics.DrawImageUnscaled(bitmap, 0, 0);

            memGraphics.Dispose();
            bitmap.Dispose();
        }

        

        Point eventMousePos;
        private void picBox_TimeEdit_MouseClick(object sender, MouseEventArgs e)
        {
            //picBox_TimeEdit.Refresh();

            if (e.Button == MouseButtons.Right)
            {
                // 마우스 위치에 있는 오브젝트 얻기
                foreach (TimeLineObject obj in lstTimeLineObj)
                {
                    selectedObj = obj.CheckPos(new Point(e.X, e.Y));
                    if (selectedObj == null) continue;
                    break;
                }

                // 컨텍스트 메뉴 호출을 위한 작업
                if (selectedObj == null) return;

                if (selectedObj.ObjType == OBJTYPE.END)
                {
                    contextMenuEndMark.Show(picBox_TimeEdit, e.Location);
                }
                else
                {
                    if (selectedObj.ObjType == OBJTYPE.EVENT)
                    {
                        contextMenuEventObject.Show(picBox_TimeEdit, e.Location); // 이벤트 삭제
                    }
                    else
                    {
                        contextMenuAddEventObject.Show(picBox_TimeEdit, e.Location); // 이벤트 추가
                        eventMousePos = e.Location;
                    }
                }
            }
            else
            {
                // 해당라인 전체를 클릭영역으로....
                selectRowIndex = TimeLineUtil.ConvPointToRowIndex(dGrid_TimeLineObj.RowTemplate.Height, timeEditPicBox_ObjectGap, new Point(e.X, e.Y));
                if (selectRowIndex >= lstTimeLineObj.Count)
                {
                    selectRowIndex = -1;
                    Console.WriteLine("해당라인에 오브젝트가 없습니다.");
                }
                else
                {
                    Console.WriteLine("선택된 오브젝트는 {0}입니다. selectRowIndex:{1}", lstTimeLineObj[selectRowIndex].Name, selectRowIndex);

                    selectedObj = lstTimeLineObj[selectRowIndex].bodyObj;
                    selectedObj.ObjType = OBJTYPE.TOTALLINE;

                    dGrid_TimeLineObj.Rows[selectRowIndex].Selected = true;
                }
            }

            dGrid_TimeLineObj.Invalidate();
            picBox_TimeEdit.Invalidate();
        }
        

        private void picBox_TimeEdit_MouseDown(object sender, MouseEventArgs e)
        {
            // 마우스 오른쪽 버튼 다운은 처리안함.
            if (e.Button == MouseButtons.Right) return;

            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                selectedObj = obj.CheckPos(new Point(e.X, e.Y));
                if (selectedObj == null) continue;
                break;
            }

            if (selectedObj != null)
            {
                GridViewSelectProcess(selectedObj);

                if (selectedObj.ObjType == OBJTYPE.BODY)
                {
                    int convIdx = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, new Point(e.X, e.Y));

                    int stick = (selectedObj as TimeBodyObject).GetStartObj().tickIdx;
                    int etick = (selectedObj as TimeBodyObject).GetEndObj().tickIdx;

                    s_offsetIdx = convIdx - stick;
                    e_offsetIdx = etick - convIdx;
                }

                //Console.WriteLine("picBox_TimeEdit_MouseDown name:{0}", selectedObj.name);
            }
            else
            {
                Console.WriteLine("picBox_TimeEdit_MouseDown selectedObj NULL!!!!");
            }

            picBox_TimeEdit.Invalidate();
        }

        
               
        private void picBox_Ruler_MouseDown(object sender, MouseEventArgs e)
        {
            // 세로 타임라인 조정
            currIdx = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, e.Location);

            MoveToTimeLine(TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, currIdx).X);

            // 해당 틱으로 이동과 스크롤처리
            MoveToTickIdxAndAutoScroll(currIdx);
            
            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
        }
        
        private void StarTimeLineProcess(int tickIndex)
        {
            startPTimeLine_picboxTimeEdit.SPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, startPTimeLine_picboxTimeEdit.SPos.Y);
            startPTimeLine_picboxTimeEdit.EPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, startPTimeLine_picboxTimeEdit.EPos.Y);
            startPTimeLine_picboxRuler.SPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, startPTimeLine_picboxRuler.SPos.Y);
            startPTimeLine_picboxRuler.EPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, startPTimeLine_picboxRuler.EPos.Y);
        }

        private void EndTimeLineProcess(int tickIndex)
        {
            endPTimeLine_picboxTimeEdit.SPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, endPTimeLine_picboxTimeEdit.SPos.Y);
            endPTimeLine_picboxTimeEdit.EPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, endPTimeLine_picboxTimeEdit.EPos.Y);
            endPTimeLine_picboxRuler.SPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, endPTimeLine_picboxRuler.SPos.Y);
            endPTimeLine_picboxRuler.EPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tickIndex, endPTimeLine_picboxRuler.EPos.Y);
        }

        private void picBox_TimeEdit_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectedObj == null)
                    return;

                if (selectedObj.CheckPanelBound(new Point(e.X, 0), boxGapWidth, picBox_TimeEdit.Width, panel_TimeEdit, panel_Ruler) == false)
                    return;

                if (selectedObj.CheckObjectLimit(new Point(e.X, 0)) == false)
                    return;

                if (selectedObj.ObjType == OBJTYPE.START || selectedObj.ObjType == OBJTYPE.END) // 시작점, 끝점 움직이기
                {
                    int convIdx = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, new Point(e.X, e.Y));
                    selectedObj.pos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, convIdx, selectedObj.pos.Y);
                    selectedObj.tickIdx = convIdx;

                    TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);

                    StarTimeLineProcess(convIdx);

                    if (selectedObj.ObjType == OBJTYPE.START)
                    {
                        EndTimeLineProcess(body.GetEndObj().tickIdx);

                        // 이벤트도 같이 움직여야 함..
                        if (body.GetEventObjectMng() != null)
                        {
                            // 변경된 시작 오브젝트의 시작인덱스 + 이벤트 오브젝트의 오프셋
                            body.GetEventObjectMng().MoveProcess(convIdx, tickWidth, boxGapWidth, selectedObj.pos.Y);
                        }
                    }
                    else
                    {
                        EndTimeLineProcess(body.GetStartObj().tickIdx);
                    }
                    
                }
                else if(selectedObj.ObjType == OBJTYPE.BODY)
                {
                    int convIdx = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, new Point(e.X, e.Y));

                    int convSIdx = convIdx - s_offsetIdx;
                    int convEIdx = convIdx + e_offsetIdx;

                    TimeBodyObject body = selectedObj as TimeBodyObject;
                    TimeObject sObj = body.GetStartObj();
                    TimeObject eObj = body.GetEndObj();

                    Point convSPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, convSIdx, eObj.pos.Y);
                    Point convEPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, convEIdx, eObj.pos.Y);

                    sObj.pos = convSPos;
                    eObj.pos = convEPos;
                    sObj.tickIdx = convSIdx;
                    eObj.tickIdx = convEIdx;
                    
                    StarTimeLineProcess(convSIdx);
                    EndTimeLineProcess(convEIdx);

                    // 이벤트도 같이 움직여야 함..
                    if (body.GetEventObjectMng() != null)
                    {
                        // 변경된 시작 오브젝트의 시작인덱스 + 이벤트 오브젝트의 오프셋
                        body.GetEventObjectMng().MoveProcess(convSIdx, tickWidth, boxGapWidth, selectedObj.pos.Y);
                    }
                }
                else if (selectedObj.ObjType == OBJTYPE.EVENT ) // 이벤트 움직이기
                {
                    TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);
                    int convIdx = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, new Point(e.X, e.Y));
                    selectedObj.pos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, convIdx, selectedObj.pos.Y);

                    int oldTickIdx = selectedObj.tickIdx;
                    int oldOffset = (selectedObj as EventObject).offsetTick;
                                        
                    if (oldTickIdx != convIdx)
                    {
                        selectedObj.tickIdx = convIdx;

                        int newOffset = convIdx - body.GetStartObj().tickIdx;

                        (selectedObj as EventObject).offsetTick = newOffset;

                        body.GetEventObjectMng().Remove_EventObject(oldOffset, (selectedObj as EventObject).index);
                        body.GetEventObjectMng().ReCalcIndex(oldOffset);

                        // body.GetEventObjectMng().Add_EventObject(convIdx, newOffset, selectedObj.pos, (selectedObj as EventObject));
                        body.GetEventObjectMng().Add_EventObject(selectedObj as EventObject);

                        //Console.WriteLine("이벤트 총수:{0}", obj.GetEventObjectMng().GetCount());
                        //obj.GetEventObjectMng().TotalPrint();
                    }

                }

                picBox_Ruler.Invalidate();
            }
            else // 단순 호버 체크는 여기서
            {
                foreach (TimeLineObject obj in lstTimeLineObj)
                {
                    obj.CheckPos(new Point(e.X, e.Y));
                }
            }

            picBox_TimeEdit.Invalidate();
        }

        private void picBox_TimeEdit_MouseUp(object sender, MouseEventArgs e)
        {
            // 좌표를 시간으로 변환하는 계산이 들어가야 함.
            s_offsetIdx = -1;
            e_offsetIdx = -1;
            
            // 이벤트 이동후 이벤트 좌표 재계산 필요
            TimeBodyObject obj = GetSelectObjectToTimeBodyObject(selectedObj);
            if (obj == null) return;
            
            if(obj.GetEventObjectMng() != null)
                obj.GetEventObjectMng().ReCalcPosition(tickWidth, boxGapWidth, selectedObj.pos.Y);

            picBox_TimeEdit.Invalidate();
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

            Point pos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, maxIdx);

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
                int x = Math.Abs(panel_TimeEdit.AutoScrollPosition.X);
                int y = dGrid_TimeLineObj.VerticalScrollingOffset;

                //// panel_TimeEdit.VerticalScroll.Value 에 값을 입력하는것은 작동 안함. 이해안됨
                panel_TimeEdit.AutoScrollPosition = new Point(x, y);

                //Console.WriteLine("{0}", dGrid_TimeLineObj.VerticalScrollingOffset);
            }
        }

        private void panel_TimeEdit_Scroll(object sender, ScrollEventArgs e)
        {
            //Console.WriteLine("Scroll {0}", panel_TimeEdit.VerticalScroll.Value);
            
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                if (lstTimeLineObj.Count == 0) return;


                int moveIndex = panel_TimeEdit.VerticalScroll.Value / dGrid_TimeLineObj.RowTemplate.Height;
                
                dGrid_TimeLineObj.FirstDisplayedScrollingRowIndex = moveIndex;

                panel_TimeEdit.AutoScrollPosition = new Point(0, moveIndex * dGrid_TimeLineObj.RowTemplate.Height);

                //Console.WriteLine("0 panel_TimeEdit AutoScroll:{0}, VV:{1}, Index:{2}", panel_TimeEdit.AutoScrollPosition, panel_TimeEdit.VerticalScroll.Value, moveIndex);
            }

            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                currOffsetIdx = panel_TimeEdit.HorizontalScroll.Value / tickWidth;
                panel_Ruler.AutoScrollPosition = new Point(panel_TimeEdit.HorizontalScroll.Value, panel_TimeEdit.VerticalScroll.Value);
            }
            
            startPTimeLine_picboxTimeEdit.EPos = new Point(startPTimeLine_picboxTimeEdit.EPos.X, startPTimeLine_picboxTimeEdit.EPos.Y + panel_TimeEdit.VerticalScroll.Value);
            endPTimeLine_picboxTimeEdit.EPos = new Point(endPTimeLine_picboxTimeEdit.EPos.X, endPTimeLine_picboxTimeEdit.EPos.Y + panel_TimeEdit.VerticalScroll.Value);

            picBox_TimeEdit.Invalidate();
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
            {
                Console.WriteLine("info 없음...");
                return;
            }

            if (info.RowIndex > lstTimeLineObj.Count)
            {
                Console.WriteLine("info 오버...");
                return;
            }

            selectedObj = lstTimeLineObj[info.RowIndex].bodyObj;
            if (selectedObj == null)
            {
                Console.WriteLine("선택된 정보 없음...");
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                // 컨텍스트 메뉴 호출을 위한 작업
                if (selectedObj != null)
                {
                    dGrid_TimeLineObj.CurrentCell = dGrid_TimeLineObj.Rows[info.RowIndex].Cells[0];
                    contextMenuEndMark.Show(dGrid_TimeLineObj, e.Location);
                }

            }
            else if(e.Button == MouseButtons.Left)
            {
                if (info.ColumnIndex == (int)GRIDCONTROL_COLUMN.LOCK || info.ColumnIndex == (int)GRIDCONTROL_COLUMN.VIEW)
                {
                    DataGridViewCheckBoxCell cell = dGrid_TimeLineObj.CurrentCell as DataGridViewCheckBoxCell;
                    if (cell == null)
                    {
                        //Console.WriteLine("Cell is null!!!");
                    }
                    else
                    {
                        int rowIdx = dGrid_TimeLineObj.CurrentCell.RowIndex;
                        int colIdx = dGrid_TimeLineObj.CurrentCell.ColumnIndex;

                        //Console.WriteLine("dGrid_TimeLineObj.CurrentCell.RowIndex {0}, colIdx {1}", dGrid_TimeLineObj.CurrentCell.RowIndex, colIdx);

                        if (Convert.ToBoolean(cell.Value) == true)
                            cell.Value = false;
                        else
                            cell.Value = true;

                        if (info.ColumnIndex == (int)GRIDCONTROL_COLUMN.LOCK) // Lock
                        {
                            lstTimeLineObj[info.RowIndex].Lock = (Boolean)cell.Value;

                        }

                        if (info.ColumnIndex == (int)GRIDCONTROL_COLUMN.VIEW) // View
                        {
                            lstTimeLineObj[info.RowIndex].View = (Boolean)cell.Value;

                        }
                    }
                }
            }

            dGrid_TimeLineObj.Rows[info.RowIndex].Selected = true;
            //Console.WriteLine("{0} {1} Lock:{2}, View:{3}", info.RowIndex, dGrid_TimeLineObj.Rows[info.RowIndex].Selected, lstTimeLineObj[info.RowIndex].Lock, lstTimeLineObj[info.RowIndex].View);

            picBox_TimeEdit.Invalidate();
        }

        // 값이 바뀌고 난후에 들어오는 이벤트...
        private void dGrid_TimeLineObj_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if (e.ColumnIndex == (int)GRIDCONTROL_COLUMN.NAME)
            {
                DataGridViewTextBoxCell cell = dGrid_TimeLineObj.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewTextBoxCell;
                lstTimeLineObj[e.RowIndex].Name = (String)cell.Value;

                //Console.WriteLine("CellValueChanged Row:{0}, Col:{1}, Value:{2}", e.RowIndex, e.ColumnIndex, Convert.ToString(cell.Value));
            }
        }

        
        

        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (selectedTimeLineObj != null)
            //    Console.WriteLine("{0}", selectedTimeLineObj.Name);

            TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);
            if (body == null)
            {
                Console.WriteLine("선택 데이터 정보 없음!");
                return;
            }

            // 데이터 처리
            int index = lstTimeLineObj.FindIndex(r => r.Name.Equals(body.name));
            Console.WriteLine("Find Index {0}", index);
            lstTimeLineObj.RemoveAt(index);
            
            // 왼쪽 판넬 처리
            //Console.WriteLine("dGrid_TimeLineObj.CurrentCell.RowIndex : {0}", dGrid_TimeLineObj.CurrentCell.RowIndex);
            dGrid_TimeLineObj.Rows.RemoveAt(dGrid_TimeLineObj.CurrentCell.RowIndex);
            if (dGrid_TimeLineObj.VerticalScrollbarVisible)
                dGrid_TimeLineObj.Columns[(int)GRIDCONTROL_COLUMN.NAME].Width = dataGridViewColumnName_Width - SystemInformation.VerticalScrollBarWidth;
            else
                dGrid_TimeLineObj.Columns[(int)GRIDCONTROL_COLUMN.NAME].Width = dataGridViewColumnName_Width;

            // 타임 에디터 갱신처리
            int tempHeight = 0;
            tempHeight += timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
            for (int i = 0; i < lstTimeLineObj.Count; i++)
            {
                TimeBodyObject bodyObj = lstTimeLineObj[i].bodyObj;
                int y = (i * dGrid_TimeLineObj.RowTemplate.Height) + timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;
                
                bodyObj.GetStartObj().pos = new Point(bodyObj.GetStartObj().pos.X, y);
                bodyObj.GetEndObj().pos = new Point(bodyObj.GetEndObj().pos.X, y);

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
            TimeLineObject obj = GetSelectObjectToTimeLineObject(selectedObj);
            if (obj == null)
            {
                Console.WriteLine("선택 데이터 정보 없음!");
                return;
            }
            obj.aniType = ANITYPE.STOP_AT_END;

            //Console.WriteLine("1 {0}, Lock:{1}, View:{2}, AniType:{3}",
            //                    selectedTimeLineObj.Name,
            //                    selectedTimeLineObj.Lock,
            //                    selectedTimeLineObj.View,
            //                    selectedTimeLineObj.aniType);
        }

        private void replayAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeLineObject obj = GetSelectObjectToTimeLineObject(selectedObj);
            obj.aniType = ANITYPE.REPLAY_AT_END;

            //Console.WriteLine("1 {0}, Lock:{1}, View:{2}, AniType:{3}",
            //                    selectedTimeLineObj.Name,
            //                    selectedTimeLineObj.Lock,
            //                    selectedTimeLineObj.View,
            //                    selectedTimeLineObj.aniType);
        }

        private void AddEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);
            //if (body != null)
            //{
            //    int tick = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, eventMousePos);
            //    TimeObject sObj = body.GetStartObj();

            //    int offsettick = tick - sObj.tickIdx;

            //    body.GetTimeLineObject().AddEvent(tick, offsettick, "test", TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, tick, sObj.pos.Y));
            //}

            int tick = TimeLineUtil.ConvPointToTickIdx(tickWidth, boxGapWidth, eventMousePos);
            AddEvent(selectedObj, tick, "USEREVENT");
        }


        private void DeleteEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("이벤트 삭제~");

            TimeBodyObject obj = GetSelectObjectToTimeBodyObject(selectedObj);
            obj.GetEventObjectMng().Remove_EventObject(selectedObj.tickIdx, (selectedObj as EventObject).index);
            obj.GetEventObjectMng().ReCalcIndex(selectedObj.tickIdx);

            if (obj.GetEventObjectMng() != null)
                obj.GetEventObjectMng().ReCalcPosition(tickWidth, boxGapWidth, selectedObj.pos.Y);

            picBox_TimeEdit.Invalidate();
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

        public SelectObject AddTimeObj(int r_uniqueID, int r_layerdepthIndex, string r_name, bool r_lock = false, bool r_view = true, int r_GroupIdx = 0, int startTick=0, int endTick=10)
        {
            if (lstTimeLineObj.Count > 255)
                MessageBox.Show("작업 제한값을 넘었습니다. (255개)");

            int y = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + timeEditPicBox_ObjectGap + timeEditPicBoxGapHeight;

            TimeLineObject obj = new TimeLineObject(new Point(boxGapWidth, y), tickWidth, r_name, r_uniqueID, r_layerdepthIndex, startTick, endTick);
            obj.Group = r_GroupIdx;
            obj.Name = r_name;
            obj.Lock = r_lock;
            obj.View = r_view;

            lstTimeLineObj.Add(obj);

            string strGrp = "";
            if (r_GroupIdx >= 0)
                strGrp = string.Format("G{0}", r_GroupIdx);

            // 좌측 판넬 세팅
            dGrid_TimeLineObj.Rows.Add(strGrp, obj.Name, obj.Lock, obj.View);
            dGrid_TimeLineObjHeight += dGrid_TimeLineObj.RowTemplate.Height;
            if (dGrid_TimeLineObj.VerticalScrollbarVisible)
                dGrid_TimeLineObj.Columns[(int)GRIDCONTROL_COLUMN.NAME].Width = dataGridViewColumnName_Width - SystemInformation.VerticalScrollBarWidth;
            else
                dGrid_TimeLineObj.Columns[(int)GRIDCONTROL_COLUMN.NAME].Width = dataGridViewColumnName_Width;

            //Console.WriteLine("1 dGrid_TimeLineObj Height {0}, Count {1}, Row Height {2}, dGrid_TimeLineObjHeight {3}, SystemInformation.HorizontalScrollBarHeight {4}", dGrid_TimeLineObj.Height, lstTimeLineObj.Count, dGrid_TimeLineObj.RowTemplate.Height, dGrid_TimeLineObjHeight, SystemInformation.HorizontalScrollBarHeight);

            // 우측 판넬 세팅 
            int tempH = (lstTimeLineObj.Count * dGrid_TimeLineObj.RowTemplate.Height) + picBox_TimeEdit_TrimH;
            if (tempH <= picBox_TimeEdit_MinHeight)
                picBox_TimeEdit.Height = picBox_TimeEdit_MinHeight;
            else
                picBox_TimeEdit.Height = tempH;

            nowTimeLine_picboxTimeEdit.EPos = new Point(nowTimeLine_picboxTimeEdit.EPos.X, picBox_TimeEdit.Height);

            return obj.bodyObj;
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
            //Console.WriteLine("SizeChanged dGrid_TimeLineObj.Height {0}, dGrid_TimeLineObjHeight {1}", dGrid_TimeLineObj.Height, dGrid_TimeLineObjHeight);
        }

        // 테스트용 임시 데이터 생성용
        private void picBox_View_Click(object sender, EventArgs e)
        {
            //TimeLineUI_Reset();

            for (int i = 0; i < nTestMaxObject; i++)
                AddTimeObj(0, 0, string.Format("test{0}", i), true, true);

            picBox_TimeEdit.Invalidate();
            picBox_Ruler.Invalidate();
            dGrid_TimeLineObj.Invalidate();
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

            // 사운드 오브젝트 추가 - 디폴트
            AddTimeObj(EventObjectMng.nSoundObjectID, 0, "SOUND OBJECT", true, true, -1, 0, maxIdx);

            // 카메라 오브젝트 추가 - 디폴트
            AddTimeObj(EventObjectMng.nCameraObjectID, 0, "CAMERA OBJECT", true, true, -1, 0, maxIdx);
        }

        private void picBox_TimeEdit_Move(object sender, EventArgs e)
        {
            //Console.WriteLine("0 panel_TimeEdit.AutoScrollPosition:{0}", panel_TimeEdit.AutoScrollPosition);
            // 휠이동시 120씩 움직이기 때문에 다시 보정해주기
            int moveIndex = panel_TimeEdit.AutoScrollPosition.Y / dGrid_TimeLineObj.RowTemplate.Height;
            moveIndex *= -1;

            int x = Math.Abs(panel_TimeEdit.AutoScrollPosition.X);

            panel_TimeEdit.AutoScrollPosition = new Point(x, moveIndex * dGrid_TimeLineObj.RowTemplate.Height);

            if(dGrid_TimeLineObj.Rows.Count > 0)
                dGrid_TimeLineObj.FirstDisplayedScrollingRowIndex = moveIndex;
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            nCurrZoomRatio = trackBar1.Value;
            lbZoomRatio.Text = trackBar1.Value.ToString();

            ZoomProcess(trackBar1.Value, trackBar1.Minimum, trackBar1.Maximum);

            picBox_Ruler.Invalidate();
            picBox_TimeEdit.Invalidate();

        }

        private void ZoomProcess(int trackV, int min, int max)
        {
            // 판넬폭 - (스크롤바 + 아웃 라인 폭)
            int calcPanelBox = panel_TimeEdit.Width - (SystemInformation.VerticalScrollBarWidth + 2);

            // 계산된 판넬폭 - 양끝의 박스 갭
            int calcPicBoxWidth = calcPanelBox - (2 * boxGapWidth);

            // 줌배율 적용된 틱 폭
            tickWidth = minTickWidth * trackV;

            // 픽처박스 넓이 = 양쪽 갭 폭 + 최대 인덱스 * 틱 폭
            picBox_TimeEdit.Width = (boxGapWidth * 2) + (maxIdx * tickWidth);
            picBox_Ruler.Width = picBox_TimeEdit.Width;
            Console.WriteLine("picboxWidth:{0}", picBox_TimeEdit.Width);

            int calWidth = panel_Ruler.Width - boxGapWidth;         // 타임에디터 픽쳐박스의 세로스크롤바의 폭이 빠졌는지 체크
            int tickCountPerPanel = (calWidth / tickWidth) + 1;     // 한 화면당 나타낼 틱갯수 (경계조건때문에 + 1)

            // 판넬당 표시되는 최대 인덱스 값
            maxIdxPerPanel = tickCountPerPanel - 1;

            // 최소 시작 인덱스 ( 임의의 값으로 세팅할수 있음 하지만 될수있는한 안하는게 좋음. )
            offsetStartIdx_FromZero = (maxIdxPerPanel / 2);// + 10;         // 보기 좋은 중앙 보정값( +2 )
            offsetStart_FromLast = maxIdx - offsetStartIdx_FromZero;

            // 최대 오프셋값 = (전체 인덱스 - 판넬당 최대인덱스) 
            maxOffsetIdx_FromZero = maxIdx - maxIdxPerPanel;

            panel_TimeEdit.HorizontalScroll.SmallChange = tickWidth;    // 양 사이드의 화살표 누를때
            panel_TimeEdit.HorizontalScroll.LargeChange = tickWidth;    // 썸네일 밖에 영역을 누를때
            //Console.WriteLine("Min:{0}, Max:{1}", panel_TimeEdit.HorizontalScroll.Minimum, panel_TimeEdit.HorizontalScroll.Maximum);
            
            // 타임 라인 오브젝트 변경
            foreach (TimeLineObject obj in lstTimeLineObj)
            {
                TimeObject sObj = obj.bodyObj.GetStartObj();
                TimeObject eObj = obj.bodyObj.GetEndObj();

                Point convSPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, sObj.tickIdx, eObj.pos.Y);
                Point convEPos = TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, eObj.tickIdx, eObj.pos.Y);

                sObj.pos = convSPos;
                eObj.pos = convEPos;

                if (obj.bodyObj.GetEventObjectMng() != null)
                {
                    // 변경된 시작 오브젝트의 시작인덱스 + 이벤트 오브젝트의 오프셋
                    obj.bodyObj.GetEventObjectMng().ReCalcPosition(tickWidth, boxGapWidth, eObj.pos.Y);
                }
            }

            // 타임 라인 변경
            if (selectedObj != null)
            {
                TimeBodyObject body = GetSelectObjectToTimeBodyObject(selectedObj);
                TimeObject sObj = body.GetStartObj();
                TimeObject eObj = body.GetEndObj();

                StarTimeLineProcess(sObj.tickIdx);
                EndTimeLineProcess(eObj.tickIdx);
            }

            // 타임 라인 이동
            MoveToTimeLine(TimeLineUtil.ConvTickIdxToPoint(tickWidth, boxGapWidth, currIdx).X);

            // 해당 틱으로 이동과 스크롤처리
            //MoveToTickIdxAndAutoScroll(currIdx);
            //Console.WriteLine("tickWidth : {0}", tickWidth);

            HorizontalTickInit(tickWidth);
        }

        // 선택된 오브젝트에서 TimeLineObject 얻기
        private TimeLineObject GetSelectObjectToTimeLineObject(SelectObject selObj)
        {
            if (selObj == null) return null;

            TimeBodyObject bodyObj = GetSelectObjectToTimeBodyObject(selObj);

            return bodyObj.GetTimeLineObject();
        }

        // 선택된 오브젝트에서 TimeBodyObject 얻기
        private TimeBodyObject GetSelectObjectToTimeBodyObject(SelectObject selObj)
        {
            if (selObj == null) return null;

            if (selObj.ObjType == OBJTYPE.BODY || selObj.ObjType == OBJTYPE.TOTALLINE)
            {
                return (selObj as TimeBodyObject);
            }
            else
            {
                return (selObj.GetParent() as TimeBodyObject);
            }
        }

        private DataGridViewRow GridViewSelectProcess(SelectObject obj)
        {
            TimeBodyObject bodyObj = GetSelectObjectToTimeBodyObject(obj);
           
            DataGridViewRow row = dGrid_TimeLineObj.Rows
                            .Cast<DataGridViewRow>()
                            .Where(r => r.Cells["name"].Value.ToString().Equals(bodyObj.name))
                            .First();

            dGrid_TimeLineObj.Rows[row.Index].Selected = true;
            dGrid_TimeLineObj.CurrentCell = dGrid_TimeLineObj.Rows[row.Index].Cells[0];

            return row;
        }
    }
}

