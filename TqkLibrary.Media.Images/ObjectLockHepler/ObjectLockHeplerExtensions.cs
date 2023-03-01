using System.Threading;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObjectLockHeplerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ObjectLockHepler<T> LockHepler<T>(this T @object, CancellationToken cancellationToken = default) where T : class
        {
            ObjectLockHepler<T> bitmapLockHepler = new ObjectLockHepler<T>(@object);
            bitmapLockHepler.Lock(cancellationToken);
            return bitmapLockHepler;
        }
    }
}
