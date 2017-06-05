﻿using AutoCompare.Helpers;
using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    internal class EnumerableConfiguration : MemberConfiguration
    {
        public Expression Matcher { get; protected set; }
        public Type MatcherType { get; protected set; }
        public string Match { get; protected set; }
        public object DefaultId { get; protected set; }
    }

    internal class EnumerableConfiguration<T> : EnumerableConfiguration, IEnumerableConfiguration<T> where T : class
    {
        public IEnumerableConfiguration<T> MatchUsing<TMember>(Expression<Func<T, TMember>> member)
        {
            return MatchUsing(member, default(TMember));
        }

        public IEnumerableConfiguration<T> MatchUsing<TMember>(Expression<Func<T, TMember>> member, TMember defaultId)
        {
            DefaultId = defaultId;
            Match = ReflectionHelper.GetMemberInfo(member).Name;
            Matcher = member;
            MatcherType = typeof(TMember);
            return this;
        }
    }
}
