
select `Value`
	from UserSettings
	where `UserId`=@UserId and `Key`=@Key;
