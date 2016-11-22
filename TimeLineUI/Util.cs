using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    public class Util
    {
        // 틱 인덱스로 좌표 얻어내기
        public static Point ConvTickIdxToPoint(int rTickWidth, int rStartGap, int idx, int y = 0)
        {
            int x = (idx * rTickWidth) + rStartGap;
            return new Point(x, y);
        }

        // 클릭한 점에서부터 현재 프레임 위치 얻어내기
        // 틱 앞뒤로 50% 영역을 클릭하면 그 틱 인덱스를 얻어야 하므로 시작위치를 틱폭의 50%를 추가해서 처리
        public static int ConvPointToTickIdx(int rTickWidth, int rStartGap, Point p)
        {
            int middleTrim = (rTickWidth / 2);
            int clickTickIdx = (p.X - rStartGap + middleTrim) / rTickWidth;
            return clickTickIdx;
        }

        public static int ConvPointToRowIndex(int rowHeight, int rStartGapY, Point p)
        {
            int clickRowIdx = (p.Y - rStartGapY) / rowHeight;
            return clickRowIdx;
        }
    }
}
