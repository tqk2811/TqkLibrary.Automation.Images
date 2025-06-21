using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using TqkLibrary.Media.Images.WaitImageHelpers;
using TqkLibrary.Media.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Media.Images.TapHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public class TapBuilder
    {
        class TapData
        {
            public bool IsTap { get; set; }
            public ActionShould ActionShould { get; set; }
            public Func<TapHelper, Point, Task<Point>>? ReCalcPoint { get; set; }
        }
        readonly TapData _tapDefault = new()
        {
            IsTap = true,
            ActionShould = ActionShould.Continue,
        };
        readonly Dictionary<string, TapData> _taps = new Dictionary<string, TapData>();
        readonly TapHelper _tapHelper;
        internal TapBuilder(TapHelper tapHelper)
        {
            this._tapHelper = tapHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder Default(bool isTap, ActionShould result)
        {
            _tapDefault.IsTap = isTap;
            _tapDefault.ActionShould = result;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder BaseTap(bool isTap, ActionShould result, params string[] names)
            => BaseTap(isTap, result, null, names);

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder BaseTap(bool isTap, ActionShould result, Func<TapHelper, Point, Task<Point>>? reCalcPoint, params string[] names)
        {
            TapData tapData = new TapData()
            {
                IsTap = isTap,
                ActionShould = result,
                ReCalcPoint = reCalcPoint,
            };
            foreach (var name in names)
            {
                _taps[name] = tapData;
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder Tap(params string[] names)
            => BaseTap(true, ActionShould.Continue, names);

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder TapBreak(params string[] names)
            => BaseTap(true, ActionShould.Break, names);

        /// <summary>
        /// 
        /// </summary>
        public TapBuilder NonTapBreak(params string[] names)
            => BaseTap(false, ActionShould.Break, names);


        async Task<Point> _CenterHAsync(TapHelper tapHelper, Point point)
        {
            Size size = await tapHelper.GetScreenSizeAsync();
            return new Point(size.Width / 2, point.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        public TapBuilder TapCenterH(params string[] names)
            => BaseTap(true, ActionShould.Continue, _CenterHAsync, names);

        async Task<Point> _CenterVAsync(TapHelper tapHelper, Point point)
        {
            Size size = await tapHelper.GetScreenSizeAsync();
            return new Point(point.X, size.Height / 2);
        }
        /// <summary>
        /// 
        /// </summary>
        public TapBuilder TapCenterV(params string[] names)
            => BaseTap(true, ActionShould.Continue, _CenterVAsync, names);



        /// <summary>
        /// 
        /// </summary>
        public async Task<ActionShould> HandlerAsync(WaitImageDataResult dataResult)
        {
            TapData tapData;
            if (_taps.ContainsKey(dataResult.Name))
            {
                tapData = _taps[dataResult.Name];
            }
            else
            {
                tapData = _tapDefault;
            }

            Point point = dataResult.FindResult.Point;
            if (tapData.ReCalcPoint is not null)
            {
                point = await tapData.ReCalcPoint(_tapHelper, point);
            }

            if (tapData.IsTap)
            {
                await _tapHelper.TapAsync(point);
            }

            await _tapHelper.DelayAsync();
            return tapData.ActionShould;
        }
    }
}
