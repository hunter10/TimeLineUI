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
    }
}
