This language started as a research proof-of-concept. I was exploring novel syntax design while considering what meaningful domains existed to design in.

## The Problem Space
Simultaneously, I was researching AI. One of my research tasks was, whenever I heard about what someone used AI for, I added it to a big list. A theme appeared: People were doing things that their operating system should already be good at. Basically, AI was a chance for people to *literally just ask* a computer to do something for them, that they felt a computer should do.

Renaming files was a common was, as was all sorts of batch operations. Wrangling documents, resizing images, things like that. People told me how much they liked AI, but all I heard was "My computer failed me". It also made me consider why people weren't using the existing tools.

- Without confidence, they could understand undo a bash command, experimentation and play feels unwelcome.
- "Learn some Python" is something someone has to feel invested in, a journey they feel they will get value from. Users who specificaly want a "quick and dirty" solve don't feel like they should be learning, or should have to.
- The terminal, or programming languages, are for precise and informed communication; and doing something 'hacky' feels like one is doing it wrong. (Despite the assurances from us programmers that we're always - always - doing something 'hacky')
- Features built into the OS are often hidden away. PowerToys is not included by default, for example.

## Design Pillars
From this research, came my design pillars.

### Examinable and Explorable
The runtime environment should let you feel like you can make mistakes. Errors shouldn't yell at you. You should be able to see what it's doing, and follow each step. The mental model required for understanding a program can be lower, if a lot of the reasoning is 'off loaded' into a medium, like a visualization of the execution path.

This should all just be "free", baked into the ecosystem.

### Cozy
The environment should be welcoming and friendly. Help documentation should not rely on third-party ecosystems (StackOverflow, YouTube, etc). Help documentation should exist *within* the coding environment. You should feel like the tool is there to help you do your task (as opposed to, say, write text to a file).

### Intuitive
The language should be, if not obvious, clear. It should look like what it does.

### Baggage
Existing language and language design principles are *not* reasonable defaults. In most cases, designing a language-feature that is familiar is a positive! I specifically threw this perfectly sound principle away in order to allow other ideas and shapes to float up. 

I considered these, among other points, and made a wishlist for the same domain as shell languages or simple scripting languages:

### Feature Wishlist
- Intuitable syntax
- Reversible runtime 
- High quality error messages.
- 'Batteries Included' core library.
- OS User-scoped environment. One install, one set of libraries.
- Debug-Always. Stack traces, step-by-step execution, and so on as a first-class feature.

From there, I took some inspirations from the enjoyable syntax of a [previous project](https://pinch.hdyar.com), and got to designing.

## Successes
My favorite thing about Pocketknife design is the left-hand column. Scanning down the characters top-to-bottom tells you the shape of the program.

As for the runtime, control flow being top-to-bottom always is really nice. It breaks my programmer brain, not having jumps in the traditional sense; but once I let go of those expectations, it just kind of works. The complicated (and inefficient) backend handles it for you. This wasn't originally a goal, it's something that fell out of reversability and debuggability. 

---
# What's Next?
The software environment. The language is at a good stopping point to pivot over to 'cozy' software design.

Once it's all minimum-viable tool, it will be time to get the reversible elements fully working (I'm close!) and make the errors 'good' (I'm not close!). There's a lot to do!