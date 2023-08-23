//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace NET.Tools.Terminal
//{
//    public class TerminalSequences
//    {
//        #region |   Static Lists   |

//        private static List<String> beginEndChars = null;

//        private static List<EscapeSequence> sequences = new List<EscapeSequence>()
//        {
//            new EscapeSequence() { Code = "LMN", Sequence = "Esc[20h", Description = "Set new line mode" },
//            new EscapeSequence() { Code = "DECCKM", Sequence = "Esc[?1h", Description = "Set cursor key to application" },
//            new EscapeSequence() { Code = "DECCOLM", Sequence = "Esc[?3h", Description = "Set number of columns to 132" },
//            new EscapeSequence() { Code = "DECSCLM", Sequence = "Esc[?4h", Description = "Set smooth scrolling" },
//            new EscapeSequence() { Code = "DECSCNM", Sequence = "Esc[?5h", Description = "Set reverse video on screen" },
//            new EscapeSequence() { Code = "DECOM", Sequence = "Esc[?6h", Description = "Set origin to relative" },
//            new EscapeSequence() { Code = "DECAWM", Sequence = "Esc[?7h", Description = "Set auto-wrap mode" },
//            new EscapeSequence() { Code = "DECARM", Sequence = "Esc[?8h", Description = "Set auto-repeat mode" },
//            new EscapeSequence() { Code = "DECINLM", Sequence = "Esc[?9h", Description = "Set interlacing mode" },
//            new EscapeSequence() { Code = "LMN", Sequence = "Esc[20l", Description = "Set line feed mode" },
//            new EscapeSequence() { Code = "DECCKM", Sequence = "Esc[?1l", Description = "Set cursor key to cursor" },
//            new EscapeSequence() { Code = "DECANM", Sequence = "Esc[?2l", Description = "Set VT52 (versus ANSI)" },
//            new EscapeSequence() { Code = "DECCOLM", Sequence = "Esc[?3l", Description = "Set number of columns to 80" },
//            new EscapeSequence() { Code = "DECSCLM", Sequence = "Esc[?4l", Description = "Set jump scrolling" },
//            new EscapeSequence() { Code = "DECSCNM", Sequence = "Esc[?5l", Description = "Set normal video on screen" },
//            new EscapeSequence() { Code = "DECOM", Sequence = "Esc[?6l", Description = "Set origin to absolute" },
//            new EscapeSequence() { Code = "DECAWM", Sequence = "Esc[?7l", Description = "Reset auto-wrap mode" },
//            new EscapeSequence() { Code = "DECARM", Sequence = "Esc[?8l", Description = "Reset auto-repeat mode" },
//            new EscapeSequence() { Code = "DECINLM", Sequence = "Esc[?9l", Description = "Reset interlacing mode" },
//            new EscapeSequence() { Code = "DECKPAM", Sequence = "Esc=", Description = "Set alternate keypad mode" },
//            new EscapeSequence() { Code = "DECKPNM", Sequence = "Esc>", Description = "Set numeric keypad mode" },
//            new EscapeSequence() { Code = "setukg0", Sequence = "Esc(A", Description = "Set United Kingdom G0 character set" },
//            new EscapeSequence() { Code = "setukg1", Sequence = "Esc)A", Description = "Set United Kingdom G1 character set" },
//            new EscapeSequence() { Code = "setusg0", Sequence = "Esc(B", Description = "Set United States G0 character set" },
//            new EscapeSequence() { Code = "setusg1", Sequence = "Esc)B", Description = "Set United States G1 character set" },
//            new EscapeSequence() { Code = "setspecg0", Sequence = "Esc(0", Description = "Set G0 special chars. & line set" },
//            new EscapeSequence() { Code = "setspecg1", Sequence = "Esc)0", Description = "Set G1 special chars. & line set" },
//            new EscapeSequence() { Code = "setaltg0", Sequence = "Esc(1", Description = "Set G0 alternate character ROM" },
//            new EscapeSequence() { Code = "setaltg1", Sequence = "Esc)1", Description = "Set G1 alternate character ROM" },
//            new EscapeSequence() { Code = "setaltspecg0", Sequence = "Esc(2", Description = "Set G0 alt char ROM and spec. graphics" },
//            new EscapeSequence() { Code = "setaltspecg1", Sequence = "Esc)2", Description = "Set G1 alt char ROM and spec. graphics" },
//            new EscapeSequence() { Code = "SS2", Sequence = "EscN", Description = "Set single shift 2" },
//            new EscapeSequence() { Code = "SS3", Sequence = "EscO", Description = "Set single shift 3" },
//            new EscapeSequence() { Code = "SGR0", Sequence = "Esc[m", Description = "Turn off character attributes" },
//            new EscapeSequence() { Code = "SGR0", Sequence = "Esc[0m", Description = "Turn off character attributes" },
//            new EscapeSequence() { Code = "SGR1", Sequence = "Esc[1m", Description = "Turn bold mode on" },
//            new EscapeSequence() { Code = "SGR2", Sequence = "Esc[2m", Description = "Turn low intensity mode on" },
//            new EscapeSequence() { Code = "SGR4", Sequence = "Esc[4m", Description = "Turn underline mode on" },
//            new EscapeSequence() { Code = "SGR5", Sequence = "Esc[5m", Description = "Turn blinking mode on" },
//            new EscapeSequence() { Code = "SGR7", Sequence = "Esc[7m", Description = "Turn reverse video on" },
//            new EscapeSequence() { Code = "SGR8", Sequence = "Esc[8m", Description = "Turn invisible text mode on" },
//            new EscapeSequence() { Code = "DECSTBM", Sequence = "Esc[Line;Liner", Description = "Set top and bottom lines of a window" },
//            new EscapeSequence() { Code = "CUU", Sequence = "Esc[ValueA", Description = "Move cursor up n lines" },
//            new EscapeSequence() { Code = "CUD", Sequence = "Esc[ValueB", Description = "Move cursor down n lines" },
//            new EscapeSequence() { Code = "CUF", Sequence = "Esc[ValueC", Description = "Move cursor right n lines" },
//            new EscapeSequence() { Code = "CUB", Sequence = "Esc[ValueD", Description = "Move cursor left n lines" },
//            new EscapeSequence() { Code = "cursorhome", Sequence = "Esc[H", Description = "Move cursor to upper left corner" },
//            new EscapeSequence() { Code = "cursorhome", Sequence = "Esc[;H", Description = "Move cursor to upper left corner" },
//            new EscapeSequence() { Code = "CUP", Sequence = "Esc[Line;ColumnH", Description = "Move cursor to screen location v,h" },
//            new EscapeSequence() { Code = "hvhome", Sequence = "Esc[f", Description = "Move cursor to upper left corner" },
//            new EscapeSequence() { Code = "hvhome", Sequence = "Esc[;f", Description = "Move cursor to upper left corner" },
//            new EscapeSequence() { Code = "CUP", Sequence = "Esc[Line;Columnf", Description = "Move cursor to screen location v,h" },
//            new EscapeSequence() { Code = "IND", Sequence = "EscD", Description = "Move/scroll window up one line" },
//            new EscapeSequence() { Code = "RI", Sequence = "EscM", Description = "Move/scroll window down one line" },
//            new EscapeSequence() { Code = "NEL", Sequence = "EscE", Description = "Move to next line" },
//            new EscapeSequence() { Code = "DECSC", Sequence = "Esc7", Description = "Save cursor position and attributes" },
//            new EscapeSequence() { Code = "DECSC", Sequence = "Esc8", Description = "Restore cursor position and attributes" },
//            new EscapeSequence() { Code = "HTS", Sequence = "EscH", Description = "Set a tab at the current column" },
//            new EscapeSequence() { Code = "TBC", Sequence = "Esc[g", Description = "Clear a tab at the current column" },
//            new EscapeSequence() { Code = "TBC", Sequence = "Esc[0g", Description = "Clear a tab at the current column" },
//            new EscapeSequence() { Code = "TBC", Sequence = "Esc[3g", Description = "Clear all tabs" },
//            new EscapeSequence() { Code = "DECDHL", Sequence = "Esc#3", Description = "Double-height letters, top half" },
//            new EscapeSequence() { Code = "DECDHL", Sequence = "Esc#4", Description = "Double-height letters, bottom half" },
//            new EscapeSequence() { Code = "DECSWL", Sequence = "Esc#5", Description = "Single width, single height letters" },
//            new EscapeSequence() { Code = "DECDWL", Sequence = "Esc#6", Description = "Double width, single height letters" },
//            new EscapeSequence() { Code = "EL0", Sequence = "Esc[K", Description = "Clear line from cursor right" },
//            new EscapeSequence() { Code = "EL0", Sequence = "Esc[0K", Description = "Clear line from cursor right" },
//            new EscapeSequence() { Code = "EL1", Sequence = "Esc[1K", Description = "Clear line from cursor left" },
//            new EscapeSequence() { Code = "EL2", Sequence = "Esc[2K", Description = "Clear entire line" },
//            new EscapeSequence() { Code = "ED0", Sequence = "Esc[J", Description = "Clear screen from cursor down" },
//            new EscapeSequence() { Code = "ED0", Sequence = "Esc[0J", Description = "Clear screen from cursor down" },
//            new EscapeSequence() { Code = "ED1", Sequence = "Esc[1J", Description = "Clear screen from cursor up" },
//            new EscapeSequence() { Code = "ED2", Sequence = "Esc[2J", Description = "Clear entire screen" },
//            new EscapeSequence() { Code = "DSR", Sequence = "Esc5n", Description = "Device status report" },
//            new EscapeSequence() { Code = "DSR", Sequence = "Esc0n", Description = "Response: terminal is OK" },
//            new EscapeSequence() { Code = "DSR", Sequence = "Esc3n", Description = "Response: terminal is not OK" },
//            new EscapeSequence() { Code = "DSR", Sequence = "Esc6n", Description = "Get cursor position" },
//            new EscapeSequence() { Code = "CPR", Sequence = "EscLine;ColumnR", Description = "Response: cursor is at v,h" },
//            new EscapeSequence() { Code = "DA", Sequence = "Esc[c", Description = "Identify what terminal type" },
//            new EscapeSequence() { Code = "DA", Sequence = "Esc[0c", Description = "Identify what terminal type (another)" },
//            new EscapeSequence() { Code = "DA", Sequence = "Esc[?1;Value0c", Description = "Response: terminal type code n" },
//            new EscapeSequence() { Code = "RIS", Sequence = "Escc", Description = "Reset terminal to initial state" },
//            new EscapeSequence() { Code = "DECALN", Sequence = "Esc#8", Description = "Screen alignment display" },
//            new EscapeSequence() { Code = "DECTST", Sequence = "Esc[2;1y", Description = "Confidence power up test" },
//            new EscapeSequence() { Code = "DECTST", Sequence = "Esc[2;2y", Description = "Confidence loopback test" },
//            new EscapeSequence() { Code = "DECTST", Sequence = "Esc[2;9y", Description = "Repeat power up test" },
//            new EscapeSequence() { Code = "DECTST", Sequence = "Esc[2;10y", Description = "Repeat loopback test" },
//            new EscapeSequence() { Code = "DECLL0", Sequence = "Esc[0q", Description = "Turn off all four leds" },
//            new EscapeSequence() { Code = "DECLL1", Sequence = "Esc[1q", Description = "Turn on LED #1" },
//            new EscapeSequence() { Code = "DECLL2", Sequence = "Esc[2q", Description = "Turn on LED #2" },
//            new EscapeSequence() { Code = "DECLL3", Sequence = "Esc[3q", Description = "Turn on LED #3" },
//            new EscapeSequence() { Code = "DECLL4", Sequence = "Esc[4q", Description = "Turn on LED #4" },
//            new EscapeSequence() { Code = "setansi", Sequence = "Esc<", Description = "Enter/exit ANSI mode (VT52)" },
//            new EscapeSequence() { Code = "altkeypad", Sequence = "Esc=", Description = "Enter alternate keypad mode" },
//            new EscapeSequence() { Code = "numkeypad", Sequence = "Esc>", Description = "Exit alternate keypad mode" },
//            new EscapeSequence() { Code = "setgr", Sequence = "EscF", Description = "Use special graphics character set" },
//            new EscapeSequence() { Code = "resetgr", Sequence = "EscG", Description = "Use normal US/UK character set" },
//            new EscapeSequence() { Code = "cursorup", Sequence = "EscA", Description = "Move cursor up one line" },
//            new EscapeSequence() { Code = "cursordn", Sequence = "EscB", Description = "Move cursor down one line" },
//            new EscapeSequence() { Code = "cursorrt", Sequence = "EscC", Description = "Move cursor right one char" },
//            new EscapeSequence() { Code = "cursorlf", Sequence = "EscD", Description = "Move cursor left one char" },
//            new EscapeSequence() { Code = "cursorhome", Sequence = "EscH", Description = "Move cursor to upper left corner" },
//            new EscapeSequence() { Code = "revindex", Sequence = "EscI", Description = "Generate a reverse line-feed" },
//            new EscapeSequence() { Code = "cleareol", Sequence = "EscK", Description = "Erase to end of current line" },
//            new EscapeSequence() { Code = "cleareos", Sequence = "EscJ", Description = "Erase to end of screen" },
//            new EscapeSequence() { Code = "ident", Sequence = "EscZ", Description = "Identify what the terminal is" },
//            new EscapeSequence() { Code = "identresp", Sequence = "Esc/Z", Description = "Correct response to ident" },
//            new EscapeSequence() { Code = "PF1", Sequence = "EscOP", Description = "PF1" },
//            new EscapeSequence() { Code = "PF2", Sequence = "EscOQ", Description = "PF2" },
//            new EscapeSequence() { Code = "PF3", Sequence = "EscOR", Description = "PF3" },
//            new EscapeSequence() { Code = "PF4", Sequence = "EscOS", Description = "PF4" },
//            new EscapeSequence() { Code = "ResetUp", Sequence = "EscA", Description = "ResetUp" },
//            new EscapeSequence() { Code = "ResetDown", Sequence = "EscB", Description = "ResetDown" },
//            new EscapeSequence() { Code = "ResetRight", Sequence = "EscC", Description = "ResetRight" },
//            new EscapeSequence() { Code = "ResetLeft", Sequence = "EscD", Description = "ResetLeft" },
//            new EscapeSequence() { Code = "SetUp", Sequence = "EscOA", Description = "SetUp" },
//            new EscapeSequence() { Code = "SetDown", Sequence = "EscOB", Description = "SetDown" },
//            new EscapeSequence() { Code = "SetRight", Sequence = "EscOC", Description = "SetRight" },
//            new EscapeSequence() { Code = "SetLeft", Sequence = "EscOD", Description = "SetLeft" },
//            new EscapeSequence() { Code = "Num0", Sequence = "EscOp", Description = "Num0" },
//            new EscapeSequence() { Code = "Num1", Sequence = "EscOq", Description = "Num1" },
//            new EscapeSequence() { Code = "Num2", Sequence = "EscOr", Description = "Num2" },
//            new EscapeSequence() { Code = "Num3", Sequence = "EscOs", Description = "Num3" },
//            new EscapeSequence() { Code = "Num4", Sequence = "EscOt", Description = "Num4" },
//            new EscapeSequence() { Code = "Num5", Sequence = "EscOu", Description = "Num5" },
//            new EscapeSequence() { Code = "Num6", Sequence = "EscOv", Description = "Num6" },
//            new EscapeSequence() { Code = "Num7", Sequence = "EscOw", Description = "Num7" },
//            new EscapeSequence() { Code = "Num8", Sequence = "EscOx", Description = "Num8" },
//            new EscapeSequence() { Code = "Num9", Sequence = "EscOy", Description = "Num9" },
//            new EscapeSequence() { Code = "Minus", Sequence = "EscOm", Description = "Minus" },
//            new EscapeSequence() { Code = "Comma", Sequence = "EscOl", Description = "Comma" },
//            new EscapeSequence() { Code = "Period", Sequence = "EscOn", Description = "Period" },
//            new EscapeSequence() { Code = "CtrlM", Sequence = "EscOM", Description = "CtrlM" },
//            new EscapeSequence() { Code = "PrintScreen", Sequence = "Esc[i", Description = "Print Screen" },
//            new EscapeSequence() { Code = "PrintLine", Sequence = "Esc[1i", Description = "Print Line" },
//            new EscapeSequence() { Code = "StopPrintLog", Sequence = "Esc[4i", Description = "Stop Print Log" },
//            new EscapeSequence() { Code = "StartPrintLog", Sequence = "Esc[5i", Description = "Start Print Log" }
//        };

//        private static List<String> BeginEndCharacters
//        {
//            get
//            {
//                if (beginEndChars == null)
//                {
//                    beginEndChars = new List<String>();

//                    foreach (EscapeSequence seq in sequences)
//                    {
//                        string beginChar = seq.Sequence.Substring(3, 1);
//                        string endChar = seq.Sequence.Substring(seq.Sequence.Length - 1, 1);

//                        if (!beginEndChars.Contains(beginChar + endChar))
//                        {
//                            beginEndChars.Add(beginChar + endChar);
//                        }
//                    }
//                }

//                return beginEndChars;
//            }
//        }

//        #endregion

//        #region |   Sequence Operations   |

//        public static string RemoveSequences(string text)
//        {
//            string sequence = System.Convert.ToChar(System.Convert.ToUInt32('\x001B')).ToString();
//            sequence += System.Convert.ToChar(System.Convert.ToUInt32('\x005B')).ToString();
//            sequence += "24;1H";
//            string[] textElements = text.Split(new string[] { sequence }, StringSplitOptions.None);

//            for (int i = 0; i < textElements.Length - 1; i++)
//            {
//                // Insert new line if next sequence not start with '\x001B'
//                if ((i < textElements.Length - 1) && textElements[i + 1].StartsWith(System.Convert.ToChar(System.Convert.ToUInt32('\x001B')).ToString()))
//                {
//                    textElements[i] += sequence;
//                }
//                else
//                {
//                    textElements[i] += "\r\n";
//                }
//            }

//            text = String.Join("", textElements);
            
//            // Real start
//            StringBuilder result = new StringBuilder();
//            char[] chars = text.ToCharArray();

//            bool isSequence = false;
//            int index = 0;

//            char beginChar = '\0';
//            char endChar = '\0';

//            while (index < chars.Length)
//            {
//                if (chars[index] == (char)27)
//                {
//                    isSequence = true;

//                    if (index + 1 == chars.Length)
//                    {
//                        break;
//                    }
//                    else
//                    {
//                        beginChar = chars[index + 1];

//                        // Find sequence with single character
//                        string beginEnd = beginChar.ToString() + beginChar.ToString();
//                        bool sequenceExists = TerminalSequences.BeginEndCharacters.Contains(beginEnd);

//                        if (sequenceExists)
//                        {
//                            isSequence = false;
//                        }

//                        index = index + 2;

//                        continue;
//                    }
//                }

//                if (isSequence)
//                {
//                    endChar = chars[index];

//                    string beginEnd = beginChar.ToString() + endChar.ToString();
//                    bool sequenceExists = TerminalSequences.BeginEndCharacters.Contains(beginEnd);

//                    if (sequenceExists)
//                    {
//                        isSequence = false;
//                        index++;
//                        continue;
//                    }
//                }
//                else
//                {
//                    result.Append(chars[index]);
//                }

//                index++;
//            }

//            return result.ToString();
//        }

//        #endregion
//    }
//}
