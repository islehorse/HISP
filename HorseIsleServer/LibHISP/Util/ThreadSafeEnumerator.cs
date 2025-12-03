using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Util
{
    public class ThreadSafeEnumerator<T> : IEnumerator<T>
    {
        IEnumerator<T> enumerator = null;
        private Mutex enumLock = new Mutex();

        public ThreadSafeEnumerator(IEnumerator<T> baseEnum)
        {
            this.enumerator = baseEnum; 
        }

        public T Current
        {
            get
            {
                lock(enumLock)
                {
                    return this.enumerator.Current;
                }
            }

        }

        object IEnumerator.Current
        {
            get
            {
                lock(enumLock)
                {
                    return this.enumerator.Current;
                }
            }
        }

        public void Dispose()
        {
            lock(enumLock)
            {
                this.enumerator.Dispose();
            }
            enumLock.Dispose();
            enumLock = null;
        }

        public bool MoveNext()
        {
            lock(enumLock)
            {
                return this.enumerator.MoveNext();
            }
        }

        public void Reset()
        {
            lock (enumLock)
            {
                this.enumerator.Reset();
            }
        }
    }
}
