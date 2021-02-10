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

We will be implementing strategies in Python.
