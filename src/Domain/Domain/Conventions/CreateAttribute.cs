﻿using System;

namespace BE.CQRS.Domain.Conventions
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CreateAttribute : BehaviorAttribute
    {
    }
}