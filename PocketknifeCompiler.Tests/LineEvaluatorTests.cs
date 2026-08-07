using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class LineEvaluatorTests
{
    private StringWriter _consoleOut = null!;

    [SetUp]
    public void Setup()
    {
        _consoleOut = new StringWriter();
        Console.SetOut(_consoleOut);
    }

    [TearDown]
    public void TearDown()
    {
        _consoleOut.Dispose();
    }

    private static LineEvaluator BuildEvaluator(string source)
    {
        var p = new Parser();
        p.Parse(source);
        var catalog = OpCatalog.GetDefaultOpCatalog();
        var compiler = new Compiler(catalog);
        var compiled = compiler.StartCompile(p.Program);
        var ev = new LineEvaluator();
        ev.SetRoot(compiled);
        return ev;
    }

    private static void StepToEnd(LineEvaluator ev)
    {
        while (ev.CanStep)
        {
            ev.Step();
        }
    }

    // ── Basic lifecycle ──────────────────────────────────────────────────────

    [Test]
    public void InitialStateIsNotStarted()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n:print");
        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        Assert.That(ev.CanStep, Is.True);
        Assert.That(ev.CanStepBack, Is.False);
    }

    [Test]
    public void AfterFirstStepIsRunning()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n:print");
        ev.Step();
        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.Running));
        Assert.That(ev.CanStepBack, Is.True);
    }

    [Test]
    public void StepToEndReachesDone()
    {
        var ev = BuildEvaluator("""
            >"Hello"
            |to-upper
            :print
            """);
        StepToEnd(ev);
        Assert.That(ev.Current.IsDone, Is.True);
        Assert.That(ev.CanStep, Is.False);
    }

    [Test]
    public void StepAfterDoneDoesNothing()
    {
        var ev = BuildEvaluator(">\"Hello\"\n:print");
        StepToEnd(ev);
        var state = ev.Current;
        ev.Step(); // should be a no-op
        Assert.That(ev.Current.Phase, Is.EqualTo(state.Phase));
    }

    [Test]
    public void ResetRestoresNotStarted()
    {
        var ev = BuildEvaluator(">\"Hello\"\n:print");
        ev.Step();
        ev.Reset();
        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        Assert.That(ev.CanStepBack, Is.False);
    }
    
    [Test]
    public void StepBack_AfterOneStepRestoresNotStarted()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n:print");
        ev.Step();
        ev.StepBack();
        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        Assert.That(ev.CanStepBack, Is.False);
    }

    [Test]
    public void StepBackWhenNothingToUndoDoesNothing()
    {
        var ev = BuildEvaluator(">\"Hello\"\n:print");
        var before = ev.Current;
        ev.StepBack(); // should be a nop
        Assert.That(ev.Current.Phase, Is.EqualTo(before.Phase));
    }

    [Test]
    public void StepForwardAndBackMultipleStepsRestoresPreviousState()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n|to-lower\n:print");
        ev.Step(); //step 1
        ev.Step(); //step 2
        var stateAfterTwo = ev.Current;
        ev.Step(); //step 3
        ev.StepBack(); //back to step 2
        Assert.That(ev.Current.Phase, Is.EqualTo(stateAfterTwo.Phase));
    }

    [Test]
    public void StepBackAllTheWayToStart()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n:print");
        while (ev.CanStep)
        {
            ev.Step();
        }
        
        while (ev.CanStepBack)
        {
            ev.StepBack();
        }
        
        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        Assert.That(ev.CanStepBack, Is.False);
    }

    // ── Undo stack count ─────────────────────────────────────────────────────

    [Test]
    public void UndoStackGrowsWithEachStep()
    {
        var ev = BuildEvaluator(">\"a\"\n|to-upper\n|to-lower\n:print");
        int steps = 0;
        while (ev.CanStep)
        {
            ev.Step();
            steps++;
            //CanStepBack is true and stack grew
            Assert.That(ev.CanStepBack, Is.True);
        }
        Assert.That(steps, Is.GreaterThan(0));
    }
    
    [Test]
    public void StepBackContextIsRestored()
    {
        var ev = BuildEvaluator(">\"Hello\"\n|to-upper\n:print");
        ev.Step(); // push input
        var timelineAfterStep1 = ev.Context!.TimelineLength;
        ev.Step(); // to-upper
        Assert.That(ev.Context!.TimelineLength, Is.GreaterThanOrEqualTo(timelineAfterStep1));
        ev.StepBack();
        Assert.That(ev.Context!.TimelineLength, Is.EqualTo(timelineAfterStep1));
    }

    [Test]
    public void BranchStepBackOutOfRestoresContext()
    {
        var ev = BuildEvaluator("""
            >"Hello"
            .
            |to-upper
            :print
            ^
            """);

        // Step until we're inside the branch body
        ev.Step(); ev.Step(); ev.Step();
        var depthInside = ev.Context!.TimelineLength;

        // Step back out of the branch body steps
        ev.StepBack();
        Assert.That(ev.Context!.TimelineLength, Is.LessThanOrEqualTo(depthInside));
    }
    
    [Test]
    public void PatternMatch_StepThrough_ProducesCorrectOutput()
    {
        var ev = BuildEvaluator("""
            >range 1 5
            ?
            + ~is-even
              |mul 10
            + ~~
              |add 1
            ^
            :print
            """);
        StepToEnd(ev);
        var lines = _consoleOut.ToString().Trim().Split(Environment.NewLine);
        // 1->2, 2->20, 3->4, 4->40
        Assert.That(lines, Is.EqualTo(new[] { "2", "20", "4", "40" }));
    }

    [Test]
    public void PatternMatchStepBackRestoresBeforeMatch()
    {
        var ev = BuildEvaluator("""
            >range 1 3
            ?
            + ~is-even
              |mul 10
            + ~~
              |add 1
            ^
            :print
            """);

        for (int x = 2; x < 7; x++)
        {
            // Step a few times into the pattern match
            for (int i = 0; i < x; i++)
            {
                ev.Step();
            }
            var timelineDepth = ev.Context!.TimelineLength;

            // Step back the same number of times
            for (int i = 0; i < x; i++)
            {
                ev.StepBack();
            }
            Assert.That(ev.Context!.TimelineLength, Is.LessThanOrEqualTo(timelineDepth));
            Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        }
        //todo: environment.newline, and are we sure it's this many times hitting print? 
        var lines = _consoleOut.ToString().Trim().Split(Environment.NewLine);
        // 1->2, 2->20, 3->4, 4->40
        Assert.That(lines, Is.EqualTo(new[] { "2", "20", "2", "20" }));
    }

    // ── RunCurrentToEnd ──────────────────────────────────────────────────────

    [Test]
    public void RunCurrentToEndCompletesEvaluation()
    {
        var ev = BuildEvaluator(">\"world\"\n|to-upper\n:print");
        ev.RunCurrentToEnd();
        Assert.That(ev.Current.IsDone, Is.True);
        Assert.That(_consoleOut.ToString().Trim(), Is.EqualTo("WORLD"));
    }

    // ── SetRoot resets state ─────────────────────────────────────────────────

    [Test]
    public void SetRootWithNewNodeResetsEvaluator()
    {
        var ev = BuildEvaluator(">\"Hello\"\n:print");
        ev.Step();
        Assert.That(ev.Current.IsStarted, Is.True);

        //build a second compiled node and set it
        var p2 = new Parser();
        p2.Parse(">\"World\"\n:print");
        var compiled2 = new Compiler(OpCatalog.GetDefaultOpCatalog()).StartCompile(p2.Program);
        ev.SetRoot(compiled2);//this should reset... everything.

        Assert.That(ev.Current.Phase, Is.EqualTo(EvalPhase.NotStarted));
        Assert.That(ev.CanStep, Is.True);
        Assert.That(ev.CanStepBack, Is.False);
    }
    
    [Test]
    public void MultiValueStreamStepThroughAllValuesProcessed()
    {
        var ev = BuildEvaluator(">\"a\" \"b\" \"c\"\n|to-upper\n:print");
        StepToEnd(ev);
        var lines = _consoleOut.ToString().Trim().Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "A", "B", "C" }));
    }

    [Test]
    public void MultiValueStreamStepBackToStartCanReplay()
    {
        var ev = BuildEvaluator(">\"x\" \"y\"\n|to-upper\n:print");
        StepToEnd(ev);
        while (ev.CanStepBack)
        {
            ev.StepBack();
        }

        //re-run from the beginning
        StepToEnd(ev);
        var lines = _consoleOut.ToString().Trim().Split(Environment.NewLine);
        Assert.That(lines, Does.Contain("X"));
        Assert.That(lines, Does.Contain("Y"));
    }
}
