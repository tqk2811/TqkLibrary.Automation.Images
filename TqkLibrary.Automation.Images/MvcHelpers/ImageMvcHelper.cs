using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.MvcHelpers.Internal;
using TqkLibrary.Automation.Images.WaitImageHelpers;

namespace TqkLibrary.Automation.Images.MvcHelpers
{
    /// <summary>
    /// MVC-style image router. Finds the next image, routes it to the matching
    /// <see cref="ImageNameAttribute"/> handler, then searches the names the handler returns,
    /// looping until a handler returns no names (completed) or a step times out.
    /// </summary>
    public class ImageMvcHelper<TColor, TDepth>
            where TColor : struct, IColor
            where TDepth : new()
    {
        sealed class RouteEntry
        {
            public required Type ControllerType { get; init; }
            public required MethodInfo Method { get; init; }
        }

        readonly WaitImageHelper<TColor, TDepth> _waiter;
        readonly List<Type> _controllerTypes = new List<Type>();
        IServiceProvider? _services;

        /// <summary>
        ///
        /// </summary>
        /// <param name="waiter">Configured waiter providing capture/template/crop/matchrate/timeout.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ImageMvcHelper(WaitImageHelper<TColor, TDepth> waiter)
        {
            _waiter = waiter ?? throw new ArgumentNullException(nameof(waiter));
        }

        /// <summary>
        /// Service provider used to resolve controllers and handler parameters.
        /// </summary>
        public ImageMvcHelper<TColor, TDepth> WithServiceProvider(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            return this;
        }

        /// <summary>
        /// Register a controller type whose <see cref="ImageNameAttribute"/> methods become routes.
        /// </summary>
        public ImageMvcHelper<TColor, TDepth> AddController<TController>() => AddController(typeof(TController));

        /// <summary>
        /// Register a controller type whose <see cref="ImageNameAttribute"/> methods become routes.
        /// </summary>
        public ImageMvcHelper<TColor, TDepth> AddController(Type controllerType)
        {
            if (controllerType is null) throw new ArgumentNullException(nameof(controllerType));
            if (!_controllerTypes.Contains(controllerType))
                _controllerTypes.Add(controllerType);
            return this;
        }

        /// <summary>
        /// Register every type in <paramref name="assembly"/> that has at least one
        /// <see cref="ImageNameAttribute"/> method.
        /// </summary>
        public ImageMvcHelper<TColor, TDepth> AddControllersFromAssembly(Assembly assembly)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            foreach (Type type in assembly.GetTypes())
            {
                if (GetHandlerMethods(type).Any())
                    AddController(type);
            }
            return this;
        }

        /// <summary>
        /// Run the router starting from <paramref name="entryNames"/>.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<MvcRunResult> StartAsync(params string[] entryNames)
        {
            if (entryNames is null || entryNames.Length == 0)
                throw new ArgumentException($"{nameof(entryNames)} must contain at least one image name");

            Dictionary<string, RouteEntry> routes = BuildRoutes();
            List<FindHistory> history = new List<FindHistory>();
            Dictionary<Type, object> controllerCache = new Dictionary<Type, object>();
            CancellationToken cancellationToken = _waiter.CancellationToken;

            string[] currentNames = entryNames;
            while (currentNames.Length > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (string name in currentNames)
                {
                    if (!routes.ContainsKey(name))
                        throw new InvalidOperationException($"No [{nameof(ImageNameAttribute)}] handler registered for image '{name}'");
                }

                WaitImageResult<TColor, TDepth> result = await _waiter.WaitUntil(currentNames).StartAsync().ConfigureAwait(false);
                WaitImageDataResult? data = result.Points.FirstOrDefault();
                if (data is null)
                    return new MvcRunResult(MvcRunReason.TimedOut, history);

                if (!routes.TryGetValue(data.Name, out RouteEntry? route))
                    throw new InvalidOperationException($"No [{nameof(ImageNameAttribute)}] handler registered for image '{data.Name}'");

                object? controller = route.Method.IsStatic ? null : ResolveController(route.ControllerType, controllerCache);
                MvcContext context = new MvcContext
                {
                    ImageName = data.Name,
                    Index = data.Index,
                    Result = data.FindResult,
                    Histories = history.ToArray(),
                    Services = _services,
                    CancellationToken = cancellationToken,
                };

                IEnumerable<string> next = await HandlerInvoker.InvokeAsync(route.Method, controller!, context, cancellationToken).ConfigureAwait(false);
                history.Add(new FindHistory(data.Name, data.Index, data.FindResult));
                currentNames = next?.ToArray() ?? Array.Empty<string>();
            }

            return new MvcRunResult(MvcRunReason.Completed, history);
        }

        Dictionary<string, RouteEntry> BuildRoutes()
        {
            Dictionary<string, RouteEntry> routes = new Dictionary<string, RouteEntry>();
            foreach (Type type in _controllerTypes)
            {
                foreach (MethodInfo method in GetHandlerMethods(type))
                {
                    ImageNameAttribute attribute = method.GetCustomAttribute<ImageNameAttribute>()!;
                    foreach (string name in attribute.Names)
                    {
                        if (routes.TryGetValue(name, out RouteEntry? existing))
                            throw new InvalidOperationException(
                                $"Image name '{name}' is handled by multiple methods: " +
                                $"{existing.Method.DeclaringType?.Name}.{existing.Method.Name} and {type.Name}.{method.Name}");
                        routes[name] = new RouteEntry { ControllerType = type, Method = method };
                    }
                }
            }
            if (routes.Count == 0)
                throw new InvalidOperationException($"No [{nameof(ImageNameAttribute)}] handlers found in the registered controllers");
            return routes;
        }

        static IEnumerable<MethodInfo> GetHandlerMethods(Type type)
            => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                   .Where(m => m.GetCustomAttribute<ImageNameAttribute>() is not null);

        object ResolveController(Type type, Dictionary<Type, object> cache)
        {
            if (cache.TryGetValue(type, out object? cached)) return cached;
            object instance = _services is not null
                ? ActivatorUtilities.GetServiceOrCreateInstance(_services, type)
                : Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Cannot create controller {type}");
            cache[type] = instance;
            return instance;
        }
    }

    /// <summary>
    /// <see cref="ImageMvcHelper{TColor, TDepth}"/> for <see cref="Bgr"/>/<see cref="byte"/>.
    /// </summary>
    public class ImageMvcHelperBgr : ImageMvcHelper<Bgr, byte>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="waiter"></param>
        public ImageMvcHelperBgr(WaitImageHelper<Bgr, byte> waiter) : base(waiter)
        {
        }
    }
}
