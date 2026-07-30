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

I considered these, among other points, and made a wishlist for the same domain as shell languages or simple scripting languages:

##3 Feature Wishlist
- Intuitable syntax
- Reversible runtime 
- High quality error messages.
- 'Batteries Included' core library.
- OS User-scoped environment. One install, one set of libraries.
- Debug-Always. Stack traces, step-by-step execution, and so on as a first-class feature.

## Constraints
- Cross-platform (I use win, mac, and linux; so this was always a constraint! Thanks, dotnet)
- All-In-One software is fine. It doesn't have to be a 'normal' programming languge (but it is!), it can be like processing or R studio.
- Reasonably Performant (I'm not heavily optimizing, but I am non-pessimizing)

From there, I took some inspirations from the syntax of a [previous project](https://pinch.hdyar.com), and hacked away at it. I showed it to friends without context, and asked them to guess what it did. While not obvious, once explaining the "pipeline" approach, things clicked into place for them and they managed to guess most or all of the features. 

As for the 'cozy' and 'debugger-first', reversible runtime, as of Summer 2026, this is still a work-in-progress.

---
# What's Next?
This research prototype has effectivley worked for my original design goals. And that's the problem! Now that I can *almost* daily drive it, I want to continue working on the rest. Get the software together, get the reversible elements fully working (I'm close!), make errors good (I'm not close!). There's a lot to do!