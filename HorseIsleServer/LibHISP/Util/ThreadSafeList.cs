﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Util
{
    public class ThreadSafeList<T> : List<T>
    {
        private Mutex listLock = new Mutex();
        new public void AddRange(IEnumerable<T> collection)
        {
            listLock.WaitOne();
            base.AddRange(collection);
            listLock.ReleaseMutex();
        }
        new public void Add(T value)
        {
            listLock.WaitOne();
            base.Add(value);
            listLock.ReleaseMutex();
        }
        new public void Clear()
        {
            listLock.WaitOne();
            base.Clear();
            listLock.ReleaseMutex();
        }

        new public bool Contains(T value)
        {
            listLock.WaitOne();
            bool test = base.Contains(value);
            listLock.ReleaseMutex();
            
            return test;
        }
        new public IEnumerator GetEnumerator()
        {
            listLock.WaitOne();
            IEnumerator res = base.ToArray().GetEnumerator();
            listLock.ReleaseMutex();
            return res;
        }

        new public void Insert(int index, T value)
        {
            listLock.WaitOne();
            base.Insert(index, value);
            listLock.ReleaseMutex();
        }

        new public void Remove(T value)
        {
            listLock.WaitOne();
            base.Remove(value);
            listLock.ReleaseMutex();
        }

        new public void RemoveAt(int index)
        {
            listLock.WaitOne();
            base.RemoveAt(index);
            listLock.ReleaseMutex();
        }

    }
}
