using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHZH.PublishExtensions.Details
{
    internal enum PublishStatus
    {
        [Description("UNKNOWN")]
        Unknown = 0,

        [Description("")]
        Created,

        [Description("")]
        Updated,

        [Description("ERROR")]
        Error,

        [Description("")]
        Unmodified,

        [Description("SKIPPED")]
        TargetNewer,

        [Description("IGNORED")]
        Ignored,

        [Description("FOLDER")]
        Folder
    }
}
