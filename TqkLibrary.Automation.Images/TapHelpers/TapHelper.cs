using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Automation.Images.TapHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TapHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int DelayAfterTap { get; set; } = 300;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<Size> GetScreenSizeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task TapAsync(Point point, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        public virtual Task DelayAsync(CancellationToken cancellationToken = default) => Task.Delay(DelayAfterTap, cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        public virtual TapBuilder Build() => new TapBuilder(this);
    }
}
