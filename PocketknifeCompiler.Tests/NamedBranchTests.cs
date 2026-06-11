using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class NamedBranchTests
{
	[TestCase("""
	          >"hello"
	          .@x
	            |to-upper
	          <
	          |append @x
	          :print
	          """, "HELLOHELLO")]
	// Read `@x` from a sibling scope. After the named branch closes, the binding is still visible via the chain.
	[TestCase("""
	          >"hello"
	          .@x
	            |to-upper
	          <
	          .
	            |append @x
	            :print
	          ^
	          """, "HELLOHELLO")]
	public void NamedBranchBindAndRead(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}

	// `@^name` reaches one binding-layer outward, skipping the inner shadow — used when an inner named-branch shadows the same name.
	// Option A (layer-step) semantics: `reachOut = N` steps N progenitor links outward, then resolves from there.
	[TestCase("""
	          >"hello"
	          .@x
	            |to-upper
	          <
	          .@y
	            .@x
	              |to-lower
	            <
	            |append @^x
	            :print
	          <
	          """, "helloHELLO")]
	public void NamedBranchReachOut(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}

	// Plain `.` then `^` (SideEffect) discards body changes — the named binding from inside a SideEffect-closed branch is NOT propagated
	[TestCase("""
	          >"hello"
	          .@x
	            |to-upper
	          ^
	          
	          :print
	          """, "hello")]
	[TestCase("""
	          >"hello"
	          .@x
	            |to-upper
	          ^
	          |append @x
	          :print
	          """, "helloHELLO")]
	public void NamedBranchSideEffectDoesntDiscardValue(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}

	// Named branch results bind across iterations: each outer item gets its own merged value.
	[TestCase("""
	          >"hi" "bye"
	          .@x
	            |to-upper
	          <
	          |append @x
	          :print
	          """, "HIHI", "BYEBYE")]
	public void NamedBranchPerItemBinding(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
	
}
