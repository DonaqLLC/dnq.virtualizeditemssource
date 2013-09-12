using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualizedListSample
{
    public class ListItemCache
    {
        private Dictionary<int, WeakReference> cache = new Dictionary<int, WeakReference>();
        public ListItemCache()
        {
        }

        public void Clear()
        {
            cache.Clear();
        }

        public int ItemsInCache
        {
            get
            {
                return cache.Count;
            }
        }

        public System.Windows.Forms.ListViewItem GetAt(int index)
        {
            if (cache.ContainsKey(index))
            {
                if (cache[index].IsAlive && cache[index].Target != null)
                    return (System.Windows.Forms.ListViewItem)cache[index].Target;
                else
                    cache.Remove(index);
            }

            return null;
        }

        public bool Has(int index)
        {
            return (cache.ContainsKey(index) && cache[index].IsAlive && cache[index].Target != null);
        }

        public void PutAt(int index, System.Windows.Forms.ListViewItem item)
        {
            cache[index] = new WeakReference(item);
        }

        public void PurgeStaleItems()
        {
            List<int> keysToRemove = new List<int>();
            foreach (int key in cache.Keys)
            {
                if (cache[key] == null || cache[key].IsAlive || cache[key].Target == null)
                    keysToRemove.Add(key);
            }

            foreach (int key in keysToRemove)
            {
                cache.Remove(key);
            }
        }
    }
}
