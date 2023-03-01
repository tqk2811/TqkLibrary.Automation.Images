using System;
using System.Collections.Generic;
using System.Threading;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class ObjectLockHepler<T> : IDisposable
        where T : class
    {
        class DictCountHelper
        {
            public int Count { get { return _count; } }
            int _count = 1;
            public void AddRef()
            {
                Interlocked.Add(ref _count, 1);
            }
            public void UnRef()
            {
                Interlocked.Add(ref _count, -1);
            }
        }
        static readonly Dictionary<object, DictCountHelper> _dictObjectLock = new Dictionary<object, DictCountHelper>();
        /// <summary>
        /// 
        /// </summary>
        public T Object { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="object"></param>
        public ObjectLockHepler(T @object)
        {
            this.Object = @object ?? throw new ArgumentNullException(nameof(@object));
            lock (_dictObjectLock)
            {
                if (_dictObjectLock.ContainsKey(@object)) _dictObjectLock[@object].AddRef();
                else _dictObjectLock[@object] = new DictCountHelper();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        ~ObjectLockHepler()
        {
            Dispose(false);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        void Dispose(bool disposing)
        {
            lock (_dictObjectLock)
            {
                if (isLock) Monitor.Exit(_dictObjectLock[Object]);
                _dictObjectLock[Object].UnRef();
                if (_dictObjectLock[Object].Count == 0) _dictObjectLock.Remove(Object);
            }
        }

        bool isLock = false;

        internal void Lock(CancellationToken cancellationToken = default)
        {
            Monitor.Enter(_dictObjectLock[Object]);
            isLock = true;
        }
    }
}
