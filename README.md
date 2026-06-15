# Pocketknife

Pocketknife is a programming languages for solving problems.
It's a tool that transforms inputs into outputs. Manipulate files, process or clean data, run commands, and more. 

It has a unique, simple, and enjoyable syntax that is easy to learn and intuit about.

## Basics
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
