using Antlr4.Runtime;
using Monkey.AST;
using Monkey.Visitors;

using System.IO;

using System.Windows;

namespace Editor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            OutputConsole.Clear();

            string code = CodeEditor.Text;

            try
            {
                // Capturar salida de Console.WriteLine
                var outputWriter = new StringWriter();
                Console.SetOut(outputWriter);

                // 1) Lexer + Parser
                var inputStream = new AntlrInputStream(code);
                var lexer = new MonkeyLexer(inputStream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MonkeyParser(tokens);

                var parseTree = parser.program();

                // 2) AST
                var builder = new AstBuilder();
                ProgramNode ast = builder.Build(parseTree);

                // 3) Type Checking
                var tc = new TypeCheckVisitor();
                ast.Accept(tc);

                if (tc.Errors.Count == 0)
                {
                    // 4) Encoder + Run
                    var encoder = new EncoderVisitor(tc);
                    encoder.GenerateAndRun(ast);
                }
                else
                {
                    OutputConsole.AppendText("Type errors:\n");
                    foreach (var err in tc.Errors)
                        OutputConsole.AppendText(err + "\n");
                }

                // Volcar salida
                OutputConsole.AppendText(outputWriter.ToString());
            }
            catch (Exception ex)
            {
                OutputConsole.AppendText("Error:\n" + ex);
            }
        }
    }
}