// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Psw.Scanners
{
    /// <summary>
    /// Manages comments for the ScriptScanner.
    /// </summary>
    public class ScriptComment
    {
        private TextScanner _scn;

        private Func<bool> _checkLineComment = () => false;
        private Func<bool> _checkBlockComment = () => false;

        private string _lineComment;
        private string _blockCommentStart, _blockCommentEnd;

        public string CommentStartChars { get; private set; } = string.Empty;

        internal ScriptComment() { }

        /// <summary>
        /// ScriptComment Constructor:<br/>
        /// - For block comments Start and End must both be valid to enable block comments. 
        /// </summary>
        /// <param name="scn">Hosting Text/Script scanner.</param>
        /// <param name="lineComment">Line comment (null/empty for none).</param>
        /// <param name="blockCommentStart">Block comment start (null/empty for none).</param>
        /// <param name="blockCommentEnd">Block comment end (null/empty for none).</param>
        public ScriptComment(TextScanner scn, string lineComment = "//", string blockCommentStart = "/*", string blockCommentEnd = "*/") {
            _scn = scn;

            // Line Comment:
            if (!string.IsNullOrWhiteSpace(lineComment)) { 
                _lineComment = lineComment;
                _checkLineComment = () => _scn.IsString(lineComment, false);
                CommentStartChars += lineComment[0];
            }
            else _checkLineComment = () => false;

            // Block Comment:
            if (!string.IsNullOrWhiteSpace(blockCommentStart) && !string.IsNullOrWhiteSpace(blockCommentEnd)) {
                _blockCommentStart = blockCommentStart; 
                _blockCommentEnd = blockCommentEnd;
                _checkBlockComment = () => _scn.IsString(blockCommentStart, false);
                if (CommentStartChars.Length > 0 && CommentStartChars[0] != blockCommentStart[0]) CommentStartChars += blockCommentStart[0];
            }
            else _checkBlockComment = () => false;
        }

        internal bool IsAtLineComment  => _checkLineComment();
        internal bool IsAtBlockComment => _checkBlockComment();
        internal bool IsAtComment      => IsAtLineComment || IsAtBlockComment;

        internal ScriptComment Clone(TextScanner scn) => new ScriptComment(scn, _lineComment, _blockCommentStart, _blockCommentEnd);

        /// <summary>
        /// Skip Line comment:<br/>
        /// - Assumes at the line comment without checking.
        /// - Skips to Eol and absorbs the NL it termNL is false, else retains the NL
        /// </summary>
        private void _SkipLineComment(bool termNL) => _scn.SkipToEol(!termNL);

        /// <summary>
        /// Skips a Block Comment and handles nesting:<br/>
        /// - Assumes at the start of a block comment without checking
        /// </summary>
        /// <returns>
        /// True: No block comment or block comment skipped and Index positioned after comment.<br/>
        /// False: For invalid block comment. Logs error and Index positioned directly after opening block comment.</returns>
        private bool _SkipBlockComment() => _scn.SkipBlock(_blockCommentStart, _blockCommentEnd);

        /// <summary>
        /// Skip consecutive sequence of Line and Block (may be nested) comments:<br/> 
        /// - NOTE: Index must currently be positioned at the start of a comment.<br/>
        /// - Set termNL to true to position Index at the newline after a Line comment, else the newline is skipped.
        /// </summary>
        /// <returns>
        /// True: Comments skipped and Index positioned after comments.<br/>
        /// False: Eos or comment error (comment error logged) - Index unchanged.
        /// </returns>
        internal bool SkipWhileComment(bool termNL = false) {
            while (IsAtComment) {
                if (IsAtLineComment) {
                    _SkipLineComment(termNL);
                    if (termNL) break;
                }

                else if (!_SkipBlockComment()) return false;
            }

            return !_scn.IsEos;
        }
    }
}
