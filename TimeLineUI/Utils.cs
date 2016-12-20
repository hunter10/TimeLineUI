using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TimeLineUI
{
    public class Utils
    {   
        public static int DEFAULT_SCREEN_WIDTH = 1280;
        public static int DEFAULT_SCREEN_HEIGHT = 720;

        public static string ByteToString(byte[] temp)
        {
            int i;
            for (i = 0; i < temp.Length; i++) if (temp[i] == 0) break;
            return System.Text.Encoding.Default.GetString(temp, 0, i);
        }

        public static byte[] IntToByte(int val)
        {
            byte[] temp = new byte[4];
            temp[3] = (byte)((val & 0xff000000) >> 24); // 알파
            temp[2] = (byte)((val & 0x00ff0000) >> 16); // RED
            temp[1] = (byte)((val & 0x0000ff00) >> 8);  // GREEN
            temp[0] = (byte)((val & 0x000000ff));       // BLUE
            return temp;
            //return BitConverter.GetBytes(val);
        }

        public static int GetFileType(string exe)
        {
            if (exe.Equals(".png") || exe.Equals(".PNG"))
                return 1;
            else if (exe.Equals(".mp3")) //|| exe.Equals(".wav"))
                return 2;
            else if (exe.Equals(".mp4"))
                return 3;
            else if (exe.Equals(".spr"))
                return 4;
            else if( exe.Equals(".ani"))
                return 5;
            else if (exe.Equals(".ect"))
                return 6;
            else if (exe.Equals(".atlas"))
                return 7;
            else if (exe.Equals(".json"))
                return 10;
            else if (exe.Equals(".skel"))
                return 11;
            else if (exe.Equals(".spine"))
                return 12;
            else if (exe.Equals(".grp"))
                return 8;
            else if (exe.Equals(".proj"))
                return 9;

            return 0;
        }

        public static Rectangle GetScrollSize(int sx, int sy, int wid, int hei)
        {
            Rectangle rc = new Rectangle(sx, sy, wid, hei);
            return rc;
        }
        
        public static Size GetScrollSize(Rectangle[] rArr, int LimitWid, int space_x, int space_y, float scale)
        {
            Rectangle rc = new Rectangle();
            
            for (int n = 0; n < rArr.Length; n++)
            {
                rc.Width += ((int)(rArr[n].Width* scale) + space_x);
                if (rc.Height < ((int)(rArr[n].Height * scale) + space_y))
                    rc.Height = ((int)(rArr[n].Height * scale) + space_y);
            }

            rc.Width += space_x;
            rc.Height += space_y;

            return new Size(rc.Width, rc.Height);
        }

        public static Size GetScaleSize(Size ResSize, float scale)
        {
            return new Size((int)(ResSize.Width*scale), (int)(ResSize.Height * scale));
        }

        public static TreeNode SerchNode(TreeNodeCollection objNodes, string text)
        {
            foreach(TreeNode node in objNodes)
            {
                if (node.Text.Equals(text)) return node;

                TreeNode findNode = SerchNode(node.Nodes, text);
                if (findNode != null) return findNode;
            }
            return null;
        }

        public static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        static Dictionary<int, int> UniqueID = new Dictionary<int, int>();
        static int countUniqueID = 0;


        public static void Init_UniqueID()
        {
            UniqueID.Clear();
            countUniqueID = 0;
        }

        public static int GetUniqueID()
        {
            countUniqueID++;

            if (!UniqueID.ContainsKey(countUniqueID))
                UniqueID.Add(countUniqueID, countUniqueID);
            else
                return GetUniqueID();

            return countUniqueID;
        }

        public static void setUniqueID(int ID)
        {
            UniqueID.Add(ID, ID);
        }

        public static int getID_gObject(int layer, int num)
        {
            int id = layer * 1000 + num;
            return id;
        }
    }
}
