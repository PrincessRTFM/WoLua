# Action queueing
_Wait for it... wait for it... NOW!_

## Overview
In vanilla, game macros are able to use `<wait.SECONDS>` to pause for up to sixty seconds after a macro line executes. This allows, for instance, an emote animation to reach a certain point before a message is sent, or for two people synchronising appropriately written macros to create a scripted scene of emotes and messages. Unfortunately, for technical reasons, WoLua scripts cannot pause their own execution. However, they _can_ queue functions (including API calls) and delays, with _millisecond_ level precision rather than seconds, and no one-minute limit. All of this is in addition to the extensive logic available to scripted commands.

## Caveats
Using the action queue is fairly simple, but has a few important things to remember:

- **Each script has one action queue.** This means that scripts won't interfere with each other, but it also means that if your script runs again while its queue isn't empty (such as if you queued a delay that hasn't expired yet) then anything your script queues on its _new_ run will be added to the end of the existing queue.\
  To get around this, you can see how many items are left in the queue via `Script.QueueSize` (to figure out what step it's currently on) and clear anything currently in the queue with `Script.ClearQueue()`.

- **Queues are halted and wiped when reloading scripts.** This is to prevent stale actions from being executed anyway, as scripts may be changed or even removed between script reloads.

- **You queue _functions_ (and delays), not chat inputs.** WoLua is like vanilla macros, only better. A queued action (function) can do everything that you can do elsewhere in a WoLua script - including even queueing _more_ things!\
  Yes, you can technically create infinite emote loops - just be sure you have a way to end the loop too.

- **Arguments to queued functions are evaluated when _queued_, not when _called_.** This is a function of the lua runtime. Queueing a function call necessarily evaluates all of the arguments being queued with it, and then _those_ values are handed to WoLua to queue. If you, eg, mix `Game.Player.Target` and strings using placeholders like `<t>`, output may break if the player changes targets. You should also ensure _in the queued function_ that any [entity wrappers](entity.md) passed as arguments are still valid, as the entity in question may have left the zone.

- **All script queues are cleared when logging out of a character.** Script queues ignore actions while not logged into a character, and automatically clear all actions when logging out or shutting down, to avoid dangling actions that attempt to execute during an invalid game state, or on a different character than initially started them. Depending on run time, it would otherwise be possible to start an RP macro, log out, log in on a _different_ character, and have the macro continue attempting to perform emotes or send messages. It's also possible that scripts not specifically written with mid-run logouts in mind could crash and require a script reload, if the user suddenly didn't have a character or any loaded game state information.

## Usage
There are only three methods relating to the action queue, and one property. All of them are on the top-level [Script API](script.md).

If you want to check how many actions (including delays!) are in your script's action queue, you can access `Script.QueueSize` to get a count. If your script is carefully written, you can use this to figure out what step of a sequence you're on!

If you have things in there that you want to remove, you can clear the entire queue with `Script.ClearQueue()` - there is no way to remove individual items, sorry. You wouldn't be able to examine them anyway, since actions are functions.

In order to queue an _action_ - actually _doing_ something - you want to call `Script.QueueAction()` with the function to execute. This can be a function you write in your script, or it can be a direct API call. If you want to pass arguments to a function, you don't need to make a wrapper - just pass the arguments to `QueueAction()` after the function and they'll be provided when it's called. <!-- Queue totally doesn't sound like a word anymore. -->

Finally, of course, you'll want to be able to queue _delays_ too, in order to space out your actions. Otherwise, what's the point of queueing them? For that, you call `Script.QueueDelay()` with the number of _milliseconds_ to wait for. Don't try using decimals because the call will fail and your script will error.

### Commands

It's possible to check the number of actions (functions and delays both) that a script has queued via the `/wolua info <identifier>` command. If the given identifier isn't valid (no such script _exists_), you'll get an error. Otherwise, if the script _exists_ but isn't _valid_ (has encountered a fatal error), the particular problem will be identified. Finally, if the script is valid, you'll be told how many actions are in its queue.

If you want to stop a script from performing queued actions, you can use the `/wolua stop <identifier>` command to empty out the action queue, although this will _not_ cancel any currently executing functions. As with the `info` subcommand, if there are problems, you'll receive an error in chat explaining them. Otherwise, you'll be told how many actions _were_ queued, before the queue was cleared.

Finally, if you need to abort _all_ queued actions across _every_ loaded script, `/wolua stopall` will do so, and also print the above information for each script, so that you know what was interrupted. This should be done with care and only when necessary, as there is _no way_ to see the specific actions a script had queued, nor to restore them. Depending on how the script was written, it may be left in an indeterminate state.\
Consider using `/wolua reload` instead, as reloading scripts also aborts and clears all loaded action queues; the main reason to use this command instead is for scripts that are so badly written that they begin a queue loop on load. If you identify any such scripts, you are _strongly recommended_ to remove them immediately.

## Implementation
A slightly more technical overview, for those curious.

As mentioned in the [script lifecycle section of the main readme file](README.md#script-lifecycle), scripts are executed entirely and immediately upon load. They are then responsible for registering their _callback function_, which is executed when the user invokes the script via `/wolua call` or the direct command, if enabled. That function is then executed _entirely and immediately_. All operations it performs are handled right away, including the _queueing_ of functions and delays.

Each game tick - many times per second, when the game processes events and user input, and updates its state - WoLua looks at the queue for each loaded script. Each queue has an associated (real world) time at which the next action may be read and executed. If this time is in the future, WoLua does nothing with that script's queue. Otherwise, WoLua tries to pull a _single_ action from the queue. If nothing is found, that queue is ignored because there's nothing to be done.

If the pulled action is a _delay_, WoLua takes the current (real world) time, adds the amount of time in the delay action, and sets that as the new "minimum time" to process further actions on that queue. The next tick, when WoLua looks at the queue again, it will see a time in the future and do nothing.

If the pulled action is a _function_, WoLua executes it immediately, providing whatever arguments were queued with it. Note that arguments are evaluated _when the function is queued_, by the lua runtime itself. The resulting values are then given to WoLua and stored in the queue, along with the function.\
As a result, if you queue a function with an argument of `Game.Player.Target` and then the player changes target before the function is called, the function still receives whatever the players target was _at the time the function was queued_.\
If, on the other hand, you queue a function to send a game command and provide it a command that uses the `<t>` chat message placeholder, that game command will use the players target _at the time it is run_, because all WoLua received (and queued) was a text string.

As an example, take the following script:

```lua
local function echo(what)
	Game.SendChat("/echo " .. tostring(what))
end

local function onInvoke(args)
	Script.QueueAction(echo, "Waiting for a few seconds...")
	Script.QueueDelay(2500)
	Script.QueueAction(echo, Game.Player.Target)
	Script.QueueAction(echo, "<t>")
end

Script(onInvoke)
```

When the user calls this script, they will see a chat message reading `Waiting for a few seconds...`. About two and a half seconds later, they'll get two more messages in chat: the name of whatever their target _was_, and a clickable link of whatever their target _is_.

The lua runtime evaluated `Game.Player.Target`, which uses WoLua APIs to get an entity wrapper representing their target at the time of evaluation, then passed that to WoLua's script queueing API along with the function `echo`. When WoLua processed that action, it called the function and gave it the stored entity wrapper. In this case, since entity wrappers stringify to the name of the entity, the chat message WoLua ran through the game was `/echo Name-Of-Entity`.

The second queued command evaluated `"<t>"` and got a string containing the text `<t>`, which is dutifully handed WoLua to queue along with the `echo` function. When that action was processed, the function was called with that string, resulting in the game being given the command `/echo <t>` instead.
