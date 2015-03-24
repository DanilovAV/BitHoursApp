using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Text;

namespace BitHoursApp.Updater
{   
    public static class ReflectionHelper
    {      
        public static object GetMemberValue(object sourceObject, string memberName)
        {
            string[] propertyChain = memberName.Split('.');
            object currentObject = sourceObject;
            foreach (string propertyName in propertyChain)
            {
                PropertyInfo property;
                currentObject = GetPropertyValue(currentObject, propertyName, out property);
            }
            return currentObject;
        }

        public static object GetPropertyValue(object sourceObject, string propertyName, out PropertyInfo property)
        {
            object value = null;
            property = GetPropertyInfo(sourceObject, propertyName);

            if (property != null && property.CanRead)
            {
                value = property.GetValue(sourceObject, null);
            }

            return value;
        }

        public static PropertyInfo GetPropertyInfo(object sourceObject, string propertyName)
        {
            PropertyInfo prop = null;

            if (sourceObject != null)
            {
                prop = GetPropertyInfo(sourceObject.GetType(), propertyName);
            }

            return prop;
        }

        public static PropertyInfo GetPropertyInfo(Type type, String propertyName)
        {
            PropertyInfo prop = null;

            if (type != null && !String.IsNullOrEmpty(propertyName))
            {
                prop = type.GetProperty(propertyName);
            }

            return prop;
        }          

        public static string GetPropertyNameFromExpression<T>(Expression<Func<T>> property) 
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression) 
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            } 
            else 
            {
                memberExpression = (MemberExpression)lambda.Body;
            }
            return memberExpression.Member.Name;
        }

        public static IList<KeyValuePair<string, object>> GetObjectsChainFromExpression<T>(object rootObject, Expression<Func<T>> expression)
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();
            var names = GetNamesFromExpression(expression);
            string currentFullName = string.Empty;
            KeyValuePair<string, object> keyValuePair = new KeyValuePair<string, object>(names.First(), rootObject);
            result.Add(keyValuePair);
            foreach (string name in names.Skip(1))
            {
                currentFullName += (currentFullName == string.Empty ? string.Empty : ".") + name;
                keyValuePair = new KeyValuePair<string, object>(name, GetMemberValue(rootObject, currentFullName));
                result.Add(keyValuePair);
            }
            return result;
        }

        public static string GetNamesStringFromExpression<T>(Expression<Func<T>> expression) {
            return String.Join(".", GetNamesFromExpression(expression));
        }

        public static IEnumerable<string> GetNamesFromExpression<T>(Expression<Func<T>> expression) {
            var lambda = (LambdaExpression)expression;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression) {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            } else {
                memberExpression = (MemberExpression)lambda.Body;
            }
            List<string> result = new List<string>();
            Expression currentExpression = memberExpression;
            while (currentExpression is MemberExpression) {
                string memberName = ((MemberExpression)currentExpression).Member.Name;
                result.Insert(0, memberName);
                currentExpression = ((MemberExpression)currentExpression).Expression;
            }
            return result;
        }
         
        public static string GetPath<T>(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;
            Expression expr = expression.Body;
            if (expr is UnaryExpression && expr.NodeType == ExpressionType.Convert && (expr as UnaryExpression).Operand is LambdaExpression)
                expr = ((expr as UnaryExpression).Operand as LambdaExpression).Body;

            while (true)
            {
                if (expr is ParameterExpression)
                    return result;
                if (expr is UnaryExpression)
                    expr = (expr as UnaryExpression).Operand;
                else if (expr is MemberExpression)
                {
                    MemberExpression me = expr as MemberExpression;
                    PropertyInfo pi = me.Member as PropertyInfo;
                    result = pi.Name + (string.IsNullOrEmpty(result) ? string.Empty : ("." + result));
                    expr = me.Expression;
                }
                else if (expr is MethodCallExpression)
                {
                    var methodCallExpession = (MethodCallExpression)expr;
                    result = methodCallExpession.Method.Name + (string.IsNullOrEmpty(result) ? string.Empty : ("." + result));
                    return result;
                }
                else
                    throw new ArgumentException(String.Format("Can not use expression '{0}' as property path. Check all path elements to be properties.", expression), "expression");
            }
        }
     
        public static string GetPath<T>(this T owner, Expression<Func<T, object>> expression)
        {
            return GetPath(expression);
        }
      
        public static IEnumerable<string> GetPaths<T>(this T owner, params Expression<Func<T, object>>[] expressions)
        {
            return expressions.Select(GetPath);
        }     
    }
}