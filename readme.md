Faulty Gods
===

This is a little prototype I made for my first time doing Ludum Dare. It was Ludum Dare 32 "Unconventional Weapon". I only spent 3 days on this, so there are still some bugs I didn't fix. It was inspired by Geometry Wars.

Here is a video of gameplay: https://youtu.be/v8f3zgB7fkE

GroundManager.cs makes use of a polygon cutting algorithm.
EnemyManager.cs also uses this polygon cutting algorithm.
At the time I rushed it, but I now realize that there should have been an abtraction for anything that is a cuttable polygon.

Moving around the player's green polygon feels quite satisfying. 
The implementation is in PlayerController.cs.

![Faulty Gods](http://i.imgur.com/ZbpLSet.png)