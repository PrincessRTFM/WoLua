-- the name of this function doesn't matter, and it can be a `local` function if you want
-- it can even be an "anonymous" function: `Script(function() end)` does exactly the same thing as this example
function invokedWhenScriptCommandIsUsed()
	-- nop
end

-- if you don't do this somewhere on script startup, your script is considered broken and produces an error
Script(invokedWhenScriptCommandIsUsed)
