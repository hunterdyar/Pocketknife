# Operators
## Pipeline
> |

The Pipline symbol takes an input (what's above it), and transforms it in some way, then produces an output (what's below it). 

For example, `|mul 2` will take a number, multiply it by two, and then that will be its output.

Pipeline may or may not change the type of an object.

## Filter
> ~

The filter operator will select which items continue to be executed on. If they match, they will continue to be used. If not, they will be removed from execution. 

Every type of filter is either 'true' or 'false' when evaluated on the data in the pipeline. 

`~gt 10` will only allow numbers larger than 10 through, for example.

It's possible to combine filters using special operators and grouping syntax.

## Asserts
> !

The bang (!) symbol is like a filter, except if something isn't matched, then the program instantly stops. Helpful for proceeding with confidence.

Any filter operator can also be used as an assert. Instead of allowing the data that's true (any that match), it tests the filter and says "all must match, or else something has gone wrong."

`!pos` will halt the program if an incoming number is negative, and `!past` will halt if an incoming date-time is in the future of the current environment's date.

## Side-Effect
> :

The colon is a side effect. It does not modify the data in the pipeline, but it does do... something. Side effects are not guaranteed to be reversible, and are not by default.

`:print` spits the current data out to the console. `:echo` ignores the data entirely and prints whatever it's arguments are out.

`:save` is the next most common side-effect. It modifies contents on the hard disk, potentially outside of the running of the program. By default, it will overwrite existing data.

## Generators
> \>

The > symbol is for the production of information. It creates the contexts that are operated on.

You can put any constant after a generator, to create a single item context. `>"hello, world"` will put the string 'hello, world' onto the context, for example.

`>range`, `>load`, `>dir` are all common generators.

## Pipe In
> |>
> 
> ^

Similar to the generator is the pipe-in operator. It takes whatever is on the context and turns it into a new context. `|>lines` will take some string, perhaps one we just loaded from a file, and split it into a string for every line, to operate on. `split` is another common one for string operations. 

## End Scopes
> ^
> 
> &
> 
> <

There are three ways to end the scope, usually opened by a generator or pipe-in.

### End

The `^` symbol is like a closing brace. It ends the scope that the generator is using. Many programs start with a >
and end with an ^, although you don't need to include it if it's the end of the file.

```
>generator
|    # some number of operators doing things
|
^    # and then we end the pipeline. 
```

### Append

The `&` symbol does the same thing the end symbol does, then it adds the current data to the outer scopes list, if it exists.

```
>gen
|
&    
```

Useful for when you are filtering out invalid data, sanitizing it, then sticking it back in the original list.

### Replace

The `<` symbol does the same thing the generator does, then it replaces whatever the outer scope was. Useful, when combined with pipe-ins, for pulling out a certain element from some list.

```
>gen
|
<
```

