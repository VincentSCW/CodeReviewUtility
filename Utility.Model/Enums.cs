using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Model
{
    /// <summary>
    /// First column: Says if item was added, deleted, or otherwise changed
    /// ' ' no modifications
    /// 'A' Added
    /// 'C' Conflicted
    /// 'D' Deleted
    /// 'I' Ignored
    /// 'M' Modified
    /// 'R' Replaced
    /// 'X' an unversioned directory created by an externals definition
    /// '?' item is not under version control
    /// '!' item is missing (removed by non-svn command) or incomplete
    /// '~' versioned item obstructed by some item of a different kind
    /// </summary>
    public enum FileModificationState
    {
        Added,
        Deleted,
        Ignored,
        Conflicted,
        Modified,
        Replaced,
        NotVersioned,
        NotModified
    }

    public enum FileDetailInfo
    {
        Path,
        Name,
        RepositoryUUID,
        Revision,
        NodeKind,
        Schedule,
        LastChangedAuthor,
        LastChangedRev,
        Checksum,
        URL,
        Unknown
    }
}
