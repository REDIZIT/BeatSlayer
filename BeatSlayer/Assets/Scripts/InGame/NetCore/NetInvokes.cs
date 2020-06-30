using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace GameNet.Invokes
{
    /*public interface ISubs
    {
        Task OnTest(IInvokes invoke);
    }
    public interface IInvokes
    {
        Task Test();
    }

    /// <summary> Extension class enables Client code to bind onto the method names and parameters on <see cref="IInvokes"/> with a guarantee of correct method names. </summary>
    public static class HubConnectionBindExtensions
    {
        public static IDisposable BindOnInterface<T>(this HubConnection connection, Expression<Func<ISubs, Func<T, Task>>> boundMethod, Action<T> handler)
            => connection.On<T>(_GetMethodName(boundMethod), handler);

        public static IDisposable BindOnInterface<T1, T2>(this HubConnection connection, Expression<Func<ISubs, Func<T1, T2, Task>>> boundMethod, Action<T1, T2> handler)
            => connection.On<T1, T2>(_GetMethodName(boundMethod), handler);

        public static IDisposable BindOnInterface<T1, T2, T3>(this HubConnection connection, Expression<Func<ISubs, Func<T1, T2, T3, Task>>> boundMethod, Action<T1, T2, T3> handler)
            => connection.On<T1, T2, T3>(_GetMethodName(boundMethod), handler);

        private static string _GetMethodName<T>(Expression<T> boundMethod)
        {
            var unaryExpression = (UnaryExpression)boundMethod.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object;
            var methodInfo = (MethodInfo)methodInfoExpression.Value;
            return methodInfo.Name;
        }
    }*/
}