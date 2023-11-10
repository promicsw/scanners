// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

#define FUNC_VER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Psw.Scanners
{
    public class ScriptComment
    {
        private TextScanner _scn;

#if FUNC_VER
        private Func<bool> _checkLineComment = () => false;
        private Func<bool> _checkBlockComment = () => false;

        private string _lineComment;
        private string _blockCommentStart, _blockCommentEnd;
        private string _blockSkipString; 

        public string CommentStartChars { get; private set; } = string.Empty;

        public ScriptComment() { }

        public ScriptComment(TextScanner scn, string lineComment = "//", string blockCommentStart = "/*", string blockCommentEnd = "*/") {
            _scn = scn;

            //var isBlockComment = !string.IsNullOrWhiteSpace(blockCommentStart) && !string.IsNullOrWhiteSpace(blockCommentEnd);

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
                _blockSkipString = $"|{blockCommentEnd}|{blockCommentStart}";
                if (CommentStartChars.Length > 0 && CommentStartChars[0] != blockCommentStart[0]) CommentStartChars += blockCommentStart[0];
            }
            else _checkBlockComment = () => false;
        }

        public bool IsAtLineComment  => _checkLineComment();
        public bool IsAtBlockComment => _checkBlockComment();
        public bool IsAtComment      => IsAtLineComment || IsAtBlockComment;

        public ScriptComment Clone(TextScanner scn) => new ScriptComment(scn, _lineComment, _blockCommentStart, _blockCommentEnd);
#else
        private string _lineComment;
        private string _blockCommentStart;
        private string _blockCommentEnd;
        private string _blockSkipString;

        private bool _isLineComment, _isBlockComment, _isComment;

        public ScriptComment(TextScanner scn, string lineComment = "//", string blockCommentStart = "/*", string blockCommentEnd = "*/")
        {
            _scn = scn;
            _lineComment = lineComment;
            _blockCommentStart = blockCommentStart;
            _blockCommentEnd = blockCommentEnd;

            _isLineComment = !string.IsNullOrWhiteSpace(_lineComment);
            _isBlockComment = !string.IsNullOrWhiteSpace(_blockCommentStart) && !string.IsNullOrWhiteSpace(_blockCommentEnd);
            _isComment = _isLineComment || _isBlockComment;

            if (_isBlockComment) _blockSkipString = $"|{_blockCommentEnd}|{_blockCommentStart}";
        }

        public bool IsAtLineComment  => _isLineComment && _scn.IsString(_lineComment, false);
        public bool IsAtBlockComment => _isBlockComment && _scn.IsString(_blockCommentStart, false);
        public bool IsAtComment      => IsAtLineComment || IsAtBlockComment;
#endif

        //public bool IsLineComment  => !string.IsNullOrWhiteSpace(LineComment);
        //public bool IsBlockComment => !string.IsNullOrWhiteSpace(BlockCommentStart) && !string.IsNullOrWhiteSpace(BlockCommentEnd);
        //public bool IsComments     => IsLineComment && IsBlockComment;

        //public string CommentStartChars() {
        //    string csc = "";

        //    if (IsLineComment) csc += LineComment[0];
        //    if (IsBlockComment && csc.Length > 0 && csc[0] != BlockCommentStart[0]) csc += BlockCommentStart[0];
        //    return csc;
        //}



        protected void SkipLineComment(bool termNL) => _scn.SkipToEol(!termNL);

        /// <summary>
        /// Skips a Block Comment and handles nesting:<br/>
        /// </summary>
        /// <returns>
        /// True: No block comment or block comment skipped and Index positioned after comment.<br/>
        /// False: For invalid block comment. Logs error and Index positioned directly after opening block comment.</returns>
        protected bool SkipBlockComment() => _scn.SkipBlock(_blockCommentStart, _blockCommentEnd);

        //public bool SkipBlockComment() {
        //    if (!IsAtBlockComment) return true;
        //    return _scn.SkipBlock(_blockCommentStart, _blockCommentEnd);
        //}

        /// <summary>
        /// Skip consecutive sequence of Line and Block (may be nested) comments:<br/> 
        /// - NOTE: Index must currently be positioned at the start of a comment.<br/>
        /// - Set termNL to true to position Index at the newline after a Line comment, else the newline is skipped.
        /// </summary>
        /// <returns>
        /// True: Comments skipped and Index positioned after comments.<br/>
        /// False: Eos or comment error (missing */ logged as error) - Index unchanged.
        /// </returns>
        public bool SkipWhileComment(bool termNL = false) {
            while (IsAtComment) {
                if (IsAtLineComment) {
                    SkipLineComment(termNL);
                    if (termNL) break;
                }

                else if (!SkipBlockComment()) return false;
            }

            return !_scn.IsEos;
        }
    }
}
