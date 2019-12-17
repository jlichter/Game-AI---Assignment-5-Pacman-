# Game-AI---Assignment-5-Pacman-
implementation for the decision making algorithms for the ghosts in Pacman.

Team Members: Alberto Mejia & Jessica Lichter

Implementations:

	-Fixed an audio glitch in framework where after grabbing a power pellet, the music would play forever.
    -Added the ability to move on to a next level. Previously, the player did not get a new screenful of dots once all of them had been consumed
    -The score now carries over between "levels" AKA each time a screenful of dots is created
    -Scoring is displayed horizontally and updated correctly on screen

    - Each Ghost has different personality/behavior according to the real game.
    - Blinky chases PacMan as expected, a "pursuer". 
    - Pinky is an "ambusher" as in he tries to cut PacMan off by estimating PacMans location two tiles ahead. 
    - Clyde has "stupid" behavior. As in his pattern appears to be deliberately random, while also following PacMan slightly. 
    - Inky is the most interesting of them all. His behavior is truly random. Inky will randomly switch between all three behaviors (Blinky, Pinky, & Clyde).
    - Ghosts return to gate after being eaten.
    - Ghosts flee from PacMan after a power pellet is eaten. 

Bugs:

	- Ghosts don't respawn after being eaten.
	- Each Ghost you eat, if you die, when you respawn those ghosts can't touch you for some reason. 