using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Automation.Images.MvcHelpers.Internal
{
    /// <summary>
    /// Invokes a routed handler method via reflection, supporting flexible signatures:
    /// sync/async, return <c>string[]</c>/<see cref="IEnumerable{T}"/> of string/<c>void</c> and
    /// their <see cref="Task"/>/<see cref="ValueTask"/> variants; <see cref="MvcContext"/> /
    /// <see cref="CancellationToken"/> parameters plus any other parameter resolved from
    /// <see cref="MvcContext.Services"/>.
    /// </summary>
    internal static class HandlerInvoker
    {
        public static async Task<IEnumerable<string>> InvokeAsync(MethodInfo method, object controller, MvcContext context, CancellationToken cancellationToken)
        {
            object?[] args = BuildArgs(method, context, cancellationToken);

            object? ret;
            try
            {
                ret = method.Invoke(controller, args);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw; // unreachable
            }

            object? value = await UnwrapAsync(method.ReturnType, ret).ConfigureAwait(false);
            return Normalize(value, method);
        }

        static object?[] BuildArgs(MethodInfo method, MvcContext context, CancellationToken cancellationToken)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object?[] args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                Type pt = p.ParameterType;

                if (pt.IsInstanceOfType(context))
                {
                    args[i] = context;
                }
                else if (pt == typeof(CancellationToken))
                {
                    args[i] = cancellationToken;
                }
                else
                {
                    object? svc = context.Services?.GetService(pt);
                    if (svc is not null)
                        args[i] = svc;
                    else if (p.HasDefaultValue)
                        args[i] = p.DefaultValue;
                    else
                        throw new InvalidOperationException(
                            $"Cannot resolve parameter '{p.Name}' of type {pt} for handler {method.DeclaringType?.Name}.{method.Name}");
                }
            }
            return args;
        }

        static async Task<object?> UnwrapAsync(Type returnType, object? ret)
        {
            if (ret is null) return null;

            if (typeof(Task).IsAssignableFrom(returnType))
            {
                Task task = (Task)ret;
                await task.ConfigureAwait(false);
                return returnType.IsGenericType ? GetResult(task) : null; // Task<T> vs Task
            }

            if (returnType == typeof(ValueTask))
            {
                await ((ValueTask)ret).ConfigureAwait(false);
                return null;
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                Task task = (Task)returnType.GetMethod(nameof(ValueTask<int>.AsTask))!.Invoke(ret, null)!;
                await task.ConfigureAwait(false);
                return GetResult(task);
            }

            return ret; // synchronous value
        }

        static object? GetResult(Task task) => task.GetType().GetProperty("Result")?.GetValue(task);

        static IEnumerable<string> Normalize(object? value, MethodInfo method) => value switch
        {
            null => Enumerable.Empty<string>(),
            IEnumerable<string> names => names,
            _ => throw new InvalidOperationException(
                $"Handler {method.DeclaringType?.Name}.{method.Name} must return IEnumerable<string> (e.g. string[]) or void, got {value.GetType()}"),
        };
    }
}
