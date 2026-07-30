# Syntax Overview

Pocketknife operates top to bottom, one line at a time.
Each line starts with a symbol, which tells you what that line of will do.

Pocketknife is a pipeline oriented language. You can think of it as a list of data, which we call the **context**, that we perform operations on one at a time, which transforms the data.

I might describe a task I have as "extract all the zip files in a directory and copy the files to the same folder". 

As a pipeline, you might describe it as a sequence of steps, where the 'noun' of each step flows into the next: "take a directory. On it, loop through all files. Select just the zip files from these. Extract these folders. Loop through all the files that were just extracted, copy them." This phrasing ("on it", "from these", "that were") is a grammatical way to indicate the context moving through the steps.

In pocketknife, that might look like this:

```
>dir "source/"
|>files
~ext zip
:extract-to "source/"
^ //end the files scope, back to dir scope

|>files //iterates again, but now has new files after extract-to ran
~not [~ext zip]
:copy-to "output"
^
```

This is a somewhat convoluted example, so we can see a number of interesting symbols. Just look down the first column of characters:

```
>
|>
~
:
^

|>
~
:
^

```
This gives us the shape of the program. It loads data, splits whatever was loaded into more data, filters some of it out, does *something else*. Then it goes back to the first loaded dta, splits it into more data again, filters again, and does *something else* again. Then it ends.

Everything is clear except for the ':' symbol. The colon is called a side-effect, and it doesn't do anything to the transformed data. But it *does do something*. Usually for modifying the world outside the program, like saving or copying files. 

---
