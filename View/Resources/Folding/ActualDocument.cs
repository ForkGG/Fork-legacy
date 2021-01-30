using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace Fork.View.Resources.Folding
{
    public class ActualDocument
    {
        private ActualDocument(IEnumerable<ActualLine> lines, TextDocument document)
        {
            Lines = new LinkedList<ActualLine>(lines);
            Document = document;
        }

        public LinkedList<ActualLine> Lines { get; }
        public TextDocument Document { get; }

        public static ActualDocument BuildActualDocument(TextDocument document)
        {
            List<ActualLine> lines = new List<ActualLine>();
            foreach (DocumentLine line in document.Lines)
            {
                string l = LineToString(line, document);
                ActualLine actualLine = new ActualLine(l, LineOffset(l), line);
                lines.Add(actualLine);
            }

            return new ActualDocument(lines, document);
        }

        private static string LineToString(DocumentLine line, TextDocument document)
        {
            char[] chars = new char[line.Length];
            int docOffset = line.Offset;
            for (int i = 0; i < line.Length; i++) chars[i] = document.GetCharAt(docOffset + i);

            string result = new string(chars);
            return result;
        }

        private static int LineOffset(string line)
        {
            int offset = 0;
            if (!string.IsNullOrEmpty(line))
                foreach (char c in line)
                    switch (c)
                    {
                        case ' ':
                            offset++;
                            break;
                        case '\t':
                            offset += 2;
                            break;
                        default:
                            return offset;
                    }

            return offset;
        }

        public class ActualLine
        {
            public ActualLine(string line, int frontOffset, DocumentLine documentLine)
            {
                Line = line;
                FrontOffset = frontOffset;
                DocumentLine = documentLine;
            }

            public string Line { get; }
            public int FrontOffset { get; }
            public DocumentLine DocumentLine { get; }
        }
    }
}