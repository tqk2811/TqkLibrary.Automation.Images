using System;
using System.Drawing;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageBuilder
    {
        internal readonly WaitImageHelper waitImageHelper;
        internal WaitImageBuilder(WaitImageHelper waitImageHelper, params string[] finds)
        {
            this.Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this.waitImageHelper = waitImageHelper;
        }

        internal string[] Finds { get; }
        internal TapFlag Tapflag { get; private set; } = TapFlag.None;
        internal bool IsThrow { get; private set; } = false;
        internal bool IsFirst { get; private set; } = true;
        internal bool IsLoop { get; set; } = true;
        internal Func<int, Point, string[], bool> TapCallback { get; private set; } = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapFirst(Func<int, Point, string[], bool> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = true;
            Tapflag = TapFlag.First;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapRandom(Func<int, Point, string[], bool> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.Random;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapAll(Func<int, Point, string[], bool> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.All;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitImageBuilder WithThrow()
        {
            IsThrow = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitImageResult Start()
        {
            return new WaitImageResult(this).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<WaitImageResult> StartAsync()
        {
            return new WaitImageResult(this).StartAsync();
        }
    }
}
