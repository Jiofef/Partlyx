using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    public class DICommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DICommandFactory(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        T ICommandFactory.Create<T>(params object[] args)
        {
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), args);
        }

        async Task<T> ICommandFactory.CreateAsync<T>(params object[] args)
        {
            var creator = CreateAsyncCache<T>.Creator;

            if (creator != null)
                return await creator(_serviceProvider, args).ConfigureAwait(false);

            // fallback: sync create
            var instance = ((ICommandFactory)this).Create<T>(args);

            if (instance is IAsyncInitializable asyncInit)
                await asyncInit.InitializeAsync(args).ConfigureAwait(false);

            return instance;
        }

        // ================= CACHE =================

        private static class CreateAsyncCache<T> where T : class
        {
            public static readonly Func<IServiceProvider, object[], Task<T>>? Creator
                = BuildCreator();

            private static Func<IServiceProvider, object[], Task<T>>? BuildCreator()
            {
                var type = typeof(T);

                var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "CreateAsync" &&
                        (typeof(Task<T>).IsAssignableFrom(m.ReturnType) ||
                         (m.ReturnType.IsGenericType &&
                          m.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
                    );

                if (method == null)
                    return null;

                return async (sp, args) =>
                {
                    var callArgs = BuildArguments(method, sp, args);
                    var result = method.Invoke(null, callArgs);
                    return await AwaitAs<T>(result).ConfigureAwait(false);
                };
            }
        }

        // ================= HELPERS =================

        private static object?[] BuildArguments(MethodInfo method, IServiceProvider sp, object[] args)
        {
            var parameters = method.GetParameters();
            var callArgs = new object?[parameters.Length];

            int offset = 0;

            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IServiceProvider))
            {
                callArgs[0] = sp;
                offset = 1;
            }

            for (int i = offset; i < parameters.Length; i++)
            {
                int src = i - offset;
                callArgs[i] = src < args.Length
                    ? args[src]
                    : GetDefault(parameters[i].ParameterType);
            }

            return callArgs;
        }

        private static async Task<T> AwaitAs<T>(object? obj) where T : class
        {
            if (obj == null)
                throw new InvalidOperationException("CreateAsync returned null");

            if (obj is Task<T> taskT)
                return await taskT.ConfigureAwait(false);

            if (obj is Task task)
            {
                await task.ConfigureAwait(false);
                throw new InvalidOperationException("CreateAsync returned Task without result");
            }

            var type = obj.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var asTask = type.GetMethod("AsTask")!;
                return await (Task<T>)asTask.Invoke(obj, null)!;
            }

            throw new InvalidOperationException("Unsupported CreateAsync return type");
        }

        private static object? GetDefault(Type t)
            => t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}