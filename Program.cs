// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

// var text = "1+2/2-10*2"; //"3 + 5 * 2 - 8 / 4";
// var lexer = new Lexer(text);
// var parser = new Parser(lexer);
// var result = parser.Parse();
// Console.WriteLine(result); // Output: 10

Console.WriteLine("Calculator");
while (true)
{
    Console.WriteLine("Enter an expression (or press Enter to exit): ");
    var line = Console.ReadLine();
    if (string.IsNullOrEmpty(line)) break;
    var lexer = new Lexer(line);
    var parser = new Parser(lexer);
    var result = parser.Parse();
    Console.WriteLine($"Result: {result}");
}

public enum TokenType
{
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    // LeftParen,
    // RightParen,
    EOF
}

public record Token(string Value, TokenType Type);

class Lexer
{
    private readonly string text;
    private int position;
    private char currentChar;

    public Lexer(string text)
    {
        this.text = text;
        position = 0;
        currentChar = text[position];

    }

    private void Error() => throw new Exception("Invalid character");

    private void Advance()
    {
        position++;
        if (position > text.Length - 1)
            currentChar = '\0'; // Indicates end of input
        else
            currentChar = text[position];
    }

    private void SkipWhitespace()
    {
        while (currentChar != '\0' && char.IsWhiteSpace(currentChar))
            Advance();
    }

    private string ParseNumber()
    {
        var result = "";
        while (currentChar != '\0' && char.IsDigit(currentChar))
        {
            result += currentChar;
            Advance();
        }
        return result;
    }

    public Token GetNextToken()
    {
        while (currentChar != '\0')
        {
            if (char.IsWhiteSpace(currentChar))
            {
                SkipWhitespace();
                continue;
            }

            switch (currentChar)
            {
                case '+':
                    Advance();
                    return new Token("+", TokenType.Plus);
                case '-':
                    Advance();
                    return new Token("-", TokenType.Minus);
                case '*':
                    Advance();
                    return new Token("*", TokenType.Multiply);
                case '/':
                    Advance();
                    return new Token("/", TokenType.Divide);
                default:
                    if (char.IsDigit(currentChar))
                        return new Token(ParseNumber(), TokenType.Number);
                    Error();
                    break;
            }
            ;

        }
        return new Token("", TokenType.EOF);
    }


}

class Parser
{
    private readonly Lexer lexer;
    private Token currentToken;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
        currentToken = lexer.GetNextToken();
    }

    private void Error() => throw new Exception("Invalid syntax");

    private readonly Dictionary<TokenType, int> precedences = new()
    {
        { TokenType.Plus, 10 },
        { TokenType.Minus, 10 },
        { TokenType.Multiply, 20 },
        { TokenType.Divide, 20 }
    };

    int applyOperator(int left, Token op, int right) => op.Type switch
    {
        TokenType.Plus => left + right,
        TokenType.Minus => left - right,
        TokenType.Multiply => left * right,
        TokenType.Divide => left / right,
        _ => throw new Exception("Invalid operator")
    };

    private void Eat(TokenType expectedTokenType)
    {
        if (currentToken.Type == expectedTokenType)
            currentToken = lexer.GetNextToken();
        else
            Error();
    }

    private int ParsePrefix()
    {
        var token = currentToken;
        Eat(TokenType.Number);
        return int.Parse(token.Value);
    }

    private int ParseExpression(int precedence = 0)
    {
        var left = ParsePrefix();

        while (currentToken.Type != TokenType.EOF && precedences.GetValueOrDefault(currentToken.Type, 0) > precedence)
        {
            var op = currentToken;
            Eat(op.Type);
            var right = ParseExpression(precedences[op.Type]);
            left = applyOperator(left, op, right);
        }

        return left;
    }
    
    public int Parse() => ParseExpression();
}