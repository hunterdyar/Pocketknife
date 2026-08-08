# The Runtime

The goal of Pocketknife is to behave like it looks. The execution is top to bottom, line-by-line. It achieves this through a novel runtime. 

## The Basics
Pocketknife uses a tree-walk interpreter that operates on what we call the context. The context is basically a list of the items that are being operated on.

## Pipelines
Pocketknife is a pipeline oriented language. This means that the language describes a flow charts for data. Each step modifies that data (the context), then passes it along to the next step.

## Line-By-Line Evaluation
The pipeline works on sets of data. A common use is to do something with a directory of files, for example, and you would describe the step you want.

A normal language does this with a loop. While loops, for loops, so on. Then the action inside the loop. "On these, do this, then do that". Pocketknife flips that around and operates on the entire set of data one step at a time. "do this on these, do that on these". This is also called [Breadth-First](https://en.wikipedia.org/wiki/Breadth-first_search) evaluation.

There are a few reasons for this:

1. Pocketknife works like it looks like it works like.
2. Code execution doesn't jump around, so the user's mental model of the
   program going "top to bottom, line by line" stays true.
3. The reversability of the language is much easier to reason about.

### Pattern Matching
Even pattern matches, Pocketknifes version of switch or if/else statements, are evaluated line-by-line. The runtime marks elements in the loop as inactive during the match (e.g. ~test a), and then those are skipped when running the following the operators (|op a). So you can step forward and backward, line-by-line, even along all kinds of branches.

```
?
+ ~test a
  |op a
+ ~test b
  |op b
+ ~~
  |else
^
```

## Default Closers
Because of the line-by-line syntax, you don't need to close (^) open branches at the end of the file. It isn't ambiguous in what order they branches close in; so we can just close any open scopes when parsing. 

## Reversibility
Pocketknife is [Reversible](https://en.wikipedia.org/wiki/Reversible_computing). It can return to a prior state. The default behavior is to do this on a line-by-line level of granularity.

If the user hits an error or something unexpected, then can step back and observe how this happened.

There are exceptions. Side effect operators (:) are not guaranteed to be reversible. `:print`, for example, won't delete lines from the console; it will only print them out again each time it's run.

The reversibility is implemented with a history stack, that keeps copies of the state.

## Efficiency
If "saving copies of everything as you run" and "iterating over and skipping items constantly sounds inefficient to you... you're right. Pocketknife is not an efficient runtime, all things considered. My goal is it to be good enough to get out of the way.

Luckily, computers are pretty fast, and Pocketknife scripts are generally quite small. It's fine! Or it isn't. It's an experimental language.