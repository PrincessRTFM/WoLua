function echoUserArguments(text)
	if #text == 0 then -- this is how lua checks for empty strings
		Game.PrintError("You didn't provide any input!")
	else
		Game.PrintMessage("You passed: " .. text)
	end
end

Script(echoUserArguments)
