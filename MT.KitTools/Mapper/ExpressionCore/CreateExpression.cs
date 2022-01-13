﻿using MT.KitTools.ExpressionHelper;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.Mapper.ExpressionCore
{
    internal partial class CreateExpression
    {
        internal static LambdaExpression ExpressionBuilder(MapInfo p)
        {
            var body = new List<Expression>();
            if (p.ActionType == ActionType.NewObj)
            {
                var pExp = Expression.Parameter(typeof(object), "p");
                p.SourceExpression = Expression.Variable(p.SourceType, "source");
                if (p.SourceType.IsValueType)
                {
                    body.Add(Expression.Assign(p.SourceExpression, Expression.Unbox(pExp, p.SourceType)));
                }
                else
                {
                    body.Add(Expression.Assign(p.SourceExpression, Expression.TypeAs(pExp, p.SourceType)));
                }
                p.Parameters.Add(pExp);
                p.Variables.Add(p.SourceExpression as ParameterExpression);
            }
            else if (p.ActionType == ActionType.Assign)
            {               
                p.TargetExpression = Expression.Parameter(p.TargetType, "tar");
                p.SourceExpression = Expression.Parameter(p.SourceType, "from");                
                p.Parameters.Add(p.SourceExpression as ParameterExpression);
                p.Parameters.Add(p.TargetExpression as ParameterExpression);
            }
            else
                throw new ArgumentException($"Unknow ActionType {p.ActionType}");

            var action = GetHandler(p);
            action.Invoke(p, body);
            BlockExpression block = Expression.Block(p.Variables, body);
            LambdaExpression lambda = Expression.Lambda(block, p.Parameters);
            return lambda;
        }
        internal static Action<MapInfo, List<Expression>> GetHandler(MapInfo p)
        {
            var sourceType = p.SourceType;
            var targetType = p.TargetType;
            if (sourceType.IsDictionary())
                return MapFromDictionary;
            else if (targetType.IsDictionary())
                return MapToDictionary;
            else if (sourceType.IsClass && targetType.IsClass)
                return ClassMap;
            //else if (sourceType.IsICollectionType() && targetType.IsICollectionType())
            //    return CollectionMap;
            throw new NotImplementedException($"not implement map between {sourceType.Name} and {targetType.Name}");
        }

        //internal static Action<MapInfo, List<Expression>> GetHandler2(MapInfo p)
        //{
        //    var sourceType = p.SourceType;
        //    var targetType = p.TargetType;
        //    if (sourceType.IsDictionary())
        //        return MapFromDictionary;
        //    else if (targetType.IsDictionary())
        //        return MapToDictionary;
        //    else if (sourceType.IsClass && targetType.IsClass)
        //        return ClassMap;
        //    else if (sourceType.IsICollectionType() && targetType.IsICollectionType())
        //        return CollectionMap;
        //    throw new NotImplementedException($"not implement map between {sourceType.Name} and {targetType.Name}");
        //}
    }

}
