﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand.SubCommands;

[Command("delete", "Delete a key from the store", Parent = "store")]
[Argument(typeof(string), "key", "The key to delete")]
internal class DeleteCommand(
    IStoreService store
    ) 
{
    public int Execute(
        string key
        )
    {
        if (store.Remove(key)) {
            Console.WriteLine($"Deleted key '{key}'.");
        }
        else {
            Console.WriteLine($"{Constants.ErrorChar} Key '{key}' not found.");
            return Result.Error;
        }

        return Result.Success;
    }
}
