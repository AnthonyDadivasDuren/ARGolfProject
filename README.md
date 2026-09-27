# AR Golf

A small Android augmented reality mini-golf game built with Unity and AR Foundation. Scan a real-world table or floor, tap to place the course, and drag back from the ball to aim and shoot. Move your phone around the course to line up your next putt.

The project was created for an AR prototype assignment combining real-world tracking, digital objects, interaction, and gameplay. The controls are inspired by Angry Birds' pull-and-release aiming and the obstacle-based mini-golf of Golf With Your Friends.

Built with Unity 6 (6000.6), AR Foundation (ARCore and ARKit), the Input System and URP.



https://github.com/user-attachments/assets/866062f1-2a7d-473a-89f0-4fd4b0258429





## How to Play

1. **Scan the surface.** Move your phone slowly over a flat floor or table until surfaces are detected.
2. **Place the course.** Tap a detected surface. The course appears there, facing the same way as your camera.
3. **Aim.** Touch the golf ball and drag away from where you want it to go. A line shows the direction and power of the shot.
4. **Shoot.** Let go to hit the ball. The further you drag, the harder the shot. You can only shoot when the ball has stopped.
5. **Sink it.** The hole is complete when the ball settles in the cup. Your stroke count is shown at the top of the screen.

If the ball falls off the course, it returns to where you last hit it from.

## Levels

After finishing a hole, choose **Next Level** to move on to the next course (Currently 3 existing levels), or **Restart** to replay the current one. After the last level, the game shows **Course complete!**

How it works
AR placement
ARGolfPlacement waits until the AR session is tracking. On touch release, it raycasts against detected planes and accepts upward-facing horizontal surfaces. It positions and rotates the course, enables the gameplay UI, and hides the detected-plane objects. The course can be placed once per scene session.

Aiming and shooting
GolfBallController projects screen input onto a horizontal plane through the ball. The shot direction is opposite the drag. Drag distance is capped and mapped through a squared power curve, making short putts easier to control while retaining full shot strength. A Line Renderer displays the shot preview.

The controller also counts strokes, prevents further shots after completion, resets the ball, and restores its previous shot position if it falls below the course.

Physics and hole detection
The ball uses a Sphere Collider and Rigidbody. GolfRollingResistance slows it while it contacts supporting surfaces on the Grass layer. Physics materials control turf friction and wall bounce. Continuous turf collision geometry avoids the bumps that separate tile colliders can create at their joins.

GolfHole uses a trigger to check that the ball is below the rim and moving slowly for a short settling period before marking the hole complete.

Levels and interface
GolfLevelManager activates the selected level, resets its hole detector, and moves the shared ball to that level's starting point. GolfUIController displays strokes and results and connects the restart and next-level buttons to the level manager.
