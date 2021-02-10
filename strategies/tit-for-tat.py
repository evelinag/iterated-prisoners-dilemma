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
