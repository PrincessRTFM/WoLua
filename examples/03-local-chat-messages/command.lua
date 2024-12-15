Game.PrintMessage("Yes, you can actually send chat messages on script load. It's not a GOOD idea, but you CAN do it.")

Script(function()
	Game.PrintError("Oh no! This script prints an error when called!")
	Game.PrintMessage("Don't worry, nothing's actually wrong.")
end)

-- yeah, this example is really that simple
-- all three of the above lines will be printed to the player's chatlog
-- note that you COULD use PrintError() on setup if something goes wrong, but there's no guarantee the user will see it the first time,
-- since the server sends welcome messages and dalamud prints its own welcome messages
-- still might be worth a shot, but you should also save the error and print it in the script's callback to make sure the user sees
