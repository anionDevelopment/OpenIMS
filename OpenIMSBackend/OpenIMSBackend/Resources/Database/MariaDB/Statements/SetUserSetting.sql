
insert into UserSettings(`UserId`, `Key`, `Value`)
	values(@UserId, @Key, @Value)
	on duplicate key update `Value`=@Value;
