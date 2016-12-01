using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Reflection;
using TimeLineUI;
using System.Collections;

namespace TimeLineUI_Test
{
    public partial class MainForm : Form
    {
        static int MAX_LAYER = 5;
        string currentProject;
        string[] script_text = new string[MAX_LAYER];
        ArrayList[] scriptList = new ArrayList[MAX_LAYER];
        int ScreenX = 1280, ScreenY = 720;//, TileWidth = 32, TileHeight = 32;

        

        public MainForm()
        {
            InitializeComponent();
        }

        public string[] GetScript()
        {
            return script_text;
        }

        public void SetScript()
        {
            for (int i = 0; i < MAX_LAYER; i++)
            {
                scriptList[i].Clear();

                string[] str = script_text[i].Split('\n');
                for (int n = 0; n < str.Length; n++) scriptList[i].Add(str[n]);
            }
        }

        public void DecodeScript()
        {
            bool GroupMode = false;

            for (int l = 0; l < MAX_LAYER; l++)
            {
                for (int i = 0; i < scriptList[l].Count; i++)
                {
                    string text = (string)scriptList[l][i];

                    //if (text.Length >= 8 && text.Substring(0, 8).Equals("[CAMERA]"))
                    //{
                    //    scriptList[l].RemoveAt(i--);
                    //}
                }
            }
        }

       
        

        

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadEventScriptDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveEventScriptDialog();
        }

        public bool IsOpen { get; private set; }
        
        public void LoadEventScriptDialog()
        {
            OpenFileDialog openPanel = new OpenFileDialog();
            openPanel.InitialDirectory = "d:\\";
            openPanel.Filter = "CustomFile (*.evt)|*.evt|txt (*.txt)|*.txt | All files (*.*)|(*.*)";
            if (openPanel.ShowDialog() == DialogResult.OK)
            {
                LoadEventScript(openPanel.FileName);
                Console.WriteLine("OK      " + openPanel.FileName + " " + openPanel.SafeFileName);
            }
        }

        public void SaveEventScriptDialog()
        {
            // 파일 세이브 다이얼로그

            SaveFileDialog savePanel = new SaveFileDialog();
            savePanel.InitialDirectory = @"d:\";
            savePanel.Filter = "CustomFile (*.evt)|*.evt|txt (*.txt)|*.txt | All files (*.*)|(*.*)";
            if (savePanel.ShowDialog() == DialogResult.OK)
            {
                SaveEventScript(savePanel.FileName);
                Console.WriteLine("OK      " + savePanel.FileName + " ");
            }
        }

        public void LoadEventScript(string fullPath)
        {
            using (FileStream fs = File.OpenRead(fullPath))
            {
                byte[] buffer = new byte[20], value = new byte[4];

                for (int i = 0; i < MAX_LAYER; i++)
                {
                    fs.Read(value, 0, 4);
                    int len = BitConverter.ToInt32(value, 0);
                    byte[] temp = new byte[len];
                    fs.Read(temp, 0, len);
                    script_text[i] = Encoding.UTF8.GetString(temp, 0, len);
                }

                fs.Close();
            }

            timeLineUI1.ScriptParser(script_text, MAX_LAYER);

            
        }
        
        public void SaveEventScript(string fullPath)
        {
            using (FileStream fs = File.OpenWrite(fullPath))
            {
                for (int i = 0; i < MAX_LAYER; i++)
                {
                    int len = System.Text.Encoding.UTF8.GetBytes(script_text[i]).Length;
                    fs.Write(Utils.IntToByte(len), 0, 4);
                    fs.Write(System.Text.Encoding.UTF8.GetBytes(script_text[i]), 0, len);
                }

                fs.Close();
            }
        }


        ScriptViewForm viewForm;
        private void scriptViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewForm = new ScriptViewForm();
            viewForm.SetSelect(0);
            viewForm.SetScript(GetScript());
            viewForm.Open(OnClose_ScriptForm);
        }

        void OnClose_ScriptForm(string script1, string script2, string script3, string script4, string script_ui)
        {
            string[] sc = GetScript();
            sc[0] = script1;
            sc[1] = script2;
            sc[2] = script3;
            sc[3] = script4;
            sc[4] = script_ui;

            Task task1 = Task<int>.Run(() => OnClose_ScriptForm_post());      //스레드 실행
        }

        void OnClose_ScriptForm_post()
        {
            System.Threading.Thread.Sleep(200);
            SetScript();
        }

    }
}


