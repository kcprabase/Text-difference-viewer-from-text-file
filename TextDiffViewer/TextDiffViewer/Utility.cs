using my.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.IO;

namespace TextDiffViewer
{
    /// <summary>
    /// The array of strings is compared here with diff.item[] array
    /// The css class is assigned to text lines according to the comparision
    /// </summary>
    public class Utility
    {
        private int x, y;
        private bool prevHadClass = false;
        private string trClass;
        private int trId=0;
        private bool giveId = false;
    //    private StringBuilder textOut = new StringBuilder(); //string with html tags and data to be printed
        
        /// <summary>
        /// Renders the difference for side view
        /// Writes the content on webpage using HttpContext calling functin WriteLine()
        /// Used when a whole line of text is inserted or deleted or is unchanged
        /// If only few words inside a section of difference is changed, PrintDiffLine() is called
        /// </summary>
        /// <param name="f">diffItem array sent from CompareFiles(). We will call a single member of array as a diff section
        /// if a diff section has only inserted items or deleted items. It means a whole set of lines of text is added or deleted
        /// if diff section has both deleted and inserted element, then probably only few words inside diff section is changed
        /// in such case PrintDiffLine is invoked</param>
        /// <param name="aLines">string array of first text sent from CompareFiles()</param>
        /// <param name="bLines">string array of first text sent from CompareFiles()</param>
        /// <param name="displayType">displayType sent from CompareFiles(). In this case side by side display</param>
        public void PrintDiffBody(Diff.Item[] f, string[] aLines, string[] bLines, bool diffOnly, string displayType)
        {
 //           textOut.Clear();
            int n = 0;
            int nCopy = 0;
            string trName = null;
            string btnName = null;
            string aParagraph;
            string bParagraph;
            string[] aWord;
            string[] bWord;
            Diff.Item[] wordItem;

            for (int fdx = 0; fdx < f.Length; fdx++)
            {
                Diff.Item aItem = f[fdx];
                if (diffOnly)
                {
                    trName = n.ToString() + "n";
                    btnName = "btn" + n.ToString();
                    if ((nCopy < aItem.StartA) && (nCopy < aLines.Length))
                    {
                      //  WriteLine(-2, null, "tdTextBlank", "", -2, null, "tdTextBlank", "", trName);
                                 CreateSideViewExpandButton(trName, btnName);
                    }

                    //increase line numbers and create hidden rows
                    while ((nCopy < aItem.StartA) && (nCopy < aLines.Length))
                {
                    WriteLine(nCopy, null, "tdText", aLines[nCopy], n, null, "tdText", bLines[n], trName);
                    n++;
                    nCopy++;
                } // while
                }else{
                // write unchanged lines
                while ((nCopy < aItem.StartA) && (nCopy < aLines.Length))
                {
                    WriteLine(nCopy, null, "tdText", aLines[nCopy], n, null, "tdText", bLines[n], null);
                    n++;
                    nCopy++;
                } // while
                    }
                // if lines are deleted and inserted at the same place
                if (aItem.deletedA != 0 && aItem.insertedB != 0)
                {
                    aParagraph = null;
                    bParagraph = null;
                    x = nCopy + 1;
                    y = n + 1;
                    // write deleted lines
                    for (int m = 0; m < aItem.deletedA; m++)
                    {
                        aParagraph += aLines[aItem.StartA + m];
                        nCopy++;
                    } // for

                    // write inserted lines
                    while (n < aItem.StartB + aItem.insertedB)
                    {
                        bParagraph += bLines[n];
                        n++;
                    } // while

                    char myChar = aParagraph[aParagraph.Length - 1];
                    if (myChar.ToString().Contains("\r"))
                        aParagraph = aParagraph.Remove(aParagraph.Length - 1);
                    aParagraph = aParagraph.Replace("\r", "\r ");
                    myChar = bParagraph[bParagraph.Length - 1];
                    if (myChar.ToString().Contains("\r"))
                        bParagraph = bParagraph.Remove(bParagraph.Length - 1);
                    bParagraph = bParagraph.Replace("\r", "\r ");
                    wordItem = Diff.DiffText(aParagraph, bParagraph, true, true, false, true);
                    aWord = aParagraph.Split(' ');
                    bWord = bParagraph.Split(' ');
                    HttpContext context = HttpContext.Current;

                    // for modified content of the file
                    if (wordItem.Length != 0)
                    {
                        PrintDiffLine(wordItem, aWord, bWord, context, "side");
                        trId++;
                    }
                    //In the case where the difference between lines is only an enter
                    else
                    {
                        nCopy = nCopy - aItem.deletedA;
                        n = n - aItem.insertedB;
                        if (aItem.deletedA > aItem.insertedB)
                        {
                            while (n < aItem.insertedB)
                            {
                                WriteLine(nCopy, "", "dForSideView", aLines[nCopy], n, "", "iForSideView", bLines[n], null);
                                n++;
                                nCopy++;
                            }
                            while (nCopy < aItem.deletedA)
                            {
                                WriteLine(nCopy, "", "dForSideView", aLines[nCopy], -1, null, "tdText", "", null);
                                nCopy++;
                            }
                        }
                        else
                        {
                            if (aItem.deletedA < aItem.insertedB)
                            {
                                while (nCopy < aItem.deletedA)
                                {
                                    WriteLine(nCopy, "", "dForSideView", aLines[nCopy], n, "", "iForSideView", bLines[n], null);
                                    n++;
                                    nCopy++;
                                }
                                while (n < aItem.insertedB)
                                {
                                    WriteLine(-1, null, "tdText", "", n, "", "iForSideView", bLines[n], null);
                                    n++;
                                }
                            }
                        }

                    }
                }
                else
                {
                    // write deleted lines
                    for (int m = 0; m < aItem.deletedA; m++)
                    {
                        if (m == 0) giveId = true;
                        WriteLine(nCopy, "dSpec", "dForSideView", aLines[aItem.StartA + m], -1, null, "tdText", "", null);
                        nCopy++;
                        //n++;
                    } // for

                    // write inserted lines
                    if (n < aItem.StartB + aItem.insertedB) giveId = true;
                    while (n < aItem.StartB + aItem.insertedB)
                    {
                        WriteLine(-1, null, "tdText", "", n, "iSpec", "iForSideView", bLines[n], null);

                        n++;
                        //nCopy++;
                    } // while
                }
            } // for
            if (diffOnly) {
                trName = nCopy.ToString() + "nCopy";
                btnName = "btnn" + nCopy.ToString();
                if (nCopy != aLines.Length)
                    //      WriteLine(-2, null, "tdTextBlank", "", -2, null, "tdTextBlank", "", trName);
                    CreateSideViewExpandButton(trName, btnName);
                //increase line numbers and Create Hidden rows
                while (nCopy < aLines.Length)
                {
                    WriteLine(nCopy, null, "tdText", aLines[nCopy], n, null, "tdText", bLines[n], trName);
                    n++;
                    nCopy++;
                }               
            }
            else { 
            // write rest of unchanged lines
            while (nCopy < aLines.Length)
            {
                WriteLine(nCopy, null, "tdText", aLines[nCopy], n, null, "tdText", bLines[n], null);
                n++;
                nCopy++;
            }
            }
            //         myTable.InnerHtml = textOut.ToString();
            //      return textOut.ToString();
        }//printDiffbody

        /// <summary>
        /// This Overloaded function PrintDiffBody is called without displayType. Display type is Inline view in this case.
        /// Returns string that is finally included in InnerHtml for table created by function CompareFiles()
        /// Writes the content on webpage using HttpContext calling functin WriteLine()
        /// </summary>

        public void PrintDiffBody(Diff.Item[] f, string[] aLines, string[] bLines, bool diffOnly)
        {
  //          textOut = textOut.Clear();
            int n = 0;
            int nn = 0;
            string aParagraph;
            string bParagraph;
            string[] aWord;
            string[] bWord;
            string trName = null;
            string btnId = null;
            Diff.Item[] wordItem;
            for (int fdx = 0; fdx < f.Length; fdx++)
            {
                Diff.Item aItem = f[fdx];

                if (diffOnly)
                {
                    trName = n.ToString() + "n";
                    btnId = "btn" + n.ToString();
                    if ((n < aItem.StartB) && (n < bLines.Length))
                    {
                        CreateInlineViewExpandButton(trName, btnId);
                    }
                    // increase line numbers for unchanged lines
                    while ((n < aItem.StartB) && (n < bLines.Length))
                    {
                        WriteLine(nn, n, null, null, bLines[n], trName);
                        nn++;
                        n++;
                    } // while
                }
                else {
                // write unchanged lines
                while ((n < aItem.StartB) && (n < bLines.Length))
                {
                    WriteLine(nn, n, null, null, bLines[n], null);
                    nn++;
                    n++;
                } // while
                }

                // if lines are deleted and inserted at the same place
                if (aItem.deletedA > 0 && aItem.insertedB > 0)
                {
                    aParagraph = null;
                    bParagraph = null;
                    x = nn + 1;
                    y = n + 1;
                    // write deleted lines
                    for (int m = 0; m < aItem.deletedA; m++)
                    {
                        aParagraph += aLines[aItem.StartA + m];
                        nn++;
                    } // for

                    // write inserted lines
                    while (n < aItem.StartB + aItem.insertedB)
                    {
                        bParagraph += bLines[n];
                        n++;
                    } // while

                    char myChar = aParagraph[aParagraph.Length - 1];
                    if (myChar.ToString().Contains("\r"))
                        aParagraph = aParagraph.Remove(aParagraph.Length - 1);
                    aParagraph = aParagraph.Replace("\r", "\r ");
                    myChar = bParagraph[bParagraph.Length - 1];
                    if (myChar.ToString().Contains("\r"))
                        bParagraph = bParagraph.Remove(bParagraph.Length - 1);
                    bParagraph = bParagraph.Replace("\r", "\r ");
                    wordItem = Diff.DiffText(aParagraph, bParagraph, true, true, false, true);
                    aWord = aParagraph.Split(' ');
                    bWord = bParagraph.Split(' ');
                    HttpContext context = HttpContext.Current;
                    context.Response.Write("<tr id='" + trId + "'");
                    PrintDiffLine(x, y, wordItem, aWord, bWord, context);
                    trId++;
                }//if
                else
                {
                    // write deleted lines
                    for (int m = 0; m < aItem.deletedA; m++)
                    {
                        if (m == 0)
                            giveId = true;
                        WriteLine(nn, -1, "dSpec","d", aLines[aItem.StartA + m], null);
                        nn++;
                    } // for
                    if (n < aItem.StartB + aItem.insertedB)
                        giveId = true;
                    // write inserted lines
                    while (n < aItem.StartB + aItem.insertedB)
                    {
                        WriteLine(-1, n, "iSpec", "i", bLines[n], null);
                        n++;
                    } // while
                }//else
            } // while
            if(diffOnly){
                trName = n.ToString() + "n";
                btnId = "btnn" + n.ToString();
                if (n < bLines.Length)
                    CreateInlineViewExpandButton(trName, btnId);
                //increase line numbers
                while (n < bLines.Length)
                {
                    WriteLine(nn, n, null, null, bLines[n], trName);
                    n++;
                    nn++;
                } // while
                //if (n < bLines.Length) { 
                //WriteLine(-1, -1, null, "tdText", "");
                //WriteLine(-1, -1, null, "tdText", "");
                //}
            }else{
            // write rest of unchanged lines
            while (n < bLines.Length)
            {
                WriteLine(nn, n, null, null, bLines[n], null);
                n++;
                nn++;
            } // while
                }
        }//PrintDiffBody

        /// <summary>
        /// For Inline View
        /// This function renders a complete line of text. Not a particular word inside a line.
        /// We have used html table to display our text viewer
        /// WriteLine is called by function PrintDiffBody to manipulate text 
        /// we use HttpContext to write html elements on web page
        /// previously stored all the html codes in a string variable and added to innerhtml of table already created using htmlgenericcontrol
        /// which caused browser to stop responding when string size was large
        /// </summary>
        /// <param name="mr">number line of old file version to be received from PrintDiffBody, printed with each line of text</param>
        /// <param name="nr">number line of new file version to be received from PrintDiffBody, printed with each line of text</param>
        /// <param name="typ">css class for line to be printed. either "i" or "d" sent from PrintDiffBody</param>
        /// <param name="backTyp">css class for background color green for section with insertion, red for section with addition</param>
        /// <param name="aText">Line of text from two uploaded text files finally to be printed with css class assigned to each</param>
        /// <param name="trName">name of tablerow given for rows that are to be hidden when difference only is checked. Name are same for a section of text
        /// separated by difference section</param>
        public void WriteLine(int mr, int nr, string typ, string backTyp, string aText, string trName)
        {
            // textOut.Clear();
            HttpContext ctx = HttpContext.Current;
            if (trName != null){
                if (trId == 0) {
                    ctx.Response.Write("<tr id=" + trId + " name=" + trName + " style=\"display:none\" ><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓</a>");
                    trId++;
                }
                else
                    ctx.Response.Write("<tr name=" + trName + " style=\"display:none\"><td class=\"idButton\">");
            }
            else
            {
                if (trId == 0 || giveId) {
                    ctx.Response.Write("<tr id=" + trId + "><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓</a>"); 
                    trId++;
                    giveId = false;
                }else
                ctx.Response.Write("<tr><td class=\"idButton\">");
            }
            if (mr >= 0)
                ctx.Response.Write((mr + 1).ToString());
            else
                ctx.Response.Write("&nbsp;");
            ctx.Response.Write("</td><td class=\"idButton\">");
            if (nr >= 0)
                ctx.Response.Write((nr + 1).ToString());
            else
                    ctx.Response.Write("&nbsp;");
            ctx.Response.Write("</td><td class=\"" + backTyp + "\"><span ");
            if (typ != null)
                ctx.Response.Write(" class=\"" + typ + "\"");
            aText = ctx.Server.HtmlEncode(aText).Replace("\r", "");
            ctx.Response.Write(">" + aText + "</span></td></tr>");

        }//WriteLine

        /// <summary>
        /// For Side by Side view
        /// </summary>
        /// <param name="na">line number for old file version</param>
        /// <param name="typA">css class for line of text for old file version</param>
        /// <param name="backTypA">background color for text for old version</param>
        /// <param name="aText">Texts from old version</param>
        /// <param name="nb">line number for new file version</param>
        /// <param name="typB">css class for text lines of new version file</param>
        /// <param name="backTypB">background color for text for new version</param>
        /// <param name="bText">Texts from new version</param>
        /// <param name="trName">name of tablerow given for rows that are to be hidden when difference only is checked. Name are same for a section of text
        /// separated by difference section</param>
        public void WriteLine(int na, string typA, string backTypA, string aText, int nb, string typB, string backTypB, string bText, string trName)
        {
            //    textOut.Clear();
            HttpContext ctx = HttpContext.Current;
       //     if (na == -2) {
       //         ctx.Response.Write("<tr><td class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "')\" >...</td><td class=" + backTypA + "></td><td class=\"idButton\" class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "')\" >...</td><td td class=" + backTypB + "></td></tr>");
       //   }else{
            if (trName != null){
                if (trId == 0) {
                    ctx.Response.Write("<tr id=" + trId + " name=" + trName + " style=\"display:none\" ><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓</a>");
                    trId++;
                }
                else
                    ctx.Response.Write("<tr name=" + trName + " style=\"display:none\"><td class=\"idButton\">");
            }
            else { 
            if (trId == 0 || giveId)
                {
                ctx.Response.Write("<tr id=" + trId + "><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓</a>");
                trId++;
                giveId = false;
                }
            else
                ctx.Response.Write("<tr><td class=\"idButton\">");
            }
            if (na >= 0)
                ctx.Response.Write((na + 1).ToString());
            else
                    ctx.Response.Write("&nbsp;");
            
            ctx.Response.Write("</td><td class="+ backTypA +"><span  ");
            if (typA != null)
                ctx.Response.Write(" class=\"" + typA + "\"");
            aText = ctx.Server.HtmlEncode(aText).Replace("\r", "");
            ctx.Response.Write(">" + aText + "</span></td>");

            ctx.Response.Write("<td class=\"idButton\">");
            if (nb >= 0)
                ctx.Response.Write((nb + 1).ToString());
            else
            {
                if (nb == -1)
                    ctx.Response.Write("&nbsp;");
                else
                    ctx.Response.Write(" .....");
            }
            ctx.Response.Write("</td><td class=" + backTypB + "><span");
            if (typB != null)
                ctx.Response.Write(" class=\"" + typB + "\"");
            bText = ctx.Server.HtmlEncode(bText).Replace("\r", "");
            ctx.Response.Write(">" + bText + "</span></td></tr>");
        }
  //  }
        /// <summary>
        /// PrintDiffLine for Inline View
        /// PrintDiffLine is invoked from PrintDiffBody if a diff section has both inserted and deleted elements at the same time.
        /// Similar to PrintDiffBody but this function prints one word (or set of characters separated by a space) at a time placing it inside a span tag
        /// Used when only single or few words inside a diff section is changed
        /// </summary>
        /// <param name="x">Line number for old version</param>
        /// <param name="y">Line number for new version</param>
        /// <param name="wordItem">diffItem array (like diff sections). diffitem in case of printdiffline is generated for each word
        /// and instead of whole text, diffitem in this case is calculated between inserted and deleted lines of same diff sections sent from printdiffbody
        /// </param>
        /// <param name="aWord">Words from old file version </param>
        /// <param name="bWord">words from new file version</param>
        /// <param name="context">current httpcontext sent from printdiffbody</param>
        public void PrintDiffLine(int x, int y, Diff.Item[] wordItem, string[] aWord, string[] bWord, HttpContext context)
        {
  //          textOut = textOut.Clear();

            //Printing lines of old files and deleted part highlighed red
            trClass = "d";
            context.Response.Write("class = " + trClass + "><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓" + x + "</a></td><td class=\"idButton\">" + "&nbsp;" + "</td><td>");
            int num = 0;
            //   int nn = 0;
            for (int fdx = 0; fdx < wordItem.Length; fdx++)
            {
                Diff.Item aItem = wordItem[fdx];

                // write unchanged lines
                while ((num < aItem.StartA) && (num < aWord.Length))
                {
                    WriteWord(null, aWord[num], context);
                    //          nn++;
                    num++;
                } // while

                // write deleted lines
                for (int m = 0; m < aItem.deletedA; m++)
                {
                    WriteWord("dSpec", aWord[aItem.StartA + m], context);
                    num++;
                    //             nn++;
                } // for

                // write inserted lines
                //while (num < aItem.StartB + aItem.insertedB)
                //{
                ////    WriteWord("i", bWord[num], context);
                //    num++;
                //} // while

            } // while

            // write rest of unchanged lines
            while (num < aWord.Length)
            {
                WriteWord(null, aWord[num], context);
                num++;
                //         nn++;
            } // while
            context.Response.Write("</td></tr>");

            //print lines of new file with inserted words highlighted green
            trClass = "i";
            context.Response.Write("<tr class = " + trClass + "><td class=\"idButton\">" + "&nbsp;" + "</td><td class=\"idButton\">" + y + "</td><td>");
            num = 0;
            //   int nn = 0;
            for (int fdx = 0; fdx < wordItem.Length; fdx++)
            {
                Diff.Item aItem = wordItem[fdx];

                // write unchanged lines
                while ((num < aItem.StartB) && (num < bWord.Length))
                {
                    WriteWord(null, bWord[num], context);
                    //          nn++;
                    num++;
                } // while

                // write deleted lines
                //for (int m = 0; m < aItem.deletedA; m++)
                //{
                //    WriteWord("d", aWord[aItem.StartA + m], context);
                //    //             nn++;
                //} // for

                // write inserted lines
                while (num < aItem.StartB + aItem.insertedB)
                {
                    WriteWord("iSpec", bWord[num], context);
                    num++;
                } // while

            } // while

            // write rest of unchanged lines
            while (num < bWord.Length)
            {
                WriteWord(null, bWord[num], context);
                num++;
                //         nn++;
            } // while
            context.Response.Write("</td></tr>");
        }


        /// <summary>
        /// PrintDiffLine for side by side view
        /// </summary>
        /// <param name="displayType">fileVersion is used just to overload the function and use it for side by side view.
        /// displayType specifies that the work is to be done for side by side view</param>
        public void PrintDiffLine(Diff.Item[] wordItem, string[] aWord, string[] bWord, HttpContext context, string displayType)
        {
  //          textOut = textOut.Clear();

            prevHadClass = false;
            //Printing lines of old files and deleted part highlighed red

            context.Response.Write("<tr id='" + trId + "'><td class=\"idButton\"><a href=#" + (trId + 1) + " onclick=\"getUtlCurrentId(" + (trId + 1) + ")\">↓" + x + "</a></td><td class=\"tdTextA\">");
            int num = 0;
            int nn = 0;
            int fdy = 0;
            int fdx = 0;

            //   int nn = 0;
            while ((num < aWord.Length || nn < bWord.Length))
            {

                if (num == aWord.Length && prevHadClass == false)
                {

                    context.Response.Write("</td><td class=\"idButton\">" + y + "</td><td class=\"tdTextB\">");
                    prevHadClass = true;
                }
                if (nn == bWord.Length && prevHadClass == true)
                {
                    if (num != aWord.Length)
                    {
                        y++;
                        context.Response.Write("</td></tr><tr><td class=\"idButton\">" + x + "</td><td class=\"tdTextA\">");
                    }// if
                    prevHadClass = false;
                }// if
                Diff.Item aItem = wordItem[fdx];
                Diff.Item bItem = wordItem[fdy];

                // write unchanged lines for old text
                while (num < aItem.StartA && num < aWord.Length && prevHadClass == false)
                {
                    WriteWord(null, aWord[num], context, num, aWord, nn, bWord);
                    //          nn++;
                    num++;
                    if (num == aWord.Length && prevHadClass == false)
                    {

                        context.Response.Write("</td><td class=\"idButton\">" + y + "</td><td class=\"tdTextB\">");
                        prevHadClass = true;
                    }// if
                } // while


                //Write unchanged lines for new text
                while (nn < bItem.StartB && nn < bWord.Length && prevHadClass == true)
                {
                    WriteWord(null, bWord[nn], context, num, aWord, nn, bWord);
                    nn++;
                    if (nn == bWord.Length && prevHadClass == true)
                    {
                        if (num != aWord.Length)
                            context.Response.Write("</td></tr><tr><td class=\"idButton\">" + x + "</td><td class=\"tdTextA\">");
                        prevHadClass = false;
                    }// if

                }// while

                // write deleted lines
                while (aItem.StartA <= num && num < aItem.StartA + aItem.deletedA && prevHadClass == false)
                {

                    WriteWord("dSpec", aWord[num], context, num, aWord, nn, bWord);
                    num++;
                    if (num == aWord.Length && prevHadClass == false)
                    {
                        if (nn != bWord.Length)
                            context.Response.Write("</td><td class=\"idButton\">" + y + "</td><td class=\"tdTextB\">");
                        else
                            context.Response.Write("</td><td class=\"idButton\">" + "&nbsp;" + "</td><td class=\"tdTextB\">");
                        prevHadClass = true;
                    }// if
                } // While

                //write inserted lines
                while (bItem.StartB <= nn && nn < bItem.StartB + bItem.insertedB && prevHadClass == true)
                {

                    WriteWord("iSpec", bWord[nn], context, num, aWord, nn, bWord);
                    nn++;
                    if (nn == bWord.Length && prevHadClass == true)
                    {
                        if (num != aWord.Length)
                            context.Response.Write("</td></tr><tr><td class=\"idButton\">" + x + "</td><td class=\"tdTextA\">");
                        prevHadClass = false;
                    }// if


                } // while

                //Write remaining unchanged lines in textA
                while (num < aWord.Length && num >= wordItem[wordItem.Length - 1].StartA + wordItem[wordItem.Length - 1].deletedA && prevHadClass==false)
                {
                    WriteWord(null, aWord[num], context, num, aWord, nn, bWord);
                    num++;
                    if (num == aWord.Length && prevHadClass == false)
                    {
                        if (nn != bWord.Length)
                            context.Response.Write("</td><td class=\"idButton\">" + y + "</td><td class=\"tdTextB\">");
                        else
                            context.Response.Write("</td><td class=\"idButton\">" + "&nbsp;" + "</td><td class=\"tdTextB\">");
                        prevHadClass = true;
                    }// if
                }// While

                //Write remaining unchanged lines in textB
                while (nn < bWord.Length && nn >= wordItem[wordItem.Length - 1].StartB + wordItem[wordItem.Length - 1].insertedB && prevHadClass==true)
                {
                    WriteWord(null, bWord[nn], context, num, aWord, nn, bWord);
                    nn++;
                }//While


                if (num == aItem.StartA + aItem.deletedA && fdx < wordItem.Length - 1)
                {
                    fdx++;

                }// If
                if (nn == bItem.StartB + bItem.insertedB && fdy < wordItem.Length - 1)
                {
                    fdy++;

                }// If
            } // while
            context.Response.Write("</td></tr>");
        }
        

        /// <summary>
        /// WriteWord for inline view
        /// Works like WriteLine, prints one word at a time embedded inside span tag
        /// </summary>
        /// <param name="typ">css class for each word</param>
        /// <param name="aText">each word of diff sections</param>
        /// <param name="context">context sent from PrintDiffLine</param>
        private void WriteWord(string typ, string aText, HttpContext context)
        {
            // textOut.Clear();
            //  HttpContext ctx = HttpContext.Current;
            context.Response.Write("<span ");
            if (typ != null)
            {
                prevHadClass = true;
                context.Response.Write(" class=\"" + typ + "\"");
            }
            aText = context.Server.HtmlEncode(aText);
            if (!aText.Contains("\r"))
            {
                context.Response.Write(">" + aText + "&nbsp;</span>");
            }
            else
            {
                context.Response.Write(">" + aText + "</span>");
            }
            if (aText.Contains("\r"))
            {
                if (trClass == "d")
                {
                    x++;
                    context.Response.Write("</tr><tr class = " + trClass + "><td class=\"idButton\">" + x + "</td><td class=\"idButton\">" + "&nbsp" + "</td><td>");
                }
                else if (trClass == "i")
                {
                    y++;
                    context.Response.Write("</tr><tr class = " + trClass + "><td class=\"idButton\">" + "&nbsp" + "</td><td class=\"idButton\">" + y + "</td><td>");
                }
            }
        }

        /// <summary>
        /// WriteWord for side view
        /// </summary>
        /// <param name="typ">css class for each word</param>
        /// <param name="aText">each word of diff sections</param>
        /// <param name="context">context sent from PrintDiffLine</param>
        /// <param name="num">number of deleted words in diff section</param>
        /// <param name="aWord">array of all the deleted words from old file version. Used just to get length of array</param>
        /// <param name="nn">number of inserted words in diff section</param>
        /// <param name="bWord">array of all the inserted words in new file version. Used just to get length of array
        /// Then why not just send the array length instead of whole array?
        /// Because we are lazy.
        /// You do it</param>
        private void WriteWord(string typ, string aText, HttpContext context, int num, string[] aWord, int nn, string[] bWord)
        {
            // textOut.Clear();
            //  HttpContext ctx = HttpContext.Current;

            context.Response.Write("<span ");
            if (typ != null)
            {

                context.Response.Write(" class=\"" + typ + "\"");
            }
            aText = context.Server.HtmlEncode(aText);
            if (!aText.Contains("\r"))
            {
                context.Response.Write(">" + aText + "&nbsp;</span>");
            }
            else
            {
                context.Response.Write(">" + aText + "</span>");
            }
            if (aText.Contains("\r"))
            {
                if (prevHadClass == false)
                {
                    prevHadClass = true;
                    x++;
                    if (nn != bWord.Length)
                        context.Response.Write("</td><td class=\"idButton\">" + y + "</td><td class=\"tdTextB\">");
                    else
                        context.Response.Write("</td><td class=\"idButton\">" + "&nbsp;" + "</td><td class=\"tdTextB\">");
                }
                else if (prevHadClass == true)
                {
                    prevHadClass = false;
                    y++;
                    if (num != aWord.Length)
                        context.Response.Write("</td></tr><tr><td class=\"idButton\">" + x + "</td><td class=\"tdTextA\">");
                    else
                        context.Response.Write("</td></tr><tr class = " + trClass + " ><td class=\"idButton\">" + "&nbsp;" + "</td><td class=\"tdTextA\">");
                }
            }
        }

        /// <summary>
        /// For side by side view
        /// Create a table row with two buttons used to expand hidden unchanged lines of text
        /// </summary>
        /// <param name="trName">name of rows that a particular pair of buttons can expand</param>
        /// <param name="btnId">name of button pairs that expand certain number of hidden lines below them and disappear after click</param>
        private void CreateSideViewExpandButton(string trName, string btnId) {
            HttpContext ctx = HttpContext.Current;
            ctx.Response.Write("<tr id =" + btnId + "><td name =" + btnId + " class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "','" + btnId + "', 0)\" >...</td><td class=\" tdTextBlank \"></td><td name =" + btnId + " class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "','" + btnId + "', 0)\" >...</td><td td class=\" tdTextBlank \"></td></tr>");
        }

        /// <summary>
        /// For inline view
        /// Create a table row with two buttons used to expand hidden unchanged lines of text
        /// </summary>
        /// <param name="trName">name of rows that a particular pair of buttons can expand</param>
        /// <param name="btnId">name of button pairs that expand certain number of hidden lines below them and disappear after click</param>
        private void CreateInlineViewExpandButton(string trName, string btnId) {
            HttpContext ctx = HttpContext.Current;
            ctx.Response.Write("<tr id =" + btnId + "><td name =" + btnId + " class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "','" + btnId + "', 0)\" >...</td></td><td name =" + btnId + " class=\"idButton\"  onclick=\"ToggleVisibility('" + trName + "','" + btnId + "', 0)\" >...</td><td td style=\"background-color:#d3d3d3\"></td></tr>");
        }

        /// <summary>
        /// this function records important tasks that happens when website is used to view difference in texts
        /// and assign current time with tasks
        /// </summary>
        /// <param name="filename">path of our log file in the server</param>
        /// <param name="logMessage">message to be printed in the log file when a task is done</param>
        public static void Log(string filename, string logMessage)
        {            
            StreamWriter sw = new StreamWriter(filename , true);
     //       sw.Write("\r\n");
            sw.Write("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
 //           sw.WriteLine("  :");
            sw.WriteLine("  :{0}", logMessage);
            sw.WriteLine("--------------------------------------------------------------");
            sw.Flush();
            sw.Close();
        }

    }
}