using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineUI
{
    public class EventObjectGrp
    {
        public int nKey;    // Key = 틱값
        public List<EventObject> lstObjects = new List<EventObject>();

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
    }


    public class EventObjectMng
    {
        private SortedDictionary<int, EventObjectGrp> dicEvents = new SortedDictionary<int, EventObjectGrp>();

        public SortedDictionary<int, EventObjectGrp> GetDic()
        {
            return dicEvents;
        }

        public int GetCount()
        {
            return dicEvents.Count;
        }

        public void Add_Group(EventObjectGrp val)
        {
            if (!dicEvents.ContainsKey(val.nKey))
                dicEvents.Add(val.nKey, val);
        }

        public EventObjectGrp Get_Group(int key)
        {
            if (!dicEvents.ContainsKey(key)) return null;

            return dicEvents[key];
        }

        public void Remove_Group(int key)
        {
            if (!dicEvents.ContainsKey(key)) return;
            dicEvents.Remove(key);
        }

        public void Add_EventObject(int key, EventObject val)
        {
            EventObjectGrp workGrp = Get_Group(key);
            if (workGrp == null) return;

            workGrp.Add_Event(val);
        }

        public EventObject Get_EventObject(int key, int index)
        {
            EventObjectGrp workGrp = Get_Group(key);
            if (workGrp == null) return null;

            return workGrp.Get_Event(index);
        }

        public void Remove_EventObject(int key, int index)
        {
            EventObjectGrp workGrp = Get_Group(key);
            if (workGrp == null) return;

            workGrp.Remove_Event(index);
        }
    }
}
