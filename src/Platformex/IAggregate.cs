﻿using System.Threading.Tasks;
using Orleans;

namespace Platformex
{
    public interface IAggregate : IGrainWithStringKey
    {
        Task<CommandResult> DoAsync(ICommand command);
    }

    public interface IAggregate<out T> : IAggregate where T : Identity<T>
    {
    }
}