# CS464 Assignment 02 - Saleha - BSSE23081

- **Video:** https://drive.google.com/file/d/1t1iIliN8qK9lw6iLWjLmqfOSt_F0NmxT/view?usp=sharing
- **Input:** old Input system, with Active Input Handling set to Both. I read all keys in Update.
- **Ground check:** CheckSphere at a Feet object under the player, radius 0.2, only the Ground layer in the mask. It draws a gizmo, green on the ground and red in the air.
- **Camera:** smoothing with 1 - e^(-k*dt), k = 4, and a look-ahead of 0.2 s. I smooth the point the camera looks at, and the camera follows that point.
- **Juice:** squash when landing, using Ease.OutBack. It only plays after a real landing (more than 0.1 s in the air).

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | Feels quick but I can still stop on the small platforms. Also gives about 5 m of jump distance, and my biggest gap is only 3 m. |
| Jump height | 1.8 m | The ledges are 1.2 m high so this clears them easily. Real peak comes out around 1.74 m because of the 0.02 s physics steps. |
| Time to apex | 0.61 s | Comes from the jump height: 5.94 / 9.81. Short enough that the jump feels snappy. |
| Fall multiplier | 2.2 | Fall takes 0.41 s instead of 0.61 s. Same peak, but the player doesn't float on the way down. |
| Coyote time | 0.1 s | At 5 m/s that is 0.5 m past the edge, enough for the run-off jumps in Level03. |
| Jump buffer | 0.12 s | About 7 frames at 60 fps, which is 0.6 m before landing at 5 m/s. An early press still counts. |
| Camera smoothing | k = 4 | Time constant is 1/4 = 0.25 s. At full speed the camera point trails by 5/4 = 1.25 m. Higher felt jumpy and made me feel sick, lower felt too slow. |
| Look-ahead | 0.2 s | At 5 m/s it aims 1 m ahead of the player, which almost cancels the 1.25 m lag so the player stays near the middle. 0.35 s swung the view too much. |

## Four answers

1. **The jump, in numbers.** Jump height is 1.8 m and g is 9.81, so the launch speed is sqrt(2 x 9.81 x 1.8) = about 5.94 m/s. The mass is 1 kg so jumpForce is also about 5.94 (as an Impulse). Time to the top is 5.94 / 9.81 = 0.61 s. With fall multiplier 2.2 the fall takes 0.61 / sqrt(2.2) = 0.41 s, so the whole jump is about 1.01 s. Running at 5 m/s that is about 5 m. My widest gap is 3 m, so yes it clears it, with about 2 m extra.

2. **Update and FixedUpdate.** In Update I read the keys, work out the move direction from the camera, check the ground, do the coyote and buffer timers, queue the jump and start the squash. In FixedUpdate I set the Rigidbody velocity (keeping y), apply the queued jump once, and add the extra fall gravity. If I swapped them, GetKeyDown is only true for one frame, and at 144 fps about two frames out of three have no FixedUpdate, so jump presses would get lost. And putting the physics stuff in Update would make the movement depend on the frame rate.

3. **Vector3.Dot.** My project doesn't use it yet, but I could use `Vector3.Dot(move, forward)` where forward is the camera's flat forward. If it is positive the player is walking away from the camera, if it is negative they are walking toward it, and zero means sideways. Another place is the ground check with a contact normal: `Vector3.Dot(normal, Vector3.up) > 0.5f`. Positive and above 0.5 means it is a floor and not a wall.

4. **The juice.** It is a squash when landing, using Ease.OutBack. The body goes down to y = 0.7 and comes back to 1, going a little past 1 before it settles. x and z get bigger to 1/sqrt(0.7) = about 1.2 so the volume stays the same. It lasts 0.25 s, which is 15 frames at 60 fps. That is long enough to notice, but short enough to finish before the next jump. I used LerpUnclamped so the overshoot doesn't get cut off, it only starts after more than 0.1 s in the air so small bumps don't trigger it, and at the end the scale is set to exactly (1, 1, 1).

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | Walk, then jump a 2 m gap and a 3 m gap | Nothing, the jump goes about 5 m so both are easy. |
| Level02 | Climb two 1.2 m ledges | Nothing, 1.2 m is less than the 1.74 m peak. |
| Level03 | Run off narrow platforms and jump two 3 m gaps, using coyote time | Nothing, 3 m is in reach and a press up to 0.1 s after the edge still jumps. |
| Level04 | Jump 2.5 to 3 m gaps that also go up or down 1.2 m | Nothing, a 1.2 m rise over a 3 m gap still fits in the jump. |
| Level05 | Follow an L-shaped route with turns, to test W/A/S/D relative to the camera | Nothing, the gaps are 2.5 to 3 m and one has a 1.2 m rise. |
