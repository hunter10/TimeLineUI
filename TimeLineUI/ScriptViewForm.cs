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
    public partial class ScriptViewForm : Form
    {
        public delegate void EventDelegate(string script1, string script2, string script3, string script4, string script_ui);
        Delegate m_delegate;
        string[] m_script;

        public ScriptViewForm()
        {
            InitializeComponent();

            layer_dd.SelectedIndex = 0;
        }

        public void Open(EventDelegate dele)
        {
            m_delegate = dele;
            this.ShowDialog();
        }
        public void SetSelect(int index)
        {
            layer_dd.SelectedIndex = index;
        }
        public int GetSelect()
        {
            return layer_dd.SelectedIndex;
        }
        public void SetScript(string[] script)
        {
            textBox1.Text = script[layer_dd.SelectedIndex];
            m_script = script;
        }

        private void layer_dd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_script == null) return;

            textBox1.Text = m_script[layer_dd.SelectedIndex];
        }
    }
}
