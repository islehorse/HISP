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

        public T Current => this.enumerator.Current;

        object IEnumerator.Current => this.enumerator.Current;

        public void Dispose()
        {
            enumLock.WaitOne();
            this.enumerator.Dispose();
            enumLock.ReleaseMutex();
            enumLock.Dispose();
        }

        public bool MoveNext()
        {
            enumLock.WaitOne();
            bool val = this.enumerator.MoveNext();
            enumLock.ReleaseMutex();
            return val;
        }

        public void Reset()
        {
            enumLock.WaitOne();
            this.enumerator.Reset();
            enumLock.ReleaseMutex();
        }
    }
}
