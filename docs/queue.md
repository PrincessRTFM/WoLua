# Action queueing
_Wait for it... wait for it... NOW!_

## Overview
In vanilla, game macros are able to use `<wait.SECONDS>` to pause for up to sixty seconds after a macro line executes. This allows, for instance, an emote animation to reach a certain point before a message is sent, or for two people synchronising appropriately written macros to create a scripted scene of emotes and messages. Unfortunately, for technical reasons, WoLua scripts cannot pause their own execution. However, they _can_ queue functions (including API calls) and delays, with _millisecond_ level precision rather than seconds, and no one-minute limit. All of this is in addition to the extensive logic available to scripted commands.

## Caveats
Using the action queue is fairly simple, but has a few important things to remember:

- **Each script has one action queue.** This means that scripts won't interfere with each other, but it also means that if your script runs again while its queue isn't empty (such as if you queued a delay that hasn't expired yet) then anything your script queues on its _new_ run will be added to the end of the existing queue.
  To get around this, you can see how many items are left in the queue via `Script.QueueSize` (to figure out what step it's currently on) and clear anything currently in the queue with `Script.ClearQueue()`.

- **Queues are halted and wiped when reloading scripts.** This is to prevent stale actions from being executed anyway, as scripts may be changed or even removed between script reloads.

- **You queue _functions_ (and delays), not chat inputs.** WoLua is like vanilla macros, only better. A queued action (function) can do everything that you can do elsewhere in a WoLua script - including even queueing _more_ things!
  Yes, you can technically create infinite emote loops - just be sure you have a way to end the loop too...

## Usage
There are only three methods relating to the action queue, and one property. All of them are on the top-level [Script API](script.md).

If you want to check how many actions (including delays!) are in your script's action queue, you can access `Script.QueueSize` to get a count. If your script is carefully written, you can use this to figure out what step of a sequence you're on!

If you have things in there that you want to remove, you can clear the entire queue with `Script.ClearQueue()` - there is no way to remove individual items, sorry. You wouldn't be able to examine them anyway, since actions are functions.

In order to queue an _action_ - actually _doing_ something - you want to call `Script.QueueAction()` with the function to execute. This can be a function you write in your script, or it can be a direct API call. If you want to pass arguments to a function, you don't need to make a wrapper - just pass the arguments to `QueueAction()` after the function and they'll be provided when it's called. <!-- Queue totally doesn't sound like a word anymore. -->

Finally, of course, you'll want to be able to queue _delays_ too, in order to space out your actions. Otherwise, what's the point of queueing them? For that, you call `Script.QueueDelay()` with the number of _milliseconds_ to wait for. Don't try using decimals because the call will fail and your script will error.
