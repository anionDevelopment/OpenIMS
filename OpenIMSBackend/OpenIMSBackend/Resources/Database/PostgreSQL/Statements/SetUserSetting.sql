
insert into "UserSettings"("UserId", "Key", "Value")
	values(@UserId, @Key, @Value)
	on conflict("UserId", "Key") do update set "Value"=@Value;
