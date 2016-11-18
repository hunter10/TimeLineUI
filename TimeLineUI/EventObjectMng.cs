using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TimeLineUI
{
    /*
    public class EventObjectGrp
    {
        //private int nKey;    // Key = 틱값
        public List<EventObject> lstObjects = new List<EventObject>();
        public int tickKey { get; set; }

        public EventObjectGrp(int tickIdx)
        {
            tickKey = tickIdx;
        }

        public void Add_Event(EventObject obj)
        {
            lstObjects.Add(obj);
        }

        public EventObject Get_Event(int index)
        {
            if (index > -1)
                return lstObjects[index];

            return null;
        }

        public void Remove_Event(int index)
        {
            lstObjects.RemoveAt(index);
        }


        // 오브젝트들 그리기
        public void DrawEvents(Graphics g)
        {
            foreach(EventObject obj in lstObjects)
            {
                obj.DrawMark(g);
            }
        }

        public SelectObject CheckPos(Point p)
        {
            foreach (EventObject obj in lstObjects)
            {
                SelectObject eObj = obj.CheckBoxPos(p);
                if (eObj != null) return eObj;
            }

            return null;
        }
    }
    */

    public class EventObjectMng
    {
        private SortedDictionary<int, List<EventObject>> dicEvents = new SortedDictionary<int, List<EventObject>>();
        public SortedDictionary<int, List<EventObject>> GetDic()
        {
            return dicEvents;
        }

        public int GetCount()
        {
            return dicEvents.Count;
        }

        /*
        public void Add_Group(EventObjectGrp val)
        {
            if (!dicEvents.ContainsKey(val.tickKey))
                dicEvents.Add(val.tickKey, val);
        }

        public EventObjectGrp Get_Group(int key)
        {
            if (!dicEvents.ContainsKey(key)) return null;
            return dicEvents[key];
        }

        public bool Find_Group(int key)
        {
            return dicEvents.ContainsKey(key);
        }

        public void Remove_Group(int key)
        {
            if (!dicEvents.ContainsKey(key)) return;
            dicEvents.Remove(key);
        }
        */

        // 키 값이 그룹역활을 함.
        //public void Add_EventObjects(int key, List<EventObject> val)
        //{
        //    if (!dicEvents.ContainsKey(key))
        //        dicEvents.Add(key, val);
        //}

        public void Add_EventObject(int key, EventObject val)
        {
            if (dicEvents.ContainsKey(key))
            {
                (dicEvents[key]).Add(val);
            }
            else
            {
                List<EventObject> temp = new List<EventObject>();
                temp.Add(val);
                dicEvents.Add(key, temp);
            }
        }

        public List<EventObject> Get_EventObjects(int key)
        {
            if (dicEvents.ContainsKey(key))
            {
                return dicEvents[key];
            }

            return null;
        }

        public EventObject Get_EventObject(int key, int index)
        {
            if (dicEvents.ContainsKey(key))
            {
                return (dicEvents[key])[index];
            }

            return null;
        }

        //public void Remove_EventObject(int key, int index)
        //{
        //    EventObjectGrp workGrp = Get_Group(key);
        //    if (workGrp == null) return;

        //    workGrp.Remove_Event(index);
        //}

        public void DrawEvents(Graphics g)
        {
            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                foreach(EventObject lstObj in obj.Value)
                {
                    lstObj.DrawMark(g);
                }
            }
        }

        public SelectObject CheckPos(Point p)
        {
            foreach (KeyValuePair<int, List<EventObject>> obj in dicEvents)
            {
                foreach (EventObject lstObj in obj.Value)
                {
                    SelectObject eObj = lstObj.CheckBoxPos(p);
                    if (eObj != null) return eObj;
                }
            }

            return null;
        }
    }
}
