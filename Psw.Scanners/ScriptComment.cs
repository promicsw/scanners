// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psw.Scanners
{
    internal class ScriptComment
    {
        public string LineComment = "//";
        public string BlockCommentStart = "/*";
        public string BlockCommentEnd = "*/";

        // ToDo: comments may be empty?
        public string CommentStartChar => LineComment[0] == BlockCommentStart[0] ? LineComment[0].ToString() : $"{LineComment[0]}{BlockCommentStart[0]}";
    }
}
