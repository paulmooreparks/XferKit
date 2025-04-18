﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services.Impl;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand;

[Command("store", "Manage the key/value store")]
internal class StoreCommand {
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;
    private readonly IStoreService _store;

    public StoreCommand(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        IStoreService store
        ) 
    {
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _store = store;
    }

    public async Task<int> Execute(
        Command command,
        InvocationContext context
        ) 
    {
        var replContext = new SubcommandReplContext(
            _workspaceService,
            new CommandSplitter()
            );

        var result = await command.Repl(
            _serviceProvider,
            context,
            replContext
            );

        return result;
    }
}
