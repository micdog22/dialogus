
using Dialogus.Core;
using Xunit;
using FluentAssertions;

namespace Dialogus.Tests;

public class ParserRuntimeTests
{
    [Fact]
    public void Should_Parse_And_Run_Branching()
    {
        var src = @"::intro
""Olá, {player}!""
? ""Quest"" -> quest
? ""Sair"" -> end

::quest
""Ok, pegue 3 cristais.""
set hasQuest = true
goto end

::end
""Até mais!""
";
        var doc = DialogueParser.Parse(src);
        doc.Nodes.Should().ContainKey("intro");
        var store = new VariableStore(new() { ["player"] = new Value.Str("Ana") });
        var runner = new DialogueRunner(doc, store);
        runner.Start("intro");

        runner.State.Should().BeOfType<DialogueState.Line>();
        var l1 = (DialogueState.Line)runner.State;
        l1.Text.Should().Contain("Olá, Ana");

        runner.Next();
        runner.State.Should().BeOfType<DialogueState.Choices>();
        var ch = (DialogueState.Choices)runner.State;
        ch.Options.Count.Should().Be(2);

        runner.Choose(0); // quest
        runner.State.Should().BeOfType<DialogueState.Line>();
        runner.Next(); // set
        runner.Next(); // goto -> end line
        runner.State.Should().BeOfType<DialogueState.Line>();
        runner.Next();
        runner.State.Should().BeOfType<DialogueState.End>();
    }

    [Fact]
    public void If_Should_Work()
    {
        var src = @"::n
if hasKey
""Abra a porta.""
endif
""Fim.""
";
        var doc = DialogueParser.Parse(src);
        var store = new VariableStore(new() { ["hasKey"] = new Value.Bool(true) });
        var runner = new DialogueRunner(doc, store);
        runner.Start("n");
        var l1 = (DialogueState.Line)runner.State;
        l1.Text.Should().Be("Abra a porta.");
        runner.Next();
        var l2 = (DialogueState.Line)runner.State;
        l2.Text.Should().Be("Fim.");
    }
}
