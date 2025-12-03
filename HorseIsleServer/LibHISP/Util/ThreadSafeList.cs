using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Util
{
    public class ThreadSafeList<T> : List<T>
    {
        private Mutex listLock = new Mutex();

        new public T this[int key]
        {
            get
            {
                lock(listLock)
                {
                    T val = base[key];
                    return val;
                }
            }
        }

        new public void AddRange(IEnumerable<T> collection)
        {
            lock(listLock)
            {
                base.AddRange(collection);
            }
        }
        new public void Add(T value)
        {
            lock (listLock)
            {
                base.Add(value);
            }
        }
        new public void Clear()
        {
            lock(listLock)
            {
                base.Clear();
            }
        }

        new public bool Contains(T value)
        {
            lock(listLock)
            {
                return base.Contains(value);
            }
        }
        new public IEnumerator GetEnumerator()
        {
            lock(listLock)
            {
                return new ThreadSafeEnumerator<T>(base.GetEnumerator());
            }
        }

        new public void Insert(int index, T value)
        {
            lock(listLock)
            {
                base.Insert(index, value);
            }
        }

        new public void Remove(T value)
        {
            lock(listLock)
            {
                base.Remove(value);
            }
        }

        new public void RemoveAt(int index)
        {
            lock(listLock)
            {
                base.RemoveAt(index);
            }
        }

    }
}
