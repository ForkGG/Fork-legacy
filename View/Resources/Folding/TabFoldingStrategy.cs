using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Fork.View.Resources.Folding;

public class TabFoldingStrategy
{
    // How many spaces == one tab
    private const int SpacesInTab = 2;

    /// <summary>
    ///     Creates a new TabFoldingStrategy.
    /// </summary>
    public TabFoldingStrategy()
    {
    }

    /// <summary>
    ///     Create <see cref="NewFolding" />s for the specified document and updates the folding manager with them.
    /// </summary>
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        int firstErrorOffset;
        IEnumerable<NewFolding> foldings = CreateNewFoldings(document, out firstErrorOffset);
        manager.UpdateFoldings(foldings, firstErrorOffset);
    }

    /// <summary>
    ///     Create <see cref="NewFolding" />s for the specified document.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        firstErrorOffset = -1;
        return CreateNewFoldingsByLine(document);
    }

    /// <summary>
    ///     Create <see cref="NewFolding" />s for the specified document.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldingsByLine(TextDocument document)
    {
        List<NewFolding> newFoldings = new();
        if (document == null || document.LineCount <= 1)
        {
            return newFoldings;
        }

        ActualDocument actualDocument = ActualDocument.BuildActualDocument(document);
        newFoldings.AddRange(CreateFoldings(actualDocument.Lines.First, new Stack<NewFolding>()));

        newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return newFoldings;
    }

    private List<NewFolding> CreateFoldings(LinkedListNode<ActualDocument.ActualLine> linkedLine,
        Stack<NewFolding> offsetStack)
    {
        List<NewFolding> foldings = new();

        if (linkedLine.Next == null)
        {
            while (offsetStack.Count > 0)
            {
                NewFolding folding = offsetStack.Pop();
                folding.EndOffset = linkedLine.Value.DocumentLine.EndOffset;
                foldings.Add(folding);
            }

            return foldings;
        }

        int thisLineOffset = linkedLine.Value.FrontOffset;
        int nextLineOffset = linkedLine.Next.Value.FrontOffset;

        //This is the first line of a folding
        if (nextLineOffset > thisLineOffset)
        {
            int documentOffset = linkedLine.Value.DocumentLine.Offset;
            NewFolding folding = new();
            folding.Name = linkedLine.Value.Line;
            folding.StartOffset = documentOffset;
            offsetStack.Push(folding);
        }

        //This is the last line of a folding
        while (thisLineOffset > nextLineOffset)
        {
            if (offsetStack.Count > 0)
            {
                NewFolding folding = offsetStack.Pop();
                folding.EndOffset = linkedLine.Value.DocumentLine.EndOffset;
                foldings.Add(folding);
            }

            thisLineOffset -= 2;
        }

        if (linkedLine.Next != null)
        {
            foldings.AddRange(CreateFoldings(linkedLine.Next, offsetStack));
        }


        return foldings;
    }
}