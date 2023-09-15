// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Psw.Scanners
{
    public class DelimPair
    {
        public string Start, End;

        public DelimPair(string start, string end) => (Start, End) = (start, end ?? start);

        public DelimPair(char delim) => (Start, End) = (delim.ToString(), delim.ToString());
    }
}
