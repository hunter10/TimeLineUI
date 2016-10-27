using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLineUI
{
    //class TimeLineDataGridViewMouseEventArgs : MouseEventArgs
    //{
    //    public List<TimeLineObject> lstTimeLineObj;

    //    public TimeLineDataGridViewMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, List<TimeLineObject> r_list) : base(button, clicks, x, y, delta)
    //    //public TimeLineDataGridViewMouseEventArgs(List<TimeLineObject> r_list)
    //    {
    //        lstTimeLineObj = r_list;
    //    }
    //}

    class TimeLineDataGridView : DataGridView
    {
        public event EventHandler ScrollbarVisibleChanged;
        //public delegate void MyMouseEventHandler(object sender, TimeLineDataGridViewMouseEventArgs e);
        //public event MyMouseEventHandler MyMouseClick;

        public TimeLineDataGridView()
        {
            this.VerticalScrollBar.VisibleChanged += new EventHandler(VerticalScrollBar_VisibleChanged);
            //this.MyMouseClick += new MyMouseEventHandler(dGrid_TimeLineObj_MouseClick1);
        }

        public bool VerticalScrollbarVisible
        {
            get { return VerticalScrollBar.Visible; }
        }
        
        private void VerticalScrollBar_VisibleChanged(object sender, EventArgs e)
        {
            EventHandler handler = ScrollbarVisibleChanged;
            if (handler != null) handler(this, e);
        }

        //private void dGrid_TimeLineObj_MouseClick1(object sender, TimeLineDataGridViewMouseEventArgs e)
        //{
        //    
        //}
    }
}
