using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    public class DICommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public DICommandFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        T ICommandFactory.Create<T>(params object[] args)
        {
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), args);
        }

        async Task<T> ICommandFactory.CreateAsync<T>(params object[] args)
        {
            var t = typeof(T);

            // Trying to find static CreateAsync
            var staticCreate = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => string.Equals(m.Name, "CreateAsync", StringComparison.OrdinalIgnoreCase)
                                     && (typeof(Task).IsAssignableFrom(m.ReturnType) || typeof(ValueTask).IsAssignableFrom(m.ReturnType)));

            if (staticCreate != null)
            {
                // 1) Preparing args
                var parameters = staticCreate.GetParameters();
                object[] callArgs;
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IServiceProvider))
                {
                    callArgs = new object[parameters.Length];
                    callArgs[0] = _serviceProvider;
                    // Filling the remaining arguments with arguments from args
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        if (i - 1 < args.Length) callArgs[i] = args[i - 1];
                        else callArgs[i] = GetDefault(parameters[i].ParameterType);
                    }
                }
                else
                {
                    // Same as above, but without adding _serviceProvider
                    callArgs = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i < args.Length) callArgs[i] = args[i];
                        else callArgs[i] = GetDefault(parameters[i].ParameterType);
                    }
                }

                var result = staticCreate.Invoke(null, callArgs);
                return await AwaitTaskResultAs<T>(result);
            }

            // 2) If command doesn't have CreateAsync method, we create an instance synchronously with ActivatirUtilities
            var instance = ((ICommandFactory)this).Create<T>(args);

            // 3) Calling InitializeAsync, if command supports async init
            if (instance is IAsyncInitializable asyncInit)
            {
                await asyncInit.InitializeAsync(args);
            }

            return instance;
        }

        private static async Task<T> AwaitTaskResultAs<T>(object? taskObj) where T : class
        {
            if (taskObj == null) return null!;

            // Task<T>
            var taskType = taskObj.GetType();
            if (typeof(Task).IsAssignableFrom(taskType))
            {
                var awaiter = (Task)taskObj;
                await awaiter.ConfigureAwait(false);

                // Return result, if it is Task<TResult>
                if (taskType.IsGenericType)
                {
                    var resultProp = taskType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                    if (resultProp != null)
                    {
                        return (T)resultProp.GetValue(taskObj)!;
                    }
                }

                return null!;
            }

            // Throw an exception if it is ValueTask<T>
            throw new InvalidOperationException("CreateAsync returned non-Task object");
        }

        private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}
