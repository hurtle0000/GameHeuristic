## 🚀 Initial Release v0.0.1

This is the first release of a Connect 4 game framework for students to compete to produce the best heuristic to evaluate game moves

### ✨ Core Features
* **IHeuristic interface** This is the simple interface class that students have to use. Their job is to write an implementation of the Evaluate method that evaluates potential moves effectively.
* **Heuristic loader** This loads all of the student-provided implementations into the framework so they can compete against each other.
* **Tournament mode** runs a tournament from the command line where all heuristics play against each other to find a winner
* **UI mode** runs a UI where a single game can be played between twop heuristics or a tournament can be run as a round robin or knockout

### 📋 Technical notes and requirements
* .NET 8.0 SDK or higher
* Avalonia (https://avaloniaui.net/)
* Windows 11/Linux/Mac