// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace PHZH.PublishExtensions
{
    internal enum CommandId
    {
        PublishFiles = 0x100,
        EditMapping = 0x101,
        IncludeFiles = 0x102,
        IgnoreFiles = 0x103,
        ConfigureProject = 0x104,
        PublishActiveFile = 0x105
    }
}