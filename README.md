# Pocketknife

Pocketknife is a programming languages for solving problems.
It's a tool that transforms inputs into outputs. Manipulate files, process or clean data, run commands, and more. 

It has a unique, simple, and enjoyable syntax that is easy to learn and intuit about.

## Basics
To read the code, just scan down the first column. Every line starts with a symbol that will tell you exactly how the data is being transformed. Knowing this, one can intuit the flow of data without intensely reading the code. Pocketknife is a simply enough language that it's *hard* to write programs that are challenging to intuit at. (There aren't a lot of clever shorthands.)

Here's an example of some pocketknife code

```
>range 0 10
|mul 3
~is-odd
^
```

- **>** generates data. 'range' provides a list of numbers, but all sorts exist, like loading files or csv's.
- **|** transforms data. Called 'pipeline's. Here, |mul is multiply. Pipelines might transform data a new type, like |to-string
- **~** filters data. Only commands that match will continue processing.
- **^** closes a branch, which a generator can create.

## Branches
Here is an example with branches. 
I like to imagine rotating around the . symbol. The data is cloned, and anything inside the branch (. to the ^) will not affect anything else.

```
>"Downloads" "Desktop"
|>dir-files

.
~ext "jpg"
|resize-image (max-width=1920px)
:copy-to "data/memes/"
^

.
~ext "pdf"
:copy-to "data/papers"
^

```

: is for side-effects. It takes the input, but doesn't change it or use the output. I like to imagine a u shape stamping the page, like the data is going around this one. :print is the most common one.



## Variables

```
>init-table //puts an empty table on the context.
>dir-files "input" //argument version of dir-files. ">" means it doesn't take input, just produces. "|>" generates results from what comes in.

~ext "md"

.@wc
|load-text
|regex ([^\W_]+[^\s-]*)* //match all words, returns every match.
<> //"pack" turns all items in to a single list item.
|count 
^

.@fn
|filename (extension=false)
^

.@path
|filepath (type=absolute)
^

>record @fn @path @wc
< //replace the 'dir-files' scope with the record (table-row) we created
& //append to the higher scope, that's from init-table. Appending a record (our dictionary type) to a table will add it as a row.
:save-csv "input/wordcounts.csv"
```

## Pattern Matching
```
>range -50 50
?
+ ~positive
  |to-string
  |prepend "positive: "
+ ~is 0
  |to-string
  |prepend "zero: "
+ ~~
  |to-string
  |prepend "negative: "
^
:print
```
**?** starts a pattern match. **+** starts each branch, which should be followed by a filter.
If the filter matches, that branch runs.
**~~** is the 'catch' or final 'else' branch.

Each branch will operate in order, top-to-bottom; but the items will keep their original order.

---

## Design Decisions

### Simplicity
Things don't get much more complicated than this, and that's by design. Programs accomplish one thing and then move on. Complexities of implementation, algorithm choice, and so on, are handled by the libraries that provide the tools and types. 

It's built on C#, and types can be any valid c# - or dotnet, with some fiddling - type.

### Iteration
One curiosity of the language is that it doesn't handle loops or iteration like most programming languages. It works on all of the data, one line at a time. The code runs top to bottom, never jumping around or looping, thanks to a clever interpreter. The purpose of this is to allow step-by-step debugging and interactive execution (a work-in-progress). The language should feel like experimenting in a playground, and once it does what you want, you might be done and ready to throw the code away. Fine!

In the future, I hope it will also be undo-able!

### Tool, not Programs
Another goal of the language is to be a tool for the user. There is one environment for plugins/packages per user-installation, one folder to drop dll's. No package.json, no plugins, no virtual environments - if you need those tools, they are there. Because of this, this pocketknife is closer to bash (or Powershell or zsh or fish or ysh) than python.

#### So why not use those terminal tools?
Go for it! They're great! Pocketknife's advantage is the ease of reasoning about a program by reading it. 
