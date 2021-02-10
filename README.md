# Iterated Prisoner's Dilemma

## The game

[Prisoner's Dilemma](https://en.wikipedia.org/wiki/Prisoner%27s_dilemma) is a simple game
with two players. In each round, the players can choose either to _collaborate_ or to _betray_.
Depending on the choices, they get some points:

 - If both _betray_, they both get 1 point
 - If both _collaborate_, they both get 2 points
 - If player A _betrays_ but B _collaborates_, then A gets 3 points, but B gets 0 points

In an interated version of the game, this is repeated a number of times and the total score
is calculated based on, say, 100 iterations. In this case, players can follow various strategies
that can also adapt based on what the other player does. Simple ones that ignore the other
player are:

 - **Always collaborate** - we always collaborate, regardless of what the other player does
 - **Choose random** - choose randomly between collaboration and betrayal.

A somewhat more sophisticated strategy that responds to the other player:

 - **Copy the opponent** or **Tit-for-tat** - do whatever the opponent did last time.

## Implementing strategies

We will be implementing strategies in basic Python3.

Each strategy is a python script that runs in an infinite loop, reading from the standard input and writing
to standard output.
In each iteration, the standard input contains the decision the opponent made in the previous round:

- `C` for **Collaborate**,
- `B` for **Betray**,
- `0` for initial input in the first iteration.

Your script must then answer with either `C` to Collaborate or `B` to Betray, writing to the standard output.

There are examples in the [/strategies](https://github.com/evelinag/iterated-prisoners-dilemma/tree/main/strategies) folder.

### Example strategy

Let's look at the [tit-for-tat.py file](https://github.com/evelinag/iterated-prisoners-dilemma/blob/main/strategies/tit-for-tat.py) in more detail:

```
#!/usr/bin/env python
import sys

# Tit-for-tat with initial Betray

while True:
  inputs = sys.stdin.readline()
  previous = inputs[0]
  if previous == "C":
      print("C\n")
  else:
      print("B\n")
  sys.stdout.flush()
```
Notice the infinite loop that reads a line from the input, analyses it and writes out the decision. This particular tit-for-tat strategy decides to Betray in the beginning (when the input is `0`) and then copy the opponent's decisions.

You can test the strategy locally by running:

```
python3 tit-for-tat.py
```

The app will be waiting for your input and responding with its decision.

Any strategy you implement should follow the same structure.

## Submitting the strategies

All strategies will be run against each other in a competition.

To submit a strategy, you will get a URL for a web application running the competition.
You will be asked to fill in the name of your source file and the contents of your script, implementing a strategy. Please try to use unique file names, for example by including your team name in the file name.

*Warning*

This is not a robust game, it is a home-made hand crafted challenge for the REG team to have fun with. Please don't do weird things to crash my server!

The web app is saving scripts inside a Docker container, so please keep your submissions saved on your computers in case we need to restart the app.

Don't mine bitcoins inside the strategies, any strategy taking too long to decide will be automatically killed.

## Notes

This project uses and re-uses parts of:

- [SAFE Template](https://safe-stack.github.io/docs/)
- [Prisoner's Dilemma Dojo](https://github.com/tpetricek/prisoners-dilemma)
- [Wrattler AI Assistants](https://github.com/wrattler/wrattler/tree/master/aiassistants)