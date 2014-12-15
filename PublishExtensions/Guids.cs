// Guids.cs
// MUST match guids.h
using System;

namespace PHZH.PublishExtensions
{
    static class Guids
    {
        public const string PublishExtensionsPkgString = "6485d4e2-5904-4a74-ae29-5e264db05ef0";
        public const string PublishExtensionsCmdSetString = "19e83d8c-f232-4e97-9725-314c0b7b874d";
        public const string PublishCmdSetString = "19e83d8c-f232-4e97-9725-314c0b7b8750";

        public static readonly Guid PublishExtensionsCmdSet = new Guid(PublishExtensionsCmdSetString);
        public static readonly Guid PublishCmdSet = new Guid(PublishCmdSetString);

        public static readonly Guid OutputWindowPane = new Guid("6485d4e2-5904-4a74-ae29-5e264db05ef1");
    };
}