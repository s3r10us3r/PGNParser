# PGN PARSER

This is a console app thet lets you convert a [PGN](https://en.wikipedia.org/wiki/Portable_Game_Notation) file to a [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) engine friendly move notation.
The app reads all of the games in a given pgn file, or all of the pgn files in a given folder and writes them to a single txt file as UCI strings, each game in one line and each move separeted by a space.
It skips all of the information about the game in the **[]** square braces but it does not support PGN files with comments inside the game notation.
You can use it to create an opening book for your chess engine (that is its main purpose).

## Usage
`\PGNParser <input file/folder path> <output file path> [minimal game length] [number of moves saved]`
- The program will skip every game that has less turns that the specified minimal game length, a turn is when both players make a move, default value is 30.
- The program will write atmost the number of moves specified as the number of moves saved from each game, default value is 30 so 15 turns.

## Output
- After succesful run the app will write out how many games it has saved and how many errors it has ran into.

## Limitations
- It does not support algebraic chess notation with comments.
- It might run into an error where ambiguity has to be resolved by checking which move is legal e.g. pins and checks. This scenario is rare enough (in my case it happened in around 0.1% of the games parsed) that I decided to leave it as that (but I might come back to it and fix it).
