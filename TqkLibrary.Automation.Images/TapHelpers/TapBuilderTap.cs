using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.TapHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public class TapBuilderTap
    {
        readonly TapBuilder _tapBuilder;
        readonly bool _isTap;
        readonly HashSet<string> _names = new HashSet<string>();
        Func<TapHelper, Point, Task<Point>>? _reCalcPoint;

        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap(TapBuilder tapBuilder, bool isTap)
        {
            this._tapBuilder = tapBuilder;
            this._isTap = isTap;
        }

        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap ReCalcPoint(Func<TapHelper, Point, Task<Point>> reCalcPoint)
        {
            _reCalcPoint = reCalcPoint;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap Name(params string[] names)
        {
            foreach (var item in names)
            {
                _names.Add(item);
            }
            return this;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionShould"></param>
        /// <returns></returns>
        public TapBuilder AfterTap(ActionShould actionShould)
        {
            _tapBuilder.BaseTap(_isTap, actionShould, _reCalcPoint, _names.ToArray());
            return _tapBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TapBuilder Continue() => AfterTap(ActionShould.Continue);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TapBuilder Break() => AfterTap(ActionShould.Break);





        /// <summary>
        /// 
        /// </summary>
        public static async Task<Point> CenterH(TapHelper tapHelper, Point point)
        {
            Size size = await tapHelper.GetScreenSizeAsync();
            return new Point(size.Width / 2, point.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        public static async Task<Point> CenterV(TapHelper tapHelper, Point point)
        {
            Size size = await tapHelper.GetScreenSizeAsync();
            return new Point(point.X, size.Height / 2);
        }
    }
}
