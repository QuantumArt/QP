//#define MyDEBUG

// MergeEngine.cs
//
// This file contains the implementation of the functionality to compare
// and merge two Html strings.
//
// The comparison is word-wise, not character wise. Thus, the process
// consists of two steps.
//
// The first step is to parse the html string into collection of English
// words(strong typed collection WordsCollection is defined for this) in
// such a way that:
//    1) anything starts with '<' and ends with '>' is treated as Html
//       tag.
//    2) Html tags and whitespaces are treated as prefix or suffix to
//       adjacent word and be put in the prefix or suffix fileds of the
//       Word object.
//    3) English words separated by space(s), "&nbsp;", "&#xxx",
//       tailing punctuation are treated as words and be put in the
//       word field of Word class.
//    4) Whitespaces immediately after or before Html tags are ignored.
//      ( whitespaces == {' ', '\t', '\n'} )
//
// The second step is to compare and merge the two words collections by
// the algorithm proposed by [1]. The follwoing are the basic steps of
// the algorithm (read [1] for details):
//    1) Find the middle snake of the two sequences by searching from
//       both the left-up and right-bottom corners of the edit graph at
//       the same time. When the furthest reaching paths of the two
//       searches first meet, the snake is reported as middle snake. It
//       may be empty sequence(or most likely be?).
//    2) For the sub-sequences before the middle snake and the
//       sub-sequences after the middle snake, do recursion on them.
//    3) Some key nomenclature:
//       Edit Graph -- for sequences A(N) and B(M), construct graph in
//                     such a way that there is always edge from (A(i-1), B)
//                     to (A(i), B) and edge from (A, B(j-1)) to
//                     (A, B(j)) (vertical or parallel edge). If A(i)
//                     == B(j) then there is edge from (A(i-1), B(j-1))
//                     to (A(i), B(j)) (diagonal edge).
//       Snake -- not the kind of animal here ..). a sequence of diagonal
//                edges surrounded by non-diagonal edges at both ends.
//       Furthest Reaching Path -- searching from the left-up corner toward
//                the right-bottom corner, the path that goes closest to
//                the right-bottom corner(in other words, there are more
//                disgonal edges on this path).
//       LCS / SES -- Longest Common Sequence and Shortest Edit Script.
//                Simple say, the shortest path between left-up and right-bottom
//                corners of the edit graph.
//
// [1] Eugene W. Myers, "An O(ND) Difference Algorithm and Its Variations"
//     A copy of the file can be found at:
//     http://www.xmailserver.org/diff2.pdf
// [2] http://cvs.sourceforge.net/viewcvs.py/*checkout*/cvsgui/cvsgui/cvs-1.10/diff/analyze.c?&rev=1.1.1.3
//
// The file is created to be used inside Rainbow(www.Rainbowportal.net)
// to compare the staging and production contents of HtmlDocument module
// while working in Workflow mode. However, this file can be easily
// modified to be used in other senario.
//
// All of the code in this file are implemented from scratch by the
// author, with reference to the Unix Diff implementation in [2].
//
// This program is free and can be distributed or used for any purpose
// with no restriction.
//
// The author would like to thank Matt Cowan(mcowan@county.oxford.on.ca)
// for pushing this work and undertaking lots of testings.
//
// Author: Hongwei Shen
// Email:  hongwei.shen@gmail.com
// Date:   June 22, 2005

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Merger
{

    #region Data types

    /// <summary>
    /// When we compare two files, we say we delete or add
    /// some sub sequences in the original file to result
    /// in the modified file. This is to define the strong
    /// type for identifying the status of a such sequence.
    /// </summary>
    internal enum SequenceStatus
    {
        /// <summary>
        /// The sequence is inside the original
        /// file but not in the modified file
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// The sequence is inside the modifed
        /// file but not in the original file
        /// </summary>
        Inserted,

        /// <summary>
        /// The sequence is in both the origianl
        /// and the modified files
        /// </summary>
        NoChange
    }

    /// <summary>
    /// The class defines the begining and end html tag
    /// for marking up the deleted words in the merged
    /// file.
    /// </summary>
    internal class CommentOff
    {
        public static string BeginTag = "<span style=\"text-decoration: line-through; color: red\">";
        public static string EndTag = "</span>";
    }

    /// <summary>
    /// The class defines the begining and end html tag
    /// for marking up the added words in the merged
    /// file.
    /// </summary>
    internal class Added
    {
        public static string BeginTag = "<span style=\"background: #FFFF4D\">";
        public static string EndTag = "</span>";
    }

    /// <summary>
    /// Data structure for marking start and end indexes of a
    /// sequence
    /// </summary>
    internal class Sequence
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Sequence()
        {
        }

        /// <summary>
        /// Overloaded Constructor that takes the start
        /// and end indexes of the sequence. Note that
        /// the interval is open on right hand side, say,
        /// it is like [startIndex, endIndex).
        /// </summary>
        /// <param name="startIndex">
        /// The starting index of the sequence
        /// </param>
        /// <param name="endIndex">
        /// The end index of the sequence.
        /// </param>
        public Sequence(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
        /// <summary>
        /// The start index of the sequence
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// The end index of the sequence. It is
        /// open end.
        /// </summary>
        public int EndIndex;
    }

    /// <summary>
    /// This class defines middle common sequence in the original
    /// file and the modified file. It is called middle in the
    /// sense that it is the common sequence when the furthest
    /// forward reaching path in the top-down seaching first overlaps
    /// the furthest backward reaching path in the bottom up search.
    /// See the listed reference at the top for more details.
    /// </summary>
    internal class MiddleSnake
    {
        public MiddleSnake()
        {
            Source = new Sequence();
            Destination = new Sequence();
        }
        /// <summary>
        /// The indexes of middle snake in source sequence
        /// </summary>
        public Sequence Source;

        /// <summary>
        /// The indexes of middle snake in the destination
        /// sequence
        /// </summary>
        public Sequence Destination;

        /// <summary>
        /// The length of the Shortest Edit Script for the
        /// path this snake is found.
        /// </summary>
        public int SesLength;
    }


    /// <summary>
    /// An array indexer class that maps the index of an integer
    /// array from -N ~ +N to 0 ~ 2N.
    /// </summary>
    internal class IntVector
    {
        private readonly int[] _data;
        private readonly int _n;

        public IntVector(int n)
        {
            _data = new int[2 * n];
            _n = n;
        }

        public int this[int index]
        {
            get { return _data[_n + index]; }
            set { _data[_n + index] = value; }
        }
    }


    #endregion

    #region Word and Words Collection

    /// <summary>
    /// This class defines the data type for representing a
    /// word. The word may have leading or tailing html tags
    /// or other special characters. Those prefix or suffix
    /// are not compared.
    /// </summary>
    internal class Word : IComparable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Word()
        {
            Value = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
        }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="value">
        /// The word
        /// </param>
        /// <param name="prefix">
        /// The prefix of the word, such as html tags
        /// </param>
        /// <param name="suffix">
        /// The suffix of the word, such as spaces.
        /// </param>
        public Word(string value, string prefix, string suffix)
        {
            Value = value;
            Prefix = prefix;
            Suffix = suffix;
        }

        /// <summary>
        /// The word itself
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The prefix of the word
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the word
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Reconstruct the text string from the word
        /// itself without any other decoration.
        /// </summary>
        /// <returns>
        /// Constructed string</returns>
        public string Reconstruct()
        {
            return Prefix + Value + Suffix;
        }

        /// <summary>
        /// Overloaded function reconstructing the text
        /// string with additional decoration around the
        /// _word.
        /// </summary>
        /// <param name="beginTag">
        /// The begining html tag to mark the _word
        /// </param>
        /// <param name="endTag">
        /// The end html tag to mark the _word
        /// </param>
        /// <returns>
        /// The constructed string
        /// </returns>
        public string Reconstruct(string beginTag, string endTag)
        {
            return Prefix + beginTag + Value + endTag + Suffix;
        }

        #region IComparable Members
        /// <summary>
        /// Implementation of the CompareTo. It compares
        /// the _word field.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var word1 = obj as Word;
            if (word1 != null)
            {
                return string.Compare(Value, word1.Value, StringComparison.Ordinal);
            }

            throw new ArgumentException("The obj is not a Word", obj.ToString());
        }
        #endregion
    }


    /// <summary>
    /// Strongly typed collection of Word object
    /// </summary>
    internal class WordsCollection : CollectionBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WordsCollection()
        {
        }

        /// <summary>
        /// Constructor to populate collection from an ArrayList
        /// </summary>
        /// <param name="list" type="ArrayList">
        /// ArrayList of Words
        /// </param>
        public WordsCollection(IEnumerable list)
        {
            foreach (var item in list)
            {
                if (item is Word)
                {
                    List.Add(item);
                }
            }
        }

        /// <summary>
        /// Add a Word object to the collection
        /// </summary>
        /// <param name="item" type="Word">
        /// Word object
        /// </param>
        /// <returns type="integer">
        /// Zero based index of the added Word object in
        /// the colleciton
        /// </returns>
        public int Add(Word item)
        {
            return List.Add(item);
        }

        /// <summary>
        /// Add Word object to the collection at specified index
        /// </summary>
        /// <param name="index" type="integer">
        /// Zero based index
        /// </param>
        /// <param name="item" type="Word">
        /// Word object
        /// </param>
        public void Insert(int index, Word item)
        {
            List.Insert(index, item);
        }

        /// <summary>
        /// Remove the Word object from collection
        /// </summary>
        /// <param name="item" type="Word">
        /// Word object to be removed
        /// </param>
        public void Remove(Word item)
        {
            List.Remove(item);
        }

        /// <summary>
        /// Check if the Word object is in the collection
        /// </summary>
        /// <param name="item" type="Word">
        /// Word object
        /// </param>
        /// <returns type="bool">
        /// Boolean value of the checking result
        /// </returns>
        public bool Contains(Word item)
        {
            return List.Contains(item);
        }

        /// <summary>
        /// Returns zero based index of the Word object in
        /// the collection
        /// </summary>
        /// <param name="item" type="Word">
        /// Word object to be checked for index
        /// </param>
        /// <returns type="integer">
        /// Zero based index of Word object in the collection
        /// </returns>
        public int IndexOf(Word item)
        {
            return List.IndexOf(item);
        }

        /// <summary>
        /// Array indexing operator -- get Word object at
        /// the index
        /// </summary>
        public Word this[int index]
        {
            get { return (Word)List[index]; }
            set { List[index] = value; }
        }

        /// <summary>
        /// Copy this WordsCollection to another one
        /// starting at the specified index position
        /// </summary>
        /// <param name="col" type="WordsCollection">
        /// WordsCollection to be copied to
        /// </param>
        /// <param name="index" type="integer">
        /// Starting index to begin copy operations
        /// </param>
        public void CopyTo(WordsCollection col, int index)
        {
            for (var i = index; i < List.Count; i++)
            {
                col.Add(this[i]);
            }
        }

        /// <summary>
        /// Overloaded. Copy this WordsCollection to another one
        /// starting at the index zero
        /// </summary>
        /// <param name="col" type="WordCollection">
        /// WordsCollection to copy to
        /// </param>
        public void CopyTo(WordsCollection col)
        {
            CopyTo(col, 0);
        }
    }
    #endregion

    #region Html Text Paser

    /// <summary>
    /// The class defines static method that processes html text
    /// string in such a way that the text is striped out into
    /// separate english words with html tags and some special
    /// characters as the prefix or suffix of the words. This way,
    /// the original html text string can be reconstructed to
    /// retain the original appearance by concating each word
    /// object in the collection in such way as word.prefix +
    /// word.word + word.suffix.
    ///
    /// The generated words collection will be used to compare
    /// the difference with another html text string in such format.
    /// </summary>
    internal class HtmlTextParser
    {
        /// <summary>
        /// Static method that parses the passed-in string into
        /// Words collection
        /// </summary>
        /// <param name="s">
        /// String
        /// </param>
        /// <returns>
        /// Words Collection
        /// </returns>
        public static WordsCollection Parse(string s)
        {
            var curPos = 0;
            var prefix = string.Empty;
            var suffix = string.Empty;
            var words = new WordsCollection();

            while (curPos < s.Length)
            {
                // eat the leading or tailing white spaces
                var prevPos = curPos;
                while (curPos < s.Length &&
                   (char.IsControl(s[curPos]) ||
                    char.IsWhiteSpace(s[curPos])))
                {
                    curPos++;
                }
                prefix += s.Substring(prevPos, curPos - prevPos);

                if (curPos == s.Length)
                {
                    // it is possible that there are
                    // something in the prefix
                    if (prefix != string.Empty)
                    {
                        // report a empty word with prefix.
                        words.Add(new Word("", prefix, ""));
                    }
                    break;
                }

                // we have 3 different cases here,
                // 1) if the string starts with '<', we assume
                //    that it is a html tag which will be put
                //    into prefix.
                // 2) starts with '&', we need to check if it is
                //    "&nbsp;" or "&#xxx;". If it is the former,
                //    we treat it as prefix and if it is latter,
                //    we treat it as a word.
                // 3) a string that may be a real word or a set
                //    of words separated by "&nbsp;" or may have
                //    leading special character or tailing
                //    punctuation.
                //
                // Another possible case that is too complicated
                // or expensive to handle is that some special
                // characters are embeded inside the word with
                // no space separation
                if (s[curPos] == '<')
                {
                    // it is a html tag, consume it
                    // as prefix.
                    prevPos = curPos;
                    while (s[curPos] != '>' && curPos < s.Length)
                    {
                        curPos++;
                    }
                    prefix += s.Substring(prevPos, curPos - prevPos + 1);

                    if (curPos == s.Length)
                    {
                        // if we come to this point, it means
                        // the html tag is not closed. Anyway,
                        // we are not validating html, so just
                        // report a empty word with prefix.
                        words.Add(new Word("", prefix, ""));
                        break;
                    }
                    // curPos is pointing to '>', move
                    // it to next.
                    curPos++;
                    if (curPos == s.Length)
                    {
                        // the html tag is closed but nothing more
                        // behind, so report a empty word with prefix.
                        words.Add(new Word("", prefix, ""));
                        break;
                    }
                }
                else
                {
                    string word;
                    if (s[curPos] == '&')
                    {
                        prevPos = curPos;

                        // case for html whitespace
                        if (curPos + 6 < s.Length &&
                            s.Substring(prevPos, 6) == "&nbsp;")
                        {
                            prefix += "&nbsp;";
                            curPos += 6;
                            continue;
                        }

                        // case for special character like "&#123;" etc
                        var pattern = @"&#[0-9]{3};";
                        var r = new Regex(pattern);

                        if (curPos + 6 < s.Length &&
                            r.IsMatch(s.Substring(prevPos, 6)))
                        {
                            words.Add(new Word(s.Substring(prevPos, 6), prefix, ""));
                            prefix = string.Empty;
                            curPos += 6;
                            continue;
                        }

                        // case for special character like "&#12;" etc
                        pattern = @"&#[0-9]{2};";
                        r = new Regex(pattern);
                        if (curPos + 5 < s.Length &&
                            r.IsMatch(s.Substring(prevPos, 5)))
                        {
                            words.Add(new Word(s.Substring(prevPos, 5), prefix, ""));
                            prefix = string.Empty;
                            curPos += 5;
                            continue;
                        }

                        // can't think of anything else that is special,
                        // have to treat it as a '&' leaded word. Hope
                        // it is just single '&' for and in meaning.
                        prevPos = curPos;
                        while (curPos < s.Length &&
                               !char.IsControl(s[curPos]) &&
                               !char.IsWhiteSpace(s[curPos]) &&
                               s[curPos] != '<')
                        {
                            curPos++;
                        }
                        word = s.Substring(prevPos, curPos - prevPos);

                        // eat the following witespace as suffix
                        prevPos = curPos;
                        while (curPos < s.Length &&
                               (char.IsControl(s[curPos]) ||
                                char.IsWhiteSpace(s[curPos])))
                        {
                            curPos++;
                        }
                        suffix += s.Substring(prevPos, curPos - prevPos);

                        words.Add(new Word(word, prefix, suffix));
                        prefix = string.Empty;
                        suffix = string.Empty;
                    }
                    else
                    {
                        // eat the word
                        prevPos = curPos;
                        while (curPos < s.Length &&
                               !char.IsControl(s[curPos]) &&
                               !char.IsWhiteSpace(s[curPos]) &&
                               s[curPos] != '<' &&
                               s[curPos] != '&')
                        {
                            curPos++;
                        }
                        word = s.Substring(prevPos, curPos - prevPos);

                        // if there are newlines or spaces follow
                        // the word, consume it as suffix
                        prevPos = curPos;
                        while (curPos < s.Length &&
                               (char.IsControl(s[curPos]) ||
                                char.IsWhiteSpace(s[curPos])))
                        {
                            curPos++;
                        }
                        suffix = s.Substring(prevPos, curPos - prevPos);
                        ProcessWord(words, prefix, word, suffix);
                        prefix = string.Empty;
                        suffix = string.Empty;
                    }
                }
            }
            return words;
        }

        /// <summary>
        /// Further processing of a string
        /// </summary>
        /// <param name="words">
        /// Collection that new word(s) will be added in
        /// </param>
        /// <param name="prefix">
        /// prefix come with the string
        /// </param>
        /// <param name="word">
        /// A string that may be a real word or have leading or tailing
        /// special character
        /// </param>
        /// <param name="suffix">
        /// suffix comes with the string.
        /// </param>
        private static void ProcessWord(WordsCollection words,
            string prefix, string word, string suffix)
        {
            // the passed in word may have leading special
            // characters such as '(', '"' etc or tailing
            // punctuations. We need to sort this out.
            var length = word.Length;

            if (length == 1)
            {
                words.Add(new Word(word, prefix, suffix));
            }
            else if (!char.IsLetterOrDigit(word[0]))
            {
                // it is some kind of special character in the first place
                // report it separately
                words.Add(new Word(word[0].ToString(), prefix, ""));
                words.Add(new Word(word.Substring(1), "", suffix));
            }
            else if (char.IsPunctuation(word[length - 1]))
            {
                // there is a end punctuation
                words.Add(new Word(word.Substring(0, length - 1), prefix, ""));
                words.Add(new Word(word[length - 1].ToString(), "", suffix));
            }
            else
            {
                // it is a real word(hope so)
                words.Add(new Word(word, prefix, suffix));
            }
        }
    }

    #endregion

    #region Merge Engine

    /// <summary>
    /// The class provides functionality to compare two html
    /// files and merge them into a new file with differences
    /// highlighted
    /// </summary>
    public class MergeProcessor
    {
        private readonly WordsCollection _original;
        private readonly WordsCollection _modified;
        private readonly IntVector _fwdVector;
        private readonly IntVector _bwdVector;

        public int BlocksSame { get; private set; }

        public int BlocksInserted { get; private set; }

        public int BlocksDeleted { get; private set; }

        public MergeProcessor(string original, string modified)
        {
            // parse the passed in string to words
            // collections
            _original = HtmlTextParser.Parse(original);
            _modified = HtmlTextParser.Parse(modified);

            // for hold the forward searching front-line
            // in previous searching loop
            _fwdVector = new IntVector(_original.Count + _modified.Count);

            // for hold the backward searching front-line
            // in the previous seaching loop
            _bwdVector = new IntVector(_original.Count + _modified.Count);
        }

        /// <summary>
        /// Return the number of words in the parsed original file.
        /// </summary>
        public int WordsInOriginalFile => _original.Count;

        /// <summary>
        /// Return the number of words in the parsed modified file
        /// </summary>
        public int WordsInModifiedFile => _modified.Count;

        /// <summary>
        /// In the edit graph for the sequences src and des, search for the
        /// optimal(shortest) path from (src.StartIndex, des.StartIndex) to
        /// (src.EndIndex, des.EndIndex).
        ///
        /// The searching starts from both ends of the graph and when the
        /// furthest forward reaching overlaps with the furthest backward
        /// reaching, the overlapped point is reported as the middle point
        /// of the shortest path.
        ///
        /// See the listed reference for the detailed description of the
        /// algorithm
        /// </summary>
        /// <param name="src">
        /// Represents a (sub)sequence of _original
        /// </param>
        /// <param name="des"></param>
        /// <returns>
        /// The found middle snake
        /// </returns>
        private MiddleSnake FindMiddleSnake(Sequence src, Sequence des)
        {
            int d;
            var midSnake = new MiddleSnake();

            // the range of diagonal values
            var minDiag = src.StartIndex - des.EndIndex;
            var maxDiag = src.EndIndex - des.StartIndex;

            // middle point of forward searching
            var fwdMid = src.StartIndex - des.StartIndex;
            // middle point of backward searching
            var bwdMid = src.EndIndex - des.EndIndex;

            // forward seaching range
            var fwdMin = fwdMid;
            var fwdMax = fwdMid;

            // backward seaching range
            var bwdMin = bwdMid;
            var bwdMax = bwdMid;

            var odd = (fwdMin - bwdMid & 1) == 1;

            _fwdVector[fwdMid] = src.StartIndex;
            _bwdVector[bwdMid] = src.EndIndex;

#if (MyDEBUG)
            Debug.WriteLine("-- Entering Function findMiddleSnake(src, des) --");
#endif
            for (d = 1; ; d++)
            {
                // extend or shrink the search range
                if (fwdMin > minDiag)
                {
                    _fwdVector[--fwdMin - 1] = -1;
                }
                else
                {
                    ++fwdMin;
                }

                if (fwdMax < maxDiag)
                {
                    _fwdVector[++fwdMax + 1] = -1;
                }
                else
                {
                    --fwdMax;
                }
#if (MyDEBUG)
                Debug.WriteLine(d, "  D path");
#endif
                // top-down search
                int k;
                int x;
                int y;
                for (k = fwdMax; k >= fwdMin; k -= 2)
                {
                    if (_fwdVector[k - 1] < _fwdVector[k + 1])
                    {
                        x = _fwdVector[k + 1];
                    }
                    else
                    {
                        x = _fwdVector[k - 1] + 1;
                    }
                    y = x - k;
                    midSnake.Source.StartIndex = x;
                    midSnake.Destination.StartIndex = y;

                    while (x < src.EndIndex &&
                        y < des.EndIndex &&
                        _original[x].CompareTo(_modified[y]) == 0)
                    {
                        x++;
                        y++;
                    }

                    // update forward vector
                    _fwdVector[k] = x;
#if (MyDEBUG)
                    Debug.WriteLine("    Inside forward loop");
                    Debug.WriteLine(k, "    Diagonal value");
                    Debug.WriteLine(x, "    X value");
                    Debug.WriteLine(y, "    Y value");
#endif
                    if (odd && k >= bwdMin && k <= bwdMax && x >= _bwdVector[k])
                    {
                        // this is the snake we are looking for
                        // and set the end indeses of the snake
                        midSnake.Source.EndIndex = x;
                        midSnake.Destination.EndIndex = y;
                        midSnake.SesLength = 2 * d - 1;
#if (MyDEBUG)
                        Debug.WriteLine("!!!Report snake from forward search");
                        Debug.WriteLine(midSnake.Source.StartIndex, "  middle snake source start index");
                        Debug.WriteLine(midSnake.Source.EndIndex, "  middle snake source end index");
                        Debug.WriteLine(midSnake.Destination.StartIndex, "  middle snake destination start index");
                        Debug.WriteLine(midSnake.Destination.EndIndex, "  middle snake destination end index");
#endif
                        return midSnake;
                    }
                }

                // extend the search range
                if (bwdMin > minDiag)
                {
                    _bwdVector[--bwdMin - 1] = int.MaxValue;
                }
                else
                {
                    ++bwdMin;
                }

                if (bwdMax < maxDiag)
                {
                    _bwdVector[++bwdMax + 1] = int.MaxValue;
                }
                else
                {
                    --bwdMax;
                }

                // bottom-up search
                for (k = bwdMax; k >= bwdMin; k -= 2)
                {
                    if (_bwdVector[k - 1] < _bwdVector[k + 1])
                    {
                        x = _bwdVector[k - 1];
                    }
                    else
                    {
                        x = _bwdVector[k + 1] - 1;
                    }
                    y = x - k;
                    midSnake.Source.EndIndex = x;
                    midSnake.Destination.EndIndex = y;

                    while (x > src.StartIndex &&
                        y > des.StartIndex &&
                        _original[x - 1].CompareTo(_modified[y - 1]) == 0)
                    {
                        x--;
                        y--;
                    }
                    // update backward Vector
                    _bwdVector[k] = x;

#if (MyDEBUG)
                    Debug.WriteLine("     Inside backward loop");
                    Debug.WriteLine(k, "    Diagonal value");
                    Debug.WriteLine(x, "    X value");
                    Debug.WriteLine(y, "    Y value");
#endif
                    if (!odd && k >= fwdMin && k <= fwdMax && x <= _fwdVector[k])
                    {
                        // this is the snake we are looking for
                        // and set the start indexes of the snake
                        midSnake.Source.StartIndex = x;
                        midSnake.Destination.StartIndex = y;
                        midSnake.SesLength = 2 * d;
#if (MyDEBUG)
                        Debug.WriteLine("!!!Report snake from backward search");
                        Debug.WriteLine(midSnake.Source.StartIndex, "  middle snake source start index");
                        Debug.WriteLine(midSnake.Source.EndIndex, "  middle snake source end index");
                        Debug.WriteLine(midSnake.Destination.StartIndex, "  middle snake destination start index");
                        Debug.WriteLine(midSnake.Destination.EndIndex, "  middle snake destination end index");
#endif
                        return midSnake;
                    }
                }
            }
        }

        /// <summary>
        /// The function merges the two sequences and returns the merged
        /// html text string with deleted(exists in source sequence but
        /// not in destination sequence) and added(exists in destination
        /// but not in source) decorated extra html tags defined in class
        /// commentoff and class added.
        /// </summary>
        /// <param name="src">
        /// The source sequence
        /// </param>
        /// <param name="des"></param>
        /// <returns>
        /// The merged html string
        /// </returns>
        private string DoMerge(Sequence src, Sequence des)
        {
            Sequence s;
            var result = new StringBuilder();
            var tail = string.Empty;

            var y = des.StartIndex;

            // strip off the leading common sequence
            while (src.StartIndex < src.EndIndex &&
                des.StartIndex < des.EndIndex &&
                _original[src.StartIndex].CompareTo(_modified[des.StartIndex]) == 0)
            {
                src.StartIndex++;
                des.StartIndex++;
            }

            if (des.StartIndex > y)
            {
                s = new Sequence(y, des.StartIndex);
                result.Append(ConstructText(s, SequenceStatus.NoChange));
            }

            y = des.EndIndex;

            // strip off the tailing common sequence
            while (src.StartIndex < src.EndIndex &&
                des.StartIndex < des.EndIndex &&
                _original[src.EndIndex - 1].CompareTo(_modified[des.EndIndex - 1]) == 0)
            {
                src.EndIndex--;
                des.EndIndex--;
            }

            if (des.EndIndex < y)
            {
                s = new Sequence(des.EndIndex, y);
                tail = ConstructText(s, SequenceStatus.NoChange);
            }

            // length of the sequences
            var n = src.EndIndex - src.StartIndex;
            var m = des.EndIndex - des.StartIndex;

            // Special cases
            if (n < 1 && m < 1)
            {
                // both source and destination are
                // empty
                return result.Append(tail).ToString();
            }

            if (n < 1)
            {
                // source is already empty, report
                // destination as added
                result.Append(ConstructText(des, SequenceStatus.Inserted));
                result.Append(tail);
                return result.ToString();
            }

            if (m < 1)
            {
                // destination is empty, report source as
                // deleted
                result.Append(ConstructText(src, SequenceStatus.Deleted));
                result.Append(tail);
                return result.ToString();
            }

            if (m == 1 && n == 1)
            {
                // each of source and destination has only
                // one word left. At this point, we are sure
                // that they are not equal.
                result.Append(ConstructText(src, SequenceStatus.Deleted));
                result.Append(ConstructText(des, SequenceStatus.Inserted));
                result.Append(tail);
                return result.ToString();
            }

            // find the middle snake
            var snake = FindMiddleSnake(src, des);

            if (snake.SesLength > 1)
            {
                // prepare the parameters for recursion
                var leftSrc = new Sequence(src.StartIndex, snake.Source.StartIndex);
                var leftDes = new Sequence(des.StartIndex, snake.Destination.StartIndex);
                var rightSrc = new Sequence(snake.Source.EndIndex, src.EndIndex);
                var rightDes = new Sequence(snake.Destination.EndIndex, des.EndIndex);

                result.Append(DoMerge(leftSrc, leftDes));
                if (snake.Source.StartIndex < snake.Source.EndIndex)
                {
                    // the snake is not empty, report it as common
                    // sequence
                    result.Append(ConstructText(snake.Destination, SequenceStatus.NoChange));
                }
                result.Append(DoMerge(rightSrc, rightDes));
                result.Append(tail);
                return result.ToString();
            }

            // Separating this case out can at least save one
            // level of recursion.
            //
            // Only one edit edge suggests the 4 possible cases.
            // if N > M, it will be either:
            //    -              or    \
            //      \   (case 1)        \   (case 2)
            //       \                   -
            // if N < M, it will be either:
            //    |              or    \
            //     \    (case 3)        \   (case 4)
            //      \                    |
            // N and M can't be equal!
            if (n > m)
            {
                if (src.StartIndex != snake.Source.StartIndex)
                {
                    // case 1
                    var leftSrc = new Sequence(src.StartIndex, snake.Source.StartIndex);
                    result.Append(ConstructText(leftSrc, SequenceStatus.Deleted));
                    result.Append(ConstructText(snake.Destination, SequenceStatus.NoChange));
                }
                else
                {
                    // case 2
                    var rightSrc = new Sequence(snake.Source.StartIndex, src.EndIndex);
                    result.Append(ConstructText(rightSrc, SequenceStatus.Deleted));
                    result.Append(ConstructText(snake.Destination, SequenceStatus.NoChange));
                }
            }
            else
            {
                if (des.StartIndex != snake.Destination.StartIndex)
                {
                    // case 3
                    var upDes = new Sequence(des.StartIndex, snake.Destination.StartIndex);
                    result.Append(ConstructText(upDes, SequenceStatus.Inserted));
                    result.Append(ConstructText(snake.Destination, SequenceStatus.NoChange));
                }
                else
                {
                    // case 4
                    var bottomDes = new Sequence(snake.Destination.EndIndex, des.EndIndex);
                    result.Append(ConstructText(bottomDes, SequenceStatus.Inserted));
                    result.Append(ConstructText(snake.Destination, SequenceStatus.NoChange));
                }
            }
            result.Append(tail);
            return result.ToString();
        }

        /// <summary>
        /// The function returns a html text string reconstructed
        /// from the sub collection of words its starting and ending
        /// indexes are marked by parameter seq and its collection is
        /// denoted by parameter status. If the status is "deleted",
        /// then the _original collection is used, otherwise, _modified
        /// is used.
        /// </summary>
        /// <param name="seq">
        /// Sequence object that marks the start index and end
        /// index of the sub sequence
        /// </param>
        /// <param name="status">
        /// Denoting the status of the sequence. When its value is
        /// Deleted or Added, some extra decoration will be added
        /// around the word.
        /// </param>
        /// <returns>
        /// The html text string constructed
        /// </returns>
        private string ConstructText(Sequence seq, SequenceStatus status)
        {
            var result = new StringBuilder();

            switch (status)
            {
                case SequenceStatus.Deleted:
                    // the sequence exists in _original and
                    // will be marked as deleted in the merged
                    // file.
                    BlocksDeleted++;
                    for (var i = seq.StartIndex; i < seq.EndIndex; i++)
                    {
                        result.Append(_original[i].Reconstruct(CommentOff.BeginTag, CommentOff.EndTag));

                    }
                    break;
                case SequenceStatus.Inserted:
                    // the sequence exists in _modified and
                    // will be marked as added in the merged
                    // file.
                    BlocksInserted++;
                    for (var i = seq.StartIndex; i < seq.EndIndex; i++)
                    {
                        result.Append(_modified[i].Reconstruct(Added.BeginTag, Added.EndTag));
                    }
                    break;
                case SequenceStatus.NoChange:
                    // the sequence exists in both _original and
                    // _modified and will be left as what it is in
                    // the merged file. We chose to reconstruct from
                    // _modified collection
                    BlocksSame++;
                    for (var i = seq.StartIndex; i < seq.EndIndex; i++)
                    {
                        result.Append(_modified[i].Reconstruct());
                    }
                    break;
                    // this will not happen (hope)
            }
            return result.ToString();
        }

        /// <summary>
        /// The public function merges the two copies of
        /// files stored inside this class. The html tags
        /// of the destination file is used in the merged
        /// file.
        /// </summary>
        /// <returns>
        /// The merged file
        /// </returns>
        public string Merge()
        {
            var src = new Sequence(0, _original.Count);
            var des = new Sequence(0, _modified.Count);

            return DoMerge(src, des);
        }
    }
    #endregion
}
