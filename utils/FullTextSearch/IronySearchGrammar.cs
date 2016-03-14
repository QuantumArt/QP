using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Globalization;

namespace Quantumart.QP8.Utils.FullTextSearch
{
    [Language("SearchGrammar", "1.0", "Google-to-SQL query converter")]
    internal class IronySearchGrammar : Grammar
    {         
        public IronySearchGrammar() : base(false)
        {
            this.GrammarComments =
              "Google-to-SQL full-text query format converter. Based on original project by Michael Coles.\r\n" +
              "http://www.sqlservercentral.com/articles/Full-Text+Search+(2008)/64248/ \r\n" +
              "Slightly revised to work with latest version of Irony. ";

            // Terminals
            var Term = CreateTerm("Term");
            var Phrase = new StringLiteral("Phrase", "\"");
            var ImpliedAnd = new ImpliedSymbolTerminal("ImpliedAnd");

            // NonTerminals
            var BinaryExpression = new NonTerminal("BinaryExpression");
            var BinaryOp = new NonTerminal("BinaryOp");
            var Expression = new NonTerminal("Expression");
            var PrimaryExpression = new NonTerminal("PrimaryExpression");
            var ThesaurusExpression = new NonTerminal("ThesaurusExpression");
            var ThesaurusOperator = new NonTerminal("ThesaurusOperator");
            var ExactExpression = new NonTerminal("ExactExpression");
            var ParenthesizedExpression = new NonTerminal("ParenthesizedExpression");
            var ProximityExpression = new NonTerminal("ProximityExpression");
            var ProximityList = new NonTerminal("ProximityList");

            this.Root = Expression;
            Expression.Rule = PrimaryExpression | BinaryExpression;
            BinaryExpression.Rule = Expression + BinaryOp + Expression;
            BinaryOp.Rule = ImpliedAnd | "and" | "&" | "-" | "or" | "|";
            PrimaryExpression.Rule = Term
                                   | ThesaurusExpression
                                   | ExactExpression
                                   | ParenthesizedExpression
                                   | Phrase
                                   | ProximityExpression;
            ThesaurusExpression.Rule = "~" + Term;
            ExactExpression.Rule = "+" + Term | "+" + Phrase;
            ParenthesizedExpression.Rule = "(" + Expression + ")";
            ProximityExpression.Rule = "<" + ProximityList + ">";
            MakePlusRule(ProximityList, Term);

            MarkTransient(PrimaryExpression, Expression, ProximityExpression, ParenthesizedExpression, BinaryOp);
            MarkPunctuation("<", ">", "(", ")");
            RegisterOperators(10, "or", "|");
            RegisterOperators(20, "and", "&", "-");
            RegisterOperators(20, ImpliedAnd);
            //Register brace pairs to improve error reporting
            RegisterBracePair("(", ")");
            RegisterBracePair("<", ">");
            //Do not report ImpliedAnd as expected symbol - it is not really a symbol
            this.AddToNoReportGroup(ImpliedAnd);
            //also do not report braces as expected
            this.AddToNoReportGroup("(", ")", "<", ">");

            LanguageFlags |= LanguageFlags.CanRunSample;
        }

        //Creates extended identifier terminal that allows international characters
        // Following the pattern used for c# identifier terminal in TerminalFactory.CreateCSharpIdentifier method;
        private IdentifierTerminal CreateTerm(string name)
        {
            IdentifierTerminal term = new IdentifierTerminal(name, "!@#$%^*_’'.?-", "!@#$%^*_’'.?0123456789");
            term.CharCategories.AddRange(new UnicodeCategory[] {
             UnicodeCategory.UppercaseLetter, //Ul
             UnicodeCategory.LowercaseLetter, //Ll
             UnicodeCategory.TitlecaseLetter, //Lt
             UnicodeCategory.ModifierLetter,  //Lm
             UnicodeCategory.OtherLetter,     //Lo
             UnicodeCategory.LetterNumber,     //Nl
             UnicodeCategory.DecimalDigitNumber, //Nd
             UnicodeCategory.ConnectorPunctuation, //Pc
             UnicodeCategory.SpacingCombiningMark, //Mc
             UnicodeCategory.NonSpacingMark,       //Mn
             UnicodeCategory.Format                //Cf,             
          });
            //StartCharCategories are the same
            term.StartCharCategories.AddRange(term.CharCategories);
            return term;
        }                 
    }
}
