# AR Golf

A mini golf game in augmented reality. Scan a floor or table with your phone, place a small golf course on it, and putt the ball into the hole in as few strokes as you can.

Built with Unity 6 (6000.6), AR Foundation (ARCore and ARKit), the Input System and URP.

## How to Play

1. **Scan the surface.** Move your phone slowly over a flat floor or table until surfaces are detected.
2. **Place the course.** Tap a detected surface. The course appears there, facing the same way as your camera.
3. **Aim.** Touch the golf ball and drag away from where you want it to go. A line shows the direction and power of the shot.
4. **Shoot.** Let go to hit the ball. The further you drag, the harder the shot. You can only shoot when the ball has stopped.
5. **Sink it.** The hole is complete when the ball settles in the cup. Your stroke count is shown at the top of the screen.

If the ball falls off the course, it returns to where you last hit it from.

## Levels

After finishing a hole, choose **Next Level** to move on to the next course (Currently 3 existing levels), or **Restart** to replay the current one. After the last level, the game shows **Course complete!**
