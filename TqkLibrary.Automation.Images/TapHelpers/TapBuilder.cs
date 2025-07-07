using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.WaitImageHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.TapHelpers
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
        public TapBuilderTap Tap() => new TapBuilderTap(this, true);
        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap NonTap() => new TapBuilderTap(this, false);

        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap Tap(params string[] names) => new TapBuilderTap(this, true).Name(names);
        /// <summary>
        /// 
        /// </summary>
        public TapBuilderTap NonTap(params string[] names) => new TapBuilderTap(this, false).Name(names);


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
