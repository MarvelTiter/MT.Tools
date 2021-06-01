using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Shared.ReflectionUtils.Core {
    public static class MemberInfoExtension {

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public static void Invoke(this object obj, string methodName, params object[] args) {
            var type = obj.GetType();
            var parameter = Expression.Parameter(type, "e");
            var callExpression = Expression.Call(parameter, type.GetMethod(methodName, args.Select(o => o.GetType()).ToArray()), args.Select(Expression.Constant));
            Expression.Lambda(callExpression, parameter).Compile().DynamicInvoke(obj);
        }

        /// <summary>
        /// 调用方法（有返回值）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Invoke<T>(this object obj, string methodName, params object[] args) {
            var type = obj.GetType();
            var parameter = Expression.Parameter(type, "e");
            var callExpression = Expression.Call(parameter, type.GetMethod(methodName, args.Select(o => o.GetType()).ToArray()), args.Select(Expression.Constant));
            return (T)Expression.Lambda(callExpression, parameter).Compile().DynamicInvoke(obj);
        }

        /// <summary>
        /// 设置属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public static void Set<T>(this object self, PropertyInfo prop, T value) {
            var valueType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (prop.PropertyType != valueType) {
                throw new ArgumentException($"can not cast {valueType} to {prop.PropertyType}");
            }
            if (!prop.CanWrite) {
                throw new ArgumentException($"{prop.Name} is readonly");
            }
            // e.XXX
            ParameterExpression parameter = Expression.Parameter(self.GetType(), "e");
            MemberExpression memberExp = Expression.Property(parameter, prop);
            var before = (T)Expression.Lambda(memberExp, parameter).Compile().DynamicInvoke(self);
            if (Equals(value, before)) {
                return;
            }
            MethodCallExpression body = Expression.Call(parameter, prop.SetMethod, Expression.Constant(value, valueType));
            Expression.Lambda(body, parameter).Compile().DynamicInvoke(self);
        }

        public static void Set<T>(this object self, string propName, T value) {
            var prop = self.GetType().GetProperty(propName);
            self.Set<T>(prop, value);
        }
    }
}
