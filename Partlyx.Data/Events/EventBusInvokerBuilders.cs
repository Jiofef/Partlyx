using System.Linq.Expressions;
using System.Reflection;

namespace Partlyx.Infrastructure.Events;

public sealed partial class EventBus : IEventBus, IDisposable
{
    // Create Action<object, object> invoker: (target, event) => ((DeclaringType)target).Method((EventType)event)
    private static Action<object?, object?> CreateSyncInvoker(MethodInfo method, Type eventType)
    {
        // Parameters
        var targetParam = Expression.Parameter(typeof(object), "target");
        var eventParam = Expression.Parameter(typeof(object), "evt");

        Expression instanceExpr = null!;
        if (!method.IsStatic)
        {
            instanceExpr = Expression.Convert(targetParam, method.DeclaringType!);
        }

        var eventCast = Expression.Convert(eventParam, eventType);

        Expression callExpr = method.IsStatic
            ? Expression.Call(method, eventCast)
            : Expression.Call(instanceExpr, method, eventCast);

        var lambda = Expression.Lambda<Action<object?, object?>>(callExpr, targetParam, eventParam);
        return lambda.Compile();
    }

    // Create Func<object, object, Task> invoker for async methods
    private static Func<object?, object?, Task> CreateAsyncInvoker(MethodInfo method, Type eventType)
    {
        var targetParam = Expression.Parameter(typeof(object), "target");
        var eventParam = Expression.Parameter(typeof(object), "evt");

        Expression instanceExpr = null!;
        if (!method.IsStatic)
        {
            instanceExpr = Expression.Convert(targetParam, method.DeclaringType!);
        }

        var eventCast = Expression.Convert(eventParam, eventType);

        Expression callExpr = method.IsStatic
            ? Expression.Call(method, eventCast)
            : Expression.Call(instanceExpr, method, eventCast);

        // Ensure result is Task
        if (method.ReturnType == typeof(void))
        {
            // Wrap void into Task.CompletedTask
            var block = Expression.Block(callExpr, Expression.Call(typeof(Task).GetProperty(nameof(Task.CompletedTask))!.GetMethod!, Array.Empty<Expression>()));
            var lambda = Expression.Lambda<Func<object?, object?, Task>>(block, targetParam, eventParam);
            return lambda.Compile();
        }
        else if (typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            var lambda = Expression.Lambda<Func<object?, object?, Task>>(Expression.Convert(callExpr, typeof(Task)), targetParam, eventParam);
            return lambda.Compile();
        }
        else
        {
            // Method returns something else -- call and wrap into completed task
            var toStringCall = Expression.Call(Expression.Convert(callExpr, typeof(object)), typeof(object).GetMethod(nameof(ToString))!);
            var block = Expression.Block(callExpr, Expression.Call(typeof(Task).GetProperty(nameof(Task.CompletedTask))!.GetMethod!, Array.Empty<Expression>()));
            var lambda = Expression.Lambda<Func<object?, object?, Task>>(block, targetParam, eventParam);
            return lambda.Compile();
        }
    }
}