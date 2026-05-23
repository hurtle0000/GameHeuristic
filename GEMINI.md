# Connect 4 Heuristic Framework

Welcome! This framework is designed for you to practice implementing heuristic evaluation functions for the game of Connect 4.

## How to Implement Your Heuristic

1.  Create a new `.cs` file in the `src/GameHeuristic.Core/Submissions/` folder.
2.  Define a class that implements the `IHeuristic` interface.
3.  Implement the `Name` property (your name or your bot's name).
4.  Implement the `Evaluate` method.

### The `Evaluate` Method

```csharp
public double Evaluate(Player[,] board, Player player)
```

-   `board`: A 6x7 2D array representing the current state of the game.
    -   `board[0, 0]` is the top-left corner.
    -   `board[5, 6]` is the bottom-right corner.
-   `player`: The player you are evaluating the position for (Red or Yellow).
-   **Return Value**: A `double` representing how "good" the position is for the given player. Higher values are better.

### Example

```csharp
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions;

public class MyAwesomeHeuristic : IHeuristic
{
    public string Name => "Student Name - AwesomeBot";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        // Your logic here! 
        // Example: count how many pieces you have in the center column.
        return score;
    }
}
```

## Running the Framework

### Visual Match
To watch two heuristics play against each other:
```bash
dotnet run --project src/GameHeuristic.UI
```

### Headless Tournament
To run a round-robin tournament between all implementations:
```bash
dotnet run --project src/GameHeuristic.Tournament
```

## Core Rules
- Your code must be self-contained in its `.cs` file.
- Do not modify the files in `GameHeuristic.Core` outside of the `Submissions` folder.
- The framework uses Minimax with a depth of 6 for visual matches and 4 for the tournament.
- Terminal states (wins/losses) are handled by the framework; your heuristic is only called for non-terminal positions.
